using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using SMPatcher.Properties;

namespace SMPatcher
{
    class Program
    {
        static void Main(string[] args)
        {
            uint DebugStub = 0x4DE0;
            uint QRDecryptHookOne = 0x2CD558;
            uint QRDecryptHookTwo = 0x2CD5F0;
            uint ScanHookOne = 0x2CD8AC;
            uint BatterySave = 0x33DA80;
            uint QRRegistered = 0x3A7008;

            uint ForbiddenQRs = 0x4A3FF0;

            uint NoOutlines = 0x31B748;

            if (args.Length != 1)
            {
                Console.WriteLine("Usage: SMPatcher code.bin");
                return;
            }

            byte[] code;

            try
            {
                code = File.ReadAllBytes(args[0]);
            }
            catch
            {
                Console.WriteLine("Failed to open {0}!", args[0]);
                return;
            }

            var dir = Path.GetDirectoryName(args[0]);
            var fn = Path.GetFileNameWithoutExtension(args[0]);

            Console.WriteLine("Sun/Moon Patcher v1 - SciresM");

            var hash = (new SHA256CryptoServiceProvider()).ComputeHash(code);
            if (hash.SequenceEqual(Resources.moon_hash))
                Console.WriteLine("Pokemon Moon detected.");
            else if (hash.SequenceEqual(Resources.sun_hash))
                Console.WriteLine("Pokemon Sun detected");
            else
            {
                Console.WriteLine("Unknown code.bin! Contact SciresM to update the program.");
                return;
            }

            Resources.debug_stub.CopyTo(code, DebugStub);
            Resources.battery_save.CopyTo(code, BatterySave);
            Resources.qr_is_registered.CopyTo(code, QRRegistered);
            new byte[0x24].CopyTo(code, ForbiddenQRs);

            // Hooks
            BitConverter.GetBytes(0xEAF4DE23).CopyTo(code, QRDecryptHookOne);
            BitConverter.GetBytes(0xEAF4DE07).CopyTo(code, QRDecryptHookTwo);
            BitConverter.GetBytes(0xEA01C07B).CopyTo(code, ScanHookOne);

            Console.WriteLine("Patched!");
            var new_fn = Path.Combine(dir, fn + "_patched.bin");
            File.WriteAllBytes(new_fn, code);
            Console.WriteLine("Saved to {0}!", new_fn);

            BitConverter.GetBytes(0xE320F000).CopyTo(code, NoOutlines);
            var nfn2 = Path.Combine(dir, fn + "_patched_nooutlines.bin");
            File.WriteAllBytes(nfn2, code);

        }
    }
}
