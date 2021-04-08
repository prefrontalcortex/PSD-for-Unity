using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PhotoshopFile;
using subjectnerdagreement.psdexport;
using UnityEngine;

public class LoadPsdTest : MonoBehaviour
{
    public string absolutePath;
    
    [ContextMenu("Load Now")]
    void LoadNow()
    {
        var settings = new PsdExportSettings(absolutePath);
        file = Parse(settings.Psd);
        file.name = Path.GetFileNameWithoutExtension(absolutePath);
    }

    [Serializable]
    public class LayerA
    {
	    public string name;
	    public bool visible;
	    public bool isGroup;
	    public Texture2D texture;
	    public LayerA parent;
	    public Layer layer;
	    public List<LayerA> layers = new List<LayerA>();
    }

    [Serializable]
    public class File : LayerA
    {
    }

    public File file;
    
    public static File Parse(PsdFile psd)
    {
	    var file = new File();
	    LayerA current = file;
		
		// List<int> layerIndices = new List<int>();
		// List<PSDLayerGroupInfo> layerGroups = new List<PSDLayerGroupInfo>();
		List<PSDLayerGroupInfo> openGroupStack = new List<PSDLayerGroupInfo>();
		// List<bool> layerVisibility = new List<bool>();
		// Reverse loop through layers to get the layers in the
		// same way they are displayed in Photoshop
		for (int i = psd.Layers.Count - 1; i >= 0; i--)
		{
			Layer layer = psd.Layers[i];
			var layerA = new LayerA()
			{
				name = layer.Name,
				layer = layer,
				visible = layer.Visible
			};

			// layerVisibility.Add(layer.Visible);

			// Get the section info for this layer
			var secInfo = layer.AdditionalInfo
				.Where(info => info.GetType() == typeof(LayerSectionInfo))
				.ToArray();
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
