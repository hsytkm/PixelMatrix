using System;
using PixelMatrix.Core.Interfaces;

namespace PixelMatrix.Core.Extensions
{
    // 構造体を型制約付きジェネリクスで受け取ると boxing が起こらない。  https://ikorin2.hatenablog.jp/entry/2021/05/03/172217
    public static class MatrixExtension
    {
        public static int GetAllocatedSize<TMatrix, TValue>(this TMatrix matrix)
            where TMatrix : IMatrix<TValue> where TValue : struct
        {
            return matrix.Width * matrix.BytesPerData * matrix.Height;  // Strideは見ない
        }

        public static bool IsContinuous<TMatrix, TValue>(this TMatrix matrix)
            where TMatrix : IMatrix<TValue> where TValue : struct
        {
            return (matrix.Width * matrix.BytesPerData) == matrix.Stride;
        }

        public static bool IsValid<TMatrix, TValue>(this TMatrix matrix)
            where TMatrix : IMatrix<TValue> where TValue : struct
        {
            if (matrix.Pointer == IntPtr.Zero) return false;
            if (matrix.Width <= 0 || matrix.Height <= 0) return false;
            if (matrix.Stride < matrix.Width * matrix.BytesPerData) return false;
            if (matrix.GetAllocatedSize<TMatrix, TValue>() < matrix.Width * matrix.BytesPerData * matrix.Height) return false;
            return true;    //valid
        }

        /// <summary>指定行のSpanを取得します</summary>
        public static unsafe Span<TValue> GetRowSpan<TMatrix, TValue>(this TMatrix matrix, int row)
            where TMatrix : IMatrix<TValue> where TValue : struct
        {
            if (row < 0 || matrix.Height - 1 < row)
                throw new ArgumentException("invalid row");

            var ptr = matrix.Pointer + (row * matrix.Stride);
            return new Span<TValue>(ptr.ToPointer(), matrix.Width);
        }

        /// <summary>指定行のReadOnlySpanを取得します</summary>
        public static ReadOnlySpan<TValue> GetRoRowSpan<TMatrix, TValue>(this TMatrix matrix, int row)
            where TMatrix : IMatrix<TValue> where TValue : struct
        {
            return GetRowSpan<TMatrix, TValue>(matrix, row);
        }

    }
}
