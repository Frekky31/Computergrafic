using Rasterization.Objects;
using Raylib_cs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Rasterization.Core
{

    /// <summary>
    /// Simple scene graph node that stores a triangle mesh (vertices + tris),
    /// children with local transforms, a bounding volume (sphere) and a simple
    /// frustum-culling-aware Render traversal.
    ///
    /// Usage:
    /// - Construct nodes with vertex/triangle data or create empty node and assign later.
    /// - Optionally subscribe to OnTriangle callback to receive transformed triangles
    ///   (world and clip-space transformation already applied).
    /// - Call Render(rootModel, viewProjectionMatrix, frustum) to traverse and render.
    /// </summary>
    public class SceneGraphNode
    {

        static LightObject light = new() { Color = new Vector3(3, 3, 3), Position = new Vector3(5, 8, -4) };
        protected static Vector3 E = new Vector3(0, 0, -7);
        protected static Vector3 PSpecular = Vector3.One;
        protected static int specularK = 50;

        float zNear = 0.1f;
        float zFar = 100f;

        public Vertex[] Vertices { get; private set; } = [];
        public (int A, int B, int C)[] Tris { get; private set; } = [];
        // children stored with their local transform (local->parent)
        public List<(SceneGraphNode Node, Matrix4x4 Transformation)> Children { get; } = new();
        public Volume BoundingVolume { get; private set; } = new Volume();

        // Optional callback invoked for each triangle that passed frustum culling.
        // The three Vertex arguments have their Position transformed with model*viewProjection (clip-space)
        // and WorldCoordinates/Normal transformed with model (world-space).
        public Action<Vertex, Vertex, Vertex>? OnTriangle;

        public SceneGraphNode() { }

        public SceneGraphNode(Vertex[] vertices, (int A, int B, int C)[] tris)
        {
            Vertices = vertices ?? [];
            Tris = tris ?? [];
            ComputeBoundingVolume();
        }

        public void SetGeometry(Vertex[] vertices, (int A, int B, int C)[] tris)
        {
            Vertices = vertices ?? [];
            Tris = tris ?? [];
            ComputeBoundingVolume();
        }

        public void AddChild(SceneGraphNode child, Matrix4x4 localTransform)
        {
            if (child == null) throw new ArgumentNullException(nameof(child));
            Children.Add((child, localTransform));
        }

        public void RemoveChild(SceneGraphNode child)
        {
            if (child == null) return;
            Children.RemoveAll(c => c.Node == child);
        }

        private void ComputeBoundingVolume()
        {
            if (Vertices == null || Vertices.Length == 0)
            {
                BoundingVolume = new Volume(); // empty
                return;
            }

            // compute centroid (approx center) and radius as max distance to center
            Vector3 center = Vector3.Zero;
            for (int i = 0; i < Vertices.Length; i++)
                center += Vertices[i].WorldCoordinates; // use raw positions (local)
            center /= Vertices.Length;

            float r = 0f;
            for (int i = 0; i < Vertices.Length; i++)
            {
                float d = (Vertices[i].WorldCoordinates - center).Length();
                if (d > r) r = d;
            }

            BoundingVolume = new Volume(new BoundingSphere(center, r));
        }

        /// <summary>
        /// Render traversal. modelMatrix is the node's transform in world space (parent already applied).
        /// viewProjectionMatrix is typically view * projection.
        /// viewingFrustum expected to be built from the same viewProjectionMatrix (see Frustum.FromMatrix).
        /// </summary>
        public void Render(Matrix4x4 modelMatrix, Matrix4x4 viewProjectionMatrix, Frustum viewingFrustum)
        {
            // transform bounding sphere to world and test against frustum
            if (BoundingVolume.HasSphere)
            {
                var worldSphere = BoundingVolume.Sphere!.Transform(modelMatrix);
                if (!viewingFrustum.ContainsSphere(worldSphere.Center, worldSphere.Radius))
                {
                    // culled
                    return;
                }
            }

            // prepare matrices
            // modelMatrix transforms local -> world
            // viewProjectionMatrix should be view * projection (world -> clip)
            Matrix4x4 modelViewProj = modelMatrix * viewProjectionMatrix;

            // compute normal matrix (inverse transpose of model matrix) for normal transform
            Matrix4x4 normalMatrix;
            if (!Matrix4x4.Invert(modelMatrix, out Matrix4x4 invModel))
            {
                // fallback: use modelMatrix (may be wrong if non-uniform scale)
                normalMatrix = Matrix4x4.Transpose(modelMatrix);
            }
            else
            {
                normalMatrix = Matrix4x4.Transpose(invModel);
            }

            RenderTriangles(modelMatrix, viewProjectionMatrix, normalMatrix);

            // recurse children. Note: multiplication order matches caller usage:
            // children are stored with local -> parent; caller earlier used child.Transformation * modelMatrix
            foreach (var child in Children)
            {
                // newModel = child.local * thisModel
                var childModel = child.Transformation * modelMatrix;
                child.Node.Render(childModel, viewProjectionMatrix, viewingFrustum);
            }
        }

        /// <summary>
        /// Transforms stored triangles and invokes OnTriangle callback for each.
        /// </summary>
        protected virtual void RenderTriangles(Matrix4x4 modelMatrix, Matrix4x4 viewProjection, Matrix4x4 normalMatrix)
        {
            //if (Vertices == null || Vertices.Length == 0 || Tris == null || Tris.Length == 0) return;
            //foreach (var (A, B, C) in Tris)
            //{
            //    var v1 = VertexShader(Vertices[A], modelMatrix, view, proj);
            //    var v2 = VertexShader(Vertices[B], modelMatrix, view, proj);
            //    var v3 = VertexShader(Vertices[C], modelMatrix, view, proj);

            //    v1 *= (1f / v1.Position.W);
            //    v2 *= (1f / v2.Position.W);
            //    v3 *= (1f / v3.Position.W);


            //    var p1 = new Vector2(v1.Position.X * halfWidth + halfWidth, v1.Position.Y * halfHeight + halfHeight);
            //    var p2 = new Vector2(v2.Position.X * halfWidth + halfWidth, v2.Position.Y * halfHeight + halfHeight);
            //    var p3 = new Vector2(v3.Position.X * halfWidth + halfWidth, v3.Position.Y * halfHeight + halfHeight);

            //    var edge1 = p2 - p1;
            //    var edge2 = p3 - p1;
            //    float cross = edge1.X * edge2.Y - edge1.Y * edge2.X;
            //    if (cross >= 0) continue;

            //    int minX = (int)MathF.Max(0, MathF.Floor(MathF.Min(p1.X, MathF.Min(p2.X, p3.X))));
            //    int maxX = (int)MathF.Min(width - 1, MathF.Ceiling(MathF.Max(p1.X, MathF.Max(p2.X, p3.X))));
            //    int minY = (int)MathF.Max(0, MathF.Floor(MathF.Min(p1.Y, MathF.Min(p2.Y, p3.Y))));
            //    int maxY = (int)MathF.Min(height - 1, MathF.Ceiling(MathF.Max(p1.Y, MathF.Max(p2.Y, p3.Y))));


            //    Parallel.ForEach(Partitioner.Create(minY, maxY), (range, state) =>
            //    {
            //        for (int y = range.Item1; y < range.Item2; y++)
            //        {
            //            for (int x = minX; x < maxX; x++)
            //            {
            //                (float u, float v) = Rasterization(p1, p2, p3, new Vector2(x, y));
            //                if (u >= 0 && v >= 0 && (u + v) < 1)
            //                {
            //                    var q = v1 + u * (v2 - v1) + v * (v3 - v1);
            //                    var z = zFar * zNear / (zFar + (zNear - zFar) * q.Position.Z);
            //                    var q1 = z * q;
            //                    if (z < zBuffer[x, y])
            //                    {
            //                        zBuffer[x, y] = z;

            //                        Vector3 colorVec = FragmentShader(q1);
            //                        pixelBuffer[x, y] = Vector2Color(colorVec);
            //                    }
            //                }
            //            }
            //        }
            //    });
            //}
        }

        private static Vertex TransformVertexForRender(Vertex v, Matrix4x4 modelMatrix, Matrix4x4 modelViewProj, Matrix4x4 normalMatrix)
        {
            // clip/ND/clip-space position:
            Vector4 clipPos = Vector4.Transform(v.Position, modelViewProj);
            // world position (for lighting)
            Vector4 worldPos4 = Vector4.Transform(v.Position, modelMatrix);
            Vector3 worldPos = new(worldPos4.X, worldPos4.Y, worldPos4.Z);
            // normal transform
            Vector3 transformedNormal = Vector3.Normalize(Vector3.Transform(v.Normal, normalMatrix));
            // color and texcoord remain the same (no material interpolation here)
            return new Vertex(clipPos, worldPos, v.Color, v.TexCoord, transformedNormal);
        }

        public static (float, float) Rasterization(Vector2 v1, Vector2 v2, Vector2 v3, Vector2 p)
        {
            Vector2 ab = new(v2.X - v1.X, v2.Y - v1.Y);
            Vector2 ac = new(v3.X - v1.X, v3.Y - v1.Y);
            Vector2 ap = new(p.X - v1.X, p.Y - v1.Y);
            var mult = 1.0f / (ab.X * ac.Y - ac.X * ab.Y);
            float u = (ap.X * ac.Y - ac.X * ap.Y) * mult;
            float v = (ab.X * ap.Y - ap.X * ab.Y) * mult;
            return (u, v);
        }

        public static Vertex VertexShader(Vertex v, Matrix4x4 model, Matrix4x4 view, Matrix4x4 projection)
        {
            Vector4 pos = v.Position;

            _ = Matrix4x4.Invert(model, out Matrix4x4 invModel);

            Vector4 newPosition = Vector4.Transform(v.Position, model * view * projection);
            Vector4 newWorldPos = Vector4.Transform(v.Position, Matrix4x4.Multiply(Matrix4x4.Transpose(invModel), model.GetDeterminant()));
            Vector3 newNormal = Vector3.Normalize(Vector3.Transform(v.Normal, model));
            return new Vertex(newPosition, new(newWorldPos.X, newWorldPos.Y, newWorldPos.Z), v.Color, v.TexCoord, newNormal);
        }

        public static Vector3 FragmentShader(Vertex v)
        {
            Vector3 PL = Vector3.Normalize(light.Position - v.WorldCoordinates);

            Vector3 n = Vector3.Normalize(v.Normal);

            float nDotL = MathF.Max(0f, Vector3.Dot(n, PL));

            Vector3 ambient = 0.05f * v.Color;
            Vector3 diffuse = v.Color * light.Color * nDotL;

            Vector3 specular = Vector3.Zero;
            if (nDotL > 0f)
            {
                Vector3 viewDir = Vector3.Normalize(E - v.WorldCoordinates);
                Vector3 half = Vector3.Normalize(viewDir + PL);
                float specAngle = MathF.Max(0f, Vector3.Dot(n, half));
                specular = light.Color * PSpecular * MathF.Pow(specAngle, specularK);
            }

            Vector3 color = ambient + diffuse + specular;

            color.X = MathF.Max(0f, color.X);
            color.Y = MathF.Max(0f, color.Y);
            color.Z = MathF.Max(0f, color.Z);

            return color;
        }

        private static Color Vector2Color(Vector3 color)
        {
            return new Color(FloatToSrgbByte(color.X), FloatToSrgbByte(color.Y), FloatToSrgbByte(color.Z));
        }

        private static byte FloatToSrgbByte(float c)
        {
            c = MathF.Pow(c, 1.0f / 2.2f);
            c = Math.Clamp(c, 0f, 1f);
            return (byte)(c * 255f);
        }
    }

    // Simple bounding sphere
    public record BoundingSphere(Vector3 Center, float Radius)
    {
        public BoundingSphere Transform(Matrix4x4 modelMatrix)
        {
            // transform center by matrix (affine)
            Vector3 newCenter = Vector3.Transform(Center, modelMatrix);

            // approximate scale by max scale component of matrix columns (safe overestimate)
            float sx = new Vector3(modelMatrix.M11, modelMatrix.M12, modelMatrix.M13).Length();
            float sy = new Vector3(modelMatrix.M21, modelMatrix.M22, modelMatrix.M23).Length();
            float sz = new Vector3(modelMatrix.M31, modelMatrix.M32, modelMatrix.M33).Length();
            float scale = MathF.Max(sx, MathF.Max(sy, sz));

            return new BoundingSphere(newCenter, Radius * scale);
        }
    }

    public class Volume
    {
        public BoundingSphere? Sphere { get; private set; }
        public bool HasSphere => Sphere != null;

        public Volume() { Sphere = null; }

        public Volume(BoundingSphere s)
        {
            Sphere = s;
        }
    }

    /// <summary>
    /// View frustum represented by 6 planes in world space.
    /// Construct with Frustum.FromMatrix(viewProjectionMatrix) where viewProjectionMatrix =
    /// view * projection (or projection * view depending on your convention).
    /// </summary>
    public class Frustum
    {
        // planes: left, right, bottom, top, near, far
        private readonly Vector4[] planes = new Vector4[6];

        private Frustum() { }

        public static Frustum FromMatrix(Matrix4x4 m)
        {
            // Extract planes from combined view-projection matrix.
            // Note: matrix must be in the same convention you use for transform (row/column).
            // Using standard extraction:
            // left  = m4 + m1
            // right = m4 - m1
            // bottom = m4 + m2
            // top = m4 - m2
            // near  = m4 + m3
            // far   = m4 - m3
            var f = new Frustum();

            // rows
            Vector4 row1 = new(m.M11, m.M12, m.M13, m.M14);
            Vector4 row2 = new(m.M21, m.M22, m.M23, m.M24);
            Vector4 row3 = new(m.M31, m.M32, m.M33, m.M34);
            Vector4 row4 = new(m.M41, m.M42, m.M43, m.M44);

            f.planes[0] = NormalizePlane(row4 + row1); // left
            f.planes[1] = NormalizePlane(row4 - row1); // right
            f.planes[2] = NormalizePlane(row4 + row2); // bottom
            f.planes[3] = NormalizePlane(row4 - row2); // top
            f.planes[4] = NormalizePlane(row4 + row3); // near
            f.planes[5] = NormalizePlane(row4 - row3); // far

            return f;
        }

        private static Vector4 NormalizePlane(Vector4 p)
        {
            Vector3 n = new(p.X, p.Y, p.Z);
            float len = n.Length();
            if (len > 1e-6f)
                return p / len;
            return p;
        }

        public bool ContainsSphere(Vector3 center, float radius)
        {
            // For each plane, distance = dot(plane.xyz, center) + plane.w
            // If distance < -radius => completely outside
            for (int i = 0; i < 6; i++)
            {
                var pl = planes[i];
                float dist = Vector3.Dot(new Vector3(pl.X, pl.Y, pl.Z), center) + pl.W;
                if (dist < -radius) return false;
            }
            return true;
        }
    }
}
