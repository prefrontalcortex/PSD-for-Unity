using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Random = System.Random;

public class PsdUIToolkit : MonoBehaviour
{
    public VisualTreeAsset listItem;

    static void AddLevels(LoadPsdTest.LayerA current, VisualElement parent, VisualTreeAsset listItem, VisualElement imageStack)
    {
        if (!current) return;
        if (current.layers == null) return;
        foreach (var layer in current.layers)
        {
            var item = listItem.Instantiate();
            
            // item.Bind(new SerializedObject(layer));
            item.Q<Label>("name").text = layer.name;
            item.Q<Label>("extra").text = GetExtraDetailsAsString(layer);
            item.Q<Toggle>().value = layer.visible;
            item.Q<Image>("image").image = layer.texture;            
            item.Q<Label>("imageRect").text = layer.rect.ToString();
            item.Q<Image>("mask").image = layer.maskTexture;
            item.Q<Label>("maskRect").text = layer.maskRect.ToString();

            var elem = new Image()
            {
                style =
                {
                    width = layer.rect.width,
                    height = layer.rect.height,
                    position = Position.Absolute,
                    marginLeft = layer.rect.xMin,
                    marginTop = layer.rect.yMin,
                    // backgroundColor = UnityEngine.Random.ColorHSV(0,1, 1,1,1,1,1,1),
                },
                image = layer.texture,
            };
            imageStack.Insert(0, elem);
            
            parent.Add(item);

            if (layer.layers.Any())
                AddLevels(layer, item.Q<Foldout>(), listItem, imageStack);
            else
                item.Q<Foldout>().style.display = DisplayStyle.None;
        }
    }

    private static string GetExtraDetailsAsString(LoadPsdTest.LayerA layer)
    {
        if (layer.layer != null)
            return string.Join("\n", layer.layer.AdditionalInfo.Select(x => x.Key + " [" + x.GetType() + "] = " + x.ToString()));
        else
            return "(no serialized layer data)";
    }

    public static void AddDoc(VisualElement root, LoadPsdTest.File file)
    {
        root.Q<Label>("filename").text = file.name;
        var imageStack = root.Q("imageStack");
        imageStack.style.width = file.rect.width;
        imageStack.style.height = file.rect.height;
        imageStack.Clear();
        var scrollView = root.Q<ScrollView>("layerStack");
        scrollView.Clear();
        var listItem = GameObject.FindObjectOfType<PsdUIToolkit>().listItem;
        AddLevels(file, scrollView, listItem, imageStack);
    }
}
