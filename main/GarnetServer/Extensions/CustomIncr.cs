// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Garnet.common;



// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using Garnet.server;
using Tsavorite.core;

namespace Garnet
{
    public class CustomIncr : CustomRawStringFunctions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool CopyUpdater(ReadOnlySpan<byte> key, ReadOnlySpan<byte> input, ReadOnlySpan<byte> oldValue, Span<byte> newValue, ref (IMemoryOwner<byte>, int) output, ref RMWInfo rmwInfo)
        {
            if (!IsValidNumber(oldValue, out var curr))
            {
                oldValue.CopyTo(newValue);
                WriteInvalidType(ref output);
                return true;
            }

            try
            {
                checked { curr++; }
            }
            catch
            {
                WriteInvalidType(ref output);
                return true;
            }

            var fNeg = false;
            var ndigits = NumUtils.NumDigitsInLong(curr, ref fNeg);
            ndigits += fNeg ? 1 : 0;

            _ = NumUtils.LongToSpanByte(curr, newValue);
            WriteNumber(ref output, newValue);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetInitialLength(ReadOnlySpan<byte> input) => 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetLength(ReadOnlySpan<byte> value, ReadOnlySpan<byte> input)
        {
            //var datalen = input.Length - RespInputHeader.Size;
            //var slicedInputData = input.Slice(RespInputHeader.Size, datalen);

            // We don't need to TryParse here because InPlaceUpdater will raise an error before we reach this point
            var curr = NumUtils.BytesToLong(value);
            var next = curr + 1;

            var fNeg = false;
            var ndigits = NumUtils.NumDigitsInLong(next, ref fNeg);
            ndigits += fNeg ? 1 : 0;

            return ndigits;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool InitialUpdater(ReadOnlySpan<byte> key, ReadOnlySpan<byte> input, Span<byte> value, ref (IMemoryOwner<byte>, int) output, ref RMWInfo rmwInfo)
        {
            NumUtils.LongToSpanByte(1, value);
            WriteNumber(ref output, value);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool InPlaceUpdater(ReadOnlySpan<byte> key, ReadOnlySpan<byte> input, Span<byte> value, ref int valueLength, ref (IMemoryOwner<byte>, int) output, ref RMWInfo rmwInfo)
        {
            if (!IsValidNumber(value, out var curr))
            {
                WriteInvalidType(ref output);
                return true;
            }

            try
            {
                checked { curr++; }
            }
            catch
            {
                WriteInvalidType(ref output);
                return true;
            }

            var fNeg = false;
            var ndigits = NumUtils.NumDigitsInLong(curr, ref fNeg);
            ndigits += fNeg ? 1 : 0;

            if (ndigits > valueLength)
                return false;

            _ = NumUtils.LongToSpanByte(curr, value);
            valueLength = ndigits;
            WriteNumber(ref output, value);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void WriteInvalidType(ref (IMemoryOwner<byte>, int) output)
        {
            output.Item1?.Dispose();
            output.Item1 = MemoryPool.Rent(1);
            output.Item2 = 1;
            fixed (byte* ptr = output.Item1.Memory.Span)
            {
                *ptr = (byte)OperationError.INVALID_TYPE;
            }
        }

        public override bool Reader(ReadOnlySpan<byte> key, ReadOnlySpan<byte> input, ReadOnlySpan<byte> value, ref (IMemoryOwner<byte>, int) output, ref ReadInfo readInfo) => throw new NotImplementedException();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void WriteNumber(ref (IMemoryOwner<byte>, int) output, Span<byte> value)
        {
            //// Get space for null bulk string "$-1\r\n"
            var len = value.Length + 3;
            output.Item1?.Dispose();
            output.Item1 = MemoryPool.Rent(len);
            output.Item2 = len;
            fixed (byte* ptr = output.Item1.Memory.Span)
            {
                var curr = ptr;
                curr[0] = (byte)':';
                curr++;
                // NOTE: Expected to always have enough space to write into pre-allocated buffer
                var success = RespWriteUtils.WriteDirect(value, ref curr, ptr + len);
                *curr = (byte)'\r';
                curr++;
                *curr = (byte)'\n';
                curr++;
                Debug.Assert(success, "Insufficient space in pre-allocated buffer");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool IsValidNumber(ReadOnlySpan<byte> source, out long val)
        {
            val = 0;
            try
            {
                // Check for valid number
                if (!NumUtils.TryBytesToLong(source, out val))
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
