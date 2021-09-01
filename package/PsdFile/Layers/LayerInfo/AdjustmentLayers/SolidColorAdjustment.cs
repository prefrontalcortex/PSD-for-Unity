using System.Collections;
using System.Collections.Generic;
using PhotoshopFile;
using UnityEngine;

public class SolidColorAdjustment : AdjustmentLayerInfo
{
    public override string Key => "SoCo";

    public SolidColorAdjustment(PsdBinaryReader reader, int length) : base(reader, length)
    { 
        var version = reader.ReadInt32();
        Debug.Log("BEGIN " + Key + ", version: " + version);
        TypeToolObjectInfo.ReadDescriptor(reader, out var byteCount);
        Debug.Log("END " + Key + ", descriptor length: " + byteCount);
    }

    protected override void WriteData(PsdBinaryWriter writer)
    {
        
    }
    
    protected override bool WriteSupported => false;
}