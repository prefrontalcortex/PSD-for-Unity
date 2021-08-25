using System.Collections;
using System.Collections.Generic;
using System.Text;
using PhotoshopFile;
using UnityEngine;

public class HueSaturationAdjustment : AdjustmentLayerInfo
{
    public override string Key => "hue2";

    public HueSaturationAdjustment(PsdBinaryReader reader, int length) : base(reader, length)
    {
        var version = reader.ReadInt16();
        Debug.Log("BEGIN " + Key + ", version: " + version +", length: " + length);
        // return;

        var isColorization = reader.ReadBoolean();
        reader.ReadByte();
        
        var colorizationHue = reader.ReadInt16();
        var colorizationSaturation = reader.ReadInt16();
        var colorizationLightness = reader.ReadInt16();
        
        var masterHue = reader.ReadInt16();
        var masterSaturation = reader.ReadInt16();
        var masterLightness = reader.ReadInt16();

        var sb = new StringBuilder();
        sb.AppendLine(nameof(isColorization) + ": " + isColorization + "\n" + 
                  nameof(colorizationHue) + ": " + colorizationHue + "\n" + 
                  nameof(colorizationSaturation) + ": " + colorizationSaturation + "\n" + 
                  nameof(colorizationLightness) + ": " + colorizationLightness + "\n" + 
                  nameof(masterHue) + ": " + masterHue + "\n" + 
                  nameof(masterSaturation) + ": " + masterSaturation + "\n" + 
                  nameof(masterLightness) + ": " + masterLightness + "\n");
        
        for (int i = 0; i < 6; i++)
        {
            for (int k = 0; k < 4; k++)
            {
                var range = reader.ReadInt16();
                sb.AppendLine(i + " - Range " + k + " - " + range);
            }

            for (int j = 0; j < 3; j++)
            {
                var range = reader.ReadInt16();
                sb.AppendLine(i + " - Setting " + j + " - " + range);
            }
        }
        
        Debug.Log(sb);
        Debug.Log("END " + Key + ", descriptor length: ");
    }

    protected override void WriteData(PsdBinaryWriter writer)
    {
        
    }
    
    protected override bool WriteSupported => false;
}
