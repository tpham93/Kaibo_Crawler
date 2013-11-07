// Source: http://interplayoflight.wordpress.com/2013/03/03/sharpdx-and-3d-model-loading/

using Assimp;
using SharpDX;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;

namespace DrawModel
{
    class Model
    {
        private List<ModelMesh> m_meshes;

        public Model(Assimp.Scene _scene, GraphicsDevice _device, SharpDX.Toolkit.Content.ContentManager _content)
        {
            if (!_scene.HasMeshes) throw new Exception("Model::Model(): No mesh data in imported scene.");

            // TODO: Preload all materials

            // Load Meshes
            m_meshes = new List<ModelMesh>();
            LoadFromNode(_scene, _scene.RootNode, _device, _content, Matrix.Identity);
        }

        private void LoadFromNode(Assimp.Scene _scene, Assimp.Node _node, GraphicsDevice _device, SharpDX.Toolkit.Content.ContentManager _content, Matrix _transform)
        {
            // Sum up transformations recursively
            _transform = FromMatrix(_node.Transform) * _transform;
            Matrix transformInvTr = Helpers.CreateInverseTranspose(ref _transform);

            // Recursive load from scene
            if (_node.HasChildren)
            foreach(Assimp.Node node in _node.Children)
            {
                LoadFromNode(_scene, node, _device, _content, _transform);
            }

            if (_node.HasMeshes)
            foreach (int meshIndex in _node.MeshIndices)
            {
                Assimp.Mesh mesh = _scene.Meshes[meshIndex];
                ModelMesh modelMesh = new ModelMesh();

                // if mesh has a diffuse texture extract it
                Assimp.Material material = _scene.Materials[mesh.MaterialIndex];
                if (material != null && material.GetTextureCount(TextureType.Diffuse) > 0)
                {
                    TextureSlot texture = material.GetTexture(TextureType.Diffuse, 0);
                    // Create new texture for mesh
                    var dxtexture = _content.Load<Texture2D>(texture.FilePath);
                    modelMesh.DiffuseTexture = dxtexture;
                }

                // Position is mandarory
                if (!mesh.HasVertices) throw new Exception("Model::Model(): Model has no vertices.");

                // Determine the elements in the vertex
                bool hasTexCoords = mesh.HasTextureCoords(0);
                bool hasColors = mesh.HasVertexColors(0);
                bool hasNormals = mesh.HasNormals;
                bool hasTangents = mesh.Tangents != null;
                bool hasBitangents = mesh.BiTangents != null;
                int numElements = 1 + (hasTexCoords ? 1 : 0) + (hasColors ? 1 : 0) + (hasNormals ? 1 : 0) + (hasTangents ? 1 : 0) + (hasBitangents ? 1 : 0);

                // Create vertex element list: Here starts the section of creating SharpDX stuff
                VertexElement[] vertexElements = new VertexElement[numElements];
                uint elementIndex = 0;
                vertexElements[elementIndex++] = new VertexElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0);
                int vertexSize = Utilities.SizeOf<Vector3>();

                if (hasColors)
                {
                    vertexElements[elementIndex++] = new VertexElement("COLOR", 0, SharpDX.DXGI.Format.R8G8B8A8_UInt, vertexSize);
                    vertexSize += Utilities.SizeOf<Color>();
                }
                if (hasNormals)
                {
                    vertexElements[elementIndex++] = new VertexElement("NORMAL", 0, SharpDX.DXGI.Format.R32G32B32_Float, vertexSize);
                    vertexSize += Utilities.SizeOf<Vector3>();
                }
                if (hasTangents)
                {
                    vertexElements[elementIndex++] = new VertexElement("TANGENT", 0, SharpDX.DXGI.Format.R32G32B32_Float, vertexSize);
                    vertexSize += Utilities.SizeOf<Vector3>();
                }
                if (hasBitangents)
                {
                    vertexElements[elementIndex++] = new VertexElement("BITANGENT", 0, SharpDX.DXGI.Format.R32G32B32_Float, vertexSize);
                    vertexSize += Utilities.SizeOf<Vector3>();
                }
                if (hasTexCoords)
                {
                    vertexElements[elementIndex++] = new VertexElement("TEXCOORD", 0, SharpDX.DXGI.Format.R32G32_Float, vertexSize);
                    vertexSize += Utilities.SizeOf<Vector2>();
                }

                // Set the vertex elements and size
                modelMesh.InputLayout = VertexInputLayout.New( VertexBufferLayout.New(0, vertexElements) );
                modelMesh.VertexSize = vertexSize;

                // Determine primitive type
                switch (mesh.PrimitiveType)
                {
                    case Assimp.PrimitiveType.Point:    modelMesh.PrimitiveTopology = SharpDX.Toolkit.Graphics.PrimitiveType.PointList;    break;
                    case Assimp.PrimitiveType.Line:     modelMesh.PrimitiveTopology = SharpDX.Toolkit.Graphics.PrimitiveType.LineList;     break;
                    case Assimp.PrimitiveType.Triangle: modelMesh.PrimitiveTopology = SharpDX.Toolkit.Graphics.PrimitiveType.TriangleList; break;
                    default:                            throw new Exception("Model::Model(): Unknown primitive type");
                }

                // Create data stream for vertices
                //System.IO.MemoryStream vertexStream = new System.IO.MemoryStream(mesh.VertexCount * vertexSize);
                DataStream vertexStream = new DataStream(mesh.VertexCount * vertexSize, true, true);

                for (int i = 0; i < mesh.VertexCount; i++)
                {
                                       vertexStream.Write<Vector3>(Helpers.Transform(FromVector(mesh.Vertices[i]), ref _transform));
                    if (hasColors)     vertexStream.Write<Color>(FromColor(mesh.GetVertexColors(0)[i]));
                    if (hasNormals)    vertexStream.Write<Vector3>(Helpers.Transform(FromVector(mesh.Normals[i]), ref transformInvTr));
                    if (hasTangents)   vertexStream.Write<Vector3>(Helpers.Transform(FromVector(mesh.Tangents[i]), ref transformInvTr));
                    if (hasBitangents) vertexStream.Write<Vector3>(Helpers.Transform(FromVector(mesh.BiTangents[i]), ref transformInvTr));
                    if (hasTexCoords)  vertexStream.Write<Vector2>(new Vector2(mesh.GetTextureCoords(0)[i].X, mesh.GetTextureCoords(0)[i].Y));
                }

                vertexStream.Position = 0;

                // Create new vertex buffer
                var vertexBuffer = SharpDX.Toolkit.Graphics.Buffer.Vertex.New(_device, vertexStream);

                // Add it to the mesh
                modelMesh.VertexBuffer = vertexBuffer;
                modelMesh.VertexCount = mesh.VertexCount;
                modelMesh.PrimitiveCount = mesh.FaceCount;

                // Create new index buffer
                var indexBuffer = SharpDX.Toolkit.Graphics.Buffer.Index.New(_device, mesh.GetIndices());

                // Add it to the mesh
                modelMesh.IndexBuffer = indexBuffer;
                modelMesh.IndexCount = mesh.GetIndices().GetLength(0);

                m_meshes.Add(modelMesh);
            }
        }

        // Import conversation helper
        private Matrix FromMatrix(Matrix4x4 mat)
        {
            return new Matrix(mat.A1, mat.B1, mat.C1, mat.D1,
                mat.A2, mat.B2, mat.C2, mat.D2,
                mat.A3, mat.B3, mat.C3, mat.D3,
                mat.A4, mat.B4, mat.C4, mat.D4);
        }

        private Vector3 FromVector(Vector3D vec)
        {
            return new Vector3(vec.X, vec.Y, vec.Z);
        }

        private Color FromColor(Color4D color)
        {
            return new Color(color.R, color.G, color.B, color.A);
        }

        public void Draw(GraphicsDevice _graphicsDevice, Effect _effect)
        {
            foreach( ModelMesh mesh in m_meshes )
            {
                _graphicsDevice.SetVertexBuffer(0, mesh.VertexBuffer, mesh.VertexSize);
                _graphicsDevice.SetVertexInputLayout(mesh.InputLayout);
                _graphicsDevice.SetIndexBuffer(mesh.IndexBuffer, true);
                if( mesh.DiffuseTexture != null )
                    _effect.Parameters["diffuseTexture"].SetResource(mesh.DiffuseTexture);

                //_graphicsDevice.DrawIndexed(mesh.PrimitiveTopology, mesh.IndexCount);
                _graphicsDevice.Draw(mesh.PrimitiveTopology, mesh.VertexCount);
            }
        }
    }
}
