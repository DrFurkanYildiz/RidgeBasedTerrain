using UnityEngine;
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Tile : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    public OffsetCoordinates OffsetCoord { get; private set; }
    public bool Shifted { get; private set; }

    public void Initialize(TileMesh tileMesh, Vector3 position, Material material, OffsetCoordinates offsetCoord)
    {
        OffsetCoord = offsetCoord;
        Shifted = IsOdd(offsetCoord.row);

        // HexMesh
        meshFilter = gameObject.GetComponent<MeshFilter>();
        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        
        meshFilter.sharedMesh = tileMesh.HexMesh.Mesh;
        meshRenderer.sharedMaterial = material;
        transform.position = position;
    }

    private static bool IsOdd(int number) => (number % 2) != 0;
}