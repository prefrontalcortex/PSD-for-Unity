using System.IO;
using UnityEngine;

public class CreatePsdTest : MonoBehaviour
{
    public void SetFile(PsFile psFile)
    {
      this.psFile = psFile;
    }
    
    public PsFile psFile;

    [ContextMenu("Create File")]
    void CreateFileContext()
    {
      var outputPath = Path.GetFullPath(Application.dataPath + "/../testfile.psd");
      psFile.SaveTo(outputPath);
    }
}
