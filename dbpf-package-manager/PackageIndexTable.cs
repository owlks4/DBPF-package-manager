using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows;
using System.Windows.Interop;
using static DBPF_package_manager.PackageIndexTable;
using static DBPF_package_manager.ReversibleBinaryRead;
using static DBPF_package_manager.TypeIDs;
using static System.Formats.Asn1.AsnWriter;

namespace DBPF_package_manager
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
            public uint uncompressedSizeIfRequired;
            public ulong checksum; //only so that we can remember it for a short time before we write it after the index table of V3 packages when compiling to a package - we don't load it from the file or verify it.

            public byte[] substitutionBytes = null; // MUST initialise as null - as we often compare this value to null to check whether the substitution exists or not.

            public uint flags = 0x00010000; //this seems to be a common value for the flags, so we default to this for the benefit of user-added files, and hope it makes them compatible, because I'm not sure what these four bytes actually do

            public FileEntry(BinaryReader reader, bool isBigEndian, bool compressed) {
                this.hash = ReadUInt64(reader, isBigEndian);
                this.offset = ReadUInt32(reader, isBigEndian);
                this.size = ReadUInt32(reader, isBigEndian);
                this.compressed = compressed;
                if (this.compressed) { 
                    this.uncompressedSizeIfRequired = ReadUInt32(reader, isBigEndian);
                }
            }

            public FileEntry(FileEntry basis) {
                this.hash = basis.hash;
                this.offset = basis.offset;
                this.size = basis.size;
                this.compressed = basis.compressed;
                this.typeID = basis.typeID;
                this.groupID = basis.groupID;
                this.uncompressedSizeIfRequired = basis.uncompressedSizeIfRequired;
                this.substitutionBytes = basis.substitutionBytes;
            }

            public FileEntry() { 
            }

            public string getSummary() {
                return "File with hash " + this.hash + " at offset " + offset + " with size " + size + ", typeID "+typeID.name+" and groupID " + groupID;
            
            }
        }

        public PackageIndexTable(BinaryReader reader, PackageHeader info) {
            Debug.WriteLine("Reading index table...");

            if (info.indexMajorVersion == 0) {
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
                            bool isCompressed = type.groupID != 0;
                            FileEntry fileEntry = new FileEntry(reader, info.isBigEndian, isCompressed);
                            fileEntry.typeID = type.typeID;
                            fileEntry.groupID = type.groupID;
                            files[numFilesProcessed] = fileEntry;
                            numFilesProcessed++;
                        }
                    }
                }
                else if (info.indexMinorVersion == 3) { // Sims 3; Sims 4; Spore; MS; MSK

                    info.indexSubVariant = ReadUInt32(reader, info.isBigEndian);
                    files = new FileEntry[info.numFiles];

                    Debug.WriteLine("Index subvariant: " + info.indexSubVariant);

                    switch (info.indexSubVariant)
                    {
                        default:
                            MessageBox.Show("Index subvariant " + info.indexSubVariant + "is not yet implemented");
                            break;
                        case 0:
                            for (uint i = 0; i < info.numFiles; i++)
                            {
                                FileEntry fileEntry = new FileEntry();

                                fileEntry.typeID = getTypeIDById(ReadUInt32(reader, info.isBigEndian));
                                fileEntry.groupID = ReadUInt32(reader, info.isBigEndian);
                                reader.BaseStream.Position += 8; //one of these two 32-bit fields is a duplicate of the groupID.

                                if (i == 0)
                                {
                                    Debug.WriteLine("NB these fields are filled in a slightly more enlightening way in Sims 4 packages, so you might be able to get a better idea of their function by looking at those.");
                                }

                                fileEntry.offset = ReadUInt32(reader, info.isBigEndian);
                                fileEntry.size = ReadUInt32(reader, info.isBigEndian) & 0x7FFFFFFF;
                                fileEntry.uncompressedSizeIfRequired = ReadUInt32(reader, info.isBigEndian);

                                fileEntry.flags = ReadUInt32(reader, info.isBigEndian);

                                fileEntry.hash = (ulong)fileEntry.offset;

                                if (fileEntry.size != fileEntry.uncompressedSizeIfRequired)
                                {
                                    fileEntry.compressed = true;
                                }

                                files[i] = fileEntry;
                            }
                            break;
                        case 2:
                            reader.BaseStream.Position += 4;

                            for (uint i = 0; i < info.numFiles; i++)
                            {
                                FileEntry fileEntry = new FileEntry();

                                fileEntry.typeID = getTypeIDById(ReadUInt32(reader, info.isBigEndian));
                                fileEntry.hash = (ulong)(ReadUInt32(reader, info.isBigEndian)) << 32;
                                fileEntry.hash |= (ulong)(ReadUInt32(reader, info.isBigEndian));

                                fileEntry.offset = ReadUInt32(reader, info.isBigEndian);
                                fileEntry.size = ReadUInt32(reader, info.isBigEndian) & 0x7FFFFFFF;
                                fileEntry.uncompressedSizeIfRequired = ReadUInt32(reader, info.isBigEndian);

                                fileEntry.flags = ReadUInt32(reader, info.isBigEndian);

                                if (fileEntry.size != fileEntry.uncompressedSizeIfRequired)
                                {
                                    fileEntry.compressed = true;
                                }

                                files[i] = fileEntry;
                            }
                            break;
                        case 3:
                            uint allFilesTypeID = ReadUInt32(reader, info.isBigEndian);
                            reader.BaseStream.Position += 4;

                            for (uint i = 0; i < info.numFiles; i++)
                            {
                                FileEntry fileEntry = new FileEntry();

                                fileEntry.typeID = getTypeIDById(allFilesTypeID);
                                fileEntry.hash = (ulong)(ReadUInt32(reader, info.isBigEndian)) << 32;
                                fileEntry.hash |= (ulong)(ReadUInt32(reader, info.isBigEndian));

                                fileEntry.offset = ReadUInt32(reader, info.isBigEndian);
                                fileEntry.size = ReadUInt32(reader, info.isBigEndian) & 0x7FFFFFFF;
                                fileEntry.uncompressedSizeIfRequired = ReadUInt32(reader, info.isBigEndian);

                                fileEntry.flags = ReadUInt32(reader, info.isBigEndian);

                                if (fileEntry.size != fileEntry.uncompressedSizeIfRequired)
                                {
                                    fileEntry.compressed = true;
                                }

                                files[i] = fileEntry;
                            }
                            break;
                        case 4:
                            reader.BaseStream.Position += 4;

                            for (uint i = 0; i < info.numFiles; i++)
                            {
                                FileEntry fileEntry = new FileEntry();

                                fileEntry.typeID = getTypeIDById(ReadUInt32(reader, info.isBigEndian));
                                fileEntry.hash = (ulong)(ReadUInt32(reader, info.isBigEndian)) << 32;
                                fileEntry.hash |= (ulong)(ReadUInt32(reader, info.isBigEndian));

                                fileEntry.offset = ReadUInt32(reader, info.isBigEndian);
                                fileEntry.size = ReadUInt32(reader, info.isBigEndian) & 0x7FFFFFFF;
                                fileEntry.uncompressedSizeIfRequired = ReadUInt32(reader, info.isBigEndian);

                                if (fileEntry.size != fileEntry.uncompressedSizeIfRequired)
                                {
                                    fileEntry.compressed = true;
                                }

                                fileEntry.flags = ReadUInt32(reader, info.isBigEndian);

                                files[i] = fileEntry;
                            }
                            break;
                        case 7:
                            for (uint i = 0; i < info.numFiles; i++)
                            {
                                FileEntry fileEntry = new FileEntry();

                                fileEntry.typeID = getTypeIDById(ReadUInt32(reader, info.isBigEndian));
                                fileEntry.groupID = ReadUInt32(reader, info.isBigEndian);
                                fileEntry.hash = (ulong)(ReadUInt32(reader, info.isBigEndian)) << 32;
                                fileEntry.hash |= (ulong)(ReadUInt32(reader, info.isBigEndian));

                                fileEntry.offset = ReadUInt32(reader, info.isBigEndian);
                                fileEntry.size = ReadUInt32(reader, info.isBigEndian) & 0x7FFFFFFF;
                                fileEntry.uncompressedSizeIfRequired = ReadUInt32(reader, info.isBigEndian);

                                fileEntry.flags = ReadUInt32(reader, info.isBigEndian);

                                if (fileEntry.size != fileEntry.uncompressedSizeIfRequired)
                                {
                                    fileEntry.compressed = true;
                                }

                                files[i] = fileEntry;
                            }
                            break;
                    }
                }
                else {
                    MessageBox.Show("The joys of other index table minor versions await! (Not yet implemented!)");
                }
            }
            else if (info.indexMajorVersion == 7) {
                files = new FileEntry[info.numFiles];

                if (info.indexMinorVersion == 0) { //sim city 4
                    for (uint i = 0; i < info.numFiles; i++)
                    {
                        FileEntry fileEntry = new FileEntry();

                        fileEntry.typeID = getTypeIDById(ReadUInt32(reader, info.isBigEndian));
                        fileEntry.groupID = ReadUInt32(reader, info.isBigEndian);
                        fileEntry.hash = ReadUInt32(reader, info.isBigEndian);
                        fileEntry.offset = ReadUInt32(reader, info.isBigEndian);
                        fileEntry.size = ReadUInt32(reader, info.isBigEndian);
                        files[i] = fileEntry;
                    }
                }
                else if (info.indexMinorVersion == 1 || info.indexMinorVersion == 2) { //sims 2

                    for (uint i = 0; i < info.numFiles; i++) {
                        FileEntry fileEntry = new FileEntry();

                        fileEntry.typeID = getTypeIDById(ReadUInt32(reader, info.isBigEndian));
                        fileEntry.groupID = ReadUInt32(reader, info.isBigEndian);
                        if (info.indexMinorVersion == 1) {
                            fileEntry.hash = ReadUInt32(reader, info.isBigEndian);
                        }
                        else {
                            fileEntry.hash = ReadUInt64(reader, info.isBigEndian);
                        }
                        fileEntry.offset = ReadUInt32(reader, info.isBigEndian) + 4;
                        fileEntry.size = ReadUInt32(reader, info.isBigEndian) - 4;
                        files[i] = fileEntry;
                    }
                }
                else {
                    MessageBox.Show("The joys of other index table minor versions await! (Not yet implemented!)");
                }
            }
            else {
                 MessageBox.Show("The joys of other index table major versions await! (Not yet implemented!)");
             }
        }

        public byte[] reconstructByteArray(PackageHeader info, FileEntry[] newFileEntries) { //info is NOT the up to date version of the package header, but we're using it to get information on the index version

            List<byte> output = new List<byte>();

            if (info.indexMinorVersion == 2)
            {  //MSA

                FileEntry[] filesOrderedByTypeThenHash = newFileEntries.OrderBy(f => f.typeID.id).ThenBy(f => f.hash).ToArray();

                Dictionary<TypeID, uint> typeIDCounts = new Dictionary<TypeID, uint>();
                Dictionary<TypeID, uint> groupIDlookup = new Dictionary<TypeID, uint>();

                List<TypeID> typeIDsInOrder = new List<TypeID>(); //this is required because we need to keep the type IDs and their files in the order established earlier, but the dictionaries won't cut it because dictionaries aren't ordered.

                foreach (FileEntry f in filesOrderedByTypeThenHash) {
                    if (typeIDCounts.ContainsKey(f.typeID))
                    {
                        typeIDCounts[f.typeID]++;
                        if (f.groupID != groupIDlookup[f.typeID]) {
                            MessageBox.Show("Whoa whoa whoa! One type ID had files with at least two different group IDs. This is impossible, because group IDs are stored PER type ID, so two tiles of the same type id MUST have the same group ID. If you're seeing this, something is wrong.");
                        }
                    }
                    else {
                        typeIDCounts.Add(f.typeID, 1);
                        groupIDlookup.Add(f.typeID, f.groupID);
                        typeIDsInOrder.Add(f.typeID);
                    }
                }

                output.AddRange(getBytesUInt64((ulong)typeIDsInOrder.Count, info.isBigEndian));  //write type id count to output

                foreach (TypeID t in typeIDsInOrder) {  //write each type id entry to the output
                    output.AddRange(getBytesUInt32(t.id, info.isBigEndian));
                    output.AddRange(getBytesUInt32(groupIDlookup[t], info.isBigEndian));
                    output.AddRange(getBytesUInt32(typeIDCounts[t], info.isBigEndian));
                    output.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x00 });
                }

                foreach (FileEntry f in filesOrderedByTypeThenHash) {
                    output.AddRange(getBytesUInt64(f.hash, info.isBigEndian));
                    output.AddRange(getBytesUInt32(f.offset, info.isBigEndian));
                    output.AddRange(getBytesUInt32(f.size, info.isBigEndian));
                    if (f.compressed) {
                        output.AddRange(getBytesUInt32(f.uncompressedSizeIfRequired, info.isBigEndian));
                    }                    
                }
            }
            else if (info.indexMinorVersion == 3) { //Sims 3; Sims 4; Spore; MS; MSK

                Debug.WriteLine("Saving index subvariant: " + info.indexSubVariant);

                output.AddRange(getBytesUInt32(info.indexSubVariant, info.isBigEndian));

                switch (info.indexSubVariant)
                {
                    default:
                        MessageBox.Show("Index subvariant " + info.indexSubVariant + "is not yet implemented");
                        break;
                    case 0:
                        for (uint i = 0; i < newFileEntries.Length; i++)
                        {
                            FileEntry f = newFileEntries[i];

                            output.AddRange(getBytesUInt32(f.typeID.id, info.isBigEndian));
                            output.AddRange(getBytesUInt32(f.groupID, info.isBigEndian));
                            output.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x00 });
                            output.AddRange(getBytesUInt32(f.groupID, info.isBigEndian)); //[sic]

                            output.AddRange(getBytesUInt32(f.offset, info.isBigEndian));
                            output.AddRange(getBytesUInt32(f.size | 0x80000000, info.isBigEndian));
                            output.AddRange(getBytesUInt32(f.uncompressedSizeIfRequired, info.isBigEndian));

                            output.AddRange(getBytesUInt32(f.flags, info.isBigEndian));
                        }
                        break;
                    case 2:
                        output.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x00 });

                        for (uint i = 0; i < newFileEntries.Length; i++)
                        {
                            FileEntry f = newFileEntries[i];

                            output.AddRange(getBytesUInt32(f.typeID.id, info.isBigEndian));
                            output.AddRange(getBytesUInt32((uint)(f.hash >> 32), info.isBigEndian));
                            output.AddRange(getBytesUInt32((uint)(f.hash & 0xFFFFFFFF), info.isBigEndian));

                            output.AddRange(getBytesUInt32(f.offset, info.isBigEndian));
                            output.AddRange(getBytesUInt32(f.size | 0x80000000, info.isBigEndian));
                            output.AddRange(getBytesUInt32(f.uncompressedSizeIfRequired, info.isBigEndian));

                            output.AddRange(getBytesUInt32(f.flags, info.isBigEndian));
                        }
                        break;
                    case 3:
                        TypeID allFilesTypeID = null;

                        foreach (FileEntry f in newFileEntries) {
                            if (allFilesTypeID == null) {
                                allFilesTypeID = f.typeID;
                            } 
                            else if (f.typeID != allFilesTypeID) {
                                MessageBox.Show("That package index subvariant ("+info.indexSubVariant+") is only capable of storing ONE file type inside it. You cannot save it when it contains files of multiple different types. The package manager will probably now crash.", "Improper package contents");
                                return null;
                            }                            
                        }

                        output.AddRange(getBytesUInt32(allFilesTypeID.id, info.isBigEndian));
                        output.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x00 });

                        for (uint i = 0; i < newFileEntries.Length; i++)
                        {
                            FileEntry f = newFileEntries[i];

                            output.AddRange(getBytesUInt32((uint)(f.hash >> 32), info.isBigEndian));
                            output.AddRange(getBytesUInt32((uint)(f.hash & 0xFFFFFFFF), info.isBigEndian));

                            output.AddRange(getBytesUInt32(f.offset, info.isBigEndian));
                            output.AddRange(getBytesUInt32(f.size | 0x80000000, info.isBigEndian));
                            output.AddRange(getBytesUInt32(f.uncompressedSizeIfRequired, info.isBigEndian));

                            output.AddRange(getBytesUInt32(f.flags, info.isBigEndian));
                        }
                        break;
                    case 4:
                        output.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x00 });

                        for (uint i = 0; i < newFileEntries.Length; i++)
                        {
                            FileEntry f = newFileEntries[i];

                            output.AddRange(getBytesUInt32(f.typeID.id, info.isBigEndian));
                            output.AddRange(getBytesUInt32((uint)(f.hash >> 32), info.isBigEndian));
                            output.AddRange(getBytesUInt32((uint)(f.hash & 0xFFFFFFFF), info.isBigEndian));

                            output.AddRange(getBytesUInt32(f.offset, info.isBigEndian));
                            output.AddRange(getBytesUInt32(f.size | 0x80000000, info.isBigEndian));
                            output.AddRange(getBytesUInt32(f.uncompressedSizeIfRequired, info.isBigEndian));

                            output.AddRange(getBytesUInt32(f.flags, info.isBigEndian));
                        }
                        break;
                    case 7:
                        for (uint i = 0; i < newFileEntries.Length; i++)
                        {
                            FileEntry f = newFileEntries[i];

                            output.AddRange(getBytesUInt32(f.typeID.id, info.isBigEndian));
                            output.AddRange(getBytesUInt32(f.groupID, info.isBigEndian));
                            output.AddRange(getBytesUInt32((uint)(f.hash >> 32), info.isBigEndian));
                            output.AddRange(getBytesUInt32((uint)(f.hash & 0xFFFFFFFF), info.isBigEndian));

                            output.AddRange(getBytesUInt32(f.offset, info.isBigEndian));
                            output.AddRange(getBytesUInt32(f.size | 0x80000000, info.isBigEndian));
                            output.AddRange(getBytesUInt32(f.uncompressedSizeIfRequired, info.isBigEndian));

                            output.AddRange(getBytesUInt32(f.flags, info.isBigEndian));
                        }
                        break;
                }
            }
            else {
                MessageBox.Show("Saving that kind of index table is not yet implemented!");
            }

            return output.ToArray();
        }
    }
}
