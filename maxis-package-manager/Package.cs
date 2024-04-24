using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Diagnostics;
using static maxis_package_manager.ReversibleBinaryRead;
using System.Reflection.Metadata.Ecma335;
using static maxis_package_manager.PackageIndexTable;

namespace maxis_package_manager
{
    public class Package
    {
        private PackageHeader packageHeader;
        public PackageIndexTable indexTable;

        public Package(Stream f) {

            BinaryReader reader = new BinaryReader(f);

            packageHeader = new PackageHeader(reader);

            reader.BaseStream.Position = packageHeader.indexOffset;

            indexTable = new PackageIndexTable(reader, packageHeader);
        }
        public byte[] dumpToByteArray() {

            //TODO: Put all files together, compressing where necessary

            //TODO: Call a function of the index table to reconstruct it according to its version and the files in the package

            //TODO: Then call a function of the package header to reconstruct THAT, feeding it the necessary offsets and sizes from the file data blob and index table we just produced

            //TODO: Finally, stick it all together and return the output
                        
            byte[] output = new byte[0]; //we will get the size from the combined size of its constituent arrays, which we will have obtained by this point

            //TODO: insert the header, file contents and index table into the output byte array

            return output;
        }
    }
}
