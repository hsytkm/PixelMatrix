using PixelMatrix.Core.ColorSpace;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace PixelMatrix.Core
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 24)]
    public readonly struct Pixel3Matrix : IEquatable<Pixel3Matrix>
    {
        public static int Channel = 3;

        public readonly IntPtr PixelsPtr;
        //public readonly int AllocSize; //= Height * Stride;
        public readonly int Width;
        public readonly int Height;
        public readonly int BytesPerPixel;
        public readonly int Stride;

        public int Column => Width;
        public int Row => Height;

        public Pixel3Matrix(int width, int height, int bytesPerPixel, int stride, IntPtr intPtr)
        {
            if (IntPtr.Size != 8) throw new NotSupportedException();
            if (bytesPerPixel != Channel) throw new NotSupportedException();

            Width = width;
            Height = height;
            BytesPerPixel = bytesPerPixel;
            Stride = stride;
            PixelsPtr = intPtr;
        }

        #region IEquatable<T>
        public bool Equals(Pixel3Matrix other) => this == other;
        public override bool Equals(object? obj) => (obj is Pixel3Matrix other) && Equals(other);
        public override int GetHashCode() => HashCode.Combine(PixelsPtr, Width, Height, BytesPerPixel, Stride);
        public static bool operator ==(in Pixel3Matrix left, in Pixel3Matrix right)
             => (left.PixelsPtr, left.Width, left.Height, left.BytesPerPixel, left.Stride)
                == (right.PixelsPtr, right.Width, right.Height, right.BytesPerPixel, right.Stride);

        public static bool operator !=(in Pixel3Matrix left, in Pixel3Matrix right) => !(left == right);
        #endregion

        #region Properties
        public int AllocatedSize => Width * BytesPerPixel * Height;     // Strideは見ない
        public int BitsPerPixel => BytesPerPixel * 8;

        public bool IsContinuous => (Width * BytesPerPixel) == Stride;

        public bool IsValid
        {
            get
            {
                if (PixelsPtr == IntPtr.Zero) return false;
                if (Width <= 0 || Height <= 0) return false;
                if (Stride < Width * BytesPerPixel) return false;
                if (AllocatedSize < Width * BytesPerPixel * Height) return false;
                return true;    //valid
            }
        }

        public bool IsInvalid => !IsValid;
        #endregion

        #region Span
        /// <summary>指定行の画素Spanを取得します</summary>
        public unsafe ReadOnlySpan<Pixel3ch> GetRowSpan(int row)
        {
            if (row < 0 || Height - 1 < row) throw new ArgumentException("invalid row");

            var ptr = PixelsPtr + (row * Stride);
            return new ReadOnlySpan<Pixel3ch>(ptr.ToPointer(), Width);
        }
        #endregion

        #region GetChannelsAverage
        /// <summary>指定領域における各チャンネルの画素平均値を取得します</summary>
        public ColorBgr GetChannelsAverage(int x, int y, int width, int height)
        {
            if (IsInvalid) throw new ArgumentException("Invalid image.");
            if (width * height == 0) throw new ArgumentException("Area is zero.");
            if (Width < x + width) throw new ArgumentException("Width over.");
            if (Height < y + height) throw new ArgumentException("Height over.");

            var bytesPerPixel = BytesPerPixel;
            Span<ulong> sumChannels = stackalloc ulong[bytesPerPixel];

            unsafe
            {
                var stride = Stride;
                var rowHead = (byte*)PixelsPtr + (y * stride);
                var rowTail = rowHead + (height * stride);
                var columnLength = width * bytesPerPixel;

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

            Span<double> aveChannels = stackalloc double[sumChannels.Length];
            var count = (double)(width * height);

            for (var i = 0; i < aveChannels.Length; ++i)
            {
                aveChannels[i] = sumChannels[i] / count;
            }
            return new ColorBgr(aveChannels);
        }

        /// <summary>画面全体における各チャンネルの画素平均値を取得します</summary>
        public ColorBgr GetChannelsAverageOfEntire() => GetChannelsAverage(0, 0, Width, Height);
        #endregion

        #region FillAllPixels
        /// <summary>指定の画素値で画像全体を埋めます</summary>
        public void FillAllPixels(in Pixel3ch pixels)
        {
            unsafe
            {
                var pixelsHead = (byte*)PixelsPtr;
                var stride = Stride;
                var pixelsTail = pixelsHead + Height * stride;
                var widthOffset = Width * BytesPerPixel;

                for (var line = (byte*)PixelsPtr; line < pixelsTail; line += stride)
                {
                    var lineTail = line + widthOffset;
                    for (var p = (Pixel3ch*)line; p < lineTail; ++p)
                    {
                        *p = pixels;
                    }
                }
            }
        }
        #endregion

        #region FillRectangle
        /// <summary>指定領域の画素を更新します</summary>
        public void FillRectangle(in Pixel3ch pixel, int x, int y, int width, int height)
        {
            if (Width < x + width) throw new ArgumentException("vertical direction");
            if (Height < y + height) throw new ArgumentException("horizontal direction");

            unsafe
            {
                var lineHeadPtr = (byte*)GetPixelPtr(x, y);
                var lineTailPtr = lineHeadPtr + (height * Stride);
                var widthOffset = width * BytesPerPixel;

                for (var linePtr = lineHeadPtr; linePtr < lineTailPtr; linePtr += Stride)
                {
                    for (var p = (Pixel3ch*)linePtr; p < linePtr + widthOffset; p++)
                        *p = pixel;
                }
            }
        }
        #endregion

        #region WritePixel
        /// <summary>指定画素の IntPtr を取得します</summary>
        public IntPtr GetPixelPtr(int x, int y)
        {
            if (x > Width - 1 || y > Height - 1) throw new ArgumentException("Out of image.");
            return PixelsPtr + (y * Stride) + (x * BytesPerPixel);
        }

        /// <summary>指定位置の画素を更新します</summary>
        public void WritePixel(in Pixel3ch pixels, int x, int y)
        {
            if (x > Width - 1 || y > Height - 1) return;
            var ptr = GetPixelPtr(x, y);
            UnsafeHelper.WriteStructureToPtr(ptr, pixels);
        }
        #endregion

        #region CutOut
        /// <summary>画像の一部を切り出した子画像を取得します</summary>
        public Pixel3Matrix CutOutPixelMatrix(int x, int y, int width, int height)
        {
            if (Width < x + width) throw new ArgumentException("vertical direction");
            if (Height < y + height) throw new ArgumentException("horizontal direction");
            return new Pixel3Matrix(width, height, BytesPerPixel, Stride, GetPixelPtr(x, y));
        }
        #endregion

        #region CopyTo
        /// <summary>画素値をコピーします</summary>
        public void CopyTo(in Pixel3Matrix destPixels)
        {
            if (this.Width != destPixels.Width || this.Height != destPixels.Height) throw new ArgumentException("size is different.");
            if (this.PixelsPtr == destPixels.PixelsPtr) throw new ArgumentException("same pointer.");

            Update(this, destPixels);

            // 画素値のコピー（サイズチェックなし）
            static void Update(in Pixel3Matrix srcPixels, in Pixel3Matrix destPixels)
            {
                // メモリが連続していれば memcopy
                if (srcPixels.IsContinuous && destPixels.IsContinuous)
                {
                    UnsafeHelper.MemCopy(srcPixels.PixelsPtr, destPixels.PixelsPtr, srcPixels.AllocatedSize);
                    return;
                }

                unsafe
                {
                    var (width, height, bytesPerPixel) = (srcPixels.Width, srcPixels.Height, srcPixels.BytesPerPixel);
                    var srcHeadPtr = (byte*)srcPixels.PixelsPtr;
                    var srcStride = srcPixels.Stride;
                    var dstHeadPtr = (byte*)destPixels.PixelsPtr;
                    var dstStride = destPixels.Stride;

                    for (int y = 0; y < height; y++)
                    {
                        var src = srcHeadPtr + (y * srcStride);
                        var dst = dstHeadPtr + (y * dstStride);

                        for (int x = 0; x < width * bytesPerPixel; x += bytesPerPixel)
                        {
                            *(Pixel3ch*)(dst + x) = *(Pixel3ch*)(src + x);
                        }
                    }
                }
            }
        }

        /// <summary>画素値を拡大コピーします</summary>
        public void CopyToWithScaleUp(in Pixel3Matrix destination)
        {
            if (this.BitsPerPixel != 3 || destination.BitsPerPixel != 3)
                throw new ArgumentException("byte/pixel error.");

            if (destination.Width % this.Width != 0 || destination.Height % this.Height != 0)
                throw new ArgumentException("must be an integral multiple.");

            var widthRatio = destination.Width / this.Width;
            var heightRatio = destination.Height / this.Height;
            if (widthRatio != heightRatio) throw new ArgumentException("magnifications are different.");

            var magnification = widthRatio;
            if (magnification <= 1) throw new ArgumentException("ratio must be greater than 1.");

            ScaleUp(this, destination, magnification);

            static unsafe void ScaleUp(in Pixel3Matrix source, in Pixel3Matrix destination, int magnification)
            {
                var bytesPerPixel = source.BytesPerPixel;
                var srcPixelHead = (byte*)source.PixelsPtr;
                var srcStride = source.Stride;
                var srcWidth = source.Width;
                var srcHeight = source.Height;

                var destPixelHead = (byte*)destination.PixelsPtr;
                var destStride = destination.Stride;

                for (int y = 0; y < srcHeight; y++)
                {
                    var src = srcPixelHead + (srcStride * y);
                    var dest0 = destPixelHead + (destStride * y * magnification);

                    for (int x = 0; x < srcWidth * bytesPerPixel; x += bytesPerPixel)
                    {
                        var pixel = (Pixel3ch*)(src + x);
                        var dest1 = dest0 + (x * magnification);

                        for (byte* dest2 = dest1; dest2 < dest1 + (destStride * magnification); dest2 += destStride)
                        {
                            for (byte* dest3 = dest2; dest3 < dest2 + (bytesPerPixel * magnification); dest3 += bytesPerPixel)
                                *((Pixel3ch*)dest3) = *pixel;
                        }
                    }
                }
            }
        }
        #endregion

        #region ToBmpFile
        /// <summary>画像をbmpファイルに保存します</summary>
        public void ToBmpFile(string savePath)
        {
            using var ms = ToBitmapMemoryStream(savePath);
            using var fs = new FileStream(savePath, FileMode.Create);
            fs.Seek(0, SeekOrigin.Begin);

            ms.WriteTo(fs);
        }

        /// <summary>画像をbmpファイルに保存します</summary>
        public async Task ToBmpFileAsync(string savePath, CancellationToken token = default)
        {
            await Task.Yield();

            using var ms = ToBitmapMemoryStream(savePath);
            using var fs = new FileStream(savePath, FileMode.Create);
            fs.Seek(0, SeekOrigin.Begin);

            await ms.CopyToAsync(fs, token);
        }

        /// <summary>画像をbmpファイルに保存します</summary>
        private MemoryStream ToBitmapMemoryStream(string savePath)
        {
            if (IsInvalid) throw new ArgumentException("Invalid image.");
            if (File.Exists(savePath)) throw new SystemException("File is exists.");

            var bitmapBytes = GetBitmapBinary(this);
            var ms = new MemoryStream(bitmapBytes);
            ms.Seek(0, SeekOrigin.Begin);
            return ms;

            // Bitmapのバイナリ配列を取得します
            static byte[] GetBitmapBinary(in Pixel3Matrix pixel)
            {
                var height = pixel.Height;
                var srcStride = pixel.Stride;
                var destHeader = new BitmapHeader(pixel.Width, height, pixel.BitsPerPixel);
                var destBuffer = new byte[destHeader.FileSize];     // さずがにデカすぎるのでbyte[]

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
