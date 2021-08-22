using System.Collections;
using System.Collections.Generic;
using PhotoshopFile;
using UnityEngine;
using UnityEngine.UIElements;

public class TypeToolObjectInfo : LayerInfo
{
    public override string Signature => "8BIM";
    public override string Key => "TySh";

    public string text;
    public string classId;
    public byte[] remainingData;
    
    public TypeToolObjectInfo(PsdBinaryReader reader, int length)
    {
        int remaining = length;
        reader.ReadBytes(2); 
        remaining -= 2;
        for(int i = 0; i < 6; i++)
        {
            reader.ReadInt32();
            remaining -= 4;
            reader.ReadInt32();
            remaining -= 4;
        }
        reader.ReadBytes(2);
        remaining -= 2;
        reader.ReadInt32();
        remaining -= 4;
        
        // read descriptor
        // https://www.adobe.com/devnet-apps/photoshop/fileformatashtml/#50577411_21585
        text = reader.ReadUnicodeString(out var byteCount);
        remaining -= byteCount;
        var classId = reader.ReadInt32();
        remaining -= 4;
        if (classId == 0) {
            this.classId = reader.ReadInt32().ToString();
            remaining -= 4;
        }
        else {
            this.classId = reader.ReadUnicodeString(out var byteCount2);
            remaining -= byteCount2;
        }
        var itemsInDescriptor = reader.ReadInt32();
        remaining -= 4;
        
        for (int i = 0; i < itemsInDescriptor; i++)
        {
            var descLength = reader.ReadInt32();
            remaining -= 4;
            var descriptorKey = "";
            if (descLength == 0)
            {
                descriptorKey = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(4), 0, 4);
                remaining -= 4;
            }
            else
            {
                descriptorKey = reader.ReadUnicodeString(out var byteCount3);
                remaining -= byteCount3;
            }
            Debug.Log("Descriptor " + i + ": " + descriptorKey);
            switch (descriptorKey)
            {
                case "obj":
                    break; // Reference
                case "Objc":
                    break; // Descriptor
                case "VlLs":
                    break; // List
                case "doub":
                    break; // Double
                case "UntF":
                    break; // Unit float
                case "TEXT":
                case "Txt ":
                case "Txt":
                    text = reader.ReadUnicodeString(out var byteCount3);
                    Debug.Log("Got text layer text:\n" + text);
                    remaining -= byteCount3;
                    break; // String
                case "enum":
                    break; // Enumerated
                case "long":
                    break; // Integer
                case "comp":
                    break; // Large Integer
                case "bool":
                    break; // Boolean
                case "GlbO":
                    break; // GlobalObject same as Descriptor
                case "type":
                    break; // Class
                case "GlbC":
                    break; // Class
                case "alis":
                    break; // Alias
                case "tdta":
                    break; // Raw Data
            }
        }
        
        // var remaining = length - 2 - 8 * 6 - 2 - 4 - byteCount;
        
        remainingData = reader.ReadBytes((int)remaining);
    }
    
    protected override void WriteData(PsdBinaryWriter writer)
    {
    }
}
