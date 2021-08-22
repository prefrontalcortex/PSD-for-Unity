using System.Collections;
using System.Collections.Generic;
using subjectnerdagreement.psdexport;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(LoadPsdTest))]
public class LoadPSDEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
        
        // create base IMGUI inspector for stuff that's already there
        var imgui = new IMGUIContainer(OnInspectorGUI);
        root.Add(imgui);
        root.Add(new Label("Remaining Stuff"));
        
        // create/bind object field for layers

        VisualElement psdUi = null;
        
        void BuildPsdUI()
        {
            if (psdUi != null)
                psdUi.RemoveFromHierarchy();
            
            // load PSD root visual tree asset
            var path = "Packages/com.pfc.psd/Samples/Scripts/PsdFile.uxml";
            var template = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
            psdUi = template.Instantiate();
            root.Add(psdUi);

            var t = target as LoadPsdTest;

            PsdUIToolkit.AddDoc(psdUi, t.file);
        }
        
        root.Add(new Button(BuildPsdUI) { text = "Build PSD UI" });
        return root;
    }
}
