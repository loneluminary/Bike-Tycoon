using System.Collections.Generic;
using UnityEngine;

namespace Utilities.Extensions
{
    public static class Vector3Extensions
    {
        public static Vector3 SetValue(this Vector3 v, float value) => new(value, value, value);

        #region With X

        public static Vector3 WithX(this Vector3 v, float x) => new(x, v.y, v.z);

        public static Vector3 WithPosX(this Vector3 v, Transform target) => new(target.position.x, v.y, v.z);

        public static Vector3 WithX(this Vector3 v, Vector3 target) => new(target.x, v.y, v.z);

        public static Vector3 WithAddX(this Vector3 v, float x) => new(v.x + x, v.y, v.z);

        public static Vector3 WithSubtractX(this Vector3 v, float x) => new(v.x - x, v.y, v.z);

        public static Vector3 WithMultiplyX(this Vector3 v, float x) => new(v.x * x, v.y, v.z);

        #endregion

        #region Set X

        public static Vector3 SetX(this ref Vector3 v, float x)
        {
            v = new Vector3(x, v.y, v.z);
            return v;
        }

        public static Vector3 SetX(this ref Vector3 v, Vector3 target)
        {
            v = new Vector3(target.x, v.y, v.z);
            return v;
        }

        public static Vector3 SetAddX(this ref Vector3 v, float x)
        {
            v = new Vector3(v.x + x, v.y, v.z);
            return v;
        }

        public static Vector3 SetSubtractX(this ref Vector3 v, float x)
        {
            v = new Vector3(v.x - x, v.y, v.z);
            return v;
        }

        public static Vector3 SetMultiplyX(this ref Vector3 v, float x)
        {
            v = new Vector3(v.x * x, v.y, v.z);
            return v;
        }

        #endregion

        #region With Y

        public static Vector3 WithY(this Vector3 v, float y) => new(v.x, y, v.z);

        public static Vector3 WithPosY(this Vector3 v, Transform target) => new(v.x, target.position.y, v.z);

        public static Vector3 WithY(this Vector3 v, Vector3 target) => new(v.x, target.y, v.z);

        public static Vector3 WithAddY(this Vector3 v, float y) => new(v.x, v.y + y, v.z);

        public static Vector3 WithSubtractY(this Vector3 v, float y) => new(v.x, v.y - y, v.z);

        public static Vector3 WithMultiplyY(this Vector3 v, float y) => new(v.x, v.y * y, v.z);

        #endregion

        #region Set Y

        public static Vector3 SetY(this ref Vector3 v, float y)
        {
            v = new Vector3(v.x, y, v.z);
            return v;
        }

        public static Vector3 SetY(this ref Vector3 v, Vector3 target)
        {
            v = new Vector3(v.x, target.y, v.z);
            return v;
        }

        public static Vector3 SetAddY(this ref Vector3 v, float y)
        {
            v = new Vector3(v.x, v.y + y, v.z);
            return v;
        }

        public static Vector3 SetSubtractY(this ref Vector3 v, float y)
        {
            v = new Vector3(v.x, v.y - y, v.z);
            return v;
        }

        public static Vector3 SetMultiplyY(this ref Vector3 v, float y)
        {
            v = new Vector3(v.x, v.y * y, v.z);
            return v;
        }

        #endregion

        #region with Z
        public static Vector3 WithZ(this Vector3 v, float z) => new(v.x, v.y, z);

        public static Vector3 WithPosZ(this Vector3 v, Transform target) => new(v.x, v.y, target.position.z);

        public static Vector3 WithZ(this Vector3 v, Vector3 target) => new(v.x, v.y, target.z);

        public static Vector3 WithAddZ(this Vector3 v, float z) => new(v.x, v.y, v.z + z);

        public static Vector3 WithSubtractZ(this Vector3 v, float z) => new(v.x, v.y, v.z - z);

        public static Vector3 WithMultiplyZ(this Vector3 v, float z) => new(v.x, v.y, v.z * z);

        #endregion


        #region Set Z

        public static Vector3 SetZ(this ref Vector3 v, float z)
        {
            v = new Vector3(v.x, v.y, z);
            return v;
        }

        public static Vector3 SetZ(this ref Vector3 v, Vector3 target)
        {
            v = new Vector3(v.x, v.y, target.z);
            return v;
        }

        public static Vector3 SetAddZ(this ref Vector3 v, float z)
        {
            v = new Vector3(v.x, v.y, v.z + z);
            return v;
        }

        public static Vector3 SetSubtractZ(this ref Vector3 v, float z)
        {
            v = new Vector3(v.x, v.y, v.z - z);
            return v;
        }

        public static Vector3 SetMultiplyZ(this ref Vector3 v, float z)
        {
            v = new Vector3(v.x, v.y, v.z * z);
            return v;
        }

        #endregion

        #region With XY, XZ, YZ

        public static Vector3 WithXY(this Vector3 v, float x, float y) => new(x, y, v.z);

        public static Vector3 WithXZ(this Vector3 v, float x, float z) => new(x, v.y, z);

        public static Vector3 WithYZ(this Vector3 v, float y, float z) => new(v.x, y, z);

        #endregion

        #region Set XY, XZ, YZ

        public static Vector3 SetXY(this ref Vector3 v, float x, float y)
        {
            v = new Vector3(x, y, v.z);
            return v;
        }

        public static Vector3 SetXZ(this ref Vector3 v, float x, float z)
        {
            v = new Vector3(x, v.y, z);
            return v;
        }

        public static Vector3 SetYZ(this ref Vector3 v, float y, float z)
        {
            v = new Vector3(v.x, y, z);
            return v;
        }

        #endregion

        #region Clamp

        public static Vector3 Clamp(this Vector3 value, Vector3 min, Vector3 max)
        {
            return new Vector3(Mathf.Clamp(value.x, min.x, max.x), Mathf.Clamp(value.y, min.y, max.y), Mathf.Clamp(value.z, min.z, max.z));
        }

        public static Vector3 Clamp01(this Vector3 value)
        {
            return new Vector3(Mathf.Clamp01(value.x), Mathf.Clamp01(value.y), Mathf.Clamp01(value.z));
        }

        public static Vector3 ClampMagnitude(this Vector3 vector, float maxLength) => Vector3.ClampMagnitude(vector, maxLength);

        public static Vector3 Max(this Vector3 a, Vector3 b) => new(Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y), Mathf.Max(a.z, b.z));

        public static Vector3 Min(this Vector3 a, Vector3 b) => new(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y), Mathf.Min(a.z, b.z));

        #endregion

        #region Lerp

        public static Vector3 Lerp(this Vector3 current, Vector3 target, float t) =>
            Vector3.Lerp(current, target, Mathf.Clamp01(t));

        public static Vector3 LerpUnclamped(this Vector3 current, Vector3 target, float t) =>
            Vector3.LerpUnclamped(current, target, t);

        public static Vector3 MoveTowards(this Vector3 current, Vector3 target, float maxDelta) =>
            Vector3.MoveTowards(current, target, maxDelta);

        #endregion

        #region Vector 2 conversion

        public static Vector2 ToVector2XY(this Vector3 v) => new(v.x, v.y);

        public static Vector2 ToVector2XZ(this Vector3 v) => new(v.x, v.z);

        public static Vector2 ToVector2YZ(this Vector3 v) => new(v.y, v.z);

        #endregion

        #region More

        public static Vector3 WithRandomBias(this Vector3 v, float biasValue) =>
            new(
                v.x.RandomBias(biasValue),
                v.y.RandomBias(biasValue),
                v.z.RandomBias(biasValue)
            );

        public static Vector3 WithRandomBias(this Vector3 v, Vector3 biasValue) =>
            new(
                v.x.RandomBias(biasValue.x),
                v.y.RandomBias(biasValue.y),
                v.z.RandomBias(biasValue.z)
            );

        public static Vector3 GetClosest(this Vector3 position, IEnumerable<Vector3> otherPositions)
        {
            var closest = Vector3.zero;
            var shortestDistance = Mathf.Infinity;

            foreach (var otherPosition in otherPositions)
            {
                var distance = (position - otherPosition).sqrMagnitude;

                if (distance < shortestDistance)
                {
                    closest = otherPosition;
                    shortestDistance = distance;
                }
            }

            return closest;
        }

        public static bool IsClose(this Vector3 a, Vector3 b, float epsilon = Vector3.kEpsilon)
        {
            return (a - b).sqrMagnitude <= epsilon * epsilon;
        }

        public static Vector3 SetMagnitude(this Vector3 vector, float magnitude) =>
            vector.normalized * magnitude;

        public static Vector3 ScaleBy(this Vector3 v, Vector3 scale) => new(v.x * scale.x, v.y * scale.y, v.z * scale.z);

        public static Vector3 ChangeIfInfinity(this Vector3 v, float valueToChangeTo = 0)
        {
            if (float.IsInfinity(v.x))
                v.x = valueToChangeTo;
            if (float.IsInfinity(v.y))
                v.y = valueToChangeTo;
            if (float.IsInfinity(v.z))
                v.z = valueToChangeTo;

            return v;
        }

        public static float Distance(this Vector3 v, Vector3 target) => Vector3.Distance(v, target);

        public static float Distance(this Vector3 v, Transform target) =>
            Vector3.Distance(v, target.position);

        public static float DistanceWithoutHeight(this Vector3 v, Vector3 target) =>
            Vector3.Distance(v.WithY(0), target.WithY(0));

        public static float DistanceOfHeight(this Vector3 v, Vector3 target) =>
            Vector3.Distance(v.WithX(0), target.WithY(0));
        #endregion

        public static Vector3 Remap(this Vector3 vector, Vector3 sourceMin, Vector3 sourceMax, Vector3 targetMin, Vector3 targetMax) => new(vector.x.Remap(sourceMin.x, sourceMax.x, targetMin.x, targetMax.x), vector.y.Remap(sourceMin.y, sourceMax.y, targetMin.y, targetMax.y), vector.z.Remap(sourceMin.z, sourceMax.z, targetMin.z, targetMax.z));

        public static bool IsUniform(this Vector3 vector) => vector.x.Approximately(vector.y) && vector.y.Approximately(vector.z);

        public static Vector3 Abs(this Vector3 vector) => new(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));

        public static Vector3 Round(this Vector3 vector)
        {
            vector.x = Mathf.Round(vector.x);
            vector.y = Mathf.Round(vector.y);
            vector.z = Mathf.Round(vector.z);
            return vector;
        }

        public static Vector3Int RoundToInt(this Vector3 vector)
        {
            return new Vector3Int(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y), Mathf.RoundToInt(vector.z));
        }

        /// Inverts a vector
        public static Vector3 Invert(this Vector3 newValue) => new(1.0f / newValue.x, 1.0f / newValue.y, 1.0f / newValue.z);

        /// Projects a vector on another
        public static Vector3 Project(this Vector3 vector, Vector3 projectedVector)
        {
            float _dot = Vector3.Dot(vector, projectedVector);
            return _dot * projectedVector;
        }

        /// Rejects a vector on another
        public static Vector3 Reject(this Vector3 vector, Vector3 rejectedVector) => vector - vector.Project(rejectedVector);

        #region Swap

        public static Vector3 SwapXY(this Vector3 vector) => new(vector.y, vector.x, vector.z);

        public static Vector3 SwapYZ(this Vector3 vector) => new(vector.x, vector.z, vector.y);

        public static Vector3 SwapXZ(this Vector3 vector) => new(vector.z, vector.y, vector.x);

        #endregion
        
        /// Divides two Vector3 objects component-wise.
        /// <remarks>
        /// For each component in v0 (x, y, z), it is divided by the corresponding component in v1 if the component in v1 is not zero. 
        /// Otherwise, the component in v0 remains unchanged.
        /// </remarks>
        /// <example>
        /// Use 'ComponentDivide' to scale a game object proportionally:
        /// <code>
        /// myObject.transform.localScale = originalScale.ComponentDivide(targetDimensions);
        /// </code>
        /// This scales the object size to fit within the target dimensions while maintaining its original proportions.
        ///</example>
        /// <param name="v0">The Vector3 object that this method extends.</param>
        /// <param name="v1">The Vector3 object by which v0 is divided.</param>
        /// <returns>A new Vector3 object resulting from the component-wise division.</returns>
        public static Vector3 ComponentDivide(this Vector3 v0, Vector3 v1)
        {
            return new Vector3(v1.x != 0 ? v0.x / v1.x : v0.x, v1.y != 0 ? v0.y / v1.y : v0.y, v1.z != 0 ? v0.z / v1.z : v0.z);
        }
        
        
        /// Adds a random offset to the components of a <see cref="Vector3"/> within the specified range.
        /// <param name="vector">The original vector to which the random offset will be applied.</param>
        /// <param name="range">The maximum absolute value of random offsets that can be added 
        /// or subtracted to/from each component of the vector.</param>
        /// <returns>A new <see cref="Vector3"/> with random offsets applied to its X, Y, and Z components.
        /// Each offset is in the range [-<paramref name="range"/>, <paramref name="range"/>].</returns>
        public static Vector3 RandomOffset(this Vector3 vector, float range)
        {
            return vector + new Vector3(Random.Range(-range, range), Random.Range(-range, range), Random.Range(-range, range));
        }

        /// Computes a random point in an annulus (a ring-shaped area) based on minimum and 
        /// maximum radius values around a central Vector3 point (origin).
        /// <param name="origin">The center Vector3 point of the annulus.</param>
        /// <param name="minRadius">Minimum radius of the annulus.</param>
        /// <param name="maxRadius">Maximum radius of the annulus.</param>
        /// <returns>A random Vector3 point within the specified annulus.</returns>
        public static Vector3 RandomPointInAnnulus(this Vector3 origin, float minRadius, float maxRadius)
        {
            float angle = Random.value * Mathf.PI * 2f;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            // Squaring and then square-rooting radii to ensure uniform distribution within the annulus
            float minRadiusSquared = minRadius * minRadius;
            float maxRadiusSquared = maxRadius * maxRadius;
            float distance = Mathf.Sqrt(Random.value * (maxRadiusSquared - minRadiusSquared) + minRadiusSquared);

            // Converting the 2D direction vector to a 3D position vector
            Vector3 position = new Vector3(direction.x, 0, direction.y) * distance;
            return origin + position;
        }

        /// Rounds the components of a Vector3 down to the nearest multiple of the given quantization step.
        /// This is useful for reducing precision or snapping positions to a grid,
        /// for example, to limit NavMesh rebuilds or discretize movement updates.
        /// <param name="position">The original Vector3 position to be quantized.</param>
        /// <param name="quantization">The quantization step for each component (x, y, z).</param>
        /// <returns>A new Vector3 with each component rounded down to the nearest multiple of the corresponding quantization step.</returns>
        public static Vector3 Quantize(this Vector3 position, Vector3 quantization)
        {
            return Vector3.Scale(quantization, new Vector3(Mathf.Floor(position.x / quantization.x), Mathf.Floor(position.y / quantization.y), Mathf.Floor(position.z / quantization.z)));
        }
    }
}
