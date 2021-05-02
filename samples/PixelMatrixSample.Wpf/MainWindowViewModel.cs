using PixelMatrixLibrary.Core;
using PixelMatrixLibrary.Wpf.Extensions;
using PixelMatrixSample.Wpf.Extensions;
using Prism.Mvvm;
using Reactive.Bindings;
using System;
using System.Diagnostics;
using System.Windows.Media.Imaging;

namespace PixelMatrixSample.Wpf
{
    class MainWindowViewModel : BindableBase
    {
        public IReadOnlyReactiveProperty<BitmapSource> SourceImage { get; }
        public IReactiveProperty<WriteableBitmap> WriteableImage { get; }

        public MainWindowViewModel()
        {
            var bitmapImage = BitmapSourceExtension.FromFile(@"Asserts\image1.bmp");
            SourceImage = new ReactivePropertySlim<BitmapSource>(initialValue: bitmapImage);

            using var pixelContainer = bitmapImage.ToPixelMatrixContainer();
            var fullPixelMatrix = pixelContainer.FullPixels;

            // 元画像の画素値平均
            var channelAverage1 = fullPixelMatrix.GetChannelsAverageOfEntire();

            // 1. 三角領域を指定色で指定塗り
            FillTriangle(fullPixelMatrix);

            // 2. 上部を切り出して指定塗り
            var headerPixelMatrix = fullPixelMatrix.CutOutPixelMatrix(0, 0, fullPixelMatrix.Width, 30);
            headerPixelMatrix.FillAllPixels(Pixel3ch.Gray);
            var headerChannelAverage2 = headerPixelMatrix.GetChannelsAverageOfEntire();

            // 3. 上部を除いた左部を切り出してグレスケ塗り
            var leftPixelMatrix = fullPixelMatrix.CutOutPixelMatrix(0, headerPixelMatrix.Height, 50, fullPixelMatrix.Height - headerPixelMatrix.Height);
            FillGrayScaleVertical(leftPixelMatrix);

            // BitmapSourceに変換してView表示
            var writableBitmap = fullPixelMatrix.ToWriteableBitmap();
            WriteableImage = new ReactivePropertySlim<WriteableBitmap>(initialValue: writableBitmap);
        }

        // 三角領域を単色で塗り(WritePixelのテスト)
        static void FillTriangle(in PixelMatrix pixelMatrix)
        {
            int baseX = 100, baseY = 200, height = 100;
            var color = new Pixel3ch(0, 0xff, 0);

            for (int y = 0; y < height; y++)
            {
                for (int x = baseX; x < baseX + y; x++)
                    pixelMatrix.WritePixel(color, x, baseY + y);    // ホントは FillRectangle() を使うべきだけど、WritePixel() のテストなので。
            }
        }

        // 垂直方向で階調が変化するグレー塗り
        static void FillGrayScaleVertical(in PixelMatrix pixelMatrix)
        {
            const int range = 256;
            var length = pixelMatrix.Height / range;

            if (length > 0)
            {
                for (int lv = 0; lv < range; ++lv)
                {
                    var color = new Pixel3ch((byte)(lv & 0xff));
                    pixelMatrix.FillRectangle(color, 0, lv * length, pixelMatrix.Width, length);
                }
            }

            var filledHeight = length * range;
            pixelMatrix.FillRectangle(Pixel3ch.Black, 0, filledHeight, pixelMatrix.Width, pixelMatrix.Height - filledHeight);
        }

    }
}
