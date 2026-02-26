using System;
using UnityEngine;

namespace Utilities.Extensions
{
	public static class RectTransformExtensions
	{
		public static void SetAnchor(this RectTransform source, AnchorPresets align, bool preservePosition = true)
		{
			// Save position if we want to preserve it
			Vector3 savedPosition = source.position;

			switch (align)
			{
				case AnchorPresets.TopLeft:
					{
						source.anchorMin = new Vector2(0, 1);
						source.anchorMax = new Vector2(0, 1);
						break;
					}
				case AnchorPresets.TopCenter:
					{
						source.anchorMin = new Vector2(0.5f, 1);
						source.anchorMax = new Vector2(0.5f, 1);
						break;
					}
				case AnchorPresets.TopRight:
					{
						source.anchorMin = new Vector2(1, 1);
						source.anchorMax = new Vector2(1, 1);
						break;
					}
				case AnchorPresets.MiddleLeft:
					{
						source.anchorMin = new Vector2(0, 0.5f);
						source.anchorMax = new Vector2(0, 0.5f);
						break;
					}
				case AnchorPresets.MiddleCenter:
					{
						source.anchorMin = new Vector2(0.5f, 0.5f);
						source.anchorMax = new Vector2(0.5f, 0.5f);
						break;
					}
				case AnchorPresets.MiddleRight:
					{
						source.anchorMin = new Vector2(1, 0.5f);
						source.anchorMax = new Vector2(1, 0.5f);
						break;
					}

				case AnchorPresets.BottomLeft:
					{
						source.anchorMin = new Vector2(0, 0);
						source.anchorMax = new Vector2(0, 0);
						break;
					}
				case AnchorPresets.BottomCenter:
					{
						source.anchorMin = new Vector2(0.5f, 0);
						source.anchorMax = new Vector2(0.5f, 0);
						break;
					}
				case AnchorPresets.BottomRight:
					{
						source.anchorMin = new Vector2(1, 0);
						source.anchorMax = new Vector2(1, 0);
						break;
					}

				case AnchorPresets.HorStretchTop:
					{
						source.anchorMin = new Vector2(0, 1);
						source.anchorMax = new Vector2(1, 1);
						break;
					}
				case AnchorPresets.HorStretchMiddle:
					{
						source.anchorMin = new Vector2(0, 0.5f);
						source.anchorMax = new Vector2(1, 0.5f);
						break;
					}
				case AnchorPresets.HorStretchBottom:
					{
						source.anchorMin = new Vector2(0, 0);
						source.anchorMax = new Vector2(1, 0);
						break;
					}

				case AnchorPresets.VertStretchLeft:
					{
						source.anchorMin = new Vector2(0, 0);
						source.anchorMax = new Vector2(0, 1);
						break;
					}
				case AnchorPresets.VertStretchCenter:
					{
						source.anchorMin = new Vector2(0.5f, 0);
						source.anchorMax = new Vector2(0.5f, 1);
						break;
					}
				case AnchorPresets.VertStretchRight:
					{
						source.anchorMin = new Vector2(1, 0);
						source.anchorMax = new Vector2(1, 1);
						break;
					}

				case AnchorPresets.StretchAll:
					{
						source.anchorMin = new Vector2(0, 0);
						source.anchorMax = new Vector2(1, 1);
						break;
					}
				case AnchorPresets.BottomStretch:
					{
						break;
					}
				default: throw new ArgumentOutOfRangeException(nameof(align), align, null);
			}

			// Restore position after changing anchor
			if (preservePosition) source.position = savedPosition;
		}

		public static void SetPivot(this RectTransform source, PivotPresets preset)
		{
			source.pivot = preset switch
			{
				PivotPresets.TopLeft => new Vector2(0, 1),
				PivotPresets.TopCenter => new Vector2(0.5f, 1),
				PivotPresets.TopRight => new Vector2(1, 1),
				PivotPresets.MiddleLeft => new Vector2(0, 0.5f),
				PivotPresets.MiddleCenter => new Vector2(0.5f, 0.5f),
				PivotPresets.MiddleRight => new Vector2(1, 0.5f),
				PivotPresets.BottomLeft => new Vector2(0, 0),
				PivotPresets.BottomCenter => new Vector2(0.5f, 0),
				PivotPresets.BottomRight => new Vector2(1, 0),
				_ => source.pivot
			};
		}

    	// Shared array used to receive result of RectTransform.GetWorldCorners
		static Vector3[] corners = new Vector3[4];

		/// Transform the bounds of the current rect transform to the space of another transform.
		/// <param name="source">The rect to transform</param>
		/// <param name="target">The target space to transform to</param>
		/// <returns>The transformed bounds</returns>
		public static Bounds TransformBoundsTo(this RectTransform source, Transform target)
		{
			// Based on code in ScrollRect's internal GetBounds and InternalGetBounds methods
			var bounds = new Bounds();
			if (source != null) {
				source.GetWorldCorners(corners);

				var vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
				var vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

				var matrix = target.worldToLocalMatrix;
				for (int j = 0; j < 4; j++) {
					Vector3 v = matrix.MultiplyPoint3x4(corners[j]);
					vMin = Vector3.Min(v, vMin);
					vMax = Vector3.Max(v, vMax);
				}

				bounds = new Bounds(vMin, Vector3.zero);
				bounds.Encapsulate(vMax);
			}
			return bounds;
		}
	}

	public enum AnchorPresets
	{
		TopLeft,
		TopCenter,
		TopRight,

		MiddleLeft,
		MiddleCenter,
		MiddleRight,

		BottomLeft,
		BottomCenter,
		BottomRight,
		BottomStretch,

		VertStretchLeft,
		VertStretchRight,
		VertStretchCenter,

		HorStretchTop,
		HorStretchMiddle,
		HorStretchBottom,

		StretchAll
	}

	public enum PivotPresets
	{
		TopLeft,
		TopCenter,
		TopRight,

		MiddleLeft,
		MiddleCenter,
		MiddleRight,

		BottomLeft,
		BottomCenter,
		BottomRight,
	}
}