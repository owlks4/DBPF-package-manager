using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Ribbon;
using System.Windows.Media.Media3D;
using static DBPF_package_manager.PackageIndexTable;
using static DBPF_package_manager.ReversibleBinaryRead;
using UnluacNET;
using SharpGLTF;
using System.Security.Policy;

namespace DBPF_package_manager
{
    public static class TypeIDs
    {
        //Don't alter the type name strings unless you know what you're doing! Here's why:
        //The type names may be important; it seems as though it _IMG files might have to come first in the data blob, for example? (But this isn't related to the order of the type ids in the index table). Which is why they have that underscore before their name - the same as their Sims 3 documentation, interestingly enough.

        private static TypeID[] typeIDs = new TypeID[] {
            new TypeID("_IMG_MSK",0x00B2D882, "img", exportImage, replaceImage),
            new TypeID("_IMG_MSA",0x92AA4D6A, "img", exportImage, replaceImage),
            new TypeID("_MTD_MSK",0x01D0E75D,"matd"),
            new TypeID("_MTD_MSA",0xE6640542,"matd"),
            new TypeID("_MTS_MSK",0x02019972,"mtst"),
            new TypeID("_MTS_MSA",0x787E842A,"mtst"),
            new TypeID("CLIP_MSK",0x6B20C4F3,"animclip"),
            new TypeID("CLIP_MSA",0xD6BEDA43,"animclip"),
            new TypeID("GEOM_MSK",0xF9E50586,"rmdl", exportModel, replaceModel),
            new TypeID("GEOM_MSA",0x2954E734,"rmdl", exportModel, replaceModel),
            new TypeID("GEOM_MSPC",0xB359C791,"wmdl", exportModel, replaceModel),
            new TypeID("GRND_MSK",0xD5988020,"hkx"),
            new TypeID("GRND_MSA",0x1A8FEB14,"hkx"),
            new TypeID("GRPT_MS",0x2c81b60a,"fpst"),
            new TypeID("GRPT_MSK",0x8101A6EA,"fpst"),
            new TypeID("GRPT_MSA",0x0EFC1A82,"fpst"),
            new TypeID("GRRG_MSK",0x8EAF13DE,"rig"),
            new TypeID("GRRG_MSA",0x4672E5BD,"rig"),
            new TypeID("IMAG_SPORE",0x2F7D0004, "img", exportImage, replaceImage),
            new TypeID("PART_MS",0x9752e396,"swm"),
            new TypeID("PART_MSK",0xcf60795e,"swm"),
            new TypeID("PART_MSA",0x28707864,"swm"),
            new TypeID("SLOT_MSK",0x487BF9E4,"slot"),
            new TypeID("SLOT_MSA",0x2EF1E401,"slot"),
            new TypeID("SNAPPOINTDATA_MSK",0xB70F1CEA,"spd"),
            new TypeID("SNAPPOINTDATA_MSA",0x5027B4EC,"spd"),
            new TypeID("OBJECTGRIDVOLUMEDATA_MSK",0xD00DECF5,"ogvd"),
            new TypeID("OBJECTGRIDVOLUMEDATA_MSA",0x8FC0DE5A,"ogvd"),
            new TypeID("LTST_MSA",0xE55D5715,"ltst"),

            new TypeID("BNK_MSK",0xB6B5C271,"bnk"),
            new TypeID("BNK_MSA",0x2199BB60,"bnk"),
            new TypeID("BIG_MSK",0x5bca8c06,"big"),
            new TypeID("BIG_MSA",0x2699C28D,"big"),

            new TypeID("FX",0x6B772503,"fx"),
            new TypeID("LUAC_MSA",0x3681D75B,"lua", exportLuac, replaceLuac),
            new TypeID("LUAC_MSK",0x2B8E2411,"lua", exportLuac, replaceLuac),

            new TypeID("BUILDABLEREGION_MSA",0x41C4A8EF,"buildableregion"),
            new TypeID("BUILDABLEREGION_MSK",0xC84ACD30,"buildableregion"),
            new TypeID("LLMF_MSK",0x58969018,"llmf"),
            new TypeID("LLMF_MSA",0xA5DCD485,"llmf"),

            new TypeID("TTF_MSK",0x89AF85AD,"ttf"),
            new TypeID("TTF_MSA",0x276CA4B9,"ttf"),

            new TypeID("VOXELGRIDDATA_MSK",0x614ED283,"vgd"),
            new TypeID("VOXELGRIDDATA_MSA",0x9614D3C0,"vgd"),
            new TypeID("MODEL_MS",0x01661233,"model"),
            new TypeID("KEYNAMEMAP_MS",0x0166038c,"keynamemap"),
            new TypeID("GEOMETRY_MS",0x015A1849,"geometry"),
            new TypeID("OLDSPEEDTREE_MS",0x00b552ea,"oldspeedtree"),
            new TypeID("SPEEDTREE_MS",0x021d7e8c,"speedtree"),
            new TypeID("COMPOSITETEXTURE_MS",0x8e342417,"compositetexture"),
            new TypeID("SIMOUTFIT_MS",0x025ed6f4,"simoutfit"),
            new TypeID("LEVELXML_MS",0x585ee310,"xml"),
            new TypeID("LUA_MSK",0x474999b4,"lua"), // This is just regular lua text, so doesn't need any custom export/replace options, as it doesn't have to be transformed in any way.
            new TypeID("LIGHTSETXML_MS",0x50182640,"xml"),
            new TypeID("LIGHTSETBIN_MSK",0x50002128,"ltst"),
            new TypeID("XML_MS",0xdc37e964,"xml"),
            new TypeID("XML2_MS",0x6d3e3fb4,"xml"),
            new TypeID("OBJECTCONSTRUCTIONXML_MS",0xc876c85e,"objectconstructionxml"),
            new TypeID("OBJECTCONSTRUCTIONBIN_MS",0xc08ec0ee,"objectconstruction"),
            new TypeID("SLOTXML_MS",0x4045d294,"xml"),

            new TypeID("XMLBIN_MS",0xe0d83029,"xmlbin"),
            new TypeID("CABXML_MS",0xa6856948,"xml"),
            new TypeID("CABBIN_MS",0xc644f440,"cabbin"),
            new TypeID("LIGHTBOXXML_MS",0xb61215e9,"xml"),
            new TypeID("LIGHTBOXBIN_MS",0xd6215201,"lightboxbin"),
            new TypeID("XMB_MS",0x1e1e6516,"xmb"),

            new TypeID("STR#_SIMS2",0x53545223,"str#"),
            new TypeID("CTSS_SIMS2",0x43545353, "ctss"),
            new TypeID("BHAV_SIMS2",0x42484156,"bhav"),
            new TypeID("OBJD_SIMS2",0x4F424A44,"objd"),
            new TypeID("OBJf_SIMS2",0x4F424A66,"objf"),
            new TypeID("CLST_SIMS2",0xE86B1EEF,"clst")
        };

        public class TypeID {
            public string name;
            public uint id;
            public string extension;
            public Func<FileEntry, ExportDetails> typeIDSpecificExport;
            public Action<FileEntry> typeIDSpecificReplace;
            public string info = "";

            public TypeID(string name, uint id, string extension) {
                this.name = name;
                this.id = id;
                this.extension = extension;
                this.typeIDSpecificExport = exportRaw;
                this.typeIDSpecificReplace = replaceRaw;
            }

            public TypeID(string name, uint id, string extension, Func<FileEntry, ExportDetails> export, Action<FileEntry> replace)
            {
                this.name = name;
                this.id = id;
                this.extension = extension;

                if (export == null){
                    this.typeIDSpecificExport = exportRaw;
                }
                else {
                    this.typeIDSpecificExport = export;
                }

                if (replace == null) {
                    this.typeIDSpecificReplace = replaceRaw;
                }
                else {
                    this.typeIDSpecificReplace = replace;
                }
            }
        }

        public static TypeID getTypeIDById(uint id) {
            foreach (TypeID t in typeIDs) {
                if (t.id == id) {
                    return t;
                }            
            }

            // If we reach this point, we weren't able to find that typeID in the list, so we add one to the list just for this session, so that if we encounter another instance of this type, it *will* find it in the list and be counted as the same type.

            TypeID[] amendedTypeIDArray = new TypeID[typeIDs.Length + 1];

            string idAsHexString = "0x" + Convert.ToHexString(BitConverter.GetBytes(id).Reverse().ToArray());

            TypeID newTypeID = new TypeID("Type_"+ idAsHexString, id, "unknownType_"+ idAsHexString);

            for (int i = 0; i < typeIDs.Length; i++) {
                amendedTypeIDArray[i] = typeIDs[i];            
            }
            amendedTypeIDArray[amendedTypeIDArray.Length - 1] = newTypeID;

            typeIDs = amendedTypeIDArray;

            return newTypeID;        
        }

        public static TypeID getTypeIDByName(string name) {
            foreach (TypeID t in typeIDs){
                if (t.name == name){
                    return t;
                }
            }
            return null;
        }

        public static TypeID[] getAllTypeIDsWithExtension(string extension)
        {
            List<TypeID> output = new List<TypeID>();
            foreach (TypeID t in typeIDs){
                if (t.extension == extension){
                    output.Add(t);
                }
            }
            return output.ToArray();
        }

        private static string makeFilterStringForAlternatingDescsAndExtensions(string[] input) {

            string output = "";

            for (int i = 0; i < input.Length; i++) {

                if (i % 2 == 0){
                    if (i != 0) {
                        output += "|";
                    }
                    output += input[i];
                }
                else {
                    string ext = input[i];
                    output += " (*." + ext + ") | *." + ext;
                }
            }
            return output;        
        }

        public class ExportDetails {
            public byte[] content;
            public string extension;
            public string filter;

            public ExportDetails(byte[] content, string extension, string filter) {
                this.content = content;
                this.extension = extension;
                this.filter = filter;
            }        
        }

        public static void replaceRaw(FileEntry f) {

            Debug.WriteLine("Replacing with generic replace");

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = makeFilterStringForAlternatingDescsAndExtensions(new string[] {f.typeID.name, f.typeID.extension});
            openFileDialog.Title = "Replace file";

            if (openFileDialog.ShowDialog() == true)
            {
                f.substitutionBytes = File.ReadAllBytes(openFileDialog.FileName);
                Debug.WriteLine("Need to re-evaluate compression status here possibly?");
            }
        }

        public static ExportDetails exportRaw(FileEntry f) {
            return new ExportDetails(MainWindow.package.getSpecificFileEntryContent(f), f.typeID.extension, makeFilterStringForAlternatingDescsAndExtensions(new string[] { f.typeID.name, f.typeID.extension }));
        }

        public static void replaceModel(FileEntry f)
        {
            MessageBox.Show("Model replacements are not currently allowed (GLTF compatibility needs to be created first).");
            return;

            byte[] bytesOfFileToReplace = MainWindow.package.getSpecificFileEntryContent(f);
            uint magicOfFileToReplace = BitConverter.ToUInt32(bytesOfFileToReplace, 0);

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = makeFilterStringForAlternatingDescsAndExtensions(new string[] { "GLTF model", "gltf"});
            openFileDialog.Title = "Replace file";

            if (openFileDialog.ShowDialog() == true)
            {
                SharpGLTF.Schema2.ModelRoot model = SharpGLTF.Schema2.ModelRoot.Load(openFileDialog.FileName);
                ModelWrapper wrapper = new ModelWrapper(model);

                if (magicOfFileToReplace == 0x4C444D52) { //RMDL
                    f.substitutionBytes = wrapper.dumpRevolutionModelBytes();
                }
                else if (magicOfFileToReplace == 0x4C444D57) { //WMDL
                    f.substitutionBytes = wrapper.dumpWindowsModelBytes();
                } else {
                    MessageBox.Show("Unknown file magic on the model format you're trying to replace.","Error");
                }

                Debug.WriteLine("Need to re-evaluate compression status here possibly?");
            }
        }

        public static ExportDetails exportModel(FileEntry f)
        {
            //temporarily defaulting to the raw export method
            MessageBox.Show("Temporarily exporting model in its raw format (you probably won't be able to do much with this)");
            return exportRaw(f);
        }

        public static void replaceLuac(FileEntry f)
        {
            MessageBox.Show("Lua replacements are not currently allowed (lua compiling with a particular version of the lua compiler needs to be implemented first.)");
        }

        public static ulong[] bannedLuaHashes = new ulong[] { 16869782828849130396, 3213009328539534276, 9958500636700497877, 16001598926280908521,
            11266562898505549506, 17288546901413378355, 2147615780746242819, 7158314187567625540, 15714336265186944586, 8738643847246926277, 15009483124799770057};

        public static ExportDetails exportLuac(FileEntry f)
        {
            foreach (ulong h in bannedLuaHashes) {
                if (f.hash == h) {
                    MessageBox.Show("File with hash " + f.hash + " is known to cause a crash on export due to a problem with unluac; exporting it as raw luac.");
                    return new ExportDetails(MainWindow.package.getSpecificFileEntryContent(f), "luac" , makeFilterStringForAlternatingDescsAndExtensions(new string[] { f.typeID.name, "luac" }));
                }
            }

            byte[] bytes = MainWindow.package.getSpecificFileEntryContent(f);

            Stream stream = new MemoryStream(bytes);

            var header = new BHeader(stream);

            LFunction lmain = header.Function.Parse(stream, header);

            Decompiler d = new Decompiler(lmain);
            d.Decompile();

            MemoryStream output = new MemoryStream();

            using (var writer = new StreamWriter(output))
            {
                Debug.WriteLine(f.hash);
                d.Print(new Output(writer));
                writer.Flush();
            }

            return new ExportDetails(output.ToArray(), f.typeID.extension, makeFilterStringForAlternatingDescsAndExtensions(new string[] { "Lua script", "lua" }));
        }

        public static void replaceImage(FileEntry f) {
            byte[] bytesOfFileToReplace = MainWindow.package.getSpecificFileEntryContent(f);
            uint magic = BitConverter.ToUInt32(bytesOfFileToReplace, 0);

            string filetype = f.typeID.name;
            string extension = "*";

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Replace file";

            switch (magic) {
                case 0x4901FE14:
                    filetype = "TPL image";
                    extension = "tpl";
                    break;
                case 0x20534444:
                    filetype = "DDS image";
                    extension = "dds";
                    break;
                case 0x474E5089:
                    filetype = "PNG image";
                    extension = "png";
                    break;
                default:
                    Debug.WriteLine("The parser isn't sure what kind of image we're meant to be replacing here! (No file magic matches.)");
                    break;
            }

            openFileDialog.Filter = makeFilterStringForAlternatingDescsAndExtensions(new string[] { filetype, extension });

            if (openFileDialog.ShowDialog() == true)
            {
                f.substitutionBytes = File.ReadAllBytes(openFileDialog.FileName);

                if (extension == "tpl") {
                    f.substitutionBytes = convertToSimsTPL(f.substitutionBytes, (ushort)(bytesOfFileToReplace[4] << 8 | bytesOfFileToReplace[5]));
                }
               
                Debug.WriteLine("Need to re-evaluate compression status here possibly?");
            }
        }

        public static ExportDetails exportImage(FileEntry f)
        {
            byte[] bytes = MainWindow.package.getSpecificFileEntryContent(f);

            uint magic = BitConverter.ToUInt32(bytes, 0);
   
            string filetype = "Unknown image format";
            string extension = "image";

            switch (magic) {
                case 0x4901FE14:
                    filetype = "TPL image";
                    extension = "tpl";
                    break;
                case 0x20534444:
                    filetype = "DDS image";
                    extension = "dds";
                    break;
                case 0x474E5089:
                    filetype = "PNG image";
                    extension = "png";
                    break;
                default:
                    Debug.WriteLine("Unhandled file magic on this image!");
                    break;
            }

            string filterString = makeFilterStringForAlternatingDescsAndExtensions(new string[] { filetype, extension });

            if (extension == "tpl") {
                return new ExportDetails(convertToNintendoTPL(bytes), extension, filterString);
            }
            else {
                return new ExportDetails(bytes, extension, filterString);
            }
          
        }

        public static byte[] convertToNintendoTPL(byte[] simsTPL) {

            BinaryReader reader = new BinaryReader(new MemoryStream(simsTPL));

            TPLHeader tpl = new TPLHeader(reader);

            byte[] headerBytes = tpl.dumpToNintendoHeaderBytes();
            byte[] output = new byte[headerBytes.Length + tpl.imageDataSize];
    
            reader.BaseStream.Position = tpl.imageDataOffset;

            byte[] imageData = reader.ReadBytes((int)tpl.imageDataSize);

            Array.Copy(headerBytes, 0, output, 0, headerBytes.Length);
            Array.Copy(imageData, 0, output, headerBytes.Length, imageData.Length);

            reader.Close();

            return output;
        }

        public static byte[] convertToSimsTPL(byte[] nintendoTPL, ushort version)
        {
            BinaryReader reader = new BinaryReader(new MemoryStream(nintendoTPL));

            TPLHeader tpl = new TPLHeader(reader);

            byte[] headerBytes = tpl.dumpToSimsHeaderBytes(version);
            byte[] output = new byte[headerBytes.Length + tpl.imageDataSize];

            reader.BaseStream.Position = tpl.imageDataOffset;

            byte[] imageData = reader.ReadBytes((int)tpl.imageDataSize);

            Array.Copy(headerBytes, 0, output, 0, headerBytes.Length);
            Array.Copy(imageData, 0, output, headerBytes.Length, imageData.Length);

            reader.Close();

            return output;
        }
    }
}