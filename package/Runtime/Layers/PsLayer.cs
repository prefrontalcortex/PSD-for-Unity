using System;
using System.Collections.Generic;
using System.Linq;
using PhotoshopFile;
using UnityEngine;

[Serializable]
public class PsLayer : ScriptableObject
{
    public bool visible;
    public bool isGroup;
    public Texture2D texture;
    public Texture2D maskTexture;
    public Rect rect;
    public Rect maskRect;
    public PsLayer parent;
    public Layer originalLayerData;
    public List<PsLayer> childLayers = new List<PsLayer>();

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