using PixelMatrix.Core;
using System;

namespace PixelMatrix.Wpf.Extensions
{
    public static class PixelMatrixExtension
    {
        internal static double _dpiX = 96.0;
        internal static double _dpiY = _dpiX;

        #region ToBitmapSource
        /// <summary>System.Windows.Media.Imaging.BitmapSource に変換します</summary>
        public static System.Windows.Media.Imaging.BitmapSource ToBitmapSource(in this Pixel3Matrix pixel, bool isFreeze = true)
        {
            if (pixel.IsInvalid) throw new ArgumentException("Invalid ImagePixels");
            if (pixel.BytesPerPixel != Pixel3Matrix.Channel) throw new NotSupportedException("Invalid BytesPerPixel");

            var bitmapSource = System.Windows.Media.Imaging.BitmapSource.Create(
                pixel.Width, pixel.Height, _dpiX, _dpiY,
                System.Windows.Media.PixelFormats.Bgr24, null,
                pixel.PixelsPtr, pixel.Stride * pixel.Height, pixel.Stride);

            if (isFreeze) bitmapSource.Freeze();
            return bitmapSource;
        }
        #endregion

        #region ToWriteableBitmap
        /// <summary>System.Windows.Media.Imaging.WriteableBitmap の画素値を更新します(遅いです)</summary>
        public static void CopyTo(this System.Windows.Media.Imaging.WriteableBitmap writeableBitmap, in Pixel3Matrix pixel, bool isFreeze = false)
        {
            if (pixel.IsInvalid) throw new ArgumentException("Invalid Image");

            if (writeableBitmap.IsFrozen) throw new ArgumentException("WriteableBitmap is frozen");
            if (writeableBitmap.IsInvalid()) throw new ArgumentException("Invalid Image");
            if (writeableBitmap.PixelWidth != pixel.Width) throw new ArgumentException("Different Width");
            if (writeableBitmap.PixelHeight != pixel.Height) throw new ArgumentException("Different Height");
            if (writeableBitmap.GetBytesPerPixel() != pixel.BytesPerPixel) throw new ArgumentException("Different BytesPerPixel");

            writeableBitmap.WritePixels(
                new System.Windows.Int32Rect(0, 0, pixel.Width, pixel.Height),
                pixel.PixelsPtr, pixel.Stride * pixel.Height, pixel.Stride);

            if (isFreeze) writeableBitmap.Freeze();
        }

        /// <summary>System.Windows.Media.Imaging.WriteableBitmap に変換します</summary>
        public static System.Windows.Media.Imaging.WriteableBitmap ToWriteableBitmap(in this Pixel3Matrix pixel, bool isFreeze = false)
        {
            if (pixel.IsInvalid) throw new ArgumentException("Invalid ImagePixels");
            if (pixel.BytesPerPixel != Pixel3Matrix.Channel) throw new NotSupportedException("Invalid BytesPerPixel");

            var writeableBitmap = new System.Windows.Media.Imaging.WriteableBitmap(
                pixel.Width, pixel.Height, _dpiX, _dpiY,
                System.Windows.Media.PixelFormats.Bgr24, null);

            CopyTo(writeableBitmap, pixel, isFreeze);
            return writeableBitmap;
        }
        #endregion

    }
}
