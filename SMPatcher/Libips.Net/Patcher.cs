using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CodeIsle.LibIpsNet.Utils;
namespace CodeIsle.LibIpsNet
{
    public class Patcher
    {
        public const string PatchText = "PATCH";
        public const int EndOfFile = 0x454F46;
        /// <summary>
        /// Studies and patches a file.
        /// </summary>
        /// <param name="patch">The patch file to study.</param>
        /// <param name="study">The study struct to use for patching.</param>
        /// <param name="source">The unpatched source file.</param>
        /// <param name="target">The target file to copy the source file to, but with the patch applied.</param>
        public void PatchStudy(string patch, Studier.IpsStudy study, string source, string target)
        {
            using (FileStream patchStream = File.OpenRead(patch), sourceStream = File.OpenRead(source), targetStream = File.Open(target, FileMode.Create))
            {
                PatchStudy(patchStream, study, sourceStream, targetStream);
            }
        }
        /// <summary>
        /// Studies and patches a stream.
        /// </summary>
        /// <param name="patch">The patch stream to study.</param>
        /// <param name="study">The study struct to use for patching.</param>
        /// <param name="source">The unpatched source stream.</param>
        /// <param name="target">The target stream to copy the source stream to, but with the patch applied.</param>
        public void PatchStudy(Stream patch, Studier.IpsStudy study, Stream source, Stream target)
        {
            source.CopyTo(target);
            long sourceLength = source.Length;
            if (study.Error == Studier.IpsError.IpsInvalid) throw new Exceptions.IpsInvalidException();
            int outlen = (int)Clamp(target.Length, study.OutlenMin, study.OutlenMax);
            // Set target file length to new size.
            target.SetLength(outlen);

            // Skip PATCH text.
            patch.Seek(5, SeekOrigin.Begin);
            int offset = Reader.Read24(patch);
            while (offset != EndOfFile)
            {
                int size = Reader.Read16(patch);


                target.Seek(offset, SeekOrigin.Begin);
                // If RLE patch.
                if (size == 0)
                {
                    size = Reader.Read16(patch);
                    target.Write(Enumerable.Repeat<byte>(Reader.Read8(patch), offset).ToArray(), 0, offset);
                }
                // If normal patch.
                else
                {
                    byte[] data = new byte[size];
                    patch.Read(data, 0, size);
                    target.Write(data, 0, size);

                }
                offset = Reader.Read24(patch);
            }
            if (study.OutlenMax != 0xFFFFFFFF && sourceLength <= study.OutlenMax) throw new Exceptions.IpsNotThisException(); // Truncate data without this being needed is a poor idea.
        }
        /// <summary>
        /// Patches a file.
        /// </summary>
        /// <param name="patch">The patch file.</param>
        /// <param name="source">The unpatched source file.</param>
        /// <param name="target">The target file to copy the source file to, but with the patch applied.</param>
        public void Patch(string patch, string source, string target)
        {
            using (FileStream patchStream = File.Open(patch, FileMode.Open, FileAccess.Read, FileShare.None), sourceStream = File.Open(source, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), targetStream = File.Open(target, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
            {
                Patch(patchStream, sourceStream, targetStream);
            }
        }
        /// <summary>
        /// Patches a stream.
        /// </summary>
        /// <param name="patch">The patch stream.</param>
        /// <param name="source">The unpatched source stream.</param>
        /// <param name="target">The target stream to write the source stream to, but with the patch applied.</param>
        public void Patch(Stream patch, Stream source, Stream target)
        {
            Studier studier = new Studier();
            Studier.IpsStudy study = studier.Study(patch);
            PatchStudy(patch, study, source, target);
        }
        private static long Clamp(long value, long minimum, long maximum)
        {
            return (value < minimum) ? minimum : (value > maximum) ? maximum : value;
        }
    }
}
