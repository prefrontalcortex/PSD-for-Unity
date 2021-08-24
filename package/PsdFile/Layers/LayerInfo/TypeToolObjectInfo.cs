using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
        Debug.Log("Version: " + reader.ReadInt16()); 
        remaining -= 2;
        
        // xx, xy, yx, yy, tx, and ty respectively
        for(int i = 0; i < 6; i++)
        {
            Debug.Log("Value: " + reader.ReadDouble());
            remaining -= 8;
        }
        Debug.Log("Text version: " + reader.ReadInt16());
        remaining -= 2;
        Debug.Log("Descriptor version: " + reader.ReadInt32());
        remaining -= 4;
        
        ReadDescriptor();
        
        Debug.Log("Warp version: " + reader.ReadInt16());
        remaining -= 2;
        Debug.Log("Descriptor version: " + reader.ReadInt32());
        remaining -= 4;
        ReadDescriptor();
        
        for(int i = 0; i < 2; i++)
        {
            Debug.Log("Value: " + reader.ReadDouble());
            remaining -= 8;
        }
        
        void ReadDescriptor()
        {
            string ReadDescriptorKey(out int byteCount)
            {
                byteCount = 0;
                var descLength = reader.ReadInt32();
                byteCount += 4;
                var descriptorKey = "";
                if (descLength == 0)
                {
                    descriptorKey = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(4), 0, 4);
                    byteCount += 4;
                }
                else
                {
                    descriptorKey = reader.ReadAsciiChars(descLength); //reader.ReadUnicodeStringOfKnownLength(2 * descLength));
                    // descriptorKey = reader.ReadUnicodeString(out var extraBytes);
                    byteCount += descLength;
                    // byteCount += extraBytes;
                }
                // Debug.Log(descLength + " => " + descriptorKey);
                return descriptorKey;
            }
            
            // read descriptor
            // https://www.adobe.com/devnet-apps/photoshop/fileformatashtml/#50577411_21585
            // Adobe docs seem wrong (e.g. Descriptor enum wasn't correct)
            // these seem to be right: https://psd-tools.readthedocs.io/en/latest/_modules/psd_tools/psd/descriptor.html#Enumerated.get_name
            text = reader.ReadUnicodeString(out var byteCount);
            remaining -= byteCount;
            // var classId = reader.ReadInt32();
            // remaining -= 4;
            // if (classId == 0) {
            //     this.classId = reader.ReadInt32().ToString();
            //     remaining -= 4;
            // }
            // else {
            //     this.classId = reader.ReadUnicodeString(out var byteCount2);
            //     remaining -= byteCount2;
            // }
            var classId = ReadDescriptorKey(out var bb4);
            remaining -= bb4;
            Debug.Log(text + " -- " + this.classId);
            
            var itemsInDescriptor = reader.ReadInt32();
            remaining -= 4;

            Debug.Log("Items in Descriptor: " + itemsInDescriptor);
            for (int i = 0; i < itemsInDescriptor; i++)
            {
                ReadDescriptorKey(out var by);
                remaining -= by;

                var osType = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(4), 0, 4);
                remaining -= 4;
                Debug.Log("<b>OSType:</b> " + osType);
                
                switch (osType)
                {
                    case "obj":
                        break; // Reference
                    case "Objc":
                        ReadDescriptor();
                        break; // Descriptor
                    case "VlLs":
                        break; // List
                    case "doub":
                        var dbl = reader.ReadDouble();
                        remaining -= 8;
                        Debug.Log("Double: " + dbl);
                        break; // Double
                    case "UntF":
                        //  Units the following value is in. One of the following:
                        // '#Ang' = angle: base degrees
                        // '#Rsl' = density: base per inch
                        // '#Rlt' = distance: base 72ppi
                        // '#Nne' = none: coerced.
                        // '#Prc'= percent: unit value
                        // '#Pxl' = pixels: tagged unit value
                        var unitType = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(4), 0, 4);
                        remaining -= 4;
                        var actualValue = reader.ReadDouble();
                        remaining -= 8;
                        Debug.Log("Unit float: " + unitType + "=" + actualValue);
                        break; // Unit float
                    case "TEXT":
                        text = reader.ReadUnicodeString(out var byteCount3);
                        Debug.Log("  Text:\n" + text);
                        remaining -= byteCount3;
                        break; // String
                    case "enum":
                        var enumName = ReadDescriptorKey(out var bytes2);
                        remaining -= bytes2;
                        var enumClassId = ReadDescriptorKey(out var bb);
                        remaining -= bb;
                        Debug.Log("  Enum: " + enumName + ", " + enumClassId);
                        break; // Enumerated
                    case "long":
                        var val = reader.ReadInt32();
                        Debug.Log("  Long: " + val);
                        remaining -= 4;
                        break; // Integer
                    case "comp":
                        var val2 = reader.ReadInt64();
                        remaining -= 8;
                        Debug.Log("  Comp: " + val2);
                        break; // Large Integer
                    case "bool":
                        break; // Boolean
                    case "GlbO":
                        break; // GlobalObject same as Descriptor
                        ReadDescriptor();
                    case "type":
                        break; // Class
                    case "GlbC":
                        break; // Class
                    case "alis":
                        break; // Alias
                    case "tdta":
                        // Font data is a special format, see 
                        // https://github.com/layervault/psd-enginedata#file-spec
                        // and https://github.com/layervault/psd-enginedata/blob/master/spec/files/enginedata for an example
                        
                        var descLength = reader.ReadInt32();
                        remaining -= 4;
                        var data = reader.ReadBytes(descLength);
                        remaining -= descLength;
                        // File.WriteAllBytes(Application.dataPath + "/" + descLength + ".bytes", data);
                        
                        // string start marker:
                        // (þÿ (characters)
                        // 28 FE FF (hex)
                        
                        // string end marker: \r)
                        // 0D 29 (hex)

                        var sb = new StringBuilder();

                        var count = data.Length;
                        var dictionaryDepth = 0;
                        var maxLogs = 100;
                        int c = 0;
                        for (c = 0; c < count - 2; c++)
                        {
                            if (data[c] == '<' && data[c + 1] == '<') // dictionary start marker
                            {
                                c += 1;
                                dictionaryDepth++;
                            }
                            else if (data[c] == '>' && data[c + 1] == '>') // dictionary end marker
                            {
                                c += 1;
                                dictionaryDepth--;
                            }
                            else if (data[c] == 0x2f) // property start marker /
                            {
                                var startIndex = c + 1;
                                // find next space
                                for (c = startIndex; c < count; c++)
                                {
                                    if (data[c] == 0x20 || data[c] == 0x0a || data[c] == 0x09) // property end marker, a, \r, \t
                                    {
                                        var endIndex = c;
                                        var propertyName = Encoding.ASCII.GetString(data, startIndex, endIndex - startIndex);
                                        sb.AppendLine("".PadLeft(4 * dictionaryDepth) + propertyName);
                                        break;
                                    }
                                }
                            }
                            else if (data[c] == 0x28 && data[c + 1] == 0xfe && data[c + 2] == 0xff) // string start marker (þÿ
                            {
                                var startIndex = c + 3;
                                for (c = startIndex; c < count - 2; c++)
                                {
                                    if ((data[c] == 0x0d && data[c + 1] == 0x29) || (data[c] != 0x5c && data[c + 1] == 0x29)) // string end marker, either \r) or just ) but NOT \)
                                    {
                                        var endIndex = data[c] == 0x0d ? c - 1 : c; // one earlier with \r) ?
                                        // Debug.Log("Found string index at " + startIndex + " to " + endIndex);
                                        // Seems we need to parse byte pairs one-by-one
                                        // and look out for escaped \) \( parentheses. These are 3 byte (!)
                                        List<byte> buffer = new List<byte>(endIndex - startIndex);
                                        for (int k = startIndex; k < endIndex; k+=2)
                                        {
                                            if (data[k] == 0x0 && data[k + 1] == 0x5c && (data[k + 2] == 0x28 || data[k + 2] == 0x29))
                                            {
                                                buffer.Add(data[k]);
                                                buffer.Add(data[k + 2]);
                                                k += 1;
                                            }
                                            else
                                            {
                                                buffer.Add(data[k]);
                                                buffer.Add(data[k + 1]);
                                            }
                                        }
                                        var stringBytes = Encoding.BigEndianUnicode.GetString(buffer.ToArray(), 0, buffer.Count);
                                        // var stringBytes = Encoding.BigEndianUnicode.GetString(data, startIndex, endIndex - startIndex);
                                        sb.AppendLine("".PadLeft(4 * (dictionaryDepth + 1)) + stringBytes);//$"[{startIndex}–{endIndex}] {stringBytes}");
                                        break;
                                    }
                                }
                            }
                        }
                        
                        // var rawDt = Encoding.ASCII.GetString(data); // ReadDescriptorKey(out var bb3);
                        // Debug.Log("  " + "[" + descLength + "] "+ " Raw Data: " + rawDt);
                        // File.WriteAllText(Application.dataPath + "/" + descLength + ".txt", rawDt);
                        
                        // var stringIndex = rawDt.IndexOf("(˛ˇ", StringComparison.Ordinal);
                        // var nextNewline = rawDt.IndexOf('\n', stringIndex);
                        // var substring = rawDt.Substring(stringIndex, nextNewline - stringIndex);
                        // // back to bytes
                        // var bytes = Encoding.ASCII.GetBytes(substring);
                        // var utf16String = Encoding.BigEndianUnicode.GetString(bytes, 0, length);
                        // Debug.Log(utf16String);
                        
                        // var asciiChars = reader.ReadAsciiChars(remaining);
                        // Debug.Log("Remaining data: " + remaining + "\n" + asciiChars);
                        // remaining -= remaining;
                        
                        File.WriteAllText(Application.dataPath + "/" + descLength + ".txt", sb.ToString());
                        Debug.Log(sb.ToString(0, Mathf.Max(5000, sb.Length)));
                        
                        break; // Raw Data
                }
            }
        }
        
        // var remaining = length - 2 - 8 * 6 - 2 - 4 - byteCount;
        // var asciiChars = reader.ReadAsciiChars(remaining);
        Debug.Log("Remaining data: " + remaining);
        if(remaining > 0)
            remainingData = reader.ReadBytes((int)remaining);
    }
    
    protected override void WriteData(PsdBinaryWriter writer)
    {
    }
}
