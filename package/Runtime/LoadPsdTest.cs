using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PhotoshopFile;
using subjectnerdagreement.psdexport;
using UnityEngine;
using UnityEngine.Events;

public class LoadPsdTest : MonoBehaviour
{
	public UnityEngine.Object asset;
    public string absolutePath;
    public UnityEvent<File> OnLoad;
    
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
	    var settings = new PsdExportSettings(path);
	    file = Parse(settings.Psd);
	    file.name = Path.GetFileNameWithoutExtension(path);

	    OnLoad?.Invoke(file);
    }

    [Serializable]
    public class LayerA : ScriptableObject
    {
	    public bool visible;
	    public bool isGroup;
	    public Texture2D texture;
	    public Texture2D maskTexture;
	    public Rect rect;
	    public Rect maskRect;
	    public LayerA parent;
	    public Layer layer;
	    public List<LayerA> layers = new List<LayerA>();

	    public IEnumerable<LayerA> FlattenedLayersWithGroupDividers
	    {
		    get
		    {
			    var isFileRoot = this is File;
			    if(!isFileRoot)
					yield return this;
			    
			    if (layers == null || !layers.Any()) 
				    yield break;
			    
			    foreach (var l in layers)
			    {
				    if(!l) continue;
				    foreach(var f in l.FlattenedLayersWithGroupDividers)
				    {
					    if(!f) continue;
						yield return f;
				    }

				    if(l.isGroup)
						yield return GroupDivider.Create();
			    }
		    }
	    }
    }

    [Serializable]
    public class File : LayerA
    {
    }

    [Serializable]
    public class GroupDivider : LayerA
    {
	    public static GroupDivider Create()
	    {
		    var result = CreateInstance<GroupDivider>();
		    result.name = "</Layer group>";
		    return result;
	    }
    }

    public File file;

    public static string LayerDebug(int index, Layer layer)
    {
	    return "[Layer " + index + "] " + layer.Name + ": " + string.Join(", ", layer.AdditionalInfo.OfType<LayerSectionInfo>().Select(x => x.SectionType + " - " + x.Subtype));
    }
    
    public static File Parse(PsdFile psd)
    {
	    var file = ScriptableObject.CreateInstance<File>();
	    file.hideFlags = HideFlags.DontSave;
	    LayerA current = file;
	    file.rect = new Rect(0, 0, psd.ColumnCount, psd.RowCount);
		// List<int> layerIndices = new List<int>();
		// List<PSDLayerGroupInfo> layerGroups = new List<PSDLayerGroupInfo>();
		List<PSDLayerGroupInfo> openGroupStack = new List<PSDLayerGroupInfo>();
		// List<bool> layerVisibility = new List<bool>();
		// Reverse loop through layers to get the layers in the
		// same way they are displayed in Photoshop
		for (int i = psd.Layers.Count - 1; i >= 0; i--)
		{
			Layer layer = psd.Layers[i];
			var layerA = ScriptableObject.CreateInstance<LayerA>();
			layerA.hideFlags = HideFlags.DontSave;
			layerA.name = layer.Name;
			layerA.layer = layer;
			layerA.visible = layer.Visible;
			layerA.rect = layer.Rect;
			layerA.maskRect = layer.Masks.LayerMask?.Rect ?? Rect.zero;

			// layerVisibility.Add(layer.Visible);
			
			// Get the section info for this layer
			var secInfo = layer.AdditionalInfo
				.Where(info => info.GetType() == typeof(LayerSectionInfo))
				.ToArray();
			
			// Debug.Log(LayerDebug(i, layer));
			
			// Section info is basically layer group info
			bool isOpen = false;
			bool isGroup = false;
			bool closeGroup = false;
			if (secInfo.Any())
			{
				foreach (var layerSecInfo in secInfo)
				{
					LayerSectionInfo info = (LayerSectionInfo)layerSecInfo;
					isOpen = info.SectionType == LayerSectionType.OpenFolder;
					isGroup = info.SectionType == LayerSectionType.ClosedFolder | isOpen;
					closeGroup = info.SectionType == LayerSectionType.SectionDivider;
					if (isGroup || closeGroup)
						break;
				}
			}

			if (isGroup)
			{
				// Open a new layer group info, because we're iterating
				// through layers backwards, this layer number is the last logical layer
				var group = layerA;
				group.isGroup = true;
				group.parent = current;
				group.maskTexture = PSDExporter.CreateMaskTexture(layerA.layer);
				current.layers.Add(group);
				current = group;
				// openGroupStack.Add(new PSDLayerGroupInfo(layer.Name, i, layer.Visible, isOpen));
			}
			else if (closeGroup)
			{
				// Set the start index of the last LayerGroupInfo
				// var closeInfo = openGroupStack.Last();
				// closeInfo.start = i;
				// Add it to the layerGroup list
				// layerGroups.Add(closeInfo);
				// And remove it from the open group stack 
				// openGroupStack.RemoveAt(openGroupStack.Count - 1);
				current = current.parent;
			}
			else
			{
				// Normal layer
				layerA.texture = PSDExporter.CreateTexture(layerA.layer);
				layerA.maskTexture = PSDExporter.CreateMaskTexture(layerA.layer);
				current.layers.Add(layerA);
				// look for instances	
				// if (layer.Name.Contains(" Copy"))
				// {
				// }
			}
		} // End layer loop

		// Since loop was reversed...
		// layerVisibility.Reverse();
		// LayerVisibility = layerVisibility.ToArray();
		//
		// LayerIndices = layerIndices.ToArray();
		//
		// LayerGroups = layerGroups.ToArray();

		return file;
    }
}
