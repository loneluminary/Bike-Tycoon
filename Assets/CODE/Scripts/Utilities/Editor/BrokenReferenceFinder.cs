using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class BrokenReferenceFinder : EditorWindow
{
    [MenuItem("Tools/Utilities/Find Broken References")]
    public static void FindBrokenReferences()
    {
        // Open the EditorWindow to avoid calling scene-related functions directly in static methods
        GetWindow(typeof(BrokenReferenceFinder)).Show();
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Find Broken References"))
        {
            FindBrokenReferencesInScenes();
        }
    }

    private static void FindBrokenReferencesInScenes()
    {
        var scenes = new List<string>(EditorBuildSettings.scenes.Length);
        foreach (var scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
                scenes.Add(scene.path);
        }

        foreach (var scene in scenes)
        {
            Debug.Log($"Checking scene: {scene}");

            // Load the scene
            Scene sceneObj = EditorSceneManager.OpenScene(scene, OpenSceneMode.Additive);
            var rootObjects = sceneObj.GetRootGameObjects();

            // Check for missing references in root game objects
            foreach (var rootObject in rootObjects)
            {
                CheckForMissingReferences(rootObject);
            }

            // Unload the scene to avoid memory leaks
            EditorSceneManager.CloseScene(sceneObj, true);
        }
    }

    private static void CheckForMissingReferences(GameObject gameObject)
    {
        // Check all components on this GameObject
        var components = gameObject.GetComponents<Component>();
        foreach (var component in components)
        {
            if (component == null)
            {
                Debug.LogWarning($"Missing component on GameObject: {gameObject.name}", gameObject);
                continue;
            }

            // Check fields for missing references
            SerializedObject serializedObject = new SerializedObject(component);
            SerializedProperty property = serializedObject.GetIterator();
            while (property.Next(true))
            {
                if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue == null)
                {
                    Debug.LogWarning($"Missing reference in component {component.GetType().Name} on GameObject: {gameObject.name}", gameObject);
                }
            }
        }

        // Recursively check child GameObjects
        foreach (Transform child in gameObject.transform)
        {
            CheckForMissingReferences(child.gameObject);
        }
    }
}