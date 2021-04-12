﻿using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PixelMatrixLibrary.Core
{
    public class UnsafeHelper
    {
        //[DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory", SetLastError = false)]
        //private static extern void RtlMoveMemory(IntPtr dest, IntPtr src, [MarshalAs(UnmanagedType.U4)] int length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void MemCopy(IntPtr dest, IntPtr src, int length) => InternalMemCopy(dest.ToPointer(), src.ToPointer(), length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void MemCopy(void* dest, void* src, int length) => InternalMemCopy(dest, src, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void InternalMemCopy(void* dest, void* src, int length)
        {
            byte* destPtr = (byte*)dest;
            byte* srcPtr = (byte*)src;
            var tail = destPtr + length;

            while (destPtr + 7 < tail)
            {
                *(ulong*)destPtr = *(ulong*)src;
                srcPtr += 8;
                destPtr += 8;
            }

            if (destPtr + 3 < tail)
            {
                *(uint*)destPtr = *(uint*)src;
                srcPtr += 4;
                destPtr += 4;
            }

            while (destPtr < tail)
            {
                *destPtr = *srcPtr;
                ++srcPtr;
                ++destPtr;
            }
        }

        /// <summary>構造体を byte[] に書き出します</summary>
        public static void CopyStructToArray<T>(T srcData, Span<byte> destArray) where T : unmanaged
        {
            // unsafe is faster than Marshal.Copy and GCHandle.
            // https://gist.github.com/hsytkm/55b9bdfaa3eae18fcc1b91449cf16998

            var size = Marshal.SizeOf<T>();
            if (size > destArray.Length) throw new ArgumentOutOfRangeException();

            unsafe
            {
                fixed (byte* p = destArray)
                {
                    *(T*)p = srcData;
                }
            }
        }
    }
}