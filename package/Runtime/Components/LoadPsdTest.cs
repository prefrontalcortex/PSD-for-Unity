using System.IO;
using UnityEngine;
using UnityEngine.Events;

public class LoadPsdTest : MonoBehaviour
{
	public UnityEngine.Object asset;
    public string absolutePath;
    public PsFile psFile;
    public UnityEvent<PsFile> OnLoad;
    
    [ContextMenu("Load Now")]
    void LoadNow()
    {
	    LoadFromPath(absolutePath);
    }

    public void LoadFromPath(string path)
    {
#if UNITY_EDITOR
	    if (asset) path = UnityEditor.AssetDatabase.GetAssetPath(asset);
#endif
	    psFile = PsFile.Load(path);
	    psFile.name = Path.GetFileNameWithoutExtension(path);

	    OnLoad?.Invoke(psFile);
    }
}
