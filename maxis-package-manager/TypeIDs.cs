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
using static maxis_package_manager.PackageIndexTable;
using static maxis_package_manager.ReversibleBinaryRead;

namespace maxis_package_manager
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
            new TypeID("GEOM_MSK",0xF9E50586,"rmdl"),
            new TypeID("GEOM_MSA",0x2954E734,"rmdl"),
            new TypeID("GEOM_MSPC",0xB359C791,"wmdl"),
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
            new TypeID("LUAC_MSA",0x3681D75B,"luac"),
            new TypeID("LUAC_MSK",0x2B8E2411,"luac"),

       
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
            new TypeID("LEVELXML_MS",0x585ee310,"levelxml"),
            new TypeID("LUA_MSK",0x474999b4,"lua"), 
            new TypeID("LIGHTSETXML_MS",0x50182640,"ltstxml"), 
            new TypeID("LIGHTSETBIN_MSK",0x50002128,"ltst"), 
            new TypeID("XML_MS",0xdc37e964,"xml"),
            new TypeID("XML2_MS",0x6d3e3fb4,"xml2"),
            new TypeID("OBJECTCONSTRUCTIONXML_MS",0xc876c85e,"objectconstructionxml"),
            new TypeID("OBJECTCONSTRUCTIONBIN_MS",0xc08ec0ee,"objectconstruction"),
            new TypeID("SLOTXML_MS",0x4045d294,"slotxml"),
        
            new TypeID("XMLBIN_MS",0xe0d83029,"xmlbin"),
            new TypeID("CABXML_MS",0xa6856948,"cabxml"),
            new TypeID("CABBIN_MS",0xc644f440,"cabbin"),
            new TypeID("LIGHTBOXXML_MS",0xb61215e9,"lightboxxml"),
            new TypeID("LIGHTBOXBIN_MS",0xd6215201,"lightboxbin"),
            new TypeID("XMB_MS",0x1e1e6516,"xmb")
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
            TypeID newTypeID = new TypeID("Type_"+id, id, "unknownType_"+id);

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

        public static void replaceImage(FileEntry f) {
            MessageBox.Show("Replacing images has not yet been implemented!");
        }

        public static ExportDetails exportImage(FileEntry f)
        {
            byte[] bytes = MainWindow.package.getSpecificFileEntryContent(f);

            uint magic = BitConverter.ToUInt32(bytes, 0);
   
            string filetype = "Unknown image format";
            string extension = "image";

            switch (magic) {
                case 0x4901FE14:
                    filetype = "Nintendo TPL image";
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

            if (filetype == "Nintendo TPL image"){
                return new ExportDetails(convertToNintendoTPL(bytes), extension, filterString);
            }
            else {
                return new ExportDetails(bytes, extension, filterString);
            }
        }

        public static byte[] convertToNintendoTPL(byte[] simsTPL) {

            byte version = simsTPL[5];

            BinaryReader reader = new BinaryReader(new MemoryStream(simsTPL));

            ushort width = 0;
            ushort height = 0;
            byte imageFormat = 0;
            uint imageCount = 0;
            uint imageDataOffset = 0;
            uint imageSize = 0;

            switch (version) {
                case 1:
                case 2:
                    reader.BaseStream.Position = 0x1C;
                    width = ReadUInt16(reader, true);
                    height = ReadUInt16(reader, true);
                    reader.BaseStream.Position += 3;
                    imageFormat = reader.ReadByte();
                    imageCount = ReadUInt32(reader, true);
                    imageDataOffset = 0x4C;
                    imageSize = (uint)(width * height * 4);
                    break;
                case 3:
                    reader.BaseStream.Position = 0x08;
                    imageSize = ReadUInt32(reader, true);
                    reader.BaseStream.Position = 0x18;
                    width = ReadUInt16(reader, true);
                    height = ReadUInt16(reader, true);
                    reader.BaseStream.Position += 3;
                    imageFormat = reader.ReadByte();
                    imageCount = ReadUInt32(reader, true);
                    reader.BaseStream.Position = 0x38;
                    imageDataOffset = ReadUInt32(reader, true);
                    break;
                default:
                    MessageBox.Show("Unknown Sims TPL type!");
                    break;
            }

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

            for (int i = 0; i < 12; i++) {
                output.AddRange(getBytesUInt32(0x00000000, true));
            }

            reader.BaseStream.Position = imageDataOffset;

            byte[] imageData = reader.ReadBytes((int)imageSize);

            output.AddRange(imageData);

            return output.ToArray();
        }
    }
}
