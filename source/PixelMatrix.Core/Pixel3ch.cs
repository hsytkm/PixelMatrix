using System;
using System.Runtime.InteropServices;

namespace PixelMatrixLibrary.Core
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly ref struct Pixel3ch
    {
        public readonly byte Ch0;
        public readonly byte Ch1;
        public readonly byte Ch2;

        public Pixel3ch(byte ch0, byte ch1, byte ch2) => (Ch0, Ch1, Ch2) = (ch0, ch1, ch2);
        public Pixel3ch(byte level) : this(level, level, level) { }

    }
}
