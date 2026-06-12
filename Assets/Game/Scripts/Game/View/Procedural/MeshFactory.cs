using System.Collections.Generic;
using UnityEngine;

namespace Roulette.Game
{
    public static class MeshFactory
    {
        // Lathes a 2D profile (x = radius, y = height) around the Y axis.
        public static Mesh Revolve(IReadOnlyList<Vector2> profile, int segments, bool flip = false, string name = "Revolve")
        {
            segments = Mathf.Max(3, segments);
            int rings = profile.Count;
            int cols = segments + 1;

            var vertices = new Vector3[rings * cols];
            var uv = new Vector2[rings * cols];
            for (int i = 0; i < rings; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    float a = (float)j / segments * Mathf.PI * 2f;
                    float r = profile[i].x;
                    vertices[i * cols + j] = new Vector3(r * Mathf.Cos(a), profile[i].y, r * Mathf.Sin(a));
                    uv[i * cols + j] = new Vector2((float)j / segments, (float)i / (rings - 1));
                }
            }

            var tris = new List<int>(rings * segments * 6);
            for (int i = 0; i < rings - 1; i++)
            {
                for (int j = 0; j < segments; j++)
                {
                    int a = i * cols + j;
                    int b = (i + 1) * cols + j;
                    int c = i * cols + j + 1;
                    int d = (i + 1) * cols + j + 1;
                    if (!flip) { tris.Add(a); tris.Add(b); tris.Add(c); tris.Add(c); tris.Add(b); tris.Add(d); }
                    else { tris.Add(a); tris.Add(c); tris.Add(b); tris.Add(c); tris.Add(d); tris.Add(b); }
                }
            }

            return Build(name, vertices, uv, tris);
        }

        // Flat ring in the XZ plane facing +Y.
        public static Mesh Annulus(float innerRadius, float outerRadius, int segments, float y = 0f, bool faceUp = true, string name = "Annulus")
        {
            segments = Mathf.Max(3, segments);
            int cols = segments + 1;
            var vertices = new Vector3[cols * 2];
            var uv = new Vector2[cols * 2];
            for (int j = 0; j < cols; j++)
            {
                float a = (float)j / segments * Mathf.PI * 2f;
                Vector3 dir = new(Mathf.Cos(a), 0f, Mathf.Sin(a));
                vertices[j] = dir * innerRadius + Vector3.up * y;
                vertices[cols + j] = dir * outerRadius + Vector3.up * y;
                uv[j] = new Vector2((float)j / segments, 0f);
                uv[cols + j] = new Vector2((float)j / segments, 1f);
            }

            var tris = new List<int>(segments * 6);
            for (int j = 0; j < segments; j++)
            {
                int i0 = j, i1 = j + 1, o0 = cols + j, o1 = cols + j + 1;
                if (faceUp) { tris.Add(i0); tris.Add(o0); tris.Add(i1); tris.Add(i1); tris.Add(o0); tris.Add(o1); }
                else { tris.Add(i0); tris.Add(i1); tris.Add(o0); tris.Add(i1); tris.Add(o1); tris.Add(o0); }
            }
            return Build(name, vertices, uv, tris);
        }

        public static Mesh Disc(float radius, int segments, float y = 0f, bool faceUp = true, string name = "Disc")
        {
            var profile = new[] { new Vector2(0f, y), new Vector2(radius, y) };
            return Revolve(profile, segments, flip: !faceUp, name);
        }

        public static Mesh Cylinder(float radius, float height, int segments, bool caps = true, string name = "Cylinder")
        {
            var profile = new List<Vector2>();
            if (caps) profile.Add(new Vector2(0f, 0f));
            profile.Add(new Vector2(radius, 0f));
            profile.Add(new Vector2(radius, height));
            if (caps) profile.Add(new Vector2(0f, height));
            return Revolve(profile, segments, name: name);
        }

        public static Mesh Cone(float bottomRadius, float topRadius, float height, int segments, bool caps = true, string name = "Cone")
        {
            var profile = new List<Vector2>();
            if (caps) profile.Add(new Vector2(0f, 0f));
            profile.Add(new Vector2(bottomRadius, 0f));
            profile.Add(new Vector2(topRadius, height));
            if (caps && topRadius > 0f) profile.Add(new Vector2(0f, height));
            return Revolve(profile, segments, name: name);
        }

        private static Mesh Build(string name, Vector3[] vertices, Vector2[] uv, List<int> tris)
        {
            var mesh = new Mesh { name = name };
            if (vertices.Length > 65535) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
