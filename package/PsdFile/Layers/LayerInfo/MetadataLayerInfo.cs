using System.Collections;
using System.Collections.Generic;
using PhotoshopFile;
using UnityEngine;

public class MetadataLayerInfo : LayerInfo
{
    public override string Signature => "8BIM";
    public override string Key => "shmd";

    public MetadataLayerInfo(PsdBinaryReader reader, int length)
    {
        var len = reader.ReadInt32();
        for (int i = 0; i < len; i++)
        {
            var signature = reader.ReadInt32();
            var key = reader.ReadInt32();
            var copyOnDuplication = reader.ReadBoolean();
            reader.ReadBytes(3); // padding
            var dataLength = reader.ReadInt32();
            reader.ReadBytes(dataLength);
            // Debug.Log("[" + nameof(MetadataLayerInfo) + "] " + "Signature: " + signature +", Key: " + key+ ", Undocumented bytes: " + dataLength);
        }
    }
    
    protected override void WriteData(PsdBinaryWriter writer)
    {
        
    }
    
    protected override bool WriteSupported => false;
}
