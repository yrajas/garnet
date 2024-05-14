// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using Embedded.perftest;
using Garnet.server;

namespace GarnetLibFuzz
{
    public class GarnetFuzzTarget
    {
        private static readonly EmbeddedRespServer server;
        //private static RespServerSession session;

        static GarnetFuzzTarget()
        {
            server = new EmbeddedRespServer(new GarnetServerOptions());

        }

        private static int CalculateLen(ReadOnlySpan<byte> data)
        {
            int total = 0;
            for (int i = 0; i < data.Length; i++) total += data[i];
            return total;
        }

        public static void GarnetFuzz(ReadOnlySpan<byte> input)
        {
            //var c = CalculateLen(input);
            //if (c == 100) throw new Exception("Invalid input");
            //Console.WriteLine(c);
            //using var session = server.GetRespSession();
        }
    }
}
