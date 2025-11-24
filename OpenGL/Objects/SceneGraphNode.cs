using System;
using System.Collections.Generic;
using System.Numerics;
using OpenTK.Graphics.OpenGL4;

namespace OpenGL.Objects
{
    public class SceneGraphNode : IDisposable
    {
        public Vertex[] Vertices { get; private set; } = [];
        public (int A, int B, int C)[] Tris { get; private set; } = [];

        public List<(SceneGraphNode Node, Matrix4x4 Transformation)> Children { get; } = new();

        private int vao = 0;
        private int vbo = 0;
        private int ebo = 0;
        private bool buffersCreated = false;

        public SceneGraphNode() { }

        public SceneGraphNode(IEnumerable<Vertex> verts, IEnumerable<(int A, int B, int C)> tris)
        {
            SetGeometry(verts, tris);
        }

        public void SetGeometry(IEnumerable<Vertex> verts, IEnumerable<(int A, int B, int C)> tris)
        {
            Vertices = verts is Vertex[] arrV ? arrV : new List<Vertex>(verts).ToArray();
            Tris = tris is (int, int, int)[] arrT ? arrT : new List<(int, int, int)>(tris).ToArray();

            // recreate GL buffers on next Load call
            DeleteBuffers();
            buffersCreated = false;
        }

        public void AddChild(SceneGraphNode child, Matrix4x4 localTransform)
            => Children.Add((child, localTransform));

        // Public API: create all GL buffers/resources. Should be called from application Load.
        public void Load()
        {
            if (buffersCreated) return;

            if (Vertices == null || Vertices.Length == 0 || Tris == null || Tris.Length == 0)
                return;

            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

            var vertexData = new float[Vertices.Length * 11];
            for (int i = 0; i < Vertices.Length; i++)
            {
                var v = Vertices[i];
                int baseIdx = i * 11;
                vertexData[baseIdx + 0] = v.Position.X;
                vertexData[baseIdx + 1] = v.Position.Y;
                vertexData[baseIdx + 2] = v.Position.Z;

                vertexData[baseIdx + 3] = v.Color.X;
                vertexData[baseIdx + 4] = v.Color.Y;
                vertexData[baseIdx + 5] = v.Color.Z;

                vertexData[baseIdx + 6] = v.TexCoord.X;
                vertexData[baseIdx + 7] = v.TexCoord.Y;

                vertexData[baseIdx + 8] = v.Normal.X;
                vertexData[baseIdx + 9] = v.Normal.Y;
                vertexData[baseIdx + 10] = v.Normal.Z;
            }

            GL.BufferData(BufferTarget.ArrayBuffer, vertexData.Length * sizeof(float), vertexData, BufferUsageHint.StaticDraw);

            // create EBO
            ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);

            var indices = new int[Tris.Length * 3];
            for (int i = 0; i < Tris.Length; i++)
            {
                indices[i * 3 + 0] = Tris[i].A;
                indices[i * 3 + 1] = Tris[i].B;
                indices[i * 3 + 2] = Tris[i].C;
            }

            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(int), indices, BufferUsageHint.StaticDraw);

            // Unbind to leave a clean state
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            buffersCreated = true;
        }

        public void Render(Matrix4x4 modelMatrix, Matrix4x4 viewProjectionMatrix, int shaderProgram, float time)
        {
            if (!buffersCreated)
                return; // ensure Load() was called; do not create buffers during render

            Matrix4x4 normalM;
            if (Matrix4x4.Invert(modelMatrix, out var inv))
                normalM = Matrix4x4.Transpose(inv);
            else
                normalM = Matrix4x4.Transpose(modelMatrix);

            var mvp = modelMatrix * viewProjectionMatrix;

            RenderTriangles(modelMatrix, viewProjectionMatrix, normalM, mvp, shaderProgram, time);

            foreach (var (Node, Transformation) in Children)
            {
                Node.Render(Transformation * modelMatrix, viewProjectionMatrix, shaderProgram, time);
            }
        }

        private void RenderTriangles(Matrix4x4 model, Matrix4x4 viewProj, Matrix4x4 normalM, Matrix4x4 mvp, int shaderProgram, float time)
        {
            if (!buffersCreated) return;

            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);

            const int floatsPerVertex = 11;
            const int stride = floatsPerVertex * sizeof(float);

            int posLoc = GL.GetAttribLocation(shaderProgram, "inPos");
            if (posLoc != -1)
            {
                GL.EnableVertexAttribArray(posLoc);
                GL.VertexAttribPointer(posLoc, 3, VertexAttribPointerType.Float, false, stride, 0);
            }

            int colorLoc = GL.GetAttribLocation(shaderProgram, "inColor");
            if (colorLoc != -1)
            {
                GL.EnableVertexAttribArray(colorLoc);
                GL.VertexAttribPointer(colorLoc, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
            }

            int texLoc = GL.GetAttribLocation(shaderProgram, "inTex");
            if (texLoc != -1)
            {
                GL.EnableVertexAttribArray(texLoc);
                GL.VertexAttribPointer(texLoc, 2, VertexAttribPointerType.Float, false, stride, 6 * sizeof(float));
            }

            int normalLoc = GL.GetAttribLocation(shaderProgram, "inNormal");
            if (normalLoc != -1)
            {
                GL.EnableVertexAttribArray(normalLoc);
                GL.VertexAttribPointer(normalLoc, 3, VertexAttribPointerType.Float, false, stride, 8 * sizeof(float));
            }

            int mLoc = GL.GetUniformLocation(shaderProgram, "inMatrix");
            if (mLoc != -1)
            {
                GL.UniformMatrix4(mLoc, 1, false, ref mvp.M11);
            }

            int normalMLoc = GL.GetUniformLocation(shaderProgram, "normalMatrix");
            if (normalMLoc != -1)
            {
                GL.UniformMatrix4(normalMLoc, 1, false, ref normalM.M11);
            }

            GL.Uniform1(GL.GetUniformLocation(shaderProgram, "inTime"), (float)time);

            int indexCount = Tris.Length * 3;
            GL.DrawElements(PrimitiveType.Triangles, indexCount, DrawElementsType.UnsignedInt, 0);

            if (posLoc != -1) GL.DisableVertexAttribArray(posLoc);
            if (colorLoc != -1) GL.DisableVertexAttribArray(colorLoc);
            if (texLoc != -1) GL.DisableVertexAttribArray(texLoc);
            if (normalLoc != -1) GL.DisableVertexAttribArray(normalLoc);

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        private void DeleteBuffers()
        {
            if (vao != 0) { GL.DeleteVertexArray(vao); vao = 0; }
            if (vbo != 0) { GL.DeleteBuffer(vbo); vbo = 0; }
            if (ebo != 0) { GL.DeleteBuffer(ebo); ebo = 0; }
            buffersCreated = false;
        }

        public void Dispose()
        {
            DeleteBuffers();
            GC.SuppressFinalize(this);
        }

        ~SceneGraphNode()
        {
            try { DeleteBuffers(); } catch { }
        }
    }
}
