using UnityEditor;
using UnityEngine.UIElements;

[CustomEditor(typeof(LoadPsd))]
public class LoadPSDEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        var t = target as LoadPsd;
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
            var path = "Packages/com.pfc.psd/Samples/UI/PsdFile.uxml";
            var template = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
            psdUi = template.Instantiate();
            root.Add(psdUi);

            var t = target as LoadPsd;

            PsdUIToolkit.AddDoc(psdUi, t.file);
        }

        void LoadAndBuildPsdUI()
        {
            t.LoadFromPath(t.absolutePath);
            BuildPsdUI();
        }
        
        root.Add(new Button(LoadAndBuildPsdUI) { text = "Load & Build PSD UI" });
        root.Add(new Button(BuildPsdUI) { text = "Build PSD UI" });
        return root;
    }
}
