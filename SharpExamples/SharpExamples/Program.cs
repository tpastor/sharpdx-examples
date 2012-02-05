// Copyright (c) 2010-2011 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// -----------------------------------------------------------------------------
// Original code from SlimDX project.
// Greetings to SlimDX Group. Original code published with the following license:
// -----------------------------------------------------------------------------
/*
* Copyright (c) 2007-2011 SlimDX Group
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/
using System;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using System.IO;
using System.Windows.Forms;
using SharpExamples;
using SharpDX.Multimedia;
using SharpDX.RawInput;
using System.Runtime.InteropServices;
using System.Collections.Generic;


namespace MiniTri
{
    /// <summary>
    ///   SharpDX port of SharpDX-MiniTri Direct3D 11 Sample
    /// </summary>
    internal static class Program
    {
        // default pp steps
        private static aiPostProcessSteps ppsteps =
            //aiPostProcessSteps.aiProcess_CalcTangentSpace | // calculate tangents and bitangents if possible
            aiPostProcessSteps.aiProcess_JoinIdenticalVertices | // join identical vertices/ optimize indexing
            aiPostProcessSteps.aiProcess_ValidateDataStructure | // perform a full validation of the loader's output
            aiPostProcessSteps.aiProcess_ImproveCacheLocality | // improve the cache locality of the output vertices
            aiPostProcessSteps.aiProcess_RemoveRedundantMaterials | // remove redundant materials
            aiPostProcessSteps.aiProcess_FindDegenerates | // remove degenerated polygons from the import
            aiPostProcessSteps.aiProcess_FindInvalidData | // detect invalid model data, such as invalid normal vectors
            aiPostProcessSteps.aiProcess_GenUVCoords | // convert spherical, cylindrical, box and planar mapping to proper UVs            
            aiPostProcessSteps.aiProcess_FindInstances | // search for instanced meshes and remove them by references to one master
            aiPostProcessSteps.aiProcess_LimitBoneWeights | // limit bone weights to 4 per vertex
            aiPostProcessSteps.aiProcess_OptimizeMeshes | // join small meshes, if possible;
            (aiPostProcessSteps)0;


        [STAThread]
        private static void Main()
        {           
            
            var form = new RenderForm("SharpDX - MiniTri Direct3D 11 Sample");

            // SwapChain description
            var desc = new SwapChainDescription()
                           {
                               BufferCount = 3,
                               ModeDescription= 
                                   new ModeDescription(form.ClientSize.Width, form.ClientSize.Height,
                                                       new Rational(60, 1), Format.R8G8B8A8_UNorm),
                               IsWindowed = true,
                               OutputHandle = form.Handle,
                               SampleDescription = new SampleDescription(1,0),
                               SwapEffect = SwapEffect.Sequential,
                               Usage = Usage.RenderTargetOutput
                               
                           };

            // Create Device and SwapChain
            Device device;
            SwapChain swapChain;
            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None, desc, out device, out swapChain);
            var context = device.ImmediateContext;

            // Ignore all windows events
            var factory = swapChain.GetParent<Factory>();
            factory.MakeWindowAssociation(form.Handle, WindowAssociationFlags.IgnoreAll);
                        
            
            // New RenderTargetView from the backbuffer
            var backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);
            var renderView = new RenderTargetView(device, backBuffer);

            Texture2DDescription depthBufferDesc;
            depthBufferDesc.Width = form.Width;
            depthBufferDesc.Height = form.Height;
            depthBufferDesc.MipLevels = 1;
            depthBufferDesc.ArraySize = 1;
            depthBufferDesc.Format = Format.D24_UNorm_S8_UInt;
            depthBufferDesc.SampleDescription.Count = 1;
            depthBufferDesc.SampleDescription.Quality = 0;
            depthBufferDesc.Usage = ResourceUsage.Default;
            depthBufferDesc.BindFlags = BindFlags.DepthStencil;
            depthBufferDesc.CpuAccessFlags = CpuAccessFlags.None ;
            depthBufferDesc.OptionFlags = ResourceOptionFlags.None;

            Texture2D DepthStencilTexture = new Texture2D(device, depthBufferDesc);

            DepthStencilViewDescription depthStencilViewDesc = new DepthStencilViewDescription();
            depthStencilViewDesc.Format = Format.D24_UNorm_S8_UInt;
            depthStencilViewDesc.Dimension =  DepthStencilViewDimension.Texture2D;
            depthStencilViewDesc.Texture2D.MipSlice = 0;
            DepthStencilView depthStencilView = new DepthStencilView(device, DepthStencilTexture, depthStencilViewDesc);
            context.OutputMerger.SetTargets(depthStencilView,renderView);


            DepthStencilStateDescription depthDesc = new DepthStencilStateDescription()
            {
                IsDepthEnabled = true,
                IsStencilEnabled = false,
                DepthWriteMask = DepthWriteMask.All,
                DepthComparison = Comparison.Less
            };
            DepthStencilState depthStencilState = new DepthStencilState(device, depthDesc);

            RasterizerStateDescription rasdesc = new RasterizerStateDescription()
            {
                CullMode = CullMode.None,
                FillMode = FillMode.Solid,
                IsFrontCounterClockwise = true,
                DepthBias = 0,
                DepthBiasClamp = 0,
                SlopeScaledDepthBias = 0,
                IsDepthClipEnabled = true,
                IsMultisampleEnabled =true,                            
            };
            context.Rasterizer.State = new RasterizerState(device, rasdesc);


            //////////////////////////////

            var flags = (ppsteps |
                aiPostProcessSteps.aiProcess_GenSmoothNormals | // generate smooth normal vectors if not existing
                aiPostProcessSteps.aiProcess_SplitLargeMeshes | // split large, unrenderable meshes into submeshes
                aiPostProcessSteps.aiProcess_Triangulate | // triangulate polygons with more than 3 edges
                aiPostProcessSteps.aiProcess_ConvertToLeftHanded | // convert everything to D3D left handed space
                aiPostProcessSteps.aiProcess_SortByPType | // make 'clean' meshes which consist of a single typ of primitives
                (aiPostProcessSteps)0);

            // default model
            var path = @"C:\Users\thiago.dias.pastor\Desktop\assimp--2.0.863-sdk\test\models\MS3D\jeep1.ms3d";

            Importer importer = new Importer();            

            //var path = "man.3ds";
            aiScene scene = importer.ReadFile(path, flags);
            String directory = null;
            if (scene != null)
            {
                directory = Path.GetDirectoryName(path);                
            }
            else
            {
                MessageBox.Show("Failed to open file: " + path + ". Either Assimp screwed up or the path is not valid.");
                Application.Exit();
            }

            SceneLoader SceneLoader = new SceneLoader();
            List<model> models = new List<model>();
            for (int i = 0; i < scene.mNumMeshes; i++)
			{
                models.Add(SceneLoader.CreateMesh(device, scene.mMeshes[i], scene.mMaterials,directory));                
			}


            //////////////////////////////

            // Compile Vertex and Pixel shaders
            var effectByteCode = ShaderBytecode.CompileFromFile("MiniTri.fx", "fx_5_0", ShaderFlags.None, EffectFlags.None);
            var effect = new Effect(device, effectByteCode);
            var technique = effect.GetTechniqueByIndex(0);
            var pass = technique.GetPassByIndex(0);

            // Layout from VertexShader input signature
            var passSignature = pass.Description.Signature;

            // Layout from VertexShader input signature
            var layout = new InputLayout(
                device,
                passSignature,
                new[]
                    {
                        new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                        new InputElement("TEXCOORD", 0, Format.R32G32_Float, 12, 0)
                    });            

            // Prepare All the stages            
            context.Rasterizer.SetViewports(new Viewport(0, 0, form.ClientSize.Width, form.ClientSize.Height, 0.0f, 1.0f));                        
            context.OutputMerger.DepthStencilState = depthStencilState;                
            
            Input input = new Input(form);
            //FreeLook FreeLook = new SharpExamples.FreeLook(input, (float)form.Width / (float)form.Height);
            CameraFirstPerson FreeLook = new CameraFirstPerson(input, 0, 0, Vector3.Zero, form.Width, form.Height);
            //FreeLook.SetEyeTarget(new Vector3(300), Vector3.Zero);
            Clock Clock = new SharpExamples.Clock();
            Clock.Start();

            effect.GetVariableByName("projection").AsMatrix().SetMatrix(FreeLook.Projection);

            // Main loop
            RenderLoop.Run(form, () =>
                                      {
                                          foreach (var item in models)
                                          {
                                                context.InputAssembler.InputLayout = layout;
                                                context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
                                                int SizeInBytes = Marshal.SizeOf(typeof(VertexPostitionTexture));
                                                context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(item.vertex, SizeInBytes, 0));
                                                context.InputAssembler.SetIndexBuffer(item.indices, Format.R32_UInt, 0);

                                                float elapsed = Clock.Update();
                                                FreeLook.Update(elapsed);

                                                effect.GetVariableByName("view").AsMatrix().SetMatrix(FreeLook.View);
                                                effect.GetVariableByName("World").AsMatrix().SetMatrix(Matrix.Scaling(5));
                                                effect.GetVariableByName("tex0").AsShaderResource().SetResource(item.ShaderResourceView);
                                                context.ClearRenderTargetView(renderView, Colors.Black);
                                                context.ClearDepthStencilView(depthStencilView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);

                                                for (int i = 0; i < technique.Description.PassCount; ++i)
                                                {
                                                    pass.Apply(context);
                                                    context.DrawIndexed(item.numberIndices, 0, 0);
                                                }
                                                swapChain.Present(0, PresentFlags.None);
                                          }

                                          
                                      });

            // Release all resources
            foreach (var item in models)
            {
                item.vertex.Dispose();
                item.indices.Dispose();
                item.ShaderResourceView.Dispose();                
            }
            
            layout.Dispose();
            renderView.Dispose();
            backBuffer.Dispose();
            context.ClearState();
            context.Flush();
            device.Dispose();
            context.Dispose();
            swapChain.Dispose();
            factory.Dispose();
        }
    }
}