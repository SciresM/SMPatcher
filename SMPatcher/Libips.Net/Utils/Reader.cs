using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
namespace CodeIsle.LibIpsNet.Utils
{
    public class Reader
    {
        // Helper to read 8 bit.
        public static byte Read8(Stream stream, int offset = -1)
        {
            if (offset != -1 && stream.Position != offset)
            {
                stream.Seek(offset, SeekOrigin.Begin);
            }
            if (stream.Position < stream.Length)
            {
                return (byte)stream.ReadByte();
            }
            else
            {
                return 0;
            }
        }
        // Helper to read 16bit.
        public static int Read16(Stream stream)
        {
            if (stream.Position + 1 < stream.Length)
            {
                byte[] data = new byte[2];
                stream.Read(data, 0, 2);
                return (data[0] << 8) | data[1];
            }
            else
            {
                return 0;
            }
        }
        // Helper to read 24bit.
        public static int Read24(Stream stream)
        {
            if (stream.Position + 1 < stream.Length)
            {
                byte[] data = new byte[3];
                stream.Read(data, 0, 3);
                return (data[0] << 16) | (data[1] << 8) | data[2];
            }
            else
            {
                return 0;
            }
        }
    }
}
