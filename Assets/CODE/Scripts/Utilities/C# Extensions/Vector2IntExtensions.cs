using UnityEngine;

namespace Utilities.Extensions
{
    public static class Vector2IntExtensions
    {
        public static Vector2Int SetValue(this Vector2Int v, int value) => new(value, value);

        public static Vector2Int WithX(this Vector2Int vector, int x) => new(x, vector.y);

        public static Vector2Int WithY(this Vector2Int vector, int y) => new(vector.x, y);

        public static Vector2Int WithAddX(this Vector2Int v, int x) => new(v.x + x, v.y);

        public static Vector2Int WithSubtractX(this Vector2Int v, int x) => new(v.x - x, v.y);

        public static Vector2Int WithMultiplyX(this Vector2Int v, int x) => new(v.x * x, v.y);

        public static Vector2Int WithAddY(this Vector2Int v, int y) => new(v.x, v.y + y);

        public static Vector2Int WithSubtractY(this Vector2Int v, int y) => new(v.x, v.y - y);

        public static Vector2Int WithMultiplyY(this Vector2Int v, int y) => new(v.x, v.y * y);

        public static Vector2Int Clamp(this Vector2Int value, Vector2Int min, Vector2Int max) => new(Mathf.Clamp(value.x, min.x, max.x), Mathf.Clamp(value.y, min.y, max.y));

        public static Vector2Int Max(this Vector2Int a, Vector2Int b) => new(Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y));

        public static Vector2Int Min(this Vector2Int a, Vector2Int b) => new(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y));

        /// <summary>
        /// Remaps the components of a Vector2Int from one range to another.
        /// For example, remapping a vector from (0,0)-(100,100) to (0,0)-(1,1) would scale the vector down proportionally.
        /// </summary>
        public static Vector2Int Remap(
            this Vector2Int vector,
            Vector2Int sourceRange,
            Vector2Int targetRange
        ) =>
            new(
                vector.x.Remap(sourceRange, targetRange),
                vector.y.Remap(sourceRange, targetRange)
            );

        public static Vector2Int Abs(this Vector2Int vector) =>
            new(Mathf.Abs(vector.x), Mathf.Abs(vector.y));
    }
}
