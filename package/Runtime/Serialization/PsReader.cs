using System.Collections.Generic;
using System.Linq;
using PhotoshopFile;
using UnityEngine;

public static class PsReader
{
    internal static PsFile Parse(PsdFile psd)
    {
	    var file = ScriptableObject.CreateInstance<PsFile>();
	    file.hideFlags = HideFlags.DontSave;
	    PsLayer current = file;
	    file.rect = new Rect(0, 0, psd.ColumnCount, psd.RowCount);
		// List<PSDLayerGroupInfo> openGroupStack = new List<PSDLayerGroupInfo>();
		
		// Reverse loop through layers to get the layers in the
		// same way they are displayed in Photoshop
		for (int i = psd.Layers.Count - 1; i >= 0; i--)
		{
			Layer layer = psd.Layers[i];
			var layerA = ScriptableObject.CreateInstance<PsLayer>();
			layerA.hideFlags = HideFlags.DontSave;
			layerA.name = layer.Name;
			layerA.originalLayerData = layer;
			layerA.visible = layer.Visible;
			layerA.rect = layer.Rect;
			layerA.opacity = layer.Opacity / 255f;
			layerA.maskRect = layer.Masks.LayerMask?.Rect ?? Rect.zero;

			// layerVisibility.Add(layer.Visible);
			
			// Get the section info for this layer
			var secInfo = layer.AdditionalInfo
				.Where(info => info.GetType() == typeof(LayerSectionInfo))
				.ToArray();
			
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
			
			// get the layer color if there's a SheetColorLayerInfo available
			var layerColorInfo = layer.AdditionalInfo.OfType<SheetColorLayerInfo>().FirstOrDefault();
			if (layerColorInfo != null)
				layerA.layerColor = (PsLayer.LayerColor) layerColorInfo.layerColor;

			if (isGroup)
			{
				var group = layerA;
				group.isGroup = true;
				group.parent = current;
				group.maskTexture = TextureConversion.CreateMaskTexture(layerA.originalLayerData);
				current.childLayers.Add(group);
				current = group;
			}
			else if (closeGroup)
			{
				current = current.parent;
			}
			else
			{
				// Normal layer
				layerA.texture = TextureConversion.CreateTexture(layerA.originalLayerData);
				layerA.maskTexture = TextureConversion.CreateMaskTexture(layerA.originalLayerData);
				current.childLayers.Add(layerA);
			}
		}

		return file;
    }

}
