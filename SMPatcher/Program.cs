using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using CodeIsle.LibIpsNet;

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

        public bool IsUltra;
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

            GetSingletonAccessorInstance = 0x48B4,

            IsUltra = false
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

            GetSingletonAccessorInstance = 0x48B4,

            IsUltra = false
        };

        private static readonly Offsets Offsets_1_2 = new Offsets
        {
            CTRIsDebugMode = 0x4DE0,

            DecryptQRCodeStart = 0x2CEA3C,
            DecryptQRCodeEnd = 0x2CEAD4,

            AnalyzeQRBinaryStart = 0x2CED90,
            AnalyzeQRBinaryEnd = 0x2CF214,
            QRReaderSaveDataBatteryQuery = 0x33F6B0,

            QRReaderSaveDataIsRegisteredData = 0x3A8DD0,

            ForbiddenQRs = 0x4A5FF0,

            NoOutlines = 0x31CFCC,

            CRC16 = 0x259D14,
            DexDataAllocator = 0x3B6730,
            aeabi_memcpy = 0x1FEBD8,
            GenerateDexDisplayData = 0x2CE714,

            GetSingletonAccessorInstance = 0x48B4,

            IsUltra = false
        };

        private static readonly Offsets Ultra_Offsets_1_0 = new Offsets
        {
            CTRIsDebugMode = 0x4DE0,

            DecryptQRCodeStart = 0x2DEEA0,
            DecryptQRCodeEnd = 0x2DEF38,

            AnalyzeQRBinaryStart = 0x2DF364,
            AnalyzeQRBinaryEnd = 0x2DF8DC,
            QRReaderSaveDataBatteryQuery = 0x353864,

            QRReaderSaveDataIsRegisteredData = 0x3C3D24,

            ForbiddenQRs = 0x4CC724,

            NoOutlines = 0x32E2B8,

            CRC16 = 0x261534,
            DexDataAllocator = 0x5500,
            aeabi_memcpy = 0x1FED44,
            GenerateDexDisplayData = 0x2DEB80,

            GetSingletonAccessorInstance = 0x48B4,

            IsUltra = true
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

        static byte[] CreateIPS(byte[] source, byte[] modified)
        {
            using (var s = new MemoryStream(source))
            using (var m = new MemoryStream(modified))
            using (var o = new MemoryStream())
            {
                var Creator = new Creator();
                Creator.Create(s, m, o);
                return o.ToArray();
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

            var old_code = (byte[]) code.Clone();

            var dir = Path.GetDirectoryName(args[0]);
            var fn = Path.GetFileNameWithoutExtension(args[0]);

            Console.WriteLine("Sun/Moon Patcher v1.3 - SciresM");

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
            else if (hash.SequenceEqual(Resources.moon_hash_1_2))
            {
                Console.WriteLine("Pokemon Moon v1.2 detected.");
                Offsets = Offsets_1_2;
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
            else if (hash.SequenceEqual(Resources.sun_hash_1_2))
            {
                Console.WriteLine("Pokemon Sun v1.2 detected");
                Offsets = Offsets_1_2;
            }
            else if (hash.SequenceEqual(Resources.ultra_moon_hash_1_0))
            {
                Console.WriteLine("Pokemon Ultra Moon v1.0 detected");
                Offsets = Ultra_Offsets_1_0;
            }
            else if (hash.SequenceEqual(Resources.ultra_sun_hash_1_0))
            {
                Console.WriteLine("Pokemon Ultra Sun v1.0 detected");
                Offsets = Ultra_Offsets_1_0;
            }
            else
            {
                Console.WriteLine("Unknown code.bin! Contact SciresM to update the program.");
                return;
            }

            Resources.debug_stub.CopyTo(code, Offsets.CTRIsDebugMode);

            if (Offsets.IsUltra) // Patch up static memory clobber.
            {
                for (var i = 0; i < Resources.debug_stub.Length; i += 4)
                {
                    if (BitConverter.ToUInt32(Resources.debug_stub, i) == 0x006A1080)
                    {
                        BitConverter.GetBytes(0x00667180).CopyTo(code, Offsets.CTRIsDebugMode + i);
                    }
                }
            }
            Resources.battery_save.CopyTo(code, Offsets.QRReaderSaveDataBatteryQuery);
            if (Offsets.IsUltra) // Patch up Stack reads.
            {
                for (var i = 0; i < Resources.battery_save.Length; i += 4)
                {
                    if (BitConverter.ToUInt32(Resources.battery_save, i) == 0xE59D70B0) // LDR R7, [SP, #0xB0]
                    {
                        // LDR R7, [SP, #0xC0]
                        BitConverter.GetBytes(0xE59D70C0).CopyTo(code, Offsets.QRReaderSaveDataBatteryQuery + i);
                    }
                    else if (BitConverter.ToUInt32(Resources.battery_save, i) == 0xE59D1088) // LDR R1, [SP, #0x88]
                    {
                        // LDR R1, [SP, #0x98]
                        BitConverter.GetBytes(0xE59D1098).CopyTo(code, Offsets.QRReaderSaveDataBatteryQuery + i);
                    } 
                }
            }
            Resources.qr_is_registered.CopyTo(code, Offsets.QRReaderSaveDataIsRegisteredData);
            if (Offsets.IsUltra) // Patch up Box offset
            {
                for (var i = 0; i < Resources.qr_is_registered.Length; i += 4)
                {
                    if (BitConverter.ToUInt32(Resources.qr_is_registered, i) == 0xE28CCB0F) // ADD R12, R12, #0x3C00
                    {
                        // ADD R12, R12, #0x4100
                        // ADD R12, R12, #0x88 (+0x190+0xE8 to box base in USUM vs SM)
                        BitConverter.GetBytes(0xE28CCC41).CopyTo(code, Offsets.QRReaderSaveDataIsRegisteredData + i);
                        BitConverter.GetBytes(0xE28CC088).CopyTo(code, Offsets.QRReaderSaveDataIsRegisteredData + i + 4);
                    }
                }
            }
            new byte[Offsets.IsUltra ? 0x30 : 0x24].CopyTo(code, Offsets.ForbiddenQRs);

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
            var nfn_ips = Path.Combine(dir, fn + "_patched.ips");
            File.WriteAllBytes(new_fn, code);
            File.WriteAllBytes(nfn_ips, CreateIPS(old_code, code));
            Console.WriteLine("Saved to {0}!", new_fn);


            BitConverter.GetBytes(0xE320F000).CopyTo(code, Offsets.NoOutlines);
            var nfn2 = Path.Combine(dir, fn + "_patched_nooutlines.bin");
            var nfn2_ips = Path.Combine(dir, fn + "_patched_nooutlines.ips");
            File.WriteAllBytes(nfn2, code);
            File.WriteAllBytes(nfn2_ips, CreateIPS(old_code, code));
        }
    }
}
