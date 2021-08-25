using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PhotoshopFile;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Color = UnityEngine.Color;

public class CreatePsdTest : MonoBehaviour
{
    public void SetFile(LoadPsdTest.File file)
    {
      this.file = file;
    }
    
    public LoadPsdTest.File file;

    [ContextMenu("Create File")]
    void CreateFileContext()
    {
      var outputPath = Path.GetFullPath(Application.dataPath + "/../testfile.psd");
      CreateFile(file, outputPath);
    }
    
    public static void CreateFile(LoadPsdTest.File file, string outputPath)
    {
        var tex = new Texture2D((int) file.rect.width, (int) file.rect.height);
        tex.SetPixels(Enumerable.Repeat(new Color(1, 0, 0, 1), tex.width * tex.height).ToArray());
        tex.Apply();
      
        var psdFile = new PsdFile(PsdFileVersion.Psd);
        psdFile.BaseLayer = new Layer(psdFile);
        // file.BaseLayer.

        psdFile.RowCount = (int) file.rect.height;
        psdFile.ColumnCount = (int) file.rect.width;
        
        psdFile.ChannelCount = 4; 
        psdFile.ColorMode = PsdColorMode.RGB;
        psdFile.BitDepth = 8;
        // psdFile.Resolution = GetResolutionInfo(input);
        psdFile.ImageCompression = ImageCompression.Rle;
        
        // Delegate to store the composite
        Action storeCompositeAction = () =>
        {
          // Allocate space for the composite image data
          int imageSize = psdFile.RowCount * psdFile.ColumnCount;
          for (short i = 0; i < psdFile.ChannelCount; i++)
          {
            var channel = new Channel(i, psdFile.BaseLayer);
            channel.ImageData = new byte[imageSize];
            channel.ImageCompression = psdFile.ImageCompression;
            psdFile.BaseLayer.Channels.Add(channel);
          }

          var channelsArray = psdFile.BaseLayer.Channels.ToIdArray();
          StoreLayerImage(channelsArray, channelsArray[3],
            tex, psdFile.BaseLayer.Rect);

          // progress.Notify(percentPerLayer);
        };

        var psdToken = new PsdSaveConfigToken() { RleCompress = true };
        
        // Delegate to store the layers
        Action storeLayersAction = () =>
        {
          // LayerList is an ArrayList, so we have to cast to get a generic
          // IEnumerable that works with LINQ.
          var pdnLayers = file.FlattenedLayersWithGroupDividers;
          var psdLayers = pdnLayers.Select(pdnLayer => // AsParallel().AsOrdered().
          {
            if (pdnLayer is LoadPsdTest.GroupDivider)
            {
              var separatorLayer = new PhotoshopFile.Layer(psdFile);
              separatorLayer.AdditionalInfo.Add(new LayerSectionInfo(LayerSectionType.SectionDivider));
              var infos = pdnLayer.layer?.AdditionalInfo?.Where(x => !(x is LayerSectionInfo));
              if(infos != null)
                separatorLayer.AdditionalInfo.AddRange(infos);
              StoreLayer2(pdnLayer, separatorLayer, psdToken);
              return separatorLayer;
            }

            var psdLayer = new PhotoshopFile.Layer(psdFile);
            StoreLayer2(pdnLayer, psdLayer, psdToken);

            var layerSectionType = LayerSectionType.Layer;
            if (pdnLayer.isGroup)
              layerSectionType = LayerSectionType.OpenFolder;
            
            psdLayer.AdditionalInfo.Add(new LayerSectionInfo(layerSectionType));
            var infos2 = pdnLayer.layer?.AdditionalInfo?.Where(x => !(x is LayerSectionInfo));
            if(infos2 != null)
              psdLayer.AdditionalInfo.AddRange(infos2);
            
            // progress.Notify(percentPerLayer);
            return psdLayer;
          }).Reverse();

          var enumerable = psdLayers.ToList();
          
          // for (int i = enumerable.Count() - 1; i >= 0; i--)
          //   Debug.Log(LoadPsdTest.LayerDebug(i, enumerable[i]));
          
          psdFile.Layers.AddRange(enumerable);
        };

        // Process composite and layers in parallel
        // Parallel.Invoke(storeCompositeAction, storeLayersAction);
        storeCompositeAction();
        storeLayersAction();

        using(var output = new FileStream(outputPath, FileMode.OpenOrCreate))
        {
          psdFile.PrepareSave();
          psdFile.Save(output, Encoding.Default);
          Debug.Log("File was saved to <a href=\"" + outputPath + "\">" + outputPath + "</a>");
        }
    }


    [Serializable]
    public class PsdSaveConfigToken
    {
      public bool RleCompress { get; set; }
    }

    public static void StoreLayer2(LoadPsdTest.LayerA layer, PhotoshopFile.Layer psdLayer, PsdSaveConfigToken psdToken)
    {
      psdLayer.Name = layer.name;
      psdLayer.Rect = layer.rect;
      psdLayer.BlendModeKey = PsdBlendMode.Normal;
      psdLayer.Opacity = 255;
      psdLayer.Visible = true;
      psdLayer.Masks = new MaskInfo();
      psdLayer.BlendingRangesData = new BlendingRanges(psdLayer);

      if (layer.texture)
      {
        // Store channel metadata
        int layerSize = (int) psdLayer.Rect.width * (int) psdLayer.Rect.height;
        for (int i = -1; i < 3; i++)
        {
          var ch = new Channel((short)i, psdLayer);
          ch.ImageCompression = psdToken.RleCompress ? ImageCompression.Rle : ImageCompression.Raw;
          ch.ImageData = new byte[layerSize];
          psdLayer.Channels.Add(ch);
        }

        // Store and compress channel image data
        var channelsArray = psdLayer.Channels.ToIdArray();
        StoreLayerImage(channelsArray, psdLayer.AlphaChannel, layer.texture, psdLayer.Rect);
      }

      if (layer.maskTexture)
      {
        var layerMask = new Mask(psdLayer, layer.maskRect, Byte.MaxValue, new BitVector32(16));
        var maskSize = layer.maskTexture.width * layer.maskTexture.height;
        psdLayer.Masks.LayerMask = layerMask;
        // layerMask.Rect = new Rect(0, 0, mask.width, mask.height);
        layerMask.PositionVsLayer = true;

        var ch = new Channel((short)-2, psdLayer);
        ch.ImageCompression = psdToken.RleCompress ? ImageCompression.Rle : ImageCompression.Raw;
        ch.ImageData = new byte[maskSize];
        psdLayer.Channels.Add(ch);
        
        layerMask.ImageData = ch.ImageData; 

        StoreMaskImage(ch, layer.maskTexture, layer.maskRect); 
      }
    }
    
    public static void StoreLayer(Texture2D layer, Texture2D mask,
      PhotoshopFile.Layer psdLayer, PsdSaveConfigToken psdToken)
    {
      // Set layer metadata
      if(layer)
      {
        psdLayer.Name = layer.name;
        psdLayer.Rect = new Rect(0, 0, layer.width, layer.height);
      }
      else
      {
        psdLayer.Name = "No Texture";
      }
      psdLayer.BlendModeKey = PsdBlendMode.Normal;
      psdLayer.Opacity = 255;
      psdLayer.Visible = true;
      psdLayer.Masks = new MaskInfo();
      psdLayer.BlendingRangesData = new BlendingRanges(psdLayer);
      
      if(layer) {
        // Store channel metadata
        int layerSize = (int) psdLayer.Rect.width * (int) psdLayer.Rect.height;
        for (int i = -1; i < 3; i++)
        {
          var ch = new Channel((short)i, psdLayer);
          ch.ImageCompression = psdToken.RleCompress ? ImageCompression.Rle : ImageCompression.Raw;
          ch.ImageData = new byte[layerSize];
          psdLayer.Channels.Add(ch);
        }

        // Store and compress channel image data
        var channelsArray = psdLayer.Channels.ToIdArray();
        StoreLayerImage(channelsArray, psdLayer.AlphaChannel, layer, psdLayer.Rect);
      }
      
      // store mask metadata
      if (mask)
      {
        var layerMask = new Mask(psdLayer, new Rect(0,0, mask.width, mask.height), Byte.MaxValue, new BitVector32(16));
        var maskSize = mask.width * mask.height;
        psdLayer.Masks.LayerMask = layerMask;
        // layerMask.Rect = new Rect(0, 0, mask.width, mask.height);
        layerMask.PositionVsLayer = true;

        var ch = new Channel((short)-2, psdLayer);
        ch.ImageCompression = psdToken.RleCompress ? ImageCompression.Rle : ImageCompression.Raw;
        ch.ImageData = new byte[maskSize];
        psdLayer.Channels.Add(ch);
        
        layerMask.ImageData = ch.ImageData; 

        StoreMaskImage(ch, mask, new Rect(0,0, mask.width, mask.height)); 
      }
    }
    
    /// <summary>
    /// Stores and compresses the image data for the layer.
    /// </summary>
    /// <param name="channels">Destination channels.</param>
    /// <param name="alphaChannel">Destination alpha channel.</param>
    /// <param name="surface">Source image from Paint.NET.</param>
    /// <param name="rect">Image rectangle to store.</param>
    unsafe private static void StoreLayerImage(Channel[] channels, Channel alphaChannel, Texture2D surface, Rect rect)
    {
      surface = MakeTextureReadable(surface, out var isCopy);
      var colors = surface.GetPixels32();
      if (isCopy) DestroyImmediate(surface);
      
      for (int y = 0; y < (int) rect.height; y++)
      {
        int srcRowIndex = ((int) rect.height - 1 - y) * (int) rect.width;
        int destRowIndex = y * (int) rect.width;
        // ColorBgra* srcRow = surface.GetRowAddress(y + rect.Top);
        // ColorBgra* srcPixel = srcRow + rect.Left;
        
        for (int x = 0; x < (int) rect.width; x++)
        {
          int srcIndex = srcRowIndex + x;
          int destIndex = destRowIndex + x;
          var srcPixel = colors[srcIndex];

          channels[0].ImageData[destIndex] = srcPixel.r;
          channels[1].ImageData[destIndex] = srcPixel.g;
          channels[2].ImageData[destIndex] = srcPixel.b;
          alphaChannel.ImageData[destIndex] = srcPixel.a;
        }
      }

      Parallel.ForEach(channels, channel =>
        channel.CompressImageData()
      );
    }

    unsafe private static void StoreMaskImage(Channel maskChannel, Texture2D mask, Rect rect)
    {
      // TODO GraphicsFormatUtility.ComponentCount etc. might be better suited here
      int channelToUse = mask.format == TextureFormat.Alpha8 ? 3 : 0;
      mask = MakeTextureReadable(mask, out var isCopy);
      var colors = mask.GetPixels32();
      if (isCopy) DestroyImmediate(mask);
      
      for (int y = 0; y < (int) rect.height; y++)
      {
        int srcRowIndex = ((int) rect.height - 1 - y) * (int) rect.width;
        int destRowIndex = y * (int) rect.width;
        // ColorBgra* srcRow = surface.GetRowAddress(y + rect.Top);
        // ColorBgra* srcPixel = srcRow + rect.Left;
        
        for (int x = 0; x < (int) rect.width; x++)
        {
          int srcIndex = srcRowIndex + x;
          int destIndex = destRowIndex + x;
          var srcPixel = colors[srcIndex];

          // r channel or a channel?
          maskChannel.ImageData[destIndex] = srcPixel[channelToUse];
        }
      }
      
      maskChannel.CompressImageData();
    }
    
    private static Texture2D MakeTextureReadable(Texture2D surface, out bool isCopy)
    {
      if (surface.isReadable)
      {
        isCopy = false;
        return surface;
      }
      
      var rt = new RenderTexture(surface.width, surface.height, 0, DefaultFormat.LDR);
      rt.Create();
      var active = RenderTexture.active;
      RenderTexture.active = rt;
      Graphics.Blit(surface, rt);
      var newTex = new Texture2D(surface.width, surface.height, DefaultFormat.LDR, TextureCreationFlags.None);
      newTex.ReadPixels(new Rect(0, 0, surface.width, surface.height), 0, 0);
      newTex.Apply();
      newTex.name = "--temp--";
      newTex.hideFlags = HideFlags.DontSave;
      RenderTexture.active = active;
      rt.Release();
      isCopy = true;
      return newTex;
    }


    // [ContextMenu("Load File")]
    // void LoadFile()
    // {
    //     var input = new FileStream(Application.dataPath + "/../testfile.psd", FileMode.Open);
    //     var psdFile = new PsdFile(input, new LoadContext());
    //     
    //     // Multichannel images are loaded by processing each channel as a
    //     // grayscale layer.
    //     if (psdFile.ColorMode == PsdColorMode.Multichannel)
    //     {
    //         CreateLayersFromChannels(psdFile);
    //         psdFile.ColorMode = PsdColorMode.Grayscale;
    //     }
    //     
    //     if (psdFile.Layers.Count == 0)
    //     {
    //         psdFile.BaseLayer.CreateMissingChannels();
    //         var layer = Layer.CreateBackgroundLayer(psdFile.ColumnCount, psdFile.RowCount);
    //         ImageDecoderPdn.DecodeImage(layer, psdFile.BaseLayer);
    //         document.Layers.Add(layer);
    //     }
    //     else
    //     {
    //         psdFile.VerifyLayerSections();
    //         ApplyLayerSections(psdFile.Layers);
    //     
    //         var pdnLayers = psdFile.Layers.AsParallel().AsOrdered()
    //             .Select(psdLayer => psdLayer.DecodeToPdnLayer())
    //             .ToList();
    //         document.Layers.AddRange(pdnLayers);
    //     }
    // }

    /// <summary>
    /// Creates a layer for each channel in a multichannel image.
    /// </summary>
    private static void CreateLayersFromChannels(PsdFile psdFile)
    {
      if (psdFile.ColorMode != PsdColorMode.Multichannel)
      {
        throw new Exception("Not a multichannel image.");
      }
      if (psdFile.Layers.Count > 0)
      {
        throw new PsdInvalidException("Multichannel image should not have layers.");
      }

      // Get alpha channel names, preferably in Unicode.
      var alphaChannelNames = (AlphaChannelNames)psdFile.ImageResources
        .Get(ResourceID.AlphaChannelNames);
      var unicodeAlphaNames = (UnicodeAlphaNames)psdFile.ImageResources
        .Get(ResourceID.UnicodeAlphaNames);
      if ((alphaChannelNames == null) && (unicodeAlphaNames == null))
      {
        throw new PsdInvalidException("No channel names found.");
      }

      var channelNames = (unicodeAlphaNames != null)
        ? unicodeAlphaNames.ChannelNames
        : alphaChannelNames.ChannelNames;
      var channels = psdFile.BaseLayer.Channels;
      if (channels.Count > channelNames.Count)
      {
        throw new PsdInvalidException("More channels than channel names.");
      }

      // Channels are stored from top to bottom, but layers are stored from
      // bottom to top.
      for (int i = channels.Count - 1; i >= 0; i--)
      {
        var channel = channels[i];
        var channelName = channelNames[i];

        // Copy metadata over from base layer
        var layer = new PhotoshopFile.Layer(psdFile);
        layer.Rect = psdFile.BaseLayer.Rect;
        layer.Visible = true;
        layer.Masks = new MaskInfo();
        layer.BlendingRangesData = new BlendingRanges(layer);

        // We do not attempt to reconstruct the appearance of the image, but
        // only to provide access to the channels image data.
        layer.Name = channelName;
        layer.BlendModeKey = PsdBlendMode.Darken;
        layer.Opacity = 255;

        // Copy channel image data into the new grayscale layer
        var layerChannel = new Channel(0, layer);
        layerChannel.ImageCompression = channel.ImageCompression;
        layerChannel.ImageData = channel.ImageData;
        layer.Channels.Add(layerChannel);

        psdFile.Layers.Add(layer);
      }
    }
}
