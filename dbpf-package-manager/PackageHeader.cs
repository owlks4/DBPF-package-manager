using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static DBPF_package_manager.ReversibleBinaryRead;

namespace DBPF_package_manager
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

        public uint indexSubVariant = 256; // only used sometimes; 256 is just a null value for our purposes

        public DateTime lastModified;

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

            lastModified = DateTime.FromBinary(ReadInt64(reader, isBigEndian));
            Debug.WriteLine("Timestamp: " + lastModified.ToString());

            numFiles = 0;
            indexLength = 0;

            if (majorVersion == 1)
            {
                MessageBox.Show("Version 1 package reading is not yet implemented");
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

        public byte[] constructHeader(uint newNumFiles, uint newIndexTableOffset, uint newIndexTableLength)
        {
            List<byte> output = new List<byte>();

            byte[] magic = new byte[] { (byte)'D', (byte)'B', (byte)'P', (byte)'F' };

            if (isBigEndian) {
                output.AddRange(magic.Reverse());
            } else {
                output.AddRange(magic);
            }

            output.AddRange(getBytesUInt32(majorVersion, isBigEndian));
            output.AddRange(getBytesUInt32(minorVersion, isBigEndian));
            
            for (int i = 0; i < 0x0C; i++) {
                output.Add(0x00);
            }

            output.AddRange(getBytesInt64(DateTime.Now.ToBinary(), isBigEndian));

            if (majorVersion == 1) {
                MessageBox.Show("Writing header for package with major version 1 not yet implemented");
            }
            else {
                output.AddRange(getBytesUInt32(indexMajorVersion, isBigEndian));
                output.AddRange(getBytesUInt32(newNumFiles, isBigEndian));
                output.AddRange(getBytesUInt32(majorVersion == 3 ? newIndexTableOffset : 0, isBigEndian)); //indexoffsetdeprecated
                output.AddRange(getBytesUInt32(newIndexTableLength, isBigEndian));
                output.AddRange(getBytesUInt32(0, isBigEndian)); //hole entry count
                output.AddRange(getBytesUInt32(0, isBigEndian)); //hole offset
                output.AddRange(getBytesUInt32(0, isBigEndian)); //hole size
                output.AddRange(getBytesUInt32(indexMinorVersion, isBigEndian));
                output.AddRange(getBytesUInt64((ulong)newIndexTableOffset, isBigEndian));
                for (int i = 0; i < 0x08; i++) {
                    output.Add(0x00);
                }
                if (majorVersion == 2){
                    for (int i = 0; i < 0x10; i++){
                        output.Add(0x00);
                    }
                }
                else {
                    for (int i = 0; i < 0x08; i++) { //some apparently crucial padding...
                        output.Add(0x55);
                    }
                    for (int i = 0; i < 0x08; i++) {
                        output.Add(0x00);
                    }
                }
            }

            return output.ToArray();
        }
    }
}
