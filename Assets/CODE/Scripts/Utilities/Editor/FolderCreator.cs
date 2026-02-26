using UnityEditor;
using System.IO;

namespace Utilities
{
	public class FolderCreator : EditorWindow
	{
		[MenuItem("Tools/Utilities/Create Folders")]
		static void CreateFolders()
		{
			const string assetsPath = "Assets";

			CreateFolder(assetsPath, "CODE");
			CreateFolder(assetsPath + "/CODE", "Editor");
			CreateFolder(assetsPath + "/CODE", "Input Systems");
			CreateFolder(assetsPath + "/CODE", "Scripts");
			CreateFolder(assetsPath + "/CODE", "Shaders");
			CreateFolder(assetsPath + "/CODE", "Scriptable Objects");

			CreateFolder(assetsPath, "Prefabs");
			CreateFolder(assetsPath + "/Prefabs", "UI");
			CreateFolder(assetsPath + "/Prefabs", "Effects");
			CreateFolder(assetsPath + "/Prefabs", "Models");

			CreateFolder(assetsPath, "ART");
			CreateFolder(assetsPath + "/ART", "Materials");
			CreateFolder(assetsPath + "/ART/Materials", "Physics");
			CreateFolder(assetsPath + "/ART", "Fonts");
			CreateFolder(assetsPath + "/ART", "Animations");
			CreateFolder(assetsPath + "/ART/Animations", "Controllers");
			CreateFolder(assetsPath + "/ART", "Audio");
			CreateFolder(assetsPath + "/ART", "Textures");
			CreateFolder(assetsPath + "/ART", "Models");

			CreateFolder(assetsPath + "/ART", "UI");
			CreateFolder(assetsPath + "/ART/UI", "Icons");
			CreateFolder(assetsPath + "/ART/UI", "GUI");

			CreateFolder(assetsPath, "Packages");

			AssetDatabase.Refresh();
		}

		static void CreateFolder(string parentPath, string folderName)
		{
			string folderPath = Path.Combine(parentPath, folderName);

			if (!AssetDatabase.IsValidFolder(folderPath))
			{
				AssetDatabase.CreateFolder(parentPath, folderName);
			}
		}
	}
}