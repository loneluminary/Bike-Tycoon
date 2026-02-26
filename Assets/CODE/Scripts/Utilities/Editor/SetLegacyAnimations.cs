using UnityEngine;
using UnityEditor;

public static class SetLegacyAnimations
{
	[MenuItem("Tools/Utilities/Set Selected Animations to Legacy")]
	static void SetSelectedAnimationsToLegacy()
	{
		// Get the selected animation clips in the Project window
		var selectedObjects = Selection.objects;

		foreach (Object obj in selectedObjects)
		{
			if (obj is AnimationClip clip)
			{
				SerializedObject serializedClip = new SerializedObject(clip);
				serializedClip.FindProperty("m_Legacy").boolValue = true;
				serializedClip.ApplyModifiedProperties();
				Debug.Log($"Set {clip.name} to Legacy mode.");
			}
			else
			{
				Debug.LogWarning($"{obj.name} is not an animation clip.");
			}
		}
	}
}