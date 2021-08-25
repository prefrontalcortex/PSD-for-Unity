using System.Collections;
using System.Collections.Generic;
using PhotoshopFile;
using UnityEngine;

public class BrightnessContrastAdjustment : AdjustmentLayerInfo
{
    public override string Key => "brit";

    public BrightnessContrastAdjustment(PsdBinaryReader reader, int length) : base(reader, length)
    {
        var brightness = reader.ReadInt16();
        var contrast = reader.ReadInt16();
        var mean = reader.ReadInt16();
        var lab = reader.ReadByte();
        
        Debug.Log(brightness + ", " + contrast + ", " + mean + ", " + lab);
    }

    protected override void WriteData(PsdBinaryWriter writer)
    {
        
    }
    
    protected override bool WriteSupported => false;
}
