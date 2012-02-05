using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using System.Runtime.InteropServices;

namespace SharpExamples
{
    public class model
    {
        public Buffer vertex;
        public Buffer indices;
        public int numberIndices;
        public int numberVertices;
        public String difuseTextureName;
        public ShaderResourceView ShaderResourceView;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct VertexPostitionTexture
    {
        public Vector3 position;
        public Vector2 textcoord;
    }

    public class SceneLoader
    {

        public model CreateMesh(Device device, aiMesh aiMesh,aiMaterialVector mMaterials, String directory )
        {
            var numFaces = (int)aiMesh.mNumFaces;
            var numVertices = (int)aiMesh.mNumVertices;
            var aiPositions = aiMesh.mVertices;
            var aiNormals = aiMesh.mNormals;
            var aiTextureCoordsAll = aiMesh.mTextureCoords;
            var aiTextureCoords = (aiTextureCoordsAll != null) ? aiTextureCoordsAll[0] : null;
                      
          VertexPostitionTexture[] VertexPostitionTextures = new VertexPostitionTexture[aiMesh.mNumVertices];
		  
		  for(int j = 0; j < aiMesh.mNumVertices;j++)
		  {
			  VertexPostitionTextures[j].position = new Vector3(aiMesh.mVertices[j].x,aiMesh.mVertices[j].y,aiMesh.mVertices[j].z);
			  VertexPostitionTextures[j].textcoord = new Vector2(aiMesh.mTextureCoords[0][j].x, aiMesh.mTextureCoords[0][j].y);
		  }	
            
            ///being brute =P
           int SizeInBytes = Marshal.SizeOf(typeof(VertexPostitionTexture));
           BufferDescription bd = new BufferDescription(SizeInBytes * (int)aiMesh.mNumVertices, ResourceUsage.Immutable, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, SizeInBytes);
           var vertices = Buffer.Create<VertexPostitionTexture>(device, VertexPostitionTextures, bd);

            var aiFaces = aiMesh.mFaces;
            var dxIndices = new uint[numFaces * 3];
            for (int i = 0; i < numFaces; ++i)
            {
                var aiFace = aiFaces[i];
                var aiIndices = aiFace.mIndices;
                for (int j = 0; j < 3; ++j)
                {
                    dxIndices[i * 3 + j] = (uint) aiIndices[j];
                }
            }
            BufferDescription bi = new BufferDescription(sizeof(uint) * numFaces * 3, ResourceUsage.Immutable, BindFlags.IndexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, sizeof(uint));
            var indices = Buffer.Create<uint>(device, dxIndices, bd);

            model modelteste = new model();
            modelteste.indices = indices;
            modelteste.numberIndices = numFaces * 3;
            modelteste.vertex = vertices;
            modelteste.numberVertices = numVertices;

            
            aiString difuse = new aiString();
            mMaterials[(int)aiMesh.mMaterialIndex].GetTextureDiffuse0(difuse);
            modelteste.difuseTextureName = difuse.Data;

            modelteste.ShaderResourceView = ShaderResourceView.FromFile(device, directory + "\\" + modelteste.difuseTextureName);

            return modelteste;
        }
    }
}
