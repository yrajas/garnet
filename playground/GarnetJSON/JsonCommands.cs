// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Buffers;
using System.Diagnostics;
using System.Text;
using Garnet.server;
using Tsavorite.core;

namespace GarnetJSON
{
    class JsonSET : CustomObjectFunctions
    {
        public override bool CopyUpdater(ReadOnlyMemory<byte> key, ReadOnlySpan<byte> input, IGarnetObject oldValue, IGarnetObject newValue, ref (IMemoryOwner<byte>, int) output, ref RMWInfo rmwInfo) => UpdateJson(input, newValue);

        public override bool InitialUpdater(ReadOnlyMemory<byte> key, ReadOnlySpan<byte> input, IGarnetObject jsonObject, ref (IMemoryOwner<byte>, int) output, ref RMWInfo rmwInfo) => UpdateJson(input, jsonObject);

        public override bool InPlaceUpdater(ReadOnlyMemory<byte> key, ReadOnlySpan<byte> input, IGarnetObject value, ref (IMemoryOwner<byte>, int) output, ref RMWInfo rmwInfo) => UpdateJson(input, value);

        public override bool NeedInitialUpdate(ReadOnlyMemory<byte> key, ReadOnlySpan<byte> input, ref (IMemoryOwner<byte>, int) output) => true;

        public override bool Reader(ReadOnlyMemory<byte> key, ReadOnlySpan<byte> input, IGarnetObject value, ref (IMemoryOwner<byte>, int) output, ref ReadInfo readInfo) => throw new NotImplementedException();

        private static bool UpdateJson(ReadOnlySpan<byte> input, IGarnetObject jsonObject)
        {
            Debug.Assert(jsonObject is JsonObject);

            int offset = 0;
            var path = CustomCommandUtils.GetNextArg(input, ref offset).ToString();
            var value = CustomCommandUtils.GetNextArg(input, ref offset).ToString();

            ((JsonObject)jsonObject).Set(path, value);
            return true;
        }
    }

    class JsonGET : CustomObjectFunctions
    {
        public override bool CopyUpdater(ReadOnlyMemory<byte> key, ReadOnlySpan<byte> input, IGarnetObject oldValue, IGarnetObject newValue, ref (IMemoryOwner<byte>, int) output, ref RMWInfo rmwInfo) => throw new NotImplementedException();
        public override bool InitialUpdater(ReadOnlyMemory<byte> key, ReadOnlySpan<byte> input, IGarnetObject value, ref (IMemoryOwner<byte>, int) output, ref RMWInfo rmwInfo) => throw new NotImplementedException();
        public override bool InPlaceUpdater(ReadOnlyMemory<byte> key, ReadOnlySpan<byte> input, IGarnetObject value, ref (IMemoryOwner<byte>, int) output, ref RMWInfo rmwInfo) => throw new NotImplementedException();
        public override bool NeedInitialUpdate(ReadOnlyMemory<byte> key, ReadOnlySpan<byte> input, ref (IMemoryOwner<byte>, int) output) => throw new NotImplementedException();

        public override bool Reader(ReadOnlyMemory<byte> key, ReadOnlySpan<byte> input, IGarnetObject value, ref (IMemoryOwner<byte>, int) output, ref ReadInfo readInfo)
        {
            Debug.Assert(value is JsonObject);

            int offset = 0;
            var path = CustomCommandUtils.GetNextArg(input, ref offset).ToString();

            var result = ((JsonObject)value).Get(path);
            CustomCommandUtils.WriteBulkString(ref output, Encoding.UTF8.GetBytes(result));
            return true;
        }
    }
}