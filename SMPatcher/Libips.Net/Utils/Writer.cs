using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
namespace CodeIsle.LibIpsNet.Utils
{
    public class Writer
    {
        // Helper to write 8bit.
        public static void Write8(byte value, Stream stream)
        {
            stream.WriteByte(value);
        }
        // Helper to write 16bit.
        public static void Write16(int value, Stream stream)
        {
            Write8((byte)(value >> 8), stream);
            Write8((byte)(value), stream);
        }
        // Helper to write 24bit.
        public static void Write24(int value, Stream stream)
        {
            Write8((byte)(value >> 16), stream);
            Write8((byte)(value >> 8), stream);
            Write8((byte)(value), stream);
        }
    }
}
