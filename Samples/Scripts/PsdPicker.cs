using System;
using System.IO;
using System.Linq;
using subjectnerdagreement.psdexport;
using UnityEngine;
using UnityEngine.UIElements;
using USFB;

public class PsdPicker : MonoBehaviour
{
	public UIDocument picker;
	public UIDocument fileRoot;
	public VisualTreeAsset listItem;

	private void OnEnable()
	{
		picker.rootVisualElement.Q<Button>().clicked += () =>
		{
			PickFile();
		};
		
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
		// Open file
		var paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", "psd", false);
		if (paths.Length == 1)
		{
			var path = paths.FirstOrDefault();
			if (!string.IsNullOrEmpty(path))
			{
				LoadPsdFile(path);
			}
		}
	}

	void LoadPsdFile(string absolutePath)
	{
		var settings = new PsdExportSettings(absolutePath);
		var file = LoadPsdTest.Parse(settings.Psd);
		file.name = Path.GetFileNameWithoutExtension(absolutePath);

		var scrollView = fileRoot.rootVisualElement.Q<ScrollView>();

		void AddLevels(LoadPsdTest.LayerA current, VisualElement parent)
		{
			foreach (var layer in current.layers)
			{
				var item = listItem.Instantiate();
				item.Q<Label>("name").text = layer.name;
				item.Q<Toggle>().value = layer.visible;
				item.Q("texture").style.backgroundImage = layer.texture;
				
				parent.Add(item);

				if (layer.layers.Any())
					AddLevels(layer, item.Q<Foldout>());
				else
					item.Q<Foldout>().style.display = DisplayStyle.None;
			}
		}

		AddLevels(file, scrollView);
	}
}
