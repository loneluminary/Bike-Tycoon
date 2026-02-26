using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using System.Linq;

public class PrefabComponentFixer : OdinEditorWindow
{
    [ShowInInspector, ListDrawerSettings(ShowFoldout = true)]
    [SerializeField] private List<MonoScript> componentsToAdd = new();

    [MenuItem("Tools/Utilities/Prefab Missing Component Fixer")]
    public static void ShowWindow()
    {
        GetWindow<PrefabComponentFixer>("Prefab Component Fixer");
    }
   
    [Button("Remove Missing Components")]
    public void FixSelectedPrefabs()
    {
        // Get selected prefabs in the Project window
        var selectedPrefabs = Selection.GetFiltered<GameObject>(SelectionMode.Assets);

        foreach (GameObject prefab in selectedPrefabs)
        {
            if (!prefab || !PrefabUtility.IsPartOfPrefabAsset(prefab)) continue;

            var components = prefab.GetComponentsInChildren<Component>(true);

            // Remove missing components
            foreach (var component in components)
            {
                if (!component)
                {
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(prefab);
                }
            }

            PrefabUtility.SavePrefabAsset(prefab);
        }

        AssetDatabase.SaveAssets();
    }

    [Button]
    public void AddComponents()
    {
        if (componentsToAdd == null || componentsToAdd.Count == 0)
        {
            Debug.LogError("Please specify at least one component to add.");
            return;
        }
        
        // Get selected prefabs in the Project window
        var selectedPrefabs = Selection.GetFiltered<GameObject>(SelectionMode.Assets);

        foreach (var prefab in selectedPrefabs)
        {
            // Add the specified components
            foreach (var componentScript in componentsToAdd.Where(componentScript => componentScript))
            {
                prefab.AddComponent(componentScript.GetClass());
                PrefabUtility.SavePrefabAsset(prefab);
            }
        }
    }
}