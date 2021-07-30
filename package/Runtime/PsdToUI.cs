using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PsdToUI : MonoBehaviour
{
    public Transform root;
    
    void Clear()
    {
        for (int i = root.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(root.GetChild(i).gameObject);
        }
    }
    
    [ContextMenu("Create Now")]
    void CreateNow()
    {
        Clear();

        var t = GetComponent<LoadPsdTest>();

        foreach (var l in t.file.layers)
        {
            MakeLayer(t.file, l, root);
        }
    }

    private void MakeLayer(LoadPsdTest.LayerA file, LoadPsdTest.LayerA layerA, Transform parent)
    {
        void ApplyRectToTransform(Component c, Rect rect)
        {
            var rt = c.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = Vector2.zero;
            rt.pivot = new Vector2(0,1);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rect.width);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rect.height);
            rt.localPosition = new Vector3(rect.x, file.rect.height - rect.y, 0);
        }
        
        RawImage maskGo = null;
        if (layerA.maskTexture)
        {
            maskGo = new GameObject(layerA.name + " (Mask)").AddComponent<RawImage>();
            maskGo.gameObject.hideFlags = HideFlags.DontSave;
            ApplyRectToTransform(maskGo, layerA.maskRect);
            maskGo.gameObject.AddComponent<Mask>();
            maskGo.texture = layerA.maskTexture;
            maskGo.transform.SetParent(parent);
            maskGo.transform.SetAsFirstSibling();
        }
        
        var go = new GameObject(layerA.name).AddComponent<RectTransform>();
        go.gameObject.hideFlags = HideFlags.DontSave;
        ApplyRectToTransform(go, layerA.rect);
        if (layerA.texture)
        {
            var tex = go.gameObject.AddComponent<RawImage>();
            tex.texture = layerA.texture;
        }
        go.transform.SetParent(maskGo ? maskGo.transform : parent);
        go.transform.SetAsFirstSibling();

        foreach (var child in layerA.layers)
        {
            MakeLayer(file, child, go.transform);
        }
    }
}
