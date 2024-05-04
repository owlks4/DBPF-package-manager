using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SharpGLTF.Schema2;
using static maxis_package_manager.ReversibleBinaryRead;

namespace maxis_package_manager
{
    public class ModelWrapper
    {
        public ModelRoot gltf = null;

        public ModelWrapper(ModelRoot gltf) {
            this.gltf = gltf;
        }

        public ModelWrapper(byte[] input, uint magic){
            BinaryReader reader = new BinaryReader(new MemoryStream(input));

            switch (magic) {
                case 0x4C444D52:
                    gltf = makeGLTFfromRevolutionModel(reader);
                    break;
                case 0x4C444D57:
                    gltf = makeGLTFfromWindowsModel(reader);
                    break;
            }

            reader.Close();
        }

        public ModelRoot makeGLTFfromRevolutionModel(BinaryReader reader){

            MessageBox.Show("CONVERSION FROM REVOLUTION MODEL TO GLTF NOT YET IMPLEMENTED!");

            return null;
        }

        public ModelRoot makeGLTFfromWindowsModel(BinaryReader reader) {

            MessageBox.Show("CONVERSION FROM WINDOWS MODEL TO GLTF NOT YET IMPLEMENTED!");

            return null;
        }

        public byte[] dumpRevolutionModelBytes() {
            List<byte> output = new List<byte>();

            MessageBox.Show("CONVERSION TO REVOLUTION MODEL NOT YET IMPLEMENTED!");

            return output.ToArray();
        }

        public byte[] dumpWindowsModelBytes() {
            List<byte> output = new List<byte>();

            MessageBox.Show("CONVERSION TO WINDOWS MODEL NOT YET IMPLEMENTED!");

            foreach (SharpGLTF.Schema2.Mesh m in gltf.LogicalMeshes) { // for example
                
            
            }

            return output.ToArray();
        }
    }
}
