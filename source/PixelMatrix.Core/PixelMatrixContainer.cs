using System;
using System.Runtime.InteropServices;

namespace PixelMatrix.Core
{
    public class PixelMatrixContainer : IDisposable
    {
        public Pixel3Matrix FullPixels { get; }
        private readonly IntPtr _allocatedMemoryPointer;
        private readonly int _allocatedSize;
        private bool _disposedValue;

        public PixelMatrixContainer(int width, int height) : this(width, height, Pixel3Matrix.Channel) { }

        private PixelMatrixContainer(int width, int height, int bytesPerPixels)
        {
            var stride = width * bytesPerPixels;

            _allocatedSize = stride * height;
            _allocatedMemoryPointer = Marshal.AllocCoTaskMem(_allocatedSize);
            GC.AddMemoryPressure(_allocatedSize);

            FullPixels = new Pixel3Matrix(width, height, bytesPerPixels, stride, _allocatedMemoryPointer);
        }

        #region IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (_disposedValue) return;

            if (disposing)
            {
                // TODO: マネージド状態を破棄します (マネージド オブジェクト)
            }

            // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
            Marshal.FreeCoTaskMem(_allocatedMemoryPointer);
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
