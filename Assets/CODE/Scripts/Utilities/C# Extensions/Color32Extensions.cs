using UnityEngine;

namespace Utilities.Extensions
{
    public static class Color32Extensions
    {
        /// Set value to color's channel.
        public static Color32 With(this Color32 color, int channel, byte value)
        {
            color[channel] = value;
            return color;
        }
        /// Set color's red channel value.
        public static Color32 WithR(this Color32 color, byte r) => color.With(0, r);
        
        /// Set color's green channel value.
        public static Color32 WithG(this Color32 color, byte g) => color.With(1, g);
        
        /// Set color's blue channel value.
        public static Color32 WithB(this Color32 color, byte b) => color.With(2, b);
        
        /// Set color's alpha channel value.
        public static Color32 WithA(this Color32 color, byte a) => color.With(3, a);
        
        /// Set values to color's channels.
        public static Color32 With(this Color32 color, int channel1, byte value1, int channel2, byte value2)
        {
            color[channel1] = value1;
            color[channel2] = value2;

            return color;
        }
        
        /// Set color's red and green channels value.
        public static Color32 WithRG(this Color32 color, byte r, byte g) => color.With(0, r, 1, g);
        
        /// Set color's red and blue channels value.
        public static Color32 WithRB(this Color32 color, byte r, byte b) => color.With(0, r, 2, b);
        
        /// Set color's red and alpha channels value.
        public static Color32 WithRA(this Color32 color, byte r, byte a) => color.With(0, r, 3, a);
        
        /// Set color's green and blue channels value.
        public static Color32 WithGB(this Color32 color, byte g, byte b) => color.With(1, g, 2, b);
        
        /// Set color's green and alpha channels value.
        public static Color32 WithGA(this Color32 color, byte g, byte a) => color.With(1, g, 3, a);
        
        /// Set color's blue and alpha channels value.
        public static Color32 WithBA(this Color32 color, byte b, byte a) => color.With(2, b, 3, a);
        
        /// Set values to color's channels.
        public static Color32 With(this Color32 color, int channel1, byte value1, int channel2, byte value2, int channel3, byte value3)
        {
            color[channel1] = value1;
            color[channel2] = value2;
            color[channel3] = value3;

            return color;
        }
        
        /// Set color's red, green, and blue channels value.
        public static Color32 WithRGB(this Color32 color, byte r, byte g, byte b) => color.With(0, r, 1, g, 2, b);
        
        /// Set color's red, green, and alpha channels value.
        public static Color32 WithRGA(this Color32 color, byte r, byte g, byte a) => color.With(0, r, 1, g, 3, a);
        
        /// Set color's red, blue, and alpha channels value.
        public static Color32 WithRBA(this Color32 color, byte r, byte b, byte a) => color.With(0, r, 2, b, 3, a);
        
        /// Set color's green, blue, and alpha channels value.
        public static Color32 WithGBA(this Color32 color, byte g, byte b, byte a) => color.With(1, g, 2, b, 3, a);
        
        /// Blends two Color32 values with a specified ratio.
        /// <param name="color1">The first color.</param>
        /// <param name="color2">The second color.</param>
        /// <param name="ratio">The blend ratio (0 to 1).</param>
        /// <returns>The blended color.</returns>
        public static Color32 Blend(this Color32 color1, Color32 color2, float ratio)
        {
            ratio = Mathf.Clamp01(ratio);
            return new Color32((byte)(color1.r * (1 - ratio) + color2.r * ratio), (byte)(color1.g * (1 - ratio) + color2.g * ratio), (byte)(color1.b * (1 - ratio) + color2.b * ratio), (byte)(color1.a * (1 - ratio) + color2.a * ratio));
        }

        /// Inverts the Color32 value.
        /// <param name="color">The color to invert.</param>
        /// <returns>The inverted color.</returns>
        public static Color32 Invert(this Color32 color) => new((byte)(255 - color.r), (byte)(255 - color.g), (byte)(255 - color.b), color.a);
    }
}