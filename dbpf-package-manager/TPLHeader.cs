using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Printing;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static DBPF_package_manager.ReversibleBinaryRead;

namespace DBPF_package_manager
{
    public class TPLHeader
    {
        public ushort version = 0;
        public ushort width = 0;
        public ushort height = 0;
        public byte imageFormat = 0;
        public uint imageCount = 0;
        public uint imageDataOffset = 0;
        public uint imageDataSize = 0;

        public uint imageTableOffset = 0;
        public uint imageHeaderOffset = 0;
        public uint paletteHeaderOffset = 0;
        public uint wrapS = 0;
        public uint wrapT = 0;
        public uint minFilter = 0;
        public uint magFilter = 0;

        public TPLHeader(BinaryReader reader) {
            
            uint magic = ReadUInt32(reader, true);

            if (magic == 0x14FE0149) {
                readSimsHeader(reader);
            } else if (magic == 0x0020AF30){
                readNintendoHeader(reader);
            }
        }

        public void readSimsHeader(BinaryReader reader) {
            reader.BaseStream.Position = 4;
            version = ReadUInt16(reader, true);

            switch (version)
            {
                case 1:
                case 2:
                    reader.BaseStream.Position = 0x1C;
                    width = ReadUInt16(reader, true);
                    height = ReadUInt16(reader, true);
                    imageFormat = (byte)ReadUInt32(reader, true);
                    imageCount = ReadUInt32(reader, true);
                    imageDataOffset = 0x4C;
                    imageDataSize = (uint)(width * height * 4);
                    break;
                case 3:
                    reader.BaseStream.Position = 0x08;
                    imageDataSize = ReadUInt32(reader, true);
                    reader.BaseStream.Position = 0x18;
                    width = ReadUInt16(reader, true);
                    height = ReadUInt16(reader, true);
                    imageFormat = (byte)ReadUInt32(reader, true);
                    imageCount = ReadUInt32(reader, true);
                    reader.BaseStream.Position = 0x38;
                    imageDataOffset = ReadUInt32(reader, true);
                    break;
                default:
                    MessageBox.Show("Unknown Sims TPL type!");
                    break;
            }
        }

        public void readNintendoHeader(BinaryReader reader) {
            imageCount = ReadUInt32(reader, true);
            imageTableOffset = ReadUInt32(reader, true);

            imageHeaderOffset = ReadUInt32(reader, true);
            paletteHeaderOffset = ReadUInt32(reader, true);

            reader.BaseStream.Position = imageHeaderOffset;

            //this does make the reader a bit naive, and makes it only read the first image in a TPL file.

            height = ReadUInt16(reader, true);
            width = ReadUInt16(reader, true);
            imageFormat = (byte)ReadUInt32(reader, true);
            imageDataOffset = ReadUInt32(reader, true);
            wrapS = ReadUInt32(reader, true);
            wrapT = ReadUInt32(reader, true);
            minFilter = ReadUInt32(reader, true);
            magFilter = ReadUInt32(reader, true);

            imageDataSize = (uint)(reader.BaseStream.Length - imageDataOffset);
        }

        public byte[] dumpToNintendoHeaderBytes() {
            List<byte> output = new List<byte>();

            output.AddRange(getBytesUInt32(0x0020AF30, true));
            output.AddRange(getBytesUInt32(0x00000001, true)); //ideally this should be image count, but I don't want to make this more than one until I'm sure that the nintendo format header creation is robust enough to support multiple images in one TPL file
            output.AddRange(getBytesUInt32(0x0000000C, true));
            output.AddRange(getBytesUInt32(0x00000014, true));
            output.AddRange(getBytesUInt32(0x00000000, true));
            output.AddRange(getBytesUInt16(height, true));
            output.AddRange(getBytesUInt16(width, true));
            output.AddRange(getBytesUInt32((uint)imageFormat, true));
            output.AddRange(getBytesUInt32(0x00000060, true));
            output.AddRange(getBytesUInt32(0x00000000, true));
            output.AddRange(getBytesUInt32(0x00000000, true));
            output.AddRange(getBytesUInt32(0x00000001, true));
            output.AddRange(getBytesUInt32(0x00000001, true));

            for (int i = 0; i < 12; i++){
                output.AddRange(getBytesUInt32(0x00000000, true));
            }

            return output.ToArray();
        }

        public byte[] dumpToSimsHeaderBytes(ushort _version)
        {
            List<byte> output = new List<byte>();

            output.AddRange(getBytesUInt32(0x14FE0149, true));
            output.AddRange(getBytesUInt16(_version, true));
            output.AddRange(getBytesUInt16(0x0000, true));

            if (_version == 2) {
                output.AddRange(getBytesUInt16(0x0001, true));
                for (int i = 0; i < 0x12; i++) {
                    output.Add(0x00);
                }
                output.AddRange(getBytesUInt16(width, true));
                output.AddRange(getBytesUInt16(height, true));
                output.AddRange(getBytesUInt32(imageFormat, true));
                output.AddRange(getBytesUInt32(0x00000001, true));
                output.AddRange(getBytesUInt32(0x00000001, true));
                output.AddRange(getBytesUInt32(0x00000001, true));
                output.AddRange(getBytesUInt32(0x00000001, true));
                output.AddRange(getBytesUInt32(0x00000000, true));
                output.AddRange(getBytesUInt32(0x00000000, true));
                output.AddRange(getBytesUInt32(0x00000000, true));
                output.AddRange(getBytesUInt32(0x00010000, true));
                output.AddRange(getBytesUInt32(0x00000000, true));
                output.AddRange(getBytesUInt32(0x00000000, true));
            }
            else if (_version == 3)
            {
                output.AddRange(getBytesUInt32(imageDataSize, true));

                output.AddRange(getBytesUInt32(0x00000000, true));
                output.AddRange(getBytesUInt32(0x00000000, true));
                output.AddRange(getBytesUInt32(0x00000000, true));

                output.AddRange(getBytesUInt16(width, true));
                output.AddRange(getBytesUInt16(height, true));

                output.AddRange(getBytesUInt32(imageFormat, true));

                output.AddRange(getBytesUInt32(0x00000001, true));
                output.AddRange(getBytesUInt32(0x00000001, true));

                output.AddRange(getBytesUInt32(0x00000001, true));
                output.AddRange(getBytesUInt32(0x00000001, true));

                output.AddRange(getBytesUInt32(0x00000000, true));
                output.AddRange(getBytesUInt32(0x00000000, true));

                output.AddRange(getBytesUInt32((uint)output.Count + 8, true)); //anticipating the final header size
                output.AddRange(getBytesUInt32(imageDataSize, true));
            }

            return output.ToArray();
        }
    }
}
