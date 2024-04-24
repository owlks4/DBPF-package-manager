using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows;
using static maxis_package_manager.ReversibleBinaryRead;
using static maxis_package_manager.TypeIDs;

namespace maxis_package_manager
{
    public class PackageIndexTable
    {
        public FileEntry[] files = null;

        public class TypeTableEntry {
            public TypeID typeID;
            public uint groupID;
            public uint fileCount;

            public TypeTableEntry(BinaryReader reader, bool isBigEndian) {
                this.typeID = getTypeIDById(ReadUInt32(reader,isBigEndian));
                this.groupID = ReadUInt32(reader, isBigEndian);
                this.fileCount = ReadUInt32(reader, isBigEndian);
                reader.BaseStream.Position += 4;
            }
        }

        public class FileEntry {
            public ulong hash;
            public uint offset;
            public uint size;
            public TypeID typeID;
            public uint groupID;
            public bool compressed = false;
            public FileEntry(BinaryReader reader, bool isBigEndian) {
                this.hash = ReadUInt64(reader, isBigEndian);
                this.offset = ReadUInt32(reader, isBigEndian);
                this.size = ReadUInt32(reader, isBigEndian);
            }
        }

        public PackageIndexTable(BinaryReader reader, PackageHeader info) {
            Debug.WriteLine("Reading index table...");

            if (info.indexMinorVersion == 2)
            {  //MSA

                ulong num_unique_types = ReadUInt64(reader, info.isBigEndian);
                TypeTableEntry[] types = new TypeTableEntry[num_unique_types];

                uint fileCount = 0;

                Debug.WriteLine("Processing file type table, package makeup is:");

                for (int i = 0; i < (uint)num_unique_types; i++)
                {
                    TypeTableEntry typeEntry = new TypeTableEntry(reader, info.isBigEndian);
                    fileCount += typeEntry.fileCount;
                    types[i] = typeEntry;
                    Debug.WriteLine(typeEntry.fileCount + "x " + typeEntry.typeID.name);
                }

                //which brings us up to the beginning of the actual files...

                files = new FileEntry[fileCount];
                uint numFilesProcessed = 0;

                foreach (TypeTableEntry type in types)
                {
                    for (int i = 0; i < type.fileCount; i++)
                    {
                        FileEntry fileEntry = new FileEntry(reader, info.isBigEndian);
                        fileEntry.typeID = type.typeID;
                        fileEntry.groupID = type.groupID;
                        if (fileEntry.groupID != 0)
                        {
                            fileEntry.compressed = true;
                        }
                        files[numFilesProcessed] = fileEntry;
                        numFilesProcessed++;
                    }
                }
            }
            else {

                MessageBox.Show("The joys of other index tables await! (Not yet implemented!)");
            
            
            
            
            
            
            
            }
        }
    }
}
