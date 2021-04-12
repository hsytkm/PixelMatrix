using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace PixelMatrixLibrary.Core
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct PixelMatrix : IEquatable<PixelMatrix>
    {
        public readonly IntPtr PixelsPtr;
        public readonly int AllocSize;
        public readonly int Width;
        public readonly int Height;
        public readonly int BytesPerPixel;
        public readonly int Stride;

        public PixelMatrix(int width, int height, int bytesPerPixel, int stride, IntPtr intPtr, int size)
        {
            if (IntPtr.Size != 8) throw new NotSupportedException();
            if (bytesPerPixel != 3) throw new NotSupportedException();
            if (Marshal.SizeOf(typeof(PixelMatrix)) != 8 + 4 * 5) throw new NotSupportedException();

            Width = width;
            Height = height;
            BytesPerPixel = bytesPerPixel;
            Stride = stride;
            PixelsPtr = intPtr;
            AllocSize = size;
        }

        #region IEquatable<T>
        public bool Equals(PixelMatrix other) => (PixelsPtr, AllocSize, Width, Height, BytesPerPixel, Stride) == (other.PixelsPtr, other.AllocSize, other.Width, other.Height, other.BytesPerPixel, other.Stride);
        public override bool Equals(object? obj) => (obj is PixelMatrix other) && Equals(other);
        public override int GetHashCode() => HashCode.Combine(PixelsPtr, AllocSize, Width, Height, BytesPerPixel, Stride);
        public static bool operator ==(in PixelMatrix left, in PixelMatrix right) => left.Equals(right);
        public static bool operator !=(in PixelMatrix left, in PixelMatrix right) => !(left == right);
        #endregion

        #region Properties
        public readonly int BitsPerPixel => BytesPerPixel * 8;

        public readonly bool IsContinuous => Width * BytesPerPixel == Stride;

        public readonly bool IsValid
        {
            get
            {
                if (PixelsPtr == IntPtr.Zero) return false;
                if (Width == 0 || Height == 0) return false;
                if (Stride < Width * BytesPerPixel) return false;
                if (AllocSize < Width * BytesPerPixel * Height) return false;
                return true;    //valid
            }
        }

        public readonly bool IsInvalid => !IsValid;
        #endregion

        #region GetChannelsAverage
        /// <summary>指定エリアの画素平均値を取得します</summary>
        public ReadOnlySpan<double> GetChannelsAverage(int rectX, int rectY, int rectWidth, int rectHeight)
        {
            if (IsInvalid) throw new ArgumentException("Invalid Image");
            if (rectWidth * rectHeight == 0) throw new ArgumentException("Area is zero.");
            if (Width < rectX + rectWidth) throw new ArgumentException("Width over.");
            if (Height < rectY + rectHeight) throw new ArgumentException("Height over.");

            var bytesPerPixel = BytesPerPixel;
            Span<ulong> sumChannels = stackalloc ulong[bytesPerPixel];

            unsafe
            {
                var stride = Stride;
                var rowHead = (byte*)PixelsPtr + (rectY * stride);
                var rowTail = rowHead + (rectHeight * stride);
                var columnLength = rectWidth * bytesPerPixel;

                for (byte* rowPtr = rowHead; rowPtr < rowTail; rowPtr += stride)
                {
                    for (byte* ptr = rowPtr; ptr < (rowPtr + columnLength); ptr += bytesPerPixel)
                    {
                        for (var c = 0; c < bytesPerPixel; ++c)
                        {
                            sumChannels[c] += *(ptr + c);
                        }
                    }
                }
            }

            var aveChannels = new double[sumChannels.Length];
            var count = (double)(rectWidth * rectHeight);

            for (var i = 0; i < aveChannels.Length; ++i)
            {
                aveChannels[i] = sumChannels[i] / count;
            }
            return aveChannels;
        }

        /// <summary>画面全体の画素平均値を取得します</summary>
        public ReadOnlySpan<double> GetChannelsAverage()
        {
            if (IsInvalid) throw new ArgumentException("Invalid Image");
            return GetChannelsAverage(0, 0, Width, Height);
        }
        #endregion

        #region Fill
        public void FillPixels(byte level) => FillPixels(new Pixel3ch(level));
        public void FillPixels(byte ch0, byte ch1, byte ch2) => FillPixels(new Pixel3ch(ch0, ch1, ch2));

        public unsafe void FillPixels(in Pixel3ch pixels)
        {
            var pixelsHead = (byte*)PixelsPtr;
            var stride = Stride;
            var pixelsTail = pixelsHead + Height * stride;

            for (var line = (byte*)PixelsPtr; line < pixelsTail; line += stride)
            {
                var lineTail = line + stride;
                for (var p = (Pixel3ch*)line; p < lineTail; ++p)
                {
                    *p = pixels;
                }
            }
        }
        #endregion

        #region ToBmpFile
        /// <summary>画像をbmpファイルに保存します</summary>
        public void ToBmpFile(string savePath)
        {
            if (IsInvalid) throw new ArgumentException("Invalid Image");

            using var ms = new MemoryStream();
            var bitmapSpan = GetBitmapBinary(this);
            ms.Read(bitmapSpan);
            ms.Seek(0, SeekOrigin.Begin);

            using var fs = new FileStream(savePath, FileMode.Create);
            fs.Seek(0, SeekOrigin.Begin);

            ms.WriteTo(fs);

            static Span<byte> GetBitmapBinary(in PixelMatrix pixel)
            {
                var height = pixel.Height;
                var srcStride = pixel.Stride;

                var destHeader = new BitmapHeader(pixel.Width, height, pixel.BitsPerPixel);
                var destBuffer = new byte[destHeader.FileSize];

                // bufferにheaderを書き込む
                UnsafeHelper.CopyStructToArray(destHeader, destBuffer);

                // 画素は左下から右上に向かって記録する
                unsafe
                {
                    var srcHead = (byte*)pixel.PixelsPtr;
                    fixed (byte* pointer = destBuffer)
                    {
                        var destHead = pointer + destHeader.OffsetBytes;
                        var destStride = destHeader.ImageStride;
                        Debug.Assert(srcStride <= destStride);

                        for (var y = 0; y < height; ++y)
                        {
                            var src = srcHead + (height - 1 - y) * srcStride;
                            var dest = destHead + (y * destStride);
                            UnsafeHelper.MemCopy(dest, src, srcStride);
                        }
                    }
                }
                return destBuffer;
            }
        }
        #endregion

    }
}
