// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Buffers;
using Garnet.server;
using Newtonsoft.Json.Linq;

namespace GarnetJSON
{
    public class JsonObjectFactory : CustomObjectFactory
    {
        public override CustomObjectBase Create(byte type)
            => new JsonObject(type);

        public override CustomObjectBase Deserialize(byte type, BinaryReader reader)
            => new JsonObject(type, reader);
    }

    public class JsonObject : CustomObjectBase
    {
        //readonly Dictionary<string, object> dict;
        readonly JObject jObject;

        public JsonObject(byte type)
            : base(type, 0, MemoryUtils.DictionaryOverhead)
        {
            jObject = new();
            // TODO: update size
        }

        public JsonObject(byte type, BinaryReader reader)
            : base(type, reader, MemoryUtils.DictionaryOverhead)
        {
            jObject = new(reader.ReadString());
            // TODO: update size
        }

        public JsonObject(JsonObject obj)
            : base(obj)
        {
            jObject = obj.jObject;
        }

        public override CustomObjectBase CloneObject() => new JsonObject(this);

        public override void SerializeObject(BinaryWriter writer)
        {
            writer.Write(jObject.ToString());
        }

        public override void Dispose()
        {
        }

        public override void Operate(byte subCommand, ReadOnlySpan<byte> input, ref (IMemoryOwner<byte>, int) output, out bool removeKey) => throw new NotImplementedException();

        public override unsafe void Scan(long start, out List<byte[]> items, out long cursor, int count = 10, byte* pattern = null, int patternLength = 0) => throw new NotImplementedException();

        public void Set(string path, string value) => jObject.SelectToken(path)?.Replace(value);

        internal string Get(string path) => jObject.ToString();
    }
}