using UnityEngine;

namespace Utilities.Extensions
{
	public static class Vector4Extensions
	{
		public static Vector4 SetValue(this Vector4 v, float value) => new(value, value, value, value);

		public static Vector4 WithX(this Vector4 v, float x) => new(x, v.y, v.z, v.w);

		public static Vector4 WithX(this Vector4 v, Vector4 target) => new(target.x, v.y, v.z, v.w);

		public static Vector4 SetX(this ref Vector4 v, float x)
		{
			v = new Vector4(x, v.y, v.z, v.w);
			return v;
		}

		public static Vector4 SetX(this ref Vector4 v, Vector4 target)
		{
			v = new Vector4(target.x, v.y, v.z, v.w);
			return v;
		}

		public static Vector4 WithY(this Vector4 v, float y) => new(v.x, y, v.z, v.w);

		public static Vector4 WithY(this Vector4 v, Vector4 target) => new(v.x, target.y, v.z, v.w);

		public static Vector4 SetY(this ref Vector4 v, float y)
		{
			v = new Vector4(v.x, y, v.z, v.w);
			return v;
		}

		public static Vector4 SetY(this ref Vector4 v, Vector4 target)
		{
			v = new Vector4(v.x, target.y, v.z, v.w);
			return v;
		}

		public static Vector4 WithZ(this Vector4 v, float z) => new(v.x, v.y, z, v.w);

		public static Vector4 WithZ(this Vector4 v, Vector4 target) => new(v.x, v.y, target.z, v.w);

		public static Vector4 SetZ(this ref Vector4 v, float z)
		{
			v = new Vector4(v.x, v.y, z, v.w);
			return v;
		}

		public static Vector4 SetZ(this ref Vector4 v, Vector4 target)
		{
			v = new Vector4(v.x, v.y, target.z, v.w);
			return v;
		}

		public static Vector4 WithW(this Vector4 v, float w) => new(v.x, v.y, v.z, w);

		public static Vector4 WithW(this Vector4 v, Vector4 target) => new(v.x, v.y, v.z, target.w);

		public static Vector4 SetW(this ref Vector4 v, float w)
		{
			v = new Vector4(v.x, v.y, v.z, w);
			return v;
		}

		public static Vector4 SetW(this ref Vector4 v, Vector4 target)
		{
			v = new Vector4(v.x, v.y, v.z, target.w);
			return v;
		}
	}
}