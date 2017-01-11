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
    class Offsets
    {
        public uint CTRIsDebugMode;

        public uint DecryptQRCodeStart;
        public uint DecryptQRCodeEnd;

        public uint AnalyzeQRBinaryStart;
        public uint AnalyzeQRBinaryEnd;

        public uint QRReaderSaveDataBatteryQuery;
        public uint QRReaderSaveDataIsRegisteredData;

        public uint ForbiddenQRs;
        public uint NoOutlines;

        public uint CRC16;
        public uint DexDataAllocator;
        public uint aeabi_memcpy;
        public uint GenerateDexDisplayData;

        public uint GetSingletonAccessorInstance;
    }


    class Program
    {
        private static readonly Offsets Offsets_1_0 = new Offsets
        {
            CTRIsDebugMode = 0x4DE0,

            DecryptQRCodeStart = 0x2CD558,
            DecryptQRCodeEnd = 0x2CD5F0,

            AnalyzeQRBinaryStart = 0x2CD8AC,
            AnalyzeQRBinaryEnd = 0x2CDD30,

            QRReaderSaveDataBatteryQuery = 0x33DA80,

            QRReaderSaveDataIsRegisteredData = 0x3A7008,

            ForbiddenQRs = 0x4A3FF0,

            NoOutlines = 0x31B748,

            CRC16 = 0x258E40,
            DexDataAllocator = 0x3B4968,
            aeabi_memcpy = 0x1FEC20,
            GenerateDexDisplayData = 0x2CD230,

            GetSingletonAccessorInstance = 0x48B4
        };

        private static readonly Offsets Offsets_1_1 = new Offsets
        {
            CTRIsDebugMode = 0x4DE0,

            DecryptQRCodeStart = 0x2CEA3C,
            DecryptQRCodeEnd = 0x2CEAD4,

            AnalyzeQRBinaryStart = 0x2CED90,
            AnalyzeQRBinaryEnd = 0x2CF214,
            QRReaderSaveDataBatteryQuery = 0x33F68C,

            QRReaderSaveDataIsRegisteredData = 0x3A8DA8,

            ForbiddenQRs = 0x4A5FF0,

            NoOutlines = 0x31CFCC,

            CRC16 = 0x259D14,
            DexDataAllocator = 0x3B6708,
            aeabi_memcpy = 0x1FEBD8,
            GenerateDexDisplayData = 0x2CE714,

            GetSingletonAccessorInstance = 0x48B4
        };

        static uint GetARMBranch(uint from, uint to)
        {
            // Fuck ARM Branches
            if (to >= from + 8)
            {
                return (0xEA000000 | (((to - (from + 8)) >> 2) & 0xFFFFFF));
            }
            else
            {
                return (0xEA000000 | (0x1000000 - (((from + 8) - to) >> 2)));
            }
        }

        static uint GetARMBranchNE(uint from, uint to)
        {
            // Fuck ARM Branches
            if (to >= from + 8)
            {
                return (0x1A000000 | (((to - (from + 8)) >> 2) & 0xFFFFFF));
            }
            else
            {
                return (0x1A000000 | (0x1000000 - (((from + 8) - to) >> 2)));
            }
        }

        static uint GetARMBranchLink(uint from, uint to)
        {
            // Fuck ARM Branches
            if (to >= from + 8)
            {
                return (0xEB000000 | (((to - (from + 8)) >> 2) & 0xFFFFFF));
            }
            else
            {
                return (0xEB000000 | (0x1000000 - (((from + 8) - to) >> 2)));
            }
        }

        static void Main(string[] args)
        {
            var Offsets = new Offsets();

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

            Console.WriteLine("Sun/Moon Patcher v1.1 - SciresM");

            var hash = (new SHA256CryptoServiceProvider()).ComputeHash(code);
            if (hash.SequenceEqual(Resources.moon_hash_1_0))
            {
                Console.WriteLine("Pokemon Moon v1.0 detected.");
                Offsets = Offsets_1_0;
            }
            else if (hash.SequenceEqual(Resources.moon_hash_1_1))
            {
                Console.WriteLine("Pokemon Moon v1.1 detected.");
                Offsets = Offsets_1_1;
            }
            else if (hash.SequenceEqual(Resources.sun_hash_1_0))
            {
                Console.WriteLine("Pokemon Sun v1.0 detected");
                Offsets = Offsets_1_0;
            }
            else if (hash.SequenceEqual(Resources.sun_hash_1_1))
            {
                Console.WriteLine("Pokemon Sun v1.1 detected");
                Offsets = Offsets_1_1;
            }
            else
            {
                Console.WriteLine("Unknown code.bin! Contact SciresM to update the program.");
                return;
            }

            Resources.debug_stub.CopyTo(code, Offsets.CTRIsDebugMode);
            Resources.battery_save.CopyTo(code, Offsets.QRReaderSaveDataBatteryQuery);
            Resources.qr_is_registered.CopyTo(code, Offsets.QRReaderSaveDataIsRegisteredData);
            new byte[0x24].CopyTo(code, Offsets.ForbiddenQRs);

            // Fix relative jumps within shellcode
            // QR Decryption Jumps
            BitConverter.GetBytes(GetARMBranch(Offsets.CTRIsDebugMode + 0x2C, Offsets.DecryptQRCodeStart + 0x4)).CopyTo(code, Offsets.CTRIsDebugMode + 0x2C);

            // QR Injection Jumps
            BitConverter.GetBytes(GetARMBranchNE(Offsets.QRReaderSaveDataBatteryQuery + 0x2C, Offsets.AnalyzeQRBinaryStart + 0x4)).CopyTo(code, Offsets.QRReaderSaveDataBatteryQuery + 0x2C);
            BitConverter.GetBytes(GetARMBranchLink(Offsets.QRReaderSaveDataBatteryQuery + 0x44, Offsets.CRC16)).CopyTo(code, Offsets.QRReaderSaveDataBatteryQuery + 0x44);
            BitConverter.GetBytes(GetARMBranchNE(Offsets.QRReaderSaveDataBatteryQuery + 0x5C, Offsets.AnalyzeQRBinaryStart + 0x4)).CopyTo(code, Offsets.QRReaderSaveDataBatteryQuery + 0x5C);
            BitConverter.GetBytes(GetARMBranchLink(Offsets.QRReaderSaveDataBatteryQuery + 0x70, Offsets.QRReaderSaveDataIsRegisteredData + 0x18)).CopyTo(code, Offsets.QRReaderSaveDataBatteryQuery + 0x70);
            BitConverter.GetBytes(GetARMBranchLink(Offsets.QRReaderSaveDataBatteryQuery + 0x8C, Offsets.DexDataAllocator)).CopyTo(code, Offsets.QRReaderSaveDataBatteryQuery + 0x8C);
            BitConverter.GetBytes(GetARMBranchLink(Offsets.QRReaderSaveDataBatteryQuery + 0xA0, Offsets.aeabi_memcpy)).CopyTo(code, Offsets.QRReaderSaveDataBatteryQuery + 0xA0);
            BitConverter.GetBytes(GetARMBranchLink(Offsets.QRReaderSaveDataBatteryQuery + 0xC0, Offsets.GenerateDexDisplayData)).CopyTo(code, Offsets.QRReaderSaveDataBatteryQuery + 0xC0);
            BitConverter.GetBytes(GetARMBranch(Offsets.QRReaderSaveDataBatteryQuery + 0xCC, Offsets.AnalyzeQRBinaryEnd)).CopyTo(code, Offsets.QRReaderSaveDataBatteryQuery + 0xCC);

            // InjectPokemonToBox Jumps
            BitConverter.GetBytes(GetARMBranchLink(Offsets.QRReaderSaveDataIsRegisteredData + 0x24, Offsets.GetSingletonAccessorInstance)).CopyTo(code, Offsets.QRReaderSaveDataIsRegisteredData + 0x24);
            BitConverter.GetBytes(GetARMBranchLink(Offsets.QRReaderSaveDataIsRegisteredData + 0x78, Offsets.aeabi_memcpy)).CopyTo(code, Offsets.QRReaderSaveDataIsRegisteredData + 0x78);


            // Hooks
            BitConverter.GetBytes(GetARMBranch(Offsets.DecryptQRCodeStart, Offsets.CTRIsDebugMode + 0xC)).CopyTo(code, Offsets.DecryptQRCodeStart);
            BitConverter.GetBytes(GetARMBranch(Offsets.DecryptQRCodeEnd, Offsets.CTRIsDebugMode + 0x34)).CopyTo(code, Offsets.DecryptQRCodeEnd);
            BitConverter.GetBytes(GetARMBranch(Offsets.AnalyzeQRBinaryStart, Offsets.QRReaderSaveDataBatteryQuery + 0x20)).CopyTo(code, Offsets.AnalyzeQRBinaryStart);

            Console.WriteLine("Patched!");
            var new_fn = Path.Combine(dir, fn + "_patched.bin");
            File.WriteAllBytes(new_fn, code);
            Console.WriteLine("Saved to {0}!", new_fn);

            BitConverter.GetBytes(0xE320F000).CopyTo(code, Offsets.NoOutlines);
            var nfn2 = Path.Combine(dir, fn + "_patched_nooutlines.bin");
            File.WriteAllBytes(nfn2, code);

        }
    }
}
