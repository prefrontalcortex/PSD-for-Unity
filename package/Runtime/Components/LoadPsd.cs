using System.IO;
using UnityEngine;
using UnityEngine.Events;

public class LoadPsd : MonoBehaviour
{
	public UnityEngine.Object asset;
    public string absolutePath;
    public PsFile file;
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
	    file = PsFile.Load(path);
	    file.name = Path.GetFileNameWithoutExtension(path);

	    OnLoad?.Invoke(file);
    }
}
