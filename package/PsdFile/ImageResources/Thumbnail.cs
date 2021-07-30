﻿/////////////////////////////////////////////////////////////////////////////////
//
// Photoshop PSD FileType Plugin for Paint.NET
//
// This software is provided under the MIT License:
//   Copyright (c) 2006-2007 Frank Blumenberg
//   Copyright (c) 2010-2013 Tao Yue
//
// Portions of this file are provided under the BSD 3-clause License:
//   Copyright (c) 2006, Jonas Beckeman
//
// See LICENSE.txt for complete licensing and attribution information.
//
/////////////////////////////////////////////////////////////////////////////////

using System.IO;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace PhotoshopFile
{
  /// <summary>
  /// Summary description for Thumbnail.
  /// </summary>
  public class Thumbnail : RawImageResource
  {
    public Texture2D Image { get; private set; }

    public Thumbnail(ResourceID id, string name)
      : base(id, name)
    {
    }

    public Thumbnail(PsdBinaryReader psdReader, ResourceID id, string name, int numBytes)
      : base(psdReader, "8BIM", id, name, numBytes)
    {
      using (var memoryStream = new MemoryStream(Data))
      using (var reader = new PsdBinaryReader(memoryStream, psdReader))
      {
        const int HEADER_LENGTH = 28;
        var format = reader.ReadUInt32();
        var width = reader.ReadUInt32();
        var height = reader.ReadUInt32();
        var widthBytes = reader.ReadUInt32();
        var size = reader.ReadUInt32();
        var compressedSize = reader.ReadUInt32();
        var bitPerPixel = reader.ReadUInt16();
        var planes = reader.ReadUInt16();

        // Raw RGB bitmap
        if (format == 0)
        {
          Image = new Texture2D((int)width, (int)height, GraphicsFormat.R8G8B8_UInt, TextureCreationFlags.None);
        }
        // JPEG bitmap
        else if (format == 1)
        {
          byte[] imgData = reader.ReadBytes(numBytes - HEADER_LENGTH);
          // using (MemoryStream stream = new MemoryStream(imgData))
          {
            Image = new Texture2D((int)width, (int)height, TextureFormat.RGB24, false);
            Image.LoadImage(imgData, false);
            // var bitmap = new Texture2D(stream);
            // Image = (Bitmap)bitmap.Clone();
          }

          // Reverse BGR pixels from old thumbnail format
          if (id == ResourceID.ThumbnailBgr)
          {
            //for(int y=0;y<m_thumbnailImage.Height;y++)
            //  for (int x = 0; x < m_thumbnailImage.Width; x++)
            //  {
            //    Color c=m_thumbnailImage.GetPixel(x,y);
            //    Color c2=Color.FromArgb(c.B, c.G, c.R);
            //    m_thumbnailImage.SetPixel(x, y, c);
            //  }
          }
        }
        else
        {
          throw new PsdInvalidException("Unknown thumbnail format.");
        }
      }
    }
  }
}