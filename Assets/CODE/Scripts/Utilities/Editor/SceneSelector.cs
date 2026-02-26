using System.IO;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.SceneManagement;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities.Extensions;
using Scene = UnityEngine.SceneManagement.Scene;

namespace Utilities
{
	[Overlay(typeof(SceneView), "Scene Selector")]
	public class SceneSelector : ToolbarOverlay
	{
		SceneSelector() : base(MainSceneDropDownToggle.id, AllSceneDropDownToggle.id) { }

		[EditorToolbarElement(id, typeof(SceneView))]
		class MainSceneDropDownToggle : EditorToolbarDropdownToggle, IAccessContainerWindow
		{
			public EditorWindow containerWindow { get; set; }

			public const string id = "MainSceneDropDownToggle";

			public MainSceneDropDownToggle()
			{
				text = "Main Scenes";
				tooltip = "Scenes in build";
				// icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/t.png");

				dropdownClicked += () =>
				{
					Scene currentScene = SceneManager.GetActiveScene();
					GenericMenu menu = new GenericMenu();

					EditorBuildSettings.scenes.ForEach(item =>
					{
						string path = item.path;
						string name = Path.GetFileNameWithoutExtension(path);
						menu.AddItem(new GUIContent(name), string.CompareOrdinal(currentScene.name, name) == 0, () =>
						{
							if (currentScene.isDirty)
							{
								if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
								{
									EditorSceneManager.OpenScene(path);
								}
							}
							else
							{
								EditorSceneManager.OpenScene(path);
							}
						});
					});

					menu.ShowAsContext();
				};
			}
		}

		[EditorToolbarElement(id, typeof(SceneView))]
		class AllSceneDropDownToggle : EditorToolbarDropdownToggle, IAccessContainerWindow
		{
			public EditorWindow containerWindow { get; set; }

			public const string id = "SceneDropDownToggle 1";

			public AllSceneDropDownToggle()
			{
				text = "All Scenes";
				tooltip = "All scenes in project";
				// icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/t.png");

				dropdownClicked += () =>
				{
					Scene currentScene = SceneManager.GetActiveScene();
					GenericMenu menu = new GenericMenu();
					AssetDatabase.FindAssets("t:scene", null).ForEach(item =>
					{
						string path = AssetDatabase.GUIDToAssetPath(item);
						string name = Path.GetFileNameWithoutExtension(path);
						menu.AddItem(new GUIContent(path), string.CompareOrdinal(currentScene.name, name) == 0, () =>
						{
							if (currentScene.isDirty)
							{
								if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
								{
									EditorSceneManager.OpenScene(path);
								}
							}
							else
							{
								EditorSceneManager.OpenScene(path);
							}
						});
					});

					menu.ShowAsContext();
				};
			}
		}
	}
}