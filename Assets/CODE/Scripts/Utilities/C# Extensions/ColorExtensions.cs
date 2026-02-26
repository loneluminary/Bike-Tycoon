using UnityEngine;

namespace Utilities.Extensions
{
    public static class ColorExtensions
    {
        /// Set value to color's channel.
        public static Color With(this Color color, int channel, float value)
        {
            color[channel] = value;
            return color;
        }

        /// Set color's red channel value.
        public static Color WithR(this Color color, float r) => color.With(0, r);

        /// Set color's green channel value.
        public static Color WithG(this Color color, float g) => color.With(1, g);

        /// Set color's blue channel value.
        public static Color WithB(this Color color, float b) => color.With(2, b);

        /// Set color's alpha channel value.
        public static Color WithA(this Color color, float a) => color.With(3, a);

        /// Set values to color's channels.
        public static Color With(this Color color, int channel1, float value1, int channel2, float value2)
        {
            color[channel1] = value1;
            color[channel2] = value2;

            return color;
        }

        /// Set color's red and green channels value.
        public static Color WithRG(this Color color, float r, float g) => color.With(0, r, 1, g);

        /// Set color's red and blue channels value.
        public static Color WithRB(this Color color, float r, float b) => color.With(0, r, 2, b);

        /// Set color's red and alpha channels value.
        public static Color WithRA(this Color color, float r, float a) => color.With(0, r, 3, a);

        /// Set color's green and blue channels value.
        public static Color WithGB(this Color color, float g, float b) => color.With(1, g, 2, b);

        /// Set color's green and alpha channels value.
        public static Color WithGA(this Color color, float g, float a) => color.With(1, g, 3, a);

        /// Set color's blue and alpha channels value.
        public static Color WithBA(this Color color, float b, float a) => color.With(2, b, 3, a);

        /// Set values to color's channels.
        public static Color With(this Color color, int channel1, float value1, int channel2, float value2, int channel3, float value3)
        {
            color[channel1] = value1;
            color[channel2] = value2;
            color[channel3] = value3;

            return color;
        }

        /// Set color's red, green, and blue channels value.
        public static Color WithRGB(this Color color, float r, float g, float b) => color.With(0, r, 1, g, 2, b);

        /// Set color's red, green, and alpha channels value.
        public static Color WithRGA(this Color color, float r, float g, float a) => color.With(0, r, 1, g, 3, a);

        /// Set color's red, blue, and alpha channels value.
        public static Color WithRBA(this Color color, float r, float b, float a) => color.With(0, r, 2, b, 3, a);

        /// Set color's green, blue, and alpha channels value.
        public static Color WithGBA(this Color color, float g, float b, float a) => color.With(1, g, 2, b, 3, a);
        
        /// Blends two colors with a specified ratio.
        /// <param name="color1">The first color.</param>
        /// <param name="color2">The second color.</param>
        /// <param name="ratio">The blend ratio (0 to 1).</param>
        /// <returns>The blended color.</returns>
        public static Color Blend(this Color color1, Color color2, float ratio)
        {
            ratio = Mathf.Clamp01(ratio);
            return new Color(color1.r * (1 - ratio) + color2.r * ratio, color1.g * (1 - ratio) + color2.g * ratio, color1.b * (1 - ratio) + color2.b * ratio, color1.a * (1 - ratio) + color2.a * ratio);
        }

        /// Inverts the color.
        /// <param name="color">The color to invert.</param>
        /// <returns>The inverted color.</returns>
        public static Color Invert(this Color color) => new(1 - color.r, 1 - color.g, 1 - color.b, color.a);
    }
}