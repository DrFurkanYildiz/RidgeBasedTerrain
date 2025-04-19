using System.Collections.Generic;
using UnityEngine;

public class HexMesh
{
    private int divisions;
    private float diameter;
    private bool frameState;
    private float frameOffset;
    private ClipOptions clipOptions;

    private float R => diameter / 2f;
    private float r => Mathf.Sqrt(3) * R / 2f;

    private List<Vector3> cornerPoints = new List<Vector3>();
    private List<Vector4> tangents = new List<Vector4>();
    private List<Color> colors = new List<Color>();
    private List<Vector2> uvs = new List<Vector2>();
    private List<BoneWeight> boneWeights = new List<BoneWeight>();
    private List<int> indices = new List<int>();
    public Mesh Mesh { get; private set; }

    public List<Vector3> Vertices { get; set; } = new List<Vector3>();
    public List<Vector3> Normals { get; set; } = new List<Vector3>();

    public HexMesh(Hexagon hexagon, HexMeshParams hexMeshParams)
    {
        diameter = hexMeshParams.Diameter;
        divisions = hexMeshParams.Divisions;
        frameState = hexMeshParams.FrameState;
        frameOffset = hexMeshParams.FrameOffset;
        clipOptions = hexMeshParams.ClipOptions;
        cornerPoints = hexagon.Points;

        CalculateVertices();
        if (frameState) AddFrame();
        CalculateIndices();
        CalculateNormals();
        CalculateTangents();
        CalculateColors();
        CalculateUVs();
        CalculateBoneWeights();

        Mesh = new Mesh
        {
            name = "HexMesh",
            vertices = Vertices.ToArray(),
            triangles = indices.ToArray(),
            normals = Normals.ToArray(),
            tangents = tangents.ToArray(),
            colors = colors.ToArray(),
            uv = uvs.ToArray(),
            boneWeights = boneWeights.ToArray()
        };
    }

    public void UpdateMesh()
    {
        // Apply updated vertices to mesh
        Mesh.vertices = Vertices.ToArray();

        // Update affected mesh properties
        Mesh.RecalculateNormals();
        Mesh.RecalculateTangents();
        Mesh.RecalculateBounds();
    }

    private void CalculateVertices()
    {
        Vertices.Clear();
        float zStep = R / divisions;
        float halfZ = zStep / 2;
        float xStep = zStep * Mathf.Sqrt(3) / 2;

        float zInitEven = diameter / 2f;
        float zInitOdd = zInitEven - halfZ;
        int triCount = divisions * 2 + (divisions * 2 - 1);

        for (int layer = 0; layer < divisions; layer++, triCount -= 2)
        {
            float zEven = zInitEven - (layer * halfZ);
            float zOdd = zInitOdd - (layer * halfZ);

            for (int i = 0; i < triCount; i++)
            {
                if ((i & 1) == 1)
                {
                    Vector3 v1 = new(xStep * layer + xStep, 0, zOdd);
                    Vector3 v2 = new(xStep * layer, 0, zOdd - halfZ);
                    Vector3 v3 = new(xStep * layer + xStep, 0, zOdd - zStep);
                    Vertices.AddRange(new[] { v1, v3, v2 });
                    zOdd -= zStep;
                }
                else
                {
                    Vector3 v1 = new(xStep * layer, 0, zEven);
                    Vector3 v3 = new(xStep * layer, 0, zEven - zStep);
                    Vector3 v2 = new(xStep * layer + xStep, 0, zEven - halfZ);
                    Vertices.AddRange(new[] { v1, v2, v3 });
                    zEven -= zStep;
                }
            }
        }

        if (clipOptions.Down) ZClip(-R / 2);
        else if (clipOptions.Up) ZClip(R / 2);

        if (clipOptions.Left) return;

        var copy = new List<Vector3>(Vertices);
        int n = copy.Count;

        Vector3 Mirror(Vector3 v) => new(-v.x, v.y, v.z);

        if (clipOptions.Right) Vertices.Clear();

        for (int i = 0; i < n; i += 3)
        {
            Vector3 p0 = Mirror(copy[i]);
            Vector3 p1 = Mirror(copy[i + 1]);
            Vector3 p2 = Mirror(copy[i + 2]);
            Vertices.AddRange(new[] { p0, p2, p1 });
        }
    }

    private void ZClip(float boundary)
    {
        float zStep = R / divisions;
        float halfZ = zStep / 2;
        List<Vector3> filtered = new();

        for (int i = 0; i < Vertices.Count; i += 3)
        {
            Vector3 p0 = Vertices[i];
            Vector3 p1 = Vertices[i + 1];
            Vector3 p2 = Vertices[i + 2];
            int toFixCount = 0;
            Vector3? toFix = null;

            if (boundary > 0)
            {
                if (p0.z > boundary)
                {
                    toFixCount++;
                    toFix = p0;
                }

                if (p1.z > boundary)
                {
                    toFixCount++;
                    toFix = p1;
                }

                if (p2.z > boundary)
                {
                    toFixCount++;
                    toFix = p2;
                }
            }
            else
            {
                if (p0.z < boundary)
                {
                    toFixCount++;
                    toFix = p0;
                }

                if (p1.z < boundary)
                {
                    toFixCount++;
                    toFix = p1;
                }

                if (p2.z < boundary)
                {
                    toFixCount++;
                    toFix = p2;
                }
            }

            if (toFixCount > 1) continue;
            else if (toFixCount == 1)
            {
                Vector3 fix = toFix.Value;
                fix.z += (boundary > 0 ? -halfZ : halfZ);
                if (toFix == p0) p0 = fix;
                else if (toFix == p1) p1 = fix;
                else if (toFix == p2) p2 = fix;
            }

            filtered.AddRange(new[] { p0, p1, p2 });
        }

        Vertices = filtered;
    }
    
    private void AddFrame()
    {
        for (int i = 0; i < 6; i++)
        {
            var a = cornerPoints[i];
            var b = cornerPoints[(i + 1) % 6];
            var c = a + Vector3.down * frameOffset;
            var d = b + Vector3.down * frameOffset;
            Vertices.AddRange(new[] { b, c, a, b, d, c });
        }
    }

    private void CalculateIndices()
    {
        indices.Clear();
        for (int i = 0; i < Vertices.Count; i += 3)
            indices.AddRange(new[] { i, i + 1, i + 2 });
    }

    private void CalculateNormals()
    {
        Normals.Clear();
        for (int i = 0; i < Vertices.Count; i += 3)
        {
            Vector3 normal = Vector3.Cross(Vertices[i + 2] - Vertices[i + 1], Vertices[i] - Vertices[i + 1]).normalized;
            Normals.AddRange(new[] { normal, normal, normal });
        }
    }

    private void CalculateTangents()
    {
        tangents.Clear();
        for (int i = 0; i < Vertices.Count; i++)
            tangents.Add(new Vector4(1, 0, 0, 1));
    }

    private void CalculateColors()
    {
        colors.Clear();
        for (int i = 0; i < Vertices.Count; i++)
            colors.Add(Color.black);
    }

    private void CalculateUVs()
    {
        uvs.Clear();
        foreach (var v in Vertices)
        {
            float u = (v.x + r) / (2 * r);
            float vCoord = (v.z + R) / (2 * R);
            uvs.Add(new Vector2(u, vCoord));
        }
    }

    private void CalculateBoneWeights()
    {
        boneWeights.Clear();
        for (int i = 0; i < Vertices.Count; i++)
            boneWeights.Add(new BoneWeight());
    }
}