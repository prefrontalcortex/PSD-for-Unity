using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PaintDotNet.Data.PhotoshopFileType;
using PhotoshopFile;
using UnityEngine;
using Color = UnityEngine.Color;

public class CreatePsdTest : MonoBehaviour
{
    public List<Texture2D> layers = new List<Texture2D>();

    [ContextMenu("psd/Create File")]
    void CreateFile()
    {
        var tex = new Texture2D(1000, 1000);
        tex.SetPixels(Enumerable.Repeat(new Color(1, 0, 0, 1), tex.width * tex.height).ToArray());
        tex.Apply();
      
        var psdFile = new PsdFile(PsdFileVersion.Psd);
        psdFile.BaseLayer = new Layer(psdFile);
        // file.BaseLayer.

        psdFile.RowCount = tex.height;
        psdFile.ColumnCount = tex.width;
        
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
          var pdnLayers = layers;
          var psdLayers = pdnLayers./*AsParallel().AsOrdered().*/Select(pdnLayer =>
          {
            var psdLayer = new PhotoshopFile.Layer(psdFile);
            StoreLayer(pdnLayer, psdLayer, psdToken);

            // progress.Notify(percentPerLayer);
            return psdLayer;
          });
          psdFile.Layers.AddRange(psdLayers);
        };

        // Process composite and layers in parallel
        // Parallel.Invoke(storeCompositeAction, storeLayersAction);
        storeCompositeAction();
        storeLayersAction();
        
        using(var output = new FileStream(Application.dataPath + "/../testfile.psd", FileMode.OpenOrCreate))
        {
          psdFile.PrepareSave();
          psdFile.Save(output, Encoding.Default);
          Debug.Log("File was saved");
        }
    }


    [Serializable]
    public class PsdSaveConfigToken
    {
      public bool RleCompress { get; set; }
    }

    public static void StoreLayer(Texture2D layer,
      PhotoshopFile.Layer psdLayer, PsdSaveConfigToken psdToken)
    {
      // Set layer metadata
      psdLayer.Name = layer.name;
      psdLayer.Rect = new Rect(0, 0, layer.width, layer.height);
      psdLayer.BlendModeKey = PsdBlendMode.Normal;
      psdLayer.Opacity = 255;
      psdLayer.Visible = true;
      psdLayer.Masks = new MaskInfo();
      psdLayer.BlendingRangesData = new BlendingRanges(psdLayer);
      
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
    
    /// <summary>
    /// Stores and compresses the image data for the layer.
    /// </summary>
    /// <param name="channels">Destination channels.</param>
    /// <param name="alphaChannel">Destination alpha channel.</param>
    /// <param name="surface">Source image from Paint.NET.</param>
    /// <param name="rect">Image rectangle to store.</param>
    unsafe private static void StoreLayerImage(Channel[] channels, Channel alphaChannel, Texture2D surface, Rect rect)
    {
      var colors = surface.GetPixels32();
      
      for (int y = 0; y < (int) rect.height; y++)
      {
        int destRowIndex = y * (int) rect.width;
        // ColorBgra* srcRow = surface.GetRowAddress(y + rect.Top);
        // ColorBgra* srcPixel = srcRow + rect.Left;
        
        for (int x = 0; x < (int) rect.width; x++)
        {
          int destIndex = destRowIndex + x;
          var srcPixel = colors[destIndex];

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

    
    
    // [ContextMenu("psd/Load File")]
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
