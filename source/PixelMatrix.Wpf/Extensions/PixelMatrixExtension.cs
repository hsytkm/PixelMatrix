using PixelMatrixLibrary.Core;
using System;

namespace PixelMatrixLibrary.Wpf.Extensions
{
    public static class PixelMatrixExtension
    {
        private const double _dpiX = 96.0;
        private const double _dpiY = _dpiX;

        #region ToBitmapSource
        /// <summary>System.Windows.Media.Imaging.BitmapSource に変換します</summary>
        public static System.Windows.Media.Imaging.BitmapSource ToBitmapSource(in this PixelMatrix pixel)
        {
            if (pixel.IsInvalid) throw new ArgumentException("Invalid ImagePixels");
            if (pixel.BytesPerPixel != 3) throw new NotSupportedException("Invalid BytesPerPixel");

            var bitmapSource = System.Windows.Media.Imaging.BitmapSource.Create(
                pixel.Width, pixel.Height, _dpiX, _dpiY,
                System.Windows.Media.PixelFormats.Bgr24, null,
                pixel.PixelsPtr, pixel.Height * pixel.Stride, pixel.Stride);

            bitmapSource.Freeze();
            return bitmapSource;
        }
        #endregion

        #region ToWriteableBitmap
        /// <summary>System.Windows.Media.Imaging.WriteableBitmap の画素値を更新します(遅いです)</summary>
        public static void CopyTo(in this PixelMatrix pixel, System.Windows.Media.Imaging.WriteableBitmap writeableBitmap)
        {
            if (pixel.IsInvalid) throw new ArgumentException("Invalid Image");

            if (writeableBitmap.IsFrozen) throw new ArgumentException("WriteableBitmap is frozen");
            if (writeableBitmap.IsInvalid()) throw new ArgumentException("Invalid Image");
            if (writeableBitmap.PixelWidth != pixel.Width) throw new ArgumentException("Different Width");
            if (writeableBitmap.PixelHeight != pixel.Height) throw new ArgumentException("Different Height");
            if (writeableBitmap.GetBytesPerPixel() != pixel.BytesPerPixel) throw new ArgumentException("Different BytesPerPixel");

            writeableBitmap.WritePixels(
                new System.Windows.Int32Rect(0, 0, pixel.Width, pixel.Height),
                pixel.PixelsPtr, pixel.AllocSize, pixel.Stride);

            //writeableBitmap.Freeze();
        }

        /// <summary>System.Windows.Media.Imaging.WriteableBitmap に変換します</summary>
        public static System.Windows.Media.Imaging.WriteableBitmap ToWriteableBitmap(in this PixelMatrix pixel, bool isFreeze = false)
        {
            if (pixel.IsInvalid) throw new ArgumentException("Invalid ImagePixels");
            if (pixel.BytesPerPixel != 3) throw new NotSupportedException("Invalid BytesPerPixel");

            var writeableBitmap = new System.Windows.Media.Imaging.WriteableBitmap(
                pixel.Width, pixel.Height, _dpiX, _dpiY,
                System.Windows.Media.PixelFormats.Bgr24, null);

            pixel.CopyTo(writeableBitmap);

            if (isFreeze) writeableBitmap.Freeze();
            return writeableBitmap;
        }
        #endregion

    }
}
