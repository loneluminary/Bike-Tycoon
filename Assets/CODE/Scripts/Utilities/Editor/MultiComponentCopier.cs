using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditorInternal;

public class MultiComponentCopier : OdinEditorWindow
{
	[OnValueChanged("UpdateCopyComponentList")] [HorizontalGroup("Copy Object", Width = 0.795f)] [DisableIf("copyFromSelectedObject")] [HideLabel]
	public GameObject copyFrom;

	[HorizontalGroup("Copy Object"), LabelText("Selection")]
	public bool copyFromSelectedObject = true;

	[ListDrawerSettings(ShowFoldout = true, NumberOfItemsPerPage = 10)] [LabelText("Copy To GameObjects")]
	public List<GameObject> destinationObjects = new();

	[ShowInInspector, TableList(IsReadOnly = true, AlwaysExpanded = true, HideToolbar = true)] [LabelText("Components to Copy")]
	public List<ComponentCopyInfo> componentCopyInfos;

	private int previousComponentCount;

	[MenuItem("Tools/Utilities/Multi Component Copier")]
	public static void OpenWindow()
	{
		MultiComponentCopier window = GetWindow<MultiComponentCopier>();
		window.titleContent = new GUIContent("Multi Component Copier");
		window.Show();
	}

	protected override void OnEnable()
	{
		base.OnEnable();

		maxSize = new Vector2(400, 500f);
		minSize = maxSize;

		EditorApplication.hierarchyChanged += OnHierarchyChanged;
		Selection.selectionChanged += OnSelectionChanged;
		UpdateCopyComponentList();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		EditorApplication.hierarchyChanged -= OnHierarchyChanged;
		Selection.selectionChanged -= OnSelectionChanged;
	}

	private void OnSelectionChanged()
	{
		if (copyFromSelectedObject)
		{
			copyFrom = Selection.activeGameObject;
			UpdateCopyComponentList();
			Repaint(); // Forces the GUI to update immediately.
		}
	}

	private void OnHierarchyChanged()
	{
		// If components on the copy-from object change, update the list.
		if (copyFrom)
		{
			var comps = copyFrom.GetComponents<Component>();
			if (comps.Length != previousComponentCount)
			{
				UpdateCopyComponentList();
			}
		}
	}

	private void UpdateCopyComponentList()
	{
		if (!copyFrom)
		{
			componentCopyInfos = new List<ComponentCopyInfo>();
			previousComponentCount = 0;
			return;
		}

		var comps = copyFrom.GetComponents<Component>();
		previousComponentCount = comps.Length;
		// Create a list of component info items (defaulting to copy = true)
		componentCopyInfos = comps.Select(c => new ComponentCopyInfo(c)).ToList();
	}

	[Button(ButtonSizes.Large), PropertySpace(100f)]
	private void RunCopy()
	{
		if (destinationObjects == null || destinationObjects.Count == 0)
		{
			Debug.LogWarning("Add at least one destination GameObject.");
			return;
		}

		foreach (var dest in destinationObjects)
		{
			if (!dest) continue;

			foreach (var info in componentCopyInfos)
			{
				if (info.Copy && info.component)
				{
					// Check if the destination already has the component type.
					var existing = dest.GetComponent(info.component.GetType());
					if (existing)
					{
						ComponentUtility.CopyComponent(info.component);
						ComponentUtility.PasteComponentValues(existing);
					}
					else
					{
						var newComp = Undo.AddComponent(dest, info.component.GetType());
						ComponentUtility.CopyComponent(info.component);
						ComponentUtility.PasteComponentValues(newComp);
					}
				}
			}
		}

		UpdateCopyComponentList();
	}
}

public class ComponentCopyInfo
{
	[ShowInInspector, ReadOnly, PropertyOrder(-1)]
	[TableColumnWidth(50)]
	public string ComponentName => component ? component.GetType().Name : "Missing";

	[TableColumnWidth(100)] [ShowInInspector, HideLabel]
	public Component component;

	[TableColumnWidth(40, false)] public bool Copy = true;

	public ComponentCopyInfo(Component comp)
	{
		component = comp;
	}
}