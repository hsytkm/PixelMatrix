using System;

namespace PixelMatrix.Core.Interfaces
{
    public interface IMatrixContainer<TMatrix, TValue>
        where TMatrix : IMatrix<TValue>
        where TValue : struct
    {
        TMatrix Matrix { get; }
    }
}
