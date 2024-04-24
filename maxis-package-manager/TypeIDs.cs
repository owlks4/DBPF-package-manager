using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace maxis_package_manager
{
    public static class TypeIDs
    {
        private static TypeID[] typeIDs = new TypeID[] {
            new TypeID("RMDL_MSK",0xF9E50586,"rmdl"),
            new TypeID("RMDL_MSA",0x2954E734,"rmdl"),
            new TypeID("WMDL_MSPC",0xB359C791,"wmdl"),
            new TypeID("MATD_MSK",0x01D0E75D,"matd"), 
            new TypeID("MATD_MSA",0xE6640542,"matd"),
            new TypeID("TPL_MSK",0x00B2D882,"tpl"),  
            new TypeID("TPL_MSA",0x92AA4D6A,"tpl"),
            new TypeID("MTST_MSK",0x02019972,"mtst"), 
            new TypeID("MTST_MSA",0x787E842A,"mtst"),
            new TypeID("FPST_MS",0x2c81b60a,"fpst"),   
            new TypeID("FPST_MSK",0x8101A6EA,"fpst"),
            new TypeID("FPST_MSA",0x0EFC1A82,"fpst"),
            new TypeID("BNK_MSK",0xB6B5C271,"bnk"),   
            new TypeID("BNK_MSA",0x2199BB60,"bnk"),
            new TypeID("BIG_MSK",0x5bca8c06,"big"),    
            new TypeID("BIG_MSA",0x2699C28D,"big"),    
            new TypeID("COLLISION_MSA",0x1A8FEB14,"collision"),
            new TypeID("FX",0x6B772503,"fx"),
            new TypeID("LUAC_MSA",0x3681D75B,"luac"),
            new TypeID("LUAC_MSK",0x2B8E2411,"luac"),  
            new TypeID("SLOT_MSK",0x487BF9E4,"slot"), 
            new TypeID("SLOT_MSA",0x2EF1E401,"slot"),  
            new TypeID("PARTICLES_MSA",0x28707864,"particles"),
            new TypeID("BUILDABLEREGION_MSA",0x41C4A8EF,"buildableregion"),
            new TypeID("BUILDABLEREGION_MSK",0xC84ACD30,"buildableregion"),
            new TypeID("LLMF_MSK",0x58969018,"llmf"),
            new TypeID("LLMF_MSA",0xA5DCD485,"llmf"),
            new TypeID("RIG_MSK",0x8EAF13DE,"grannyrig"),
            new TypeID("RIG_MSA",0x4672E5BD,"grannyrig"),
            new TypeID("ANIMCLIP_MSK",0x6B20C4F3,"animclip"),
            new TypeID("ANIMCLIP_MSA",0xD6BEDA43,"animclip"),
            new TypeID("LTST_MSA",0xE55D5715,"ltst"),
            new TypeID("TTF_MSK",0x89AF85AD,"ttf"),
            new TypeID("TTF_MSA",0x276CA4B9,"ttf"),
            new TypeID("HKX_MSK",0xD5988020,"hkx"),
            new TypeID("OBJECTGRIDVOLUMEDATA_MSK",0xD00DECF5,"ogvd"),
            new TypeID("OBJECTGRIDVOLUMEDATA_MSA",0x8FC0DE5A,"ogvd"),
            new TypeID("SNAPPOINTDATA_MSK",0xB70F1CEA,"spd"),
            new TypeID("SNAPPOINTDATA_MSA",0x5027B4EC,"spd"),
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
            new TypeID("SWARM_MS",0x9752e396,"swm"),
            new TypeID("SWARM_MSK",0xcf60795e,"swm"), 
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
            public TypeID(string name, uint id, string extension) {
                this.name = name;
                this.id = id;
                this.extension = extension;
            }    
        }

        public static TypeID getTypeIDById(uint id) {
            foreach (TypeID t in typeIDs) {
                if (t.id == id) {
                    return t;
                }            
            }
            return null;        
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
    }
}
