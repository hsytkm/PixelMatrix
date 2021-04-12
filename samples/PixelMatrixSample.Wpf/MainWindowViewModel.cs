using PixelMatrixLibrary.Wpf.Extensions;
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
            var bitmapImage = Extensions.BitmapSourceExtension.FromFile(@"Asserts\image1.bmp");
            SourceImage = new ReactivePropertySlim<BitmapSource>(initialValue: bitmapImage);

            using var pixelContainr = bitmapImage.ToPixelMatrixContainer();
            var pixelMatrix = pixelContainr.PixelMatrix;
            var channelAverage1 = pixelMatrix.GetChannelsAverage();

            pixelMatrix.FillPixels(0x80, 0x00, 0x40);

            var channelAverage2 = pixelMatrix.GetChannelsAverage();

            var writableBitmap = pixelMatrix.ToWriteableBitmap();
            WriteableImage = new ReactivePropertySlim<WriteableBitmap>(initialValue: writableBitmap);

        }
    }
}
