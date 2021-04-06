using PaintDotNet;
using UnityEngine;

public class BitmapLayer
{
    public BitmapLayer(int psdFileColumnCount, int psdFileRowCount)
    {
        
    }

    public Rect Bounds { get; set; }
    public Surface Surface { get; set; }
    public string Name { get; set; }
    public byte Opacity { get; set; }
    public bool Visible { get; set; }
    public string BlendMode { get; set; }
}

public class Surface
{
    public unsafe ColorBgra* GetRowAddress(int y)
    {
        // return (ColorBgra*)(((byte *)scan0.VoidStar) + GetRowByteOffset(y));
        return (ColorBgra*) (byte*) 0;
    }
}



public class Document
{
    public Document(int psdFileColumnCount, int psdFileRowCount)
    {
        
    }
}