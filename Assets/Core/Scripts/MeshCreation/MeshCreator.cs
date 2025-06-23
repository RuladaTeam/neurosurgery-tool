using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

namespace Core.Scripts.MeshCreation
{
    public static class MeshCreator
    {
        public static GameObject Read(byte[] verticesData, byte[] trianglesData, byte[] colorsData, Material material)
        {
            Mesh mesh = new Mesh
            {
                indexFormat = IndexFormat.UInt32
            };

            var vertexReader = new BinaryReader(new MemoryStream(verticesData));
            var trianglesReader = new BinaryReader(new MemoryStream(trianglesData));
            var colorsReader = new BinaryReader(new MemoryStream(colorsData));

            List<Vector3> vertices = ReadVertices(vertexReader);
            List<int> triangles = ReadTriangles(trianglesReader);
            List<Color> colors = ReadColors(colorsReader);
            
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.colors = colors.ToArray();

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.Optimize();

            GameObject loadedObject = new GameObject();
            loadedObject.AddComponent<MeshFilter>().mesh = mesh;
            MeshRenderer meshRenderer = loadedObject.AddComponent<MeshRenderer>();
            meshRenderer.material = material;

            return loadedObject;
        }

        public static List<Vector3> ReadVertices(BinaryReader vertexReader)
        {
            var vertices = new List<Vector3>();

            while (vertexReader.BaseStream.Position + sizeof(float) * 3 <= vertexReader.BaseStream.Length)
            {
                float x = vertexReader.ReadSingle();
                float y = vertexReader.ReadSingle();
                float z = vertexReader.ReadSingle();
                vertices.Add(new Vector3(x, y, z));
            }

            return vertices;
        }

        public static List<int> ReadTriangles(BinaryReader trianglesReader)
        {
            var triangles = new List<int>();

            while(trianglesReader.BaseStream.Position < trianglesReader.BaseStream.Length)
            {
                triangles.Add(trianglesReader.ReadInt32());
            }

            return triangles;
        }

        public static List<Color> ReadColors(BinaryReader colorsReader)
        {
            var colors = new List<Color>();

            while (colorsReader.BaseStream.Position + sizeof(byte) * 3 <= colorsReader.BaseStream.Length)
            {
                var r = colorsReader.ReadByte() / 255f;
                var g = colorsReader.ReadByte() / 255f;
                var b = colorsReader.ReadByte() / 255f;
                colors.Add(new Color(r, g, b));
            }

            return colors;
        }
        
        public static List<Color32> ReadColors32(BinaryReader colorsReader)
        {
            var colors = new List<Color32>();

            // Ensure there are at least 4 bytes left to read
            while (colorsReader.BaseStream.Position + sizeof(byte) * 3 <= colorsReader.BaseStream.Length)
            {
                byte r = colorsReader.ReadByte();
                byte g = colorsReader.ReadByte();
                byte b = colorsReader.ReadByte();

                colors.Add(new Color32(r, g, b, 1));
            }

            return colors;
        }
    }
}
