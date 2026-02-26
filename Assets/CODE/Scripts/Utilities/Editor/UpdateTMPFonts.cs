using UnityEngine;
using UnityEditor;
using TMPro;

public class UpdateTMPFonts : EditorWindow
{
    private TMP_FontAsset defaultFont;

    [MenuItem("Tools/Utilities/Update TMP Fonts")]
    public static void ShowWindow()
    {
        GetWindow<UpdateTMPFonts>("Update TMP Fonts");
    }

    private void OnGUI()
    {
        GUILayout.Label("Select the Default TMP Font Asset", EditorStyles.boldLabel);

        // Select the TMP Font Asset
        defaultFont = (TMP_FontAsset)EditorGUILayout.ObjectField("Default Font", defaultFont, typeof(TMP_FontAsset), false);

        if (GUILayout.Button("Update Fonts in Scene"))
        {
            UpdateFonts();
        }
    }

    private void UpdateFonts()
    {
        if (!defaultFont)
        {
            Debug.LogError("No TMP Font Asset selected.");
            return;
        }

        // Find all TMP components in the scene including inactive objects
        var textComponents = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        var textMeshProComponents = FindObjectsByType<TextMeshPro>(FindObjectsSortMode.None);

        // Update TMPUGUI components
        foreach (TextMeshProUGUI tmpUGUI in textComponents)
        {
            Undo.RecordObject(tmpUGUI, "Update TMP Font");
            tmpUGUI.font = defaultFont;
            EditorUtility.SetDirty(tmpUGUI);
        }

        // Update TMP components
        foreach (TextMeshPro tmp in textMeshProComponents)
        {
            Undo.RecordObject(tmp, "Update TMP Font");
            tmp.font = defaultFont;
            EditorUtility.SetDirty(tmp);
        }

        // Optionally, save the scene after making these changes
        // if (EditorSceneManager.GetActiveScene().isDirty)
        // {
        //     EditorSceneManager.SaveOpenScenes();
        // }

        Debug.Log("All TMP fonts updated to the selected font.");
    }
}
