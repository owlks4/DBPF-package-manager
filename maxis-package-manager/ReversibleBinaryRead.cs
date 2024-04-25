using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace maxis_package_manager
{
    public static class ReversibleBinaryRead
    {
        public static byte[] ReverseBytes(byte[] bytes) {
            return bytes.Reverse().ToArray();
        }

        public static uint ReadUInt32(BinaryReader reader, bool reverse) {
            if (reverse){
                return BitConverter.ToUInt32(ReverseBytes(BitConverter.GetBytes(reader.ReadUInt32())));
            } else {
                return reader.ReadUInt32();
            }
        }
        public static int ReadInt32(BinaryReader reader, bool reverse)
        {
            if (reverse) {
                return BitConverter.ToInt32(ReverseBytes(BitConverter.GetBytes(reader.ReadInt32())));
            } else {
                return reader.ReadInt32();
            }
        }

        public static ushort ReadUInt16(BinaryReader reader, bool reverse)
        {
            if (reverse){
                return BitConverter.ToUInt16(ReverseBytes(BitConverter.GetBytes(reader.ReadUInt16())));
            } else {
                return reader.ReadUInt16();
            }
        }

        public static short ReadInt16(BinaryReader reader, bool reverse)
        {
            if (reverse){
                return BitConverter.ToInt16(ReverseBytes(BitConverter.GetBytes(reader.ReadInt16())));
            } else {
                return reader.ReadInt16();
            }
        }

        public static ulong ReadUInt64(BinaryReader reader, bool reverse)
        {
            if (reverse){
                return BitConverter.ToUInt64(ReverseBytes(BitConverter.GetBytes(reader.ReadUInt64())));
            } else {
                return reader.ReadUInt64();
            }
        }

        public static long ReadInt64(BinaryReader reader, bool reverse)
        {
            if (reverse) {
                return BitConverter.ToInt64(ReverseBytes(BitConverter.GetBytes(reader.ReadInt64())));
            } else {
                return reader.ReadInt64();
            }
        }

        public static byte[] getBytesUInt64(ulong input, bool isBigEndian) {
            if (isBigEndian) {
                return BitConverter.GetBytes(input).Reverse().ToArray();
            } else {
                return BitConverter.GetBytes(input);
            }
        }

        public static byte[] getBytesInt64(long input, bool isBigEndian)
        {
            if (isBigEndian)
            {
                return BitConverter.GetBytes(input).Reverse().ToArray();
            }
            else
            {
                return BitConverter.GetBytes(input);
            }
        }

        public static byte[] getBytesUInt32(uint input, bool isBigEndian)
        {
            if (isBigEndian)
            {
                return BitConverter.GetBytes(input).Reverse().ToArray();
            }
            else
            {
                return BitConverter.GetBytes(input);
            }
        }

        public static byte[] getBytesUInt16(ushort input, bool isBigEndian)
        {
            if (isBigEndian)
            {
                return BitConverter.GetBytes(input).Reverse().ToArray();
            }
            else
            {
                return BitConverter.GetBytes(input);
            }
        }


        public static uint getFNV_1Hash(byte[] input)
        {
            uint hash = 2166136261;

            foreach (byte b in input)
            {
                hash = hash * 16777619;
                hash = hash ^ b;
            }

            return hash;
        }

        public static ulong getFNV_1_Hash_64(byte[] input)
        {
            ulong hash = 14695981039346656037;

            foreach (byte b in input)
            {
                hash = hash * 1099511628211;
                hash = hash ^ b;
            }

            return hash;
        }

    }
}
