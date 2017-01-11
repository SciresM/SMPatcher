using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeIsle.LibIpsNet.Utils;
using System.IO;
namespace CodeIsle.LibIpsNet
{
    public class Creator
    {

        // Known situations where this function does not generate an optimal patch:
        // In:  80 80 80 80 80 80 80 80 80 80 80 80 80 80 80 80 80 80 80 80 80 80 80 80
        // Out: FF FF FF FF FF FF FF FF 00 01 02 03 04 05 06 07 FF FF FF FF FF FF FF FF
        // IPS: [         RLE         ] [        Copy         ] [         RLE         ]
        // Possible improvement: RLE across the entire file, copy on top of that.
        // Rationale: It would be a huge pain to create such a multi-pass tool if it should support writing a byte
        // more than twice, and I don't like half-assing stuff.


        // Known improvements over LIPS:
        // In:  00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F
        // Out: FF 01 02 03 04 05 FF FF FF FF FF FF FF FF FF FF
        // LIPS:[      Copy     ] [            RLE            ]
        // Mine:[] [ Unchanged  ] [            RLE            ]
        // Rationale: While LIPS can break early if it finds something RLEable in the middle of a block, it's not
        // smart enough to back off if there's something unchanged between the changed area and the RLEable spot.

        // In:  FF FF FF FF FF FF FF
        // Out: 00 00 00 00 01 02 03
        // LIPS:[   RLE   ] [ Copy ]
        // Mine:[       Copy       ]
        // Rationale: Again, RLE is no good at RLE.

        // It is also known that I win in some other situations. I didn't bother checking which, though.

        // There are no known cases where LIPS wins over libips.

        /// <summary>
        /// Creates an IPS patch file from a source file path and a target file path.
        /// </summary>
        /// <param name="source">The source file that contains the original data.</param>
        /// <param name="target">The target file that contains the modified data.</param>
        /// <param name="patch">The patch file to contain the resulting patch data.</param>
        /// <returns></returns>
        public void Create(string source, string target, string patch)
        {
            using (FileStream sourceStream = new FileStream(source, FileMode.Open), targetStream = new FileStream(target, FileMode.Open), patchStream = new FileStream(patch, FileMode.Create))
            {
                Create(sourceStream, targetStream, patchStream);
            }
        }
        /// <summary>
        /// Creates an IPS patch stream from a source stream and a target stream.
        /// </summary>
        /// <param name="source">The source stream that contains the original data.</param>
        /// <param name="target">The target stream that contains the modified data.</param>
        /// <param name="patch">The patch stream to contain the resulting patch data.</param>
        /// <returns></returns>
        public void Create(Stream source, Stream target, Stream patch)
        {
            long sourcelen = source.Length;
            long targetlen = target.Length;

            bool sixteenmegabytes = false;


            if (sourcelen > 16777216)
            {
                sourcelen = 16777216;
                sixteenmegabytes = true;
            }
            if (targetlen > 16777216)
            {
                targetlen = 16777216;
                sixteenmegabytes = true;
            }

            int offset = 0;

            {

                Writer.Write8((byte)'P', patch);
                Writer.Write8((byte)'A', patch);
                Writer.Write8((byte)'T', patch);
                Writer.Write8((byte)'C', patch);
                Writer.Write8((byte)'H', patch);

                int lastknownchange = 0;
                while (offset < targetlen)
                {
                    while (offset < sourcelen && (offset < sourcelen ? Reader.Read8(source, offset) : 0) == Reader.Read8(target, offset)) offset++;

                    // Check how much we need to edit until it starts getting similar.
                    int thislen = 0;
                    int consecutiveunchanged = 0;
                    thislen = lastknownchange - offset;
                    if (thislen < 0) thislen = 0;

                    while (true)
                    {
                        int thisbyte = offset + thislen + consecutiveunchanged;
                        if (thisbyte < sourcelen && (thisbyte < sourcelen ? Reader.Read8(source, thisbyte) : 0) == Reader.Read8(target, thisbyte)) consecutiveunchanged++;
                        else
                        {
                            thislen += consecutiveunchanged + 1;
                            consecutiveunchanged = 0;
                        }
                        if (consecutiveunchanged >= 6 || thislen >= 65536) break;
                    }

                    // Avoid premature EOF.
                    if (offset == Patcher.EndOfFile)
                    {
                        offset--;
                        thislen++;
                    }

                    lastknownchange = offset + thislen;
                    if (thislen > 65535) thislen = 65535;
                    if (offset + thislen > targetlen) thislen = (int)(targetlen - offset);
                    if (offset == targetlen) continue;

                    // Check if RLE here is worthwhile.
                    int byteshere = 0;

                    for (byteshere = 0; byteshere < thislen && Reader.Read8(target, offset) == Reader.Read8(target, (offset + byteshere)); byteshere++) { }


                    if (byteshere == thislen)
                    {
                        int thisbyte = Reader.Read8(target, offset);
                        int i = 0;

                        while (true)
                        {
                            int pos = offset + byteshere + i - 1;
                            if (pos >= targetlen || Reader.Read8(target, pos) != thisbyte || byteshere + i > 65535) break;
                            if (pos >= sourcelen || (pos < sourcelen ? Reader.Read8(source, pos) : 0) != thisbyte)
                            {
                                byteshere += i;
                                thislen += i;
                                i = 0;
                            }
                            i++;
                        }

                    }
                    if ((byteshere > 8 - 5 && byteshere == thislen) || byteshere > 8)
                    {
                        Writer.Write24(offset, patch);
                        Writer.Write16(0, patch);
                        Writer.Write16(byteshere, patch);
                        Writer.Write8(Reader.Read8(target, offset), patch);
                        offset += byteshere;
                    }
                    else
                    {
                        // Check if we'd gain anything from ending the block early and switching to RLE.
                        byteshere = 0;
                        int stopat = 0;

                        while (stopat + byteshere < thislen)
                        {
                            if (Reader.Read8(target, (offset + stopat)) == Reader.Read8(target, (offset + stopat + byteshere))) byteshere++;
                            else
                            {
                                stopat += byteshere;
                                byteshere = 0;
                            }
                            // RLE-worthy despite two IPS headers.
                            if (byteshere > 8 + 5 ||
                                // RLE-worthy at end of data.
                                    (byteshere > 8 && stopat + byteshere == thislen) ||
                                    (byteshere > 8 && Compare(target, (offset + stopat + byteshere), target, (offset + stopat + byteshere + 1), 9 - 1)))//rle-worthy before another rle-worthy
                            {
                                if (stopat != 0) thislen = stopat;
                                // We don't scan the entire block if we know we'll want to RLE, that'd gain nothing.
                                break;
                            }
                        }


                        // Don't write unchanged bytes at the end of a block if we want to RLE the next couple of bytes.
                        if (offset + thislen != targetlen)
                        {
                            while (offset + thislen - 1 < sourcelen && Reader.Read8(target, (offset + thislen - 1)) == (offset + thislen - 1 < sourcelen ? Reader.Read8(source, (offset + thislen - 1)) : 0)) thislen--;
                        }
                        if (thislen > 3 && Compare(target, offset, target, (offset + 1), (thislen - 2)))
                        {
                            Writer.Write24(offset, patch);
                            Writer.Write16(0, patch);
                            Writer.Write16(thislen, patch);
                            Writer.Write8(Reader.Read8(target, offset), patch);
                        }
                        else
                        {
                            Writer.Write24(offset, patch);
                            Writer.Write16(thislen, patch);
                            int i;
                            for (i = 0; i < thislen; i++)
                            {
                                Writer.Write8(Reader.Read8(target, (offset + i)), patch);
                            }
                        }
                        offset += thislen;

                    }
                }



                Writer.Write8((byte)'E', patch);
                Writer.Write8((byte)'O', patch);
                Writer.Write8((byte)'F', patch);

                if (sourcelen > targetlen) Writer.Write24((int)targetlen, patch);

                if (sixteenmegabytes) throw new Exceptions.Ips16MegabytesException(); ;
                if (patch.Length == 8) throw new Exceptions.IpsIdenticalException();
            }

        }


        // Helper to Compare two BinaryReaders with a starting point and a count of elements.
        private bool Compare(Stream source, int sourceStart, Stream target, int targetStart, int count)
        {
            source.Seek(sourceStart, SeekOrigin.Begin);
            byte[] sourceData = new byte[count];
            source.Read(sourceData, 0, count);

            target.Seek(targetStart, SeekOrigin.Begin);
            byte[] targetData = new byte[count];
            target.Read(targetData, 0, count);

            for (int i = 0; i < count; i++)
            {
                if (sourceData[i] != targetData[i]) return false;
            }
            return true;
        }
    }
}
