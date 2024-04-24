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
    }
}
