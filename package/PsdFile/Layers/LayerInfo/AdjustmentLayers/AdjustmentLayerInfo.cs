using System.Collections;
using System.Collections.Generic;
using PhotoshopFile;
using UnityEngine;

public abstract class AdjustmentLayerInfo : LayerInfo
{
    public override string Signature => "8BIM";

    internal AdjustmentLayerInfo(PsdBinaryReader reader, int length)
    {
        Debug.Log("AdjustmentLayer: " + Key + ", " + length + " bytes");
    }
    
    protected override void WriteData(PsdBinaryWriter writer)
    {
    }
}
