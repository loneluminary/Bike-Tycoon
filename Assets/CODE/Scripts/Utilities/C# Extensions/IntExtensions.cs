using System;
using UnityEngine;

namespace Utilities.Extensions
{
    public static class IntExtensions
    {
        /// Extension method for int that converts the number to an abbreviated string (k, m, g).
        /// Returns string abbreviated string for display.
        /// Arguments: uint digits: Number of decimal digits to show.
        public static string ToAbbreviatedString(this int n, uint digits = 0)
        {
            string s;
            var nabs = Math.Abs(n);
            if (nabs < 1000)
                s = n + "";
            else if (nabs < 1000000)
                s = ((decimal)n / 1000).TruncateTo(digits) + "k";
            else if (nabs < 1000000000)
                s = ((decimal)n / 1000000).TruncateTo(digits) + "m";
            else
                s = ((decimal)n / 1000000000).TruncateTo(digits) + "g";

            return s;
        }

        /// Extension method for int that rounds the value down to the nearest multiple of binSize.
        /// Returns int rounded value.
        /// Arguments: int binSize: The multiple to round to.
        public static int RoundToMultipleOf(this int n, int binSize)
        {
            var result = (n / binSize) * binSize;
            if (n < 0)
            {
                result -= binSize;
            }

            return result;
        }

        /// Extension method for int that checks if the value is within a specified range.
        /// Returns bool true if in range, false otherwise.
        /// Arguments: int minValue, int maxValue: Range limits.
        public static bool IsInRange(this int value, int minValue, int maxValue) => value >= minValue && value <= maxValue;

        /// Extension method for int that finds the closest value within a specified range.
        /// Returns int closest value in range.
        /// Arguments: int minValue, int maxValue: Range limits.
        public static int ClosestInRange(this int value, int minValue, int maxValue)
        {
            if (value.IsInRange(minValue, maxValue)) return value;

            int diffrenceToMinValue = Mathf.Abs(value - minValue);
            int diffrenceToMaxValue = Mathf.Abs(value - maxValue);

            return (int)MathF.Min(diffrenceToMinValue, diffrenceToMaxValue);
        }

        /// Extension method for int that returns the smaller of value and max.
        /// Returns int the smaller value.
        /// Arguments: int max: Maximum allowed value.
        public static int Max(this int value, int max) => value <= max ? value : max;

        /// Extension method for int that returns the larger of value and min.
        /// Returns int the larger value.
        /// Arguments: int min: Minimum allowed value.
        public static int Min(this int value, int min) => value <= min ? min : value;

        /// Extension method for int that clamps the value between min and max.
        /// Returns int clamped value.
        /// Arguments: int min, int max: Clamp limits.
        public static int Clamp(this int value, int min, int max) => Math.Max(min, Math.Min(max, value));

        /// Extension method for int that linearly interpolates towards target by t.
        /// Returns int interpolated value.
        /// Arguments: int target: Target value. float t: Interpolation factor.
        public static int Lerp(this int current, int target, float t)
        {
            t = Mathf.Clamp01(t);
            return Mathf.RoundToInt(Mathf.Lerp(current, target, t));
        }

        /// Extension method for int that linearly interpolates towards target by t (unclamped).
        /// Returns int interpolated value.
        /// Arguments: int target: Target value. float t: Interpolation factor.
        public static int LerpUnclamped(this int current, int target, float t) => Mathf.RoundToInt(Mathf.LerpUnclamped(current, target, t));

        /// Extension method for int that moves towards target by maxDelta.
        /// Returns int moved value.
        /// Arguments: int target: Target value. int maxDelta: Maximum change per call.
        public static int MoveTowards(this int current, int target, int maxDelta)
        {
            if (Mathf.Abs(target - current) <= maxDelta) return target;

            return current + (int)Mathf.Sign(target - current) * maxDelta;
        }

        /// Extension method for int that remaps value from source range to target range.
        /// Returns int remapped value.
        /// Arguments: Vector2Int sourceRange, Vector2Int targetRange: Ranges for remapping.
        public static int Remap(this int value, Vector2Int sourceRange, Vector2Int targetRange)
        {
            if (sourceRange.x == sourceRange.y) return targetRange.x; // Avoid division by zero
            float t = (value - sourceRange.x) / (float)(sourceRange.y - sourceRange.x);
            return Mathf.RoundToInt(Mathf.Lerp(targetRange.x, targetRange.y, t));
        }

        // ---------------------------------------------------------------------------------------------

        /// Extension method for int that returns the sine of the value.
        /// Returns float sine value.
        /// Arguments: int f: Angle in radians.
        public static float Sin(this int f) => Mathf.Sin(f);

        /// Extension method for int that returns the cosine of the value.
        /// Returns float cosine value.
        /// Arguments: int f: Angle in radians.
        public static float Cos(this int f) => Mathf.Cos(f);

        /// Extension method for int that returns the tangent of the value.
        /// Returns float tangent value.
        /// Arguments: int f: Angle in radians.
        public static float Tan(this int f) => Mathf.Tan(f);

        /// Extension method for int that returns the arc-sine of the value.
        /// Returns float arc-sine value.
        /// Arguments: int f: Input value.
        public static float Asin(this int f) => Mathf.Asin(f);

        /// Extension method for int that returns the arc-cosine of the value.
        /// Returns float arc-cosine value.
        /// Arguments: int f: Input value.
        public static float Acos(this int f) => Mathf.Acos(f);

        /// Extension method for int that returns the arc-tangent of the value.
        /// Returns float arc-tangent value.
        /// Arguments: int f: Input value.
        public static float Atan(this int f) => Mathf.Atan(f);

        /// Extension method for int that returns the angle in radians whose tangent is y/x.
        /// Returns float angle in radians.
        /// Arguments: int y: Y value. int x: X value.
        public static float Atan2(this int y, int x) => Mathf.Atan2(y, x);

        /// Extension method for int that returns the square root of the value.
        /// Returns float square root.
        /// Arguments: int f: Input value.
        public static float Sqrt(this int f) => Mathf.Sqrt(f);

        /// Extension method for int that returns the absolute value.
        /// Returns float absolute value.
        /// Arguments: int f: Input value.
        public static float Abs(this int f) => Mathf.Abs(f);

        /// Extension method for int that raises the value to the given power.
        /// Returns float result.
        /// Arguments: float x: Power to raise to.
        public static float Pow(this int f, float x) => Mathf.Pow(f, x);

        /// Extension method for int that returns e raised to the value.
        /// Returns float exponential value.
        /// Arguments: int power: Power to raise e.
        public static float Exp(this int power) => Mathf.Exp(power);

        /// Extension method for int that returns the natural logarithm.
        /// Returns float natural logarithm.
        /// Arguments: int f: Input value.
        public static float Log(this int f) => Mathf.Log(f);

        /// Extension method for int that returns the logarithm in a specified base.
        /// Returns float logarithm value.
        /// Arguments: float p: Logarithm base.
        public static float Log(this int f, float p) => Mathf.Log(f, p);

        /// Extension method for int that returns the base 10 logarithm.
        /// Returns float base 10 logarithm.
        /// Arguments: int f: Input value.
        public static float Log10(this int f) => Mathf.Log10(f);

        /// Extension method for int that interpolates angles, wrapping around 360 degrees.
        /// Returns float interpolated angle.
        /// Arguments: float target: Target angle. float time: Interpolation factor.
        public static float LerpAngle(this int current, float target, float time) => Mathf.LerpAngle(current, target, time);

        /// Extension method for int that moves towards an angle, wrapping around 360 degrees.
        /// Returns float moved angle.
        /// Arguments: float target: Target angle. float time: Maximum change.
        public static float MoveTowardsAngle(this int current, float target, float time) => Mathf.MoveTowardsAngle(current, target, time);

        /// Extension method for int that loops the value so it stays within length.
        /// Returns float repeated value.
        /// Arguments: float length: Loop length.
        public static float Repeat(this int t, float length) => Mathf.Repeat(t, length);

        /// Extension method for int that returns a ping-pong value between 0 and length.
        /// Returns float ping-pong value.
        /// Arguments: float length: Ping-pong range.
        public static float PingPong(this int t, float length) => Mathf.PingPong(t, length);

        public static string ToMoneyText(this int money, float decimalSizeRate = 0.6f, string currency = "", float fontSize = 0f)
        {
            string text = money >= 0 ? currency.IsNullOrEmpty() ? "$" + money.ToString("0.00") : currency + money.ToString("0.00") : $"- {(currency.IsNullOrEmpty() ? "$" : currency)}" + Math.Abs(money).ToString("0.00");
            text = text.Replace(',', '.'); // Ensure consistent decimal formatting
            int num = text.IndexOf('.');
            if (num != -1 && fontSize > 0f)
            {
                string text2 = "<size=" + fontSize * decimalSizeRate + ">";
                text2 = text2.Replace(',', '.');
                text = text.Insert(num + 1, text2);
            }

            return text;
        }
    }
}