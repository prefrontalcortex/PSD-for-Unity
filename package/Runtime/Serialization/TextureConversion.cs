using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PhotoshopFile;
using UnityEngine;

public static class TextureConversion
{
    
		public static Texture2D CreateTexture(Layer layer)
		{
			if ((int)layer.Rect.width == 0 || (int)layer.Rect.height == 0)
				return null;

			Texture2D tex = new Texture2D((int)layer.Rect.width, (int)layer.Rect.height, TextureFormat.RGBA32, true);
			tex.hideFlags = HideFlags.DontSave;
			Color32[] pixels = new Color32[tex.width * tex.height];

			Channel red = (from l in layer.Channels where l.ID == 0 select l).First();
			Channel green = (from l in layer.Channels where l.ID == 1 select l).First();
			Channel blue = (from l in layer.Channels where l.ID == 2 select l).First();
			Channel alpha = layer.AlphaChannel;

			for (int i = 0; i < pixels.Length; i++)
			{
				byte r = red.ImageData[i];
				byte g = green.ImageData[i];
				byte b = blue.ImageData[i];
				byte a = 255;

				if (alpha != null)
					a = alpha.ImageData[i];

				int mod = i % tex.width;
				int n = ((tex.width - mod - 1) + i) - mod;
				pixels[pixels.Length - n - 1] = new Color32(r, g, b, a);
			}

			tex.SetPixels32(pixels);
			tex.Apply();

			return tex;
		}
		
		public static Texture2D CreateMaskTexture(Layer layer)
		{
			if (layer.Masks == null || layer.Masks.LayerMask == null) return null;
			var layerMask = layer.Masks.LayerMask;

			if (layerMask.Rect.width == 0 || layerMask.Rect.height == 0)
				return null;

			Texture2D tex = new Texture2D((int)layerMask.Rect.width, (int)layerMask.Rect.height, TextureFormat.Alpha8, true);
			tex.hideFlags = HideFlags.DontSave;
			Color32[] pixels = new Color32[tex.width * tex.height];

			for (int i = 0; i < pixels.Length; i++)
			{
				byte r = layerMask.ImageData[i];

				int mod = i % tex.width;
				int n = ((tex.width - mod - 1) + i) - mod;
				pixels[pixels.Length - n - 1] = new Color32(r, r, r,r); // TODO make configurable
			}

			tex.SetPixels32(pixels);
			tex.Apply();

			return tex;
		}
}
