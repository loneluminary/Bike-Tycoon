using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class RemoveCollidersOutsideBounds : OdinEditorWindow
{
	[Title("Bounding Box Settings")] public Bounds bounding = new(Vector3.zero, Vector3.one * 5);

	[Tooltip("Only colliders on these layers will be considered")]
	public LayerMask layerFilter = ~0; // Default to all layers

	[SerializeField, ReadOnly] private List<Collider> collidersToRemove = new();

	private readonly BoxBoundsHandle boundsHandle = new();

	[Button(ButtonSizes.Large)]
	private void CalculateOutOfBoundsColliders()
	{
		collidersToRemove.Clear();
		var allColliders = FindObjectsByType<Collider>(FindObjectsSortMode.None);

		foreach (Collider col in allColliders)
		{
			if (((1 << col.gameObject.layer) & layerFilter) == 0) continue; // Skip if not in layer mask

			if (!bounding.Intersects(col.bounds))
			{
				collidersToRemove.Add(col);
			}
		}

		Debug.Log($"Found {collidersToRemove.Count} colliders outside bounds.");
		SceneView.RepaintAll();
	}

	[Button(ButtonSizes.Large), GUIColor(1f, 0.3f, 0.3f)]
	public void RemoveColliders()
	{
		foreach (var col in collidersToRemove.Where(col => col))
		{
			DestroyImmediate(col);
		}

		collidersToRemove.Clear();
		Debug.Log("Removed colliders outside bounds.");
	}

	private void OnSceneGUI(SceneView sceneView)
	{
		Handles.color = Color.green;
		Color boxColor = new Color(0f, 1f, 0f, 0.2f); // Semi-transparent green

		var corners = GetBoxCorners(bounding);

		// Draw all six faces of the bounding box
		DrawFace(corners[0], corners[1], corners[2], corners[3], boxColor); // Bottom
		DrawFace(corners[4], corners[5], corners[6], corners[7], boxColor); // Top
		DrawFace(corners[0], corners[3], corners[7], corners[4], boxColor); // Left
		DrawFace(corners[1], corners[2], corners[6], corners[5], boxColor); // Right
		DrawFace(corners[3], corners[2], corners[6], corners[7], boxColor); // Front
		DrawFace(corners[0], corners[1], corners[5], corners[4], boxColor); // Back

		// Handles for interactive bounding box
		boundsHandle.center = bounding.center;
		boundsHandle.size = bounding.size;

		EditorGUI.BeginChangeCheck();
		boundsHandle.DrawHandle();
		if (EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(this, "Changed Bounding Box");
			bounding = new Bounds(boundsHandle.center, boundsHandle.size);
			EditorUtility.SetDirty(this);
			SceneView.RepaintAll();
		}
	}

	/// Draws a single rectangular face
	private void DrawFace(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Color faceColor)
	{
		Handles.DrawSolidRectangleWithOutline(new[] { a, b, c, d }, faceColor, Color.green);
	}

	/// Gets the 8 corner points of a bounding box
	private Vector3[] GetBoxCorners(Bounds bounds)
	{
		Vector3 center = bounds.center;
		Vector3 extents = bounds.extents;

		return new[]
		{
			center + new Vector3(-extents.x, -extents.y, -extents.z), // 0 Bottom-Left-Back
			center + new Vector3(extents.x, -extents.y, -extents.z), // 1 Bottom-Right-Back
			center + new Vector3(extents.x, -extents.y, extents.z), // 2 Bottom-Right-Front
			center + new Vector3(-extents.x, -extents.y, extents.z), // 3 Bottom-Left-Front

			center + new Vector3(-extents.x, extents.y, -extents.z), // 4 Top-Left-Back
			center + new Vector3(extents.x, extents.y, -extents.z), // 5 Top-Right-Back
			center + new Vector3(extents.x, extents.y, extents.z), // 6 Top-Right-Front
			center + new Vector3(-extents.x, extents.y, extents.z) // 7 Top-Left-Front
		};
	}

	private void UpdateSceneView()
	{
		SceneView.duringSceneGui -= OnSceneGUI;
		SceneView.duringSceneGui += OnSceneGUI;
		SceneView.RepaintAll();
	}

	[MenuItem("Tools/Utilities/Remove Colliders Outside Bounds")]
	private static void OpenWindow()
	{
		var window = GetWindow<RemoveCollidersOutsideBounds>();
		window.titleContent = new GUIContent("Collider Remover");
		window.Show();
		window.UpdateSceneView();
	}

	protected override void OnDisable()
	{
		base.OnDisable();

		SceneView.duringSceneGui -= OnSceneGUI;
	}
}