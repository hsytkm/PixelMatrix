using System;
using System.Runtime.InteropServices;

namespace PixelMatrixLibrary.Core
{
    public class PixelMatrixContainer : IDisposable
    {
        private const int PixelChannels = 3;

        public readonly PixelMatrix PixelMatrix;
        private readonly IntPtr _allocatedMemoryPointer;
        private readonly int _allocatedSize;
        private bool _disposedValue;

        private PixelMatrixContainer(int width, int height, int bytesPerPixels)
        {
            var stride = width * bytesPerPixels;
            var size = stride * height;

            _allocatedSize = size;
            _allocatedMemoryPointer = Marshal.AllocHGlobal(size);
            GC.AddMemoryPressure(size);

            PixelMatrix = new PixelMatrix(width, height, bytesPerPixels, stride, _allocatedMemoryPointer, size);
        }

        public PixelMatrixContainer(int width, int height) : this(width, height, PixelChannels) { }

        #region IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (_disposedValue) return;

            if (disposing)
            {
                // TODO: マネージド状態を破棄します (マネージド オブジェクト)
            }

            // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
            Marshal.FreeHGlobal(_allocatedMemoryPointer);
            GC.RemoveMemoryPressure(_allocatedSize);

            // TODO: 大きなフィールドを null に設定します

            _disposedValue = true;
        }

        // TODO: 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
        ~PixelMatrixContainer()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}
