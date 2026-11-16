using UnityEngine;
using UnityEngine.Rendering;

namespace LatteArt.EasterEgg
{
    public static class CurtainMeshBuilder
    {
        private static readonly Vector4 AcrossTangent = new(1f, 0f, 0f, -1f);

        public static void Fill(Mesh mesh, Vector2 size, Vector2Int segments, float foldDepth)
        {
            var columns = Mathf.Max(1, segments.x);
            var rows = Mathf.Max(1, segments.y);
            var vertexCount = (columns + 1) * (rows + 1);

            var positions = new Vector3[vertexCount];
            var normals = new Vector3[vertexCount];
            var tangents = new Vector4[vertexCount];
            var uvs = new Vector2[vertexCount];

            for (var row = 0; row <= rows; row++)
            {
                for (var column = 0; column <= columns; column++)
                {
                    var index = row * (columns + 1) + column;
                    var uv = new Vector2((float)column / columns, (float)row / rows);
                    positions[index] = new Vector3((uv.x - 0.5f) * size.x, (uv.y - 1f) * size.y, 0f);
                    normals[index] = Vector3.back;
                    tangents[index] = AcrossTangent;
                    uvs[index] = uv;
                }
            }

            mesh.Clear();
            mesh.indexFormat = vertexCount > ushort.MaxValue ? IndexFormat.UInt32 : IndexFormat.UInt16;
            mesh.SetVertices(positions);
            mesh.SetNormals(normals);
            mesh.SetTangents(tangents);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(BuildTriangles(columns, rows), 0);
            mesh.bounds = new Bounds(new Vector3(0f, -size.y * 0.5f, 0f), new Vector3(size.x, size.y, foldDepth * 2f));
        }

        private static int[] BuildTriangles(int columns, int rows)
        {
            var triangles = new int[columns * rows * 6];
            var next = 0;
            for (var row = 0; row < rows; row++)
            {
                for (var column = 0; column < columns; column++)
                {
                    var bottomLeft = row * (columns + 1) + column;
                    var topLeft = bottomLeft + columns + 1;

                    triangles[next++] = bottomLeft;
                    triangles[next++] = topLeft;
                    triangles[next++] = topLeft + 1;

                    triangles[next++] = bottomLeft;
                    triangles[next++] = topLeft + 1;
                    triangles[next++] = bottomLeft + 1;
                }
            }

            return triangles;
        }
    }
}
