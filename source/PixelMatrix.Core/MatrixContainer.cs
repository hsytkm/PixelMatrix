using System;
using System.Runtime.InteropServices;
using PixelMatrix.Core.Interfaces;

namespace PixelMatrix.Core
{
    public abstract class MatrixContainerBase<TMatrix, TValue> : IDisposable
        where TMatrix : IMatrix<TValue> where TValue : struct
    {
        public TMatrix Matrix { get; }
        private readonly IntPtr _allocatedMemoryPointer;
        private readonly int _allocatedSize;

        public MatrixContainerBase(int width, int height, int bytesPerData)
        {
            var stride = width * bytesPerData;

            _allocatedSize = stride * height;
            _allocatedMemoryPointer = Marshal.AllocCoTaskMem(_allocatedSize);
            GC.AddMemoryPressure(_allocatedSize);

            Matrix = CreateMatrix(width, height, bytesPerData, stride, _allocatedMemoryPointer);
        }

        protected abstract TMatrix CreateMatrix(int width, int height, int bytesPerData, int stride, IntPtr intPtr);

        #region IDisposable
        private bool _disposedValue;
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
        ~MatrixContainerBase()
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

    public class PixelMatrixContainer : MatrixContainerBase<Pixel3Matrix, Pixel3ch>
    {
        public PixelMatrixContainer(int width, int height)
            : base(width, height, Pixel3Matrix.Channel)
        { }

        protected override Pixel3Matrix CreateMatrix(int width, int height, int bytesPerData, int stride, IntPtr intPtr)
            => new Pixel3Matrix(width, height, bytesPerData, stride, intPtr);
    }

    public class DoubleMatrixContainer : MatrixContainerBase<DoubleMatrix, double>
    {
        public DoubleMatrixContainer(int width, int height)
            : base(width, height, sizeof(double))
        { }

        protected override DoubleMatrix CreateMatrix(int width, int height, int bytesPerData, int stride, IntPtr intPtr)
            => new DoubleMatrix(width, height, bytesPerData, stride, intPtr);
    }

}
