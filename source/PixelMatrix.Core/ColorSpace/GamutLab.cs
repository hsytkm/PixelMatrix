using System;

namespace PixelMatrixLibrary.Core.ColorSpace
{
    public record GamutLab
    {
        public double L { get; }
        public double A { get; }
        public double B { get; }

        private GamutLab(double l, double a, double b) => (L, A, B) = (l, a, b);

        /// <summary>
        /// RGBからLabを計算(Web拾い版) これが正しいか不明…
        /// https://qiita.com/hachisuka_nsw/items/09caabe6bec46a2a0858
        /// </summary>
        /// <param name="blue">Bch(0.0~255.0)</param>
        /// <param name="green">Gch(0.0~255.0)</param>
        /// <param name="red">Rch(0.0~255.0)</param>
        /// <returns></returns>
        public static GamutLab Create(double blue, double green, double red)
        {
            static bool IsRange(double x) => 0d <= x && x <= 255d;
            if (!IsRange(blue) || !IsRange(green) || !IsRange(red)) throw new ArgumentException("bgr out of range.");

            var rgb_r = red / 255.0;
            var rgb_g = green / 255.0;
            var rgb_b = blue / 255.0;

            rgb_r = (rgb_r > 0.04045) ? Math.Pow((rgb_r + 0.055) / 1.055, 2.4) : (rgb_r / 12.92);
            rgb_g = (rgb_g > 0.04045) ? Math.Pow((rgb_g + 0.055) / 1.055, 2.4) : (rgb_g / 12.92);
            rgb_b = (rgb_b > 0.04045) ? Math.Pow((rgb_b + 0.055) / 1.055, 2.4) : (rgb_b / 12.92);

            var x = (rgb_r * 0.4124) + (rgb_g * 0.3576) + (rgb_b * 0.1805);
            var y = (rgb_r * 0.2126) + (rgb_g * 0.7152) + (rgb_b * 0.0722);
            var z = (rgb_r * 0.0193) + (rgb_g * 0.1192) + (rgb_b * 0.9505);

            x = (x * 100.0) / 95.0470;
            //y = (y * 100.0) / 100.000;
            z = (z * 100.0) / 108.883;

            x = (x > 0.008856) ? Math.Pow(x, 1.0 / 3.0) : (7.787 * x) + (4.0 / 29.0);
            y = (y > 0.008856) ? Math.Pow(y, 1.0 / 3.0) : (7.787 * y) + (4.0 / 29.0);
            z = (z > 0.008856) ? Math.Pow(z, 1.0 / 3.0) : (7.787 * z) + (4.0 / 29.0);

            return new GamutLab(
                (116.0 * y) - 16.0,     // L
                500.0 * (x - y),        // a
                200.0 * (y - z)         // b
            );
        }
    }
}
