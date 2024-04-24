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
        public uint indexOffset;
        public uint indexLength;

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
            indexOffset = 0;
            indexLength = 0;

            if (majorVersion == 1)
            {
                numFiles = ReadInt32(reader, isBigEndian);
                indexOffset = (uint)(reader.BaseStream.Length - ReadUInt32(reader, isBigEndian));
            }
            else
            {
                reader.BaseStream.Position += 0x04;
                numFiles = ReadInt32(reader, isBigEndian);
                if (majorVersion == 2)
                {
                    reader.BaseStream.Position += 0x04;
                    indexOffset = (uint)(reader.BaseStream.Length - ReadUInt32(reader, isBigEndian));
                    reader.BaseStream.Position += 0x04;
                }
                else if (majorVersion == 3)
                {
                    indexOffset = ReadUInt32(reader, isBigEndian); //For major version 3, this value isn't subtracted from EOF; it's stored directly as an offset.
                    indexLength = ReadUInt32(reader, isBigEndian);
                }
            }

            Debug.WriteLine("Number of files: " + numFiles);
            Debug.WriteLine("Offset of file list: " + indexOffset);
        }
    }
}
