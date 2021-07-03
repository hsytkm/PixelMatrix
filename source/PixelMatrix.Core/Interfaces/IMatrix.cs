using System;

namespace PixelMatrix.Core.Interfaces
{
    public interface IMatrix<TValue>
        where TValue : struct
    {
        IntPtr Pointer { get; }
        int Width { get; }
        int Height { get; }
        int BytesPerData { get; }
        int BitsPerData { get; }
        int Stride { get; }
        Span<TValue> GetRowSpan(int row);
        ReadOnlySpan<TValue> GetRoRowSpan(int row);

    }
}
