using System;
using PhotoshopFile;
using UnityEngine;

public class SheetColorLayerInfo : LayerInfo
{
    public override string Signature => "8BIM";
    public override string Key => "lclr";
    public int colorId = 0;
    public Color color;
    public override string ToString() => $"[{nameof(SheetColorLayerInfo)}] {colorId}={Color.ToString()}";
    public string hexString;

    // Ps Layer Colors
    private static Color[] colors = new Color[]
    {
        new Color(1, 1, 1, 0),
        new Color(1, 0, 0, 1),
        new Color(1, 0.5f, 0, 1),
        new Color(1, 1, 0, 1),
        new Color(0, 1, 0, 1),
        new Color(0, 0, 1, 1),
        new Color(1, 0, 1, 1),
        new Color(0.5f, 0.5f, 0.5f, 1f),
    };

    public Color Color => colors[colorId];
    
    protected override void WriteData(PsdBinaryWriter writer)
    {
        var startPosition = writer.BaseStream.Position;

        // writer.WritePadding(startPosition, 4);
        writer.Write((byte)0);
        writer.Write((byte)colorId);
        writer.WritePadding(startPosition,8);
    }

    string Hex(Color color)
    {
        var c32 = (Color32) color;
        return "#" + BitConverter.ToString(new byte[] { c32.r, c32.g, c32.b, c32.a }).Replace("-", string.Empty);
    }
    
    public SheetColorLayerInfo(PsdBinaryReader reader, int length)
    {
        var allBytes = reader.ReadBytes(length);
        hexString = System.BitConverter.ToString(allBytes).Replace("-", " ");
        var swap = new byte[2];
        swap[0] = allBytes[1];
        swap[1] = allBytes[0];
        colorId = BitConverter.ToInt16(swap, 0);
        // if(colorId != 0)
            // Debug.Log("<color=" + Hex(colors[colorId]) + ">Layer Color: " + colorId + "</color> [" + hexString + "]");
    }
    
    protected override bool WriteSupported => true;
}
