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

namespace maxis_package_manager
{
    internal class Package
    {
        private PackageHeader packageHeader;
        private PackageIndexTable indexTable;

        public Package(Stream f) {

            BinaryReader reader = new BinaryReader(f);
            
            packageHeader = new PackageHeader(reader);

            //then set the basestream position to the index table position identified in the header, and create an index table instance, etc

            reader.BaseStream.Position = packageHeader.indexOffset;

            indexTable = new PackageIndexTable(reader);
            

        }
    }
}
