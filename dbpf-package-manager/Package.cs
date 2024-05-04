using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Diagnostics;
using static DBPF_package_manager.ReversibleBinaryRead;
using System.Reflection.Metadata.Ecma335;
using static DBPF_package_manager.PackageIndexTable;
using System.Printing.IndexedProperties;
using static DBPF_package_manager.TypeIDs;

namespace DBPF_package_manager
{
    public class Package
    {
        public PackageHeader packageHeader;
        public PackageIndexTable indexTable;
        public BinaryReader reader;

        public Package(Stream f) {

            reader = new BinaryReader(f);

            packageHeader = new PackageHeader(reader);

            reader.BaseStream.Position = packageHeader.indexOffset;

            indexTable = new PackageIndexTable(reader, packageHeader);

            if (indexTable.files != null && indexTable.files.Length == 0) {
                MessageBox.Show("Package was empty...");
            }

            surveyTypeOrderInBlob();       
        }

        public void surveyTypeOrderInBlob() {
            Debug.WriteLine("Btw... order of first occurrences of each type ID in the blob (in blob order) is:");

            if (indexTable.files == null) {
                return;
            }

            FileEntry[] filesInOffsetOrder = indexTable.files.OrderBy(f => f.offset).ToArray();
            List<TypeID> typeIDsInOrderOfBlobOccurrence = new List<TypeID>();

            foreach (FileEntry fileEntry in filesInOffsetOrder)
            {
                if (!typeIDsInOrderOfBlobOccurrence.Contains(fileEntry.typeID))
                {
                    typeIDsInOrderOfBlobOccurrence.Add(fileEntry.typeID);
                    Debug.WriteLine(fileEntry.typeID.name + " at " + fileEntry.offset);
                }
            }
        }

        public byte[] getSpecificFileEntryContent(FileEntry f) {

            for (int i = 0; i < indexTable.files.Length; i++) {
                if (indexTable.files[i] == f) {
                    MainWindow.package.reader.BaseStream.Position = f.offset;
                    byte[] bytes = f.substitutionBytes == null ? MainWindow.package.reader.ReadBytes((int)f.size) : f.substitutionBytes;

                    if (f.compressed) {
                        Debug.WriteLine("Exporting a compressed file - wouldn't it be good to handle this and make sure that the version the user exports is uncompressed?");
                    }
                    return bytes;
                }            
            }
            Debug.WriteLine("Failed to find that file entry in the package, so could not retrieve its bytes!");
            return null;
        }

        public byte[] dumpToByteArray() {

            bool hasGivenCompressionDevWarningOnce = false;

            FileEntry[] newFileEntries = new FileEntry[indexTable.files.Length];

            ////////////////// CREATE FILE DATA BLOB: //////////////////

            List<byte> fileBlob = new List<byte>();
            uint cumulativeDataLength = 0; //probably faster to retrieve than the list Count

            for (int i = 0; i < indexTable.files.Length; i++)
            {
                FileEntry f = new FileEntry(indexTable.files[i]);
                newFileEntries[i] = f;
            }

            /*
                The files seem to be alphabetical within the blob, albeit sorted by an arbitrary typeID
                order (importantly, different from the order of the typeid's numerical value. In the same
                vein as the alphabetical order for the files in the blob despite them having hashes,
                they're probably being ordered by a string that describes the type. Suspect something like
                _IMG (similar to The Sims) is putting tpl files at the top of the list.

                TODO: adjust the typeID names to reflect the order you see in practice in MSA.
            */

            newFileEntries = newFileEntries.OrderBy(f => f.typeID.name).ToArray();            

            foreach (FileEntry f in newFileEntries) {

                //Debug.WriteLine(f.getSummary());

                if (f.compressed && !hasGivenCompressionDevWarningOnce) {
                    MessageBox.Show("!!!! A compressed file - does it need special handling? Should it already have been stored as compressed at all times by DBPF-package-manager, and can we assume it is still compressed?");
                    hasGivenCompressionDevWarningOnce = true;
                }

                uint newOffset = (uint)(0x60 + cumulativeDataLength); //sets the new offset

                if (f.substitutionBytes == null) { //then read the file out of the existing binary blob and put it into the blob
                    reader.BaseStream.Position = f.offset;  //reads the data from the old offset for the last time
                    byte[] bytes = reader.ReadBytes((int)f.size);
                    fileBlob.AddRange(bytes);                    
                    cumulativeDataLength += f.size;
                    f.checksum = getFNV_1_Hash_64(bytes);
                }
                else {  //the user has substituted a new file in this one's place. Get the byte array for the substitute instead, and put it into the blob
                    fileBlob.AddRange(f.substitutionBytes);
                    f.size = (uint)f.substitutionBytes.Length;
                    cumulativeDataLength += f.size;
                    f.checksum = getFNV_1_Hash_64(f.substitutionBytes);
                }

                f.offset = newOffset; //update the old offset to the new offset that we acquired before we started writing

                if (packageHeader.indexMajorVersion == 3) {
                    while (cumulativeDataLength % 0x20 != 0)
                    { //pad to next 0x20
                        fileBlob.Add(0x00);
                        cumulativeDataLength++;
                    }
                }
            }

            ////////////////// CREATE A NEW INDEX TABLE: ////////////////// 
            ///
            byte[] newIndexTableBytes = indexTable.reconstructByteArray(packageHeader, newFileEntries); //this is the old version of the package header we're feeding it, btw - just for information as to what index version we're using.

            ////////////////// CREATE A NEW HEADER: ////////////////// 

            uint newIndexTableOffset = (uint)(0x60 + fileBlob.Count);
            uint newIndexTableLength = (uint)newIndexTableBytes.Length;

            byte[] newHeaderBytes = packageHeader.constructHeader((uint)newFileEntries.Length, newIndexTableOffset, newIndexTableLength);

            ////////////////// STICK IT ALL TOGETHER: ////////////////// 

            uint outputLength = (uint)(0x60 + fileBlob.Count + newIndexTableBytes.Length);

            uint v3TerminatingSectionStart = 0;

            if (packageHeader.majorVersion == 3) { //add terminating area for weird hashes etc
                while (outputLength % 0x20 != 0)
                {
                    outputLength++;
                }
                v3TerminatingSectionStart = outputLength;
                outputLength += (uint)newFileEntries.Length * 8;
            }

            byte[] output = new byte[outputLength]; //we will get the size from the combined size of its constituent arrays, which we will have obtained by this point

            for (int i = 0; i < newHeaderBytes.Length; i++) {
                output[i] = newHeaderBytes[i];
            }

            for (int i = 0; i < fileBlob.Count; i++) { //put the file blob into the output buffer, after the header
                output[0x60 + i] = fileBlob[i];
            }

            int indexTableStart = 0x60 + fileBlob.Count; //fileBlob.Count is the number of bytes in the blob, not the number of files represented

            for (int i = 0; i < newIndexTableBytes.Length; i++) { //put the file blob into the output buffer, after the header
                output[indexTableStart + i] = newIndexTableBytes[i];
            }

            if (packageHeader.majorVersion == 3) {
                MessageBox.Show("You're saving a V3 package (as seen in MSA). Modified V3 packages have previously been known to crash the game - it might've been fixed, but there could also still be edge cases lingering about.");

                FileEntry[] filesOrderedByTypeThenHash = newFileEntries.OrderBy(f => f.typeID.id).ThenBy(f => f.hash).ToArray(); //put the files back into this order for the checksum writing

                for (int i = 0; i < filesOrderedByTypeThenHash.Length; i++) {  //fill in the checksum blob at the end of an MSA file
                    byte[] checksumBytes = getBytesUInt64(filesOrderedByTypeThenHash[i].checksum, packageHeader.isBigEndian);
                    for (int j = 0; j < checksumBytes.Length; j++) {
                        output[v3TerminatingSectionStart + (i * 8) + j] = checksumBytes[j];
                    } 
                }
            }

            Debug.WriteLine("Do certain MSK packages need padding to a multiple of 0x20 at the end? Is it required to make them align in memory?");

            return output;
        }
    }
}
