using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Numerics;

namespace OpenGL.Objects
{
    public class SceneGraphNode : IDisposable
    {
        public Vertex[] Vertices { get; private set; } = [];
        public (int A, int B, int C)[] Tris { get; private set; } = [];

        public List<(SceneGraphNode Node, Matrix4x4 Transformation)> Children { get; } = [];

        int vaoTriangle = 0;
        int vboTriangleIndices = 0;

        public SceneGraphNode() { }

        public SceneGraphNode(IEnumerable<Vertex> verts, IEnumerable<(int A, int B, int C)> tris)
        {
            Vertices = verts is Vertex[] arrV ? arrV : new List<Vertex>(verts).ToArray();
            Tris = tris is (int, int, int)[] arrT ? arrT : new List<(int, int, int)>(tris).ToArray();
        }

        public void AddChild(SceneGraphNode child, Matrix4x4 localTransform)
            => Children.Add((child, localTransform));

        // Edit child local transform
        public void SetChildTransform(SceneGraphNode child, Matrix4x4 localTransform)
        {
            for (int i = 0; i < Children.Count; i++)
            {
                if (Children[i].Node == child)
                {
                    Children[i] = (child, localTransform);
                    return;
                }
            }
        }

        public void Load(int hProgram)
        {
            if (Vertices == null || Vertices.Length == 0 || Tris == null || Tris.Length == 0)
                return;

            List<float> posList = [];
            List<float> texCoordList = [];
            List<float> colorList = [];
            List<float> normalList = [];

            foreach (var v in Vertices)
            {
                posList.Add(v.Position.X);
                posList.Add(v.Position.Y);
                posList.Add(v.Position.Z);
                colorList.Add(v.Color.X);
                colorList.Add(v.Color.Y);
                colorList.Add(v.Color.Z);
                normalList.Add(v.Normal.X);
                normalList.Add(v.Normal.Y);
                normalList.Add(v.Normal.Z);
                texCoordList.Add(v.TexCoord.X);
                texCoordList.Add(v.TexCoord.Y);
            }

            var indices = Tris.SelectMany(t => new[] { t.A, t.C, t.B }).ToArray();
            int indexCount = indices.Length;

            GL.UseProgram(hProgram);

            var vboTriangleVertices = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboTriangleVertices);
            GL.BufferData(BufferTarget.ArrayBuffer, posList.Count * sizeof(float), posList.ToArray(), BufferUsageHint.StaticDraw);

            // upload model indices to a vbo
            vboTriangleIndices = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, vboTriangleIndices);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(int), indices, BufferUsageHint.StaticDraw);

            // upload model colors to a vbo
            var vboColors = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboColors);
            GL.BufferData(BufferTarget.ArrayBuffer, colorList.Count * sizeof(float), colorList.ToArray(), BufferUsageHint.StaticDraw);

            var vboTexCoords = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboTexCoords);
            GL.BufferData(BufferTarget.ArrayBuffer, texCoordList.Count * sizeof(float), texCoordList.ToArray(), BufferUsageHint.StaticDraw);

            var vboNormals = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboNormals);
            GL.BufferData(BufferTarget.ArrayBuffer, normalList.Count * sizeof(float), normalList.ToArray(), BufferUsageHint.StaticDraw);

            //set up a vao
            vaoTriangle = GL.GenVertexArray();
            GL.BindVertexArray(vaoTriangle);
            var posAttribIndex = GL.GetAttribLocation(hProgram, "inPos");
            if (posAttribIndex != -1)
            {
                GL.EnableVertexAttribArray(posAttribIndex);
                GL.BindBuffer(BufferTarget.ArrayBuffer, vboTriangleVertices);
                GL.VertexAttribPointer(posAttribIndex, 3, VertexAttribPointerType.Float, false, 0, 0);
            }

            var colorAttribIndex = GL.GetAttribLocation(hProgram, "inColor");
            if (colorAttribIndex != -1)
            {
                GL.EnableVertexAttribArray(colorAttribIndex);
                GL.BindBuffer(BufferTarget.ArrayBuffer, vboColors);
                GL.VertexAttribPointer(colorAttribIndex, 3, VertexAttribPointerType.Float, false, 0, 0);
            }

            var normalAttribIndex = GL.GetAttribLocation(hProgram, "inNormal");
            if (normalAttribIndex != -1) {
                GL.EnableVertexAttribArray(normalAttribIndex);
                GL.BindBuffer(BufferTarget.ArrayBuffer, vboNormals);
                GL.VertexAttribPointer(normalAttribIndex, 3, VertexAttribPointerType.Float, false, 0, 0);
            }

            var texCoordAttribIndex = GL.GetAttribLocation(hProgram, "inTexCoord");
            if (texCoordAttribIndex != -1)
            {
                GL.EnableVertexAttribArray(texCoordAttribIndex);
                GL.BindBuffer(BufferTarget.ArrayBuffer, vboTexCoords);
                GL.VertexAttribPointer(texCoordAttribIndex, 2, VertexAttribPointerType.Float, false, 0, 0);
            }

        }

        public void Render(Matrix4x4 modelMatrix, Matrix4x4 viewProjectionMatrix, int shaderProgram, float time)
        {
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

        private void RenderTriangles(Matrix4x4 model, Matrix4x4 viewProj, Matrix4x4 normalM, Matrix4x4 mvp, int hProgram, float time)
        {
            GL.UseProgram(hProgram);

            GL.BindVertexArray(vaoTriangle);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboTriangleIndices);

            GL.Uniform1(GL.GetUniformLocation(hProgram, "inTime"), (float)time);
            GL.UniformMatrix4(GL.GetUniformLocation(hProgram, "inMatrix"), 1, false, ref mvp.M11);

            int indexCount = Tris.Length * 3;

            GL.BindVertexArray(vaoTriangle);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, vboTriangleIndices);
            GL.DrawElements(PrimitiveType.Triangles, indexCount, DrawElementsType.UnsignedInt, 0);
        }

        private void DeleteBuffers()
        {
            if (vaoTriangle != 0) { GL.DeleteVertexArray(vaoTriangle); vaoTriangle = 0; }
            if (vboTriangleIndices != 0) { GL.DeleteBuffer(vboTriangleIndices); vboTriangleIndices = 0; }
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
