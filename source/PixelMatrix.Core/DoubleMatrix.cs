using System;
using System.Runtime.InteropServices;
using PixelMatrix.Core.Extensions;
using PixelMatrix.Core.Interfaces;

namespace PixelMatrix.Core
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 24)]
    public readonly struct DoubleMatrix : IEquatable<DoubleMatrix>, IMatrix<double>
    {
        public const int DataSize = sizeof(double);

        private readonly IntPtr _pointer;
        //private readonly int _allocSize; //= Height * Stride;
        private readonly int _width;
        private readonly int _height;
        private readonly int _bytesPerData;
        private readonly int _stride;

        public DoubleMatrix(int width, int height, int bytesPerData, int stride, IntPtr intPtr)
        {
            if (IntPtr.Size != 8) throw new NotSupportedException();

            _width = width;
            _height = height;
            _bytesPerData = bytesPerData;
            _stride = stride;
            _pointer = intPtr;
        }

        #region IMatrix<T>
        public int Columns => _width;
        public int Rows => _height;
        public IntPtr Pointer => _pointer;
        public int Width => _width;
        public int Height => _height;
        public int BytesPerData => _bytesPerData;
        public int BitsPerData => _bytesPerData * 8;
        public int Stride => _stride;
        #endregion

        #region MatrixExtension
        public int AllocatedSize => this.GetAllocatedSize<DoubleMatrix, double>();
        public bool IsContinuous => this.IsContinuous<DoubleMatrix, double>();
        public bool IsValid => this.IsValid<DoubleMatrix, double>();
        public bool IsInvalid => !IsValid;
        public Span<double> GetRowSpan(int row) => this.GetRowSpan<DoubleMatrix, double>(row);
        public ReadOnlySpan<double> GetRoRowSpan(int row) => this.GetRoRowSpan<DoubleMatrix, double>(row);
        #endregion

        #region IEquatable<T>
        public bool Equals(DoubleMatrix other) => this == other;
        public override bool Equals(object? obj) => (obj is DoubleMatrix other) && Equals(other);
        public override int GetHashCode() => HashCode.Combine(_pointer, _width, _height, _bytesPerData, _stride);
        public static bool operator ==(in DoubleMatrix left, in DoubleMatrix right)
             => (left._pointer, left._width, left._height, left._bytesPerData, left._stride)
                == (right._pointer, right._width, right._height, right._bytesPerData, right._stride);

        public static bool operator !=(in DoubleMatrix left, in DoubleMatrix right) => !(left == right);
        #endregion

    }
}
