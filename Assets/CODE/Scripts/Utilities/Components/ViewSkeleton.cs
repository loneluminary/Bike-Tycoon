using UnityEditor;
using UnityEngine;

namespace Utilities.Components
{
	public class ViewSkeleton : MonoBehaviour
	{

		public Transform rootNode;
		public Transform[] childNodes;

		public void OnDrawGizmosSelected()
		{
			if (rootNode)
			{
				if (childNodes == null || childNodes.Length == 0)
				{
					//get all joints to draw
					PopulateChildren();
				}

				foreach (Transform child in childNodes)
				{

					if (child == rootNode)
					{
						//list includes the root, if root then larger, green cube
						Gizmos.color = Color.green;
						Gizmos.DrawCube(child.position, new Vector3(.1f, .1f, .1f));
					}
					else
					{
						Gizmos.color = Color.blue;
						#if UNITY_EDITOR
						Handles.DrawBezier(child.position, child.parent.position, child.position, child.parent.position, Color.blue, null, 5.0f);
						#else
						Gizmos.DrawLine(child.position, child.parent.position);
						#endif
						Gizmos.DrawSphere(child.position, 0.01f);
					}
				}
			}
		}

		public void PopulateChildren()
		{
			childNodes = rootNode.GetComponentsInChildren<Transform>();
		}
	}
}