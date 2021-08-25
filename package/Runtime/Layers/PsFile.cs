using System;
using PhotoshopFile;

[Serializable]
public class PsFile : PsLayer
{
    public static PsFile Load(string absolutePath)
    {
	    var file = new PsdFile(absolutePath, new LoadContext() { Encoding = System.Text.Encoding.Default });
	    return PsReader.Parse(file);
    }

    public void SaveTo(string outputPath)
    {
	    PsWriter.CreateFile(this, outputPath);
    }
}