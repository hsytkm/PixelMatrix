using System;

namespace PixelMatrixLibrary.Core.ColorSpace
{
    public record GamutBgr
    {
        public double B { get; }
        public double G { get; }
        public double R { get; }
        public double Y { get; }

        public GamutBgr(double b, double g, double r) => (B, G, R, Y) = (b, g, r, ToLuminanceY(b, g, r));

        public GamutBgr(byte b, byte g, byte r) : this((double)b, g, r) { }

        public static double ToLuminanceY(double b, double g, double r) => 0.299 * r + 0.587 * g + 0.114 * b;
    }
}
