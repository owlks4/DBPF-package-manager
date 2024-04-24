using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static maxis_package_manager.ReversibleBinaryRead;

namespace maxis_package_manager
{
    public class PackageHeader
    {
        public string magic;
        public bool isBigEndian;
        public uint majorVersion;
        public uint minorVersion;
        public int numFiles;
        public uint indexLength;
        public uint holeEntryCount;
        public uint holeOffset;
        public uint holeSize;
        
        public uint indexMajorVersion;
        public uint indexMinorVersion;
        public uint indexOffset;

        public PackageHeader(BinaryReader reader) {

            magic = new string(reader.ReadChars(4));
            isBigEndian = (magic == "FPBD");

            switch (magic)
            {
                case "DBPF":
                    Debug.WriteLine("Little endian package (DBPF)");
                    break;
                case "FPBD":
                    Debug.WriteLine("Big endian package (FPBD)");
                    break;
                default:
                    MessageBox.Show("Critical error when reading package. File has unknown file magic: " + magic + ". Expected DBPF or FPBD.");
                    return;
            }

            majorVersion = ReadUInt32(reader, isBigEndian);
            minorVersion = ReadUInt32(reader, isBigEndian);

            Debug.WriteLine("Package version: v" + majorVersion + "." + minorVersion);

            /*
                Known versions so far:
                sims 2: v1.1 little endian                
                mswii_presumably: v2.0 little endian
                mspc: v2.0 little endian
                msk: v2.0 little endian
                sims 3: v2.0 little endian
                spore: v2.0 little endian
                sims 4: v2.1 little endian
                msa: v3.0 big endian
            */

            reader.BaseStream.Position += 0x0C;

            DateTime timestamp = DateTime.FromBinary(ReadInt64(reader, isBigEndian));
            Debug.WriteLine("Timestamp: " + timestamp.ToString());

            numFiles = 0;
            indexLength = 0;

            if (majorVersion == 1)
            {
                numFiles = ReadInt32(reader, isBigEndian);
                reader.BaseStream.Position += 0x04; // ? actually, looks important
                ReadInt32(reader, isBigEndian); // unknown, but sums with next value to make total filesize (when counting from 0x10). You'd think data length, but not sure if that checks out
                ReadInt32(reader, isBigEndian); // unknown, but sums with previous value to make total filesize (when counting from 0x10). You'd think index table length, but not sure if that checks out
                ReadInt32(reader, isBigEndian); // unknown (is 1 in example)
                ReadInt32(reader, isBigEndian); // unknown (is 1 in example)
                ReadInt32(reader, isBigEndian); // total size (when counting from 0x10)
                ReadInt32(reader, isBigEndian); // unknown (is 8 in example)
                ReadInt32(reader, isBigEndian); // unknown (is 2 in example)
                MessageBox.Show("Version 1 package reading is not yet implemented");
                //then 0x20 of seemingly padding (all 0x00)
            }
            else
            {
                indexMajorVersion = ReadUInt32(reader, isBigEndian);
                numFiles = ReadInt32(reader, isBigEndian);
                reader.BaseStream.Position += 0x04; //skip deprecated index offset (sometimes has a value, sometimes doesn't, 0 in Version 2 but has value in Version 3)
                indexLength = ReadUInt32(reader, isBigEndian);
                holeEntryCount = ReadUInt32(reader, isBigEndian);
                holeOffset = ReadUInt32(reader, isBigEndian);
                holeSize = ReadUInt32(reader, isBigEndian);
                indexMinorVersion = ReadUInt32(reader, isBigEndian);
                indexOffset = (uint)ReadUInt64(reader, isBigEndian);                
                reader.BaseStream.Position += 0x18;
            }

            Debug.WriteLine("Number of files: " + numFiles);
            Debug.WriteLine("Index table offset: " + indexOffset);
        }
    }
}
