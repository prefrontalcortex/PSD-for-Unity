using System.IO;
using UnityEngine;

public class CreatePsd : MonoBehaviour
{
    public void SetFile(PsFile psFile)
    {
      this.file = psFile;
    }
    
    public PsFile file;

    [ContextMenu("Create File")]
    void CreateFileContext()
    {
      var outputPath = Path.GetFullPath(Application.dataPath + "/../testfile.psd");
      file.SaveTo(outputPath);
    }
}
