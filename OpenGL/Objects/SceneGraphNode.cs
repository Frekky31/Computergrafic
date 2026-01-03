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
        public string Name { get; set; } = string.Empty;

        // Global lights shared by all nodes (use SceneGraphNode.AddGlobalLight(...) to populate)
        public static List<LightObject> GlobalLights { get; } = new();

        // Node-local lights (added to global ones when uploading)
        public List<LightObject> Lights { get; } = [];

        public Material? Material { get; set; } = null;

        public List<(SceneGraphNode Node, Matrix4x4 Transformation)> Children { get; } = [];

        int vaoTriangle = 0;
        int vboTriangleIndices = 0;

        public SceneGraphNode(string name)
        {
            Name = name;
        }

        public SceneGraphNode(IEnumerable<Vertex> verts, IEnumerable<(int A, int, int)> tris, string name)
        {
            Vertices = verts is Vertex[] arrV ? arrV : [.. verts];
            Tris = tris is (int, int, int)[] arrT ? arrT : [.. tris];
            Name = name;
        }

        public void AddChild(SceneGraphNode child, Matrix4x4 localTransform)
            => Children.Add((child, localTransform));

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

        public void SetChildTransform(string name, Matrix4x4 localTransform)
        {
            for (int i = 0; i < Children.Count; i++)
            {
                if (Children[i].Node.Name == name)
                {
                    Children[i] = (Children[i].Node, localTransform);
                    return;
                }
            }
        }

        // Helpers to manage global lights
        public static void AddGlobalLight(LightObject light) => GlobalLights.Add(light);
        public static void ClearGlobalLights() => GlobalLights.Clear();

        public void Load(int hProgram)
        {
            LoadNode(hProgram);
            foreach (var (Node, _) in Children)
            {
                Node.Load(hProgram);
            }
        }

        public void LoadNode(int hProgram)
        {
            if (Vertices == null || Vertices.Length == 0 || Tris == null || Tris.Length == 0)
                return;

            int vertCount = Vertices.Length;
            int posCount = vertCount * 3;
            int colorCount = vertCount * 3;
            int normalCount = vertCount * 3;
            int texCount = vertCount * 2;

            var posArray = new float[posCount];
            var colorArray = new float[colorCount];
            var normalArray = new float[normalCount];
            var texArray = new float[texCount];

            for (int i = 0, pi = 0, ci = 0, ni = 0, ti = 0; i < vertCount; i++)
            {
                var v = Vertices[i];
                posArray[pi++] = v.Position.X;
                posArray[pi++] = v.Position.Y;
                posArray[pi++] = v.Position.Z;
                colorArray[ci++] = v.Color.X;
                colorArray[ci++] = v.Color.Y;
                colorArray[ci++] = v.Color.Z;
                normalArray[ni++] = v.Normal.X;
                normalArray[ni++] = v.Normal.Y;
                normalArray[ni++] = v.Normal.Z;
                texArray[ti++] = v.TexCoord.X;
                texArray[ti++] = v.TexCoord.Y;
            }

            var indices = new int[Tris.Length * 3];
            for (int i = 0, idx = 0; i < Tris.Length; i++)
            {
                var t = Tris[i];
                indices[idx++] = t.A;
                indices[idx++] = t.C;
                indices[idx++] = t.B;
            }

            GL.UseProgram(hProgram);

            var vboTriangleVertices = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboTriangleVertices);
            GL.BufferData(BufferTarget.ArrayBuffer, posArray.Length * sizeof(float), posArray, BufferUsageHint.StaticDraw);

            // upload model indices to a vbo
            vboTriangleIndices = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, vboTriangleIndices);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(int), indices, BufferUsageHint.StaticDraw);

            // upload model colors to a vbo
            var vboColors = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboColors);
            GL.BufferData(BufferTarget.ArrayBuffer, colorArray.Length * sizeof(float), colorArray, BufferUsageHint.StaticDraw);

            var vboTexCoords = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboTexCoords);
            GL.BufferData(BufferTarget.ArrayBuffer, texArray.Length * sizeof(float), texArray, BufferUsageHint.StaticDraw);

            var vboNormals = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboNormals);
            GL.BufferData(BufferTarget.ArrayBuffer, normalArray.Length * sizeof(float), normalArray, BufferUsageHint.StaticDraw);

            //set up a vao
            vaoTriangle = GL.GenVertexArray();
            GL.BindVertexArray(vaoTriangle);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, vboTriangleIndices);
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
            if (normalAttribIndex != -1)
            {
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
            if (Vertices != null && Vertices.Length != 0 && Tris != null && Tris.Length != 0)
            {
                Matrix4x4 normalM;
                if (Matrix4x4.Invert(modelMatrix, out var inv))
                    normalM = Matrix4x4.Transpose(inv);
                else
                    normalM = Matrix4x4.Transpose(modelMatrix);

                var mvp = modelMatrix * viewProjectionMatrix;

                RenderTriangles(modelMatrix, viewProjectionMatrix, normalM, mvp, shaderProgram, time);
            }
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

            GL.UniformMatrix4(GL.GetUniformLocation(hProgram, "inModelMatrix"), 1, false, ref model.M11);
            GL.UniformMatrix4(GL.GetUniformLocation(hProgram, "inNormalMatrix"), 1, false, ref normalM.M11);

            // Apply material (bind its texture and set material uniforms) if present
            Material?.Apply(hProgram, textureUnitIndex: 0);

            // Combine global lights and node-local lights, then upload to shader
            var combined = new List<LightObject>(GlobalLights.Count + Lights.Count);
            combined.AddRange(GlobalLights);
            combined.AddRange(Lights);
            UploadLightsToShader(hProgram, combined);

            int indexCount = Tris.Length * 3;

            GL.BindVertexArray(vaoTriangle);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, vboTriangleIndices);
            GL.DrawElements(PrimitiveType.Triangles, indexCount, DrawElementsType.UnsignedInt, 0);
        }

        private void UploadLightsToShader(int hProgram, IReadOnlyList<LightObject> lights)
        {
            if (hProgram == 0) return;

            const int MAX_LIGHTS = 8;
            int count = Math.Min(lights?.Count ?? 0, MAX_LIGHTS);

            int locCount = GL.GetUniformLocation(hProgram, "lightCount");
            if (locCount != -1)
                GL.Uniform1(locCount, count);

            for (int i = 0; i < count; i++)
            {
                var L = lights[i];
                var baseName = $"lights[{i}]";

                var locPos = GL.GetUniformLocation(hProgram, baseName + ".position");
                if (locPos != -1) GL.Uniform3(locPos, L.Position.X, L.Position.Y, L.Position.Z);

                var locColor = GL.GetUniformLocation(hProgram, baseName + ".color");
                if (locColor != -1) GL.Uniform3(locColor, L.Color.X, L.Color.Y, L.Color.Z);

                var locIntensity = GL.GetUniformLocation(hProgram, baseName + ".intensity");
                if (locIntensity != -1) GL.Uniform1(locIntensity, L.Intensity);
            }
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
