using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshUtils {
    public struct Meshlet
    {
        public List<Vector3> Vertices;
        public List<int> Triangles;
        public List<Vector2> UVs;

        public void AddOffset(Vector3 offset)
        {
            for (int i = 0; i < Vertices.Count; i++)
            {
                Vertices[i] += offset;
            }
        }
    }

    private static Meshlet CombineMeshlets(List<Meshlet> meshlets)
    {
        Meshlet combinedMeshlet = new Meshlet
        {
            Vertices = new List<Vector3>(),
            Triangles = new List<int>(),
            UVs = new List<Vector2>()
        };

        int vertexOffset = 0;
        foreach (var meshlet in meshlets)
        {
            combinedMeshlet.Vertices.AddRange(meshlet.Vertices);
            combinedMeshlet.UVs.AddRange(meshlet.UVs);
            foreach (var triangleIndex in meshlet.Triangles)
            {
                combinedMeshlet.Triangles.Add(triangleIndex + vertexOffset);
            }
            vertexOffset += meshlet.Vertices.Count;
        }

        return combinedMeshlet;
    }

    public static Mesh MeshletsToMesh(Meshlet meshlet)
    {
        Mesh mesh = new Mesh();
        mesh.SetVertices(meshlet.Vertices);
        mesh.SetTriangles(meshlet.Triangles, 0);
        mesh.SetUVs(0, meshlet.UVs);
        mesh.RecalculateNormals();
        return mesh;
    }

    private static Meshlet CreateInsetPlaneMeshlet(
        Vector3 center,
        Vector3 normal,
        float size,
        float insetFromEdge = 0.1f,   // in-plane inset from outer border
        float bevelDepth = 0.0f,      // depth of the sloped ring (along -normal)
        float insetDepth = 0.05f       // total recess depth (along -normal), must be >= bevelDepth
    )
    {
        normal = normal.normalized;

        float half = size * 0.5f;

        insetFromEdge = Mathf.Clamp(insetFromEdge, 0.0001f, half - 0.0001f);
        bevelDepth = Mathf.Max(0f, bevelDepth);
        insetDepth = Mathf.Max(bevelDepth, insetDepth);

        // Build in local plane space (XZ plane, +Y is plane normal), rotate at end.
        float ih = half - insetFromEdge;

        // Outer corners (y = 0)
        Vector3 O0 = new Vector3(-half, 0f, -half);
        Vector3 O1 = new Vector3(-half, 0f,  half);
        Vector3 O2 = new Vector3( half, 0f,  half);
        Vector3 O3 = new Vector3( half, 0f, -half);

        // Bevel ring (inset in XZ, down by bevelDepth)
        Vector3 B0 = new Vector3(-ih, -bevelDepth, -ih);
        Vector3 B1 = new Vector3(-ih, -bevelDepth,  ih);
        Vector3 B2 = new Vector3( ih, -bevelDepth,  ih);
        Vector3 B3 = new Vector3( ih, -bevelDepth, -ih);

        // Inset face ring (same XZ, further down by insetDepth)
        Vector3 I0 = new Vector3(-ih, -insetDepth, -ih);
        Vector3 I1 = new Vector3(-ih, -insetDepth,  ih);
        Vector3 I2 = new Vector3( ih, -insetDepth,  ih);
        Vector3 I3 = new Vector3( ih, -insetDepth, -ih);

        // Hard-edge approach: each triangle gets its own 3 vertices (no sharing).
        var vertices = new List<Vector3>(18 * 3);
        var uvs = new List<Vector2>(18 * 3);
        var triangles = new List<int>(18 * 3);

        Vector2 UVFromLocal(Vector3 local)
            => new Vector2(local.x / size + 0.5f, local.z / size + 0.5f);

        void AddTriLocal(Vector3 a, Vector3 b, Vector3 c)
        {
            int baseIndex = vertices.Count;

            vertices.Add(center + a);
            vertices.Add(center + b);
            vertices.Add(center + c);

            uvs.Add(UVFromLocal(a));
            uvs.Add(UVFromLocal(b));
            uvs.Add(UVFromLocal(c));

            triangles.Add(baseIndex + 0);
            triangles.Add(baseIndex + 1);
            triangles.Add(baseIndex + 2);
        }

        // Add a quad as two tris with the same split you used: (a,b,c) and (c,d,a)
        void AddQuadLocal(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            AddTriLocal(a, b, c);
            AddTriLocal(c, d, a);
        }

        // 8 triangles: bevel trapezoids (outer -> bevel)
        Vector3[] O = { O0, O1, O2, O3 };
        Vector3[] B = { B0, B1, B2, B3 };
        for (int i = 0; i < 4; i++)
        {
            int j = (i + 1) & 3;
            // Matches earlier winding: AddQuad(oA, oB, bB, bA)
            AddQuadLocal(O[i], O[j], B[j], B[i]);
        }

        // 8 triangles: walls (bevel -> inset)
        Vector3[] I = { I0, I1, I2, I3 };
        for (int i = 0; i < 4; i++)
        {
            int j = (i + 1) & 3;
            AddQuadLocal(B[i], B[j], I[j], I[i]);
        }

        // 2 triangles: inset face (same winding as the top plane, so it faces outward)
        AddTriLocal(I0, I1, I2);
        AddTriLocal(I2, I3, I0);

        // Rotate from local +Y to requested normal (about center), like your original.
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, normal);
        for (int v = 0; v < vertices.Count; v++)
            vertices[v] = rotation * (vertices[v] - center) + center;

        return new Meshlet
        {
            Vertices = vertices,
            Triangles = triangles,
            UVs = uvs
        };
    }

    private static Meshlet CreatePlaneMeshlet(Vector3 center, Vector3 normal, float size)
    {
        // Make mesh facing up
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        vertices.Add(center + new Vector3(-size / 2, 0, -size / 2));
        vertices.Add(center + new Vector3(-size / 2, 0, size / 2));
        vertices.Add(center + new Vector3(size / 2, 0, size / 2));
        vertices.Add(center + new Vector3(size / 2, 0, -size / 2));

        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));

        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, normal);
        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i] = rotation * (vertices[i] - center) + center;
        }

        return new Meshlet
        {
            Vertices = vertices,
            Triangles = new List<int> { 0, 1, 2, 2, 3, 0 },
            UVs = uvs
        };
    }

    private static Meshlet CreateCylinderMeshlet(float radius, float height, int segments)
    {
        Meshlet meshlet = new Meshlet();
        meshlet.Vertices = new List<Vector3>();
        meshlet.Triangles = new List<int>();
        meshlet.UVs = new List<Vector2>();

        for (int i = 0; i <= segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            // Bottom vertex
            meshlet.Vertices.Add(new Vector3(x, 0, z));
            meshlet.UVs.Add(new Vector2((float)i / segments, 0));

            // Top vertex
            meshlet.Vertices.Add(new Vector3(x, height, z));
            meshlet.UVs.Add(new Vector2((float)i / segments, 1));

            if (i < segments)
            {
                int baseIndex = i * 2;
                // Two triangles per segment
                meshlet.Triangles.Add(baseIndex);
                meshlet.Triangles.Add(baseIndex + 1);
                meshlet.Triangles.Add(baseIndex + 2);

                meshlet.Triangles.Add(baseIndex + 2);
                meshlet.Triangles.Add(baseIndex + 1);
                meshlet.Triangles.Add(baseIndex + 3);
            }
        }

        return meshlet;        
    }

    public static Mesh MeshCreateCylinderMesh(float radius, float height, int segments)
    {
        return MeshletsToMesh(CreateCylinderMeshlet(radius, height, segments));
    }

    public static Mesh CreatePlaneMesh(int width, int height, float gridSize)
    {
        List<Meshlet> meshlets = new List<Meshlet>();

        // Define vertices
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                meshlets.Add(CreatePlaneMeshlet(
                    new Vector3(x * gridSize, 0, y * gridSize),
                    Vector3.up,
                    gridSize));
            }
        }

        return MeshletsToMesh(CombineMeshlets(meshlets));
    }

    private static Meshlet CreateCubeMeshlet(float size)
    {
        List<Meshlet> meshlets = new List<Meshlet>
        {
            CreatePlaneMeshlet(new Vector3(0, size / 2, 0), Vector3.up, size),
            CreatePlaneMeshlet(new Vector3(size / 2, 0, 0), Vector3.right, size),
            CreatePlaneMeshlet(new Vector3(0, -size / 2, 0), Vector3.down, size),
            CreatePlaneMeshlet(new Vector3(-size / 2, 0, 0), Vector3.left, size),
            CreatePlaneMeshlet(new Vector3(0, 0, size / 2), Vector3.forward, size),
            CreatePlaneMeshlet(new Vector3(0, 0, -size / 2), Vector3.back, size)
        };

        return CombineMeshlets(meshlets);
    }

    public static Mesh CreateCubeMesh(float size)
    {
        return MeshletsToMesh(CreateCubeMeshlet(size));
    }

    private static Meshlet CreateInsetCubeMeshlet(float size)
    {
        List<Meshlet> meshlets = new List<Meshlet>
        {
            CreateInsetPlaneMeshlet(new Vector3(0, size / 2, 0), Vector3.up, size),
            CreateInsetPlaneMeshlet(new Vector3(size / 2, 0, 0), Vector3.right, size),
            CreateInsetPlaneMeshlet(new Vector3(0, -size / 2, 0), Vector3.down, size),
            CreateInsetPlaneMeshlet(new Vector3(-size / 2, 0, 0), Vector3.left, size),
            CreateInsetPlaneMeshlet(new Vector3(0, 0, size / 2), Vector3.forward, size),
            CreateInsetPlaneMeshlet(new Vector3(0, 0, -size / 2), Vector3.back, size)
        };

        return CombineMeshlets(meshlets);
    }

    public static Mesh CreateInsetCubeMesh(float size)
    {
        return MeshletsToMesh(CreateInsetCubeMeshlet(size));
    }

    public static Mesh CreateInsetPieceMesh(PieceShape shape, float cellSize)
    {
        List<Meshlet> meshlets = new List<Meshlet>();

        foreach (var localCell in shape.LocalCells)
        {
            Meshlet cubeMeshlet = CreateInsetCubeMeshlet(cellSize);
            // Offset cube to cell position
            cubeMeshlet.AddOffset(new Vector3(localCell.X * cellSize, 0, localCell.Y * cellSize));
            meshlets.Add(cubeMeshlet);
        }

        return MeshletsToMesh(CombineMeshlets(meshlets));
    }

    public static Mesh CreatePieceMesh(PieceShape shape, float cellSize)
    {
        List<Meshlet> meshlets = new List<Meshlet>();

        foreach (var localCell in shape.LocalCells)
        {
            Meshlet cubeMeshlet = CreateCubeMeshlet(cellSize);
            // Offset cube to cell position
            cubeMeshlet.AddOffset(new Vector3(localCell.X * cellSize, 0, localCell.Y * cellSize));
            meshlets.Add(cubeMeshlet);
        }

        return MeshletsToMesh(CombineMeshlets(meshlets));
    }

    public static GameObject CreateGameObjectFromMesh(Mesh mesh, Material material, string name = "MeshObject", bool collider=false)
    {
        GameObject obj = new GameObject(name);
        MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
        if (collider)
        {
            obj.AddComponent<MeshCollider>().sharedMesh = mesh;
        }
        if (material == null)
        {
            material = new Material(Shader.Find("Standard"));
        }
        meshRenderer.sharedMaterial = material;
        return obj;
    }

}