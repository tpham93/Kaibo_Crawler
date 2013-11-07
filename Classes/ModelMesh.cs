using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using SharpDX;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Graphics;
using SharpDX.DXGI;
using SharpDX.D3DCompiler;

//using Buffer = SharpDX.Direct3D10.Buffer;
//using Device = SharpDX.Direct3D10.Device;

namespace DrawModel
{
    class ModelMesh
    {
        VertexInputLayout m_inputLayout;
        public VertexInputLayout InputLayout
        {
            set { m_inputLayout = value; }
            get { return m_inputLayout; }
        }

        int m_vertexSize;
        public int VertexSize
        {
            set { m_vertexSize = value; }
            get { return m_vertexSize; }
        }

        SharpDX.Toolkit.Graphics.Buffer m_vertexBuffer;
        public SharpDX.Toolkit.Graphics.Buffer VertexBuffer
        {
            set { m_vertexBuffer = value; }
            get { return m_vertexBuffer; }
        }

        SharpDX.Toolkit.Graphics.Buffer m_indexBuffer;
        public SharpDX.Toolkit.Graphics.Buffer IndexBuffer
        {
            set { m_indexBuffer = value; }
            get { return m_indexBuffer; }
        }

        int m_vertexCount;
        public int VertexCount
        {
            set { m_vertexCount = value; }
            get { return m_vertexCount; }
        }

        int m_indexCount;
        public int IndexCount
        {
            set { m_indexCount = value; }
            get { return m_indexCount; }
        }

        int m_primitiveCount;
        public int PrimitiveCount
        {
            set { m_primitiveCount = value; }
            get { return m_primitiveCount; }
        }

        PrimitiveType m_primitiveTopology;
        public PrimitiveType PrimitiveTopology
        {
            set { m_primitiveTopology = value; }
            get { return m_primitiveTopology; }
        }

        Texture2D m_diffuseTexture;
        public Texture2D DiffuseTexture
        {
            set { m_diffuseTexture = value; }
            get { return m_diffuseTexture; }
        }

        // Dispose D3D related resources
        public void Dispose()
        {
            m_vertexBuffer.Dispose();
            m_indexBuffer.Dispose();
        }

    }

}
