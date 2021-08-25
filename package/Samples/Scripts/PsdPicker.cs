using System.IO;
using System.Linq;
using subjectnerdagreement.psdexport;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
#if HAVE_USFB
using USFB;
#endif

public class PsdPicker : MonoBehaviour
{
#if HAVE_UI_TOOLKIT
	public UIDocument picker;
	public UIDocument fileRoot;
	public VisualTreeAsset listItem;
#endif
	
#if HAVE_USFB
	private void OnEnable()
	{
		picker.rootVisualElement.Q<Button>("import").clicked += PickFile;
		picker.rootVisualElement.Q<Button>("export").clicked += ExportFile;
		
		// // mock hierarchy
		// var scrollView = fileRoot.rootVisualElement.Q<ScrollView>();
		// for (int i = 0; i < 4; i++)
		// {
		// 	var item = listItem.Instantiate();
		// 	scrollView.Add(item);
		//
		// 	for (int j = 0; j < 3; j++)
		// 	{
		// 		var item2 = listItem.Instantiate();
		// 		item.Q<Foldout>().Add(item2);
		// 		
		// 		// no childs
		// 		item2.Q<Foldout>().style.display = DisplayStyle.None;
		// 	}
		// }
	}

	void PickFile()
	{
		var paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", "psd", false);
		if (paths.Length != 1) return;
		var path = paths.FirstOrDefault();
		
		if (!string.IsNullOrEmpty(path))
			LoadPsdFile(path);
	}

	void ExportFile()
	{
		var path = StandaloneFileBrowser.SaveFilePanel("Save File", lastLoadPath, file.name + "-exported", "psd");
		
		if(!string.IsNullOrEmpty(path))
			CreatePsdTest.CreateFile(file, path);
	}
#endif
	
#if HAVE_UI_TOOLKIT

	private LoadPsdTest.File file;

	private string lastLoadPath;
	void LoadPsdFile(string absolutePath)
	{
		var settings = new PsdExportSettings(absolutePath);
		file = LoadPsdTest.Parse(settings.Psd);
		file.name = Path.GetFileNameWithoutExtension(absolutePath);
		lastLoadPath = Path.GetDirectoryName(absolutePath);
		PsdUIToolkit.AddDoc(fileRoot.rootVisualElement, file);
	}
#endif
}
