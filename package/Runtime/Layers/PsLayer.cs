using System;
using System.Collections.Generic;
using System.Linq;
using PhotoshopFile;
using UnityEngine;

[Serializable]
public class PsLayer : ScriptableObject
{
    public bool visible = true;
    public bool isGroup = false;
    [Range(0,1)]
    public float opacity = 1;
    public LayerColor layerColor;

    public Texture2D texture;
    public Rect rect;
    
    public Texture2D maskTexture;
    public Rect maskRect;
    
    public PsLayer parent;
    public List<PsLayer> childLayers = new List<PsLayer>();

    public Layer originalLayerData;
    
    public enum LayerColor
    {
        NoColor,
        Red,
        Orange,
        Yellow,
        Green,
        Blue, 
        Violet, 
        Gray
    }
    
    public IEnumerable<PsLayer> FlattenedLayersWithGroupDividers
    {
        get
        {
            var isFileRoot = this is PsFile;
            if(!isFileRoot)
                yield return this;
			    
            if (childLayers == null || !childLayers.Any()) 
                yield break;
			    
            foreach (var l in childLayers)
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