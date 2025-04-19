using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Represents a hexagonal tile in the terrain grid with a specific biome type
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class BiomeTile : MonoBehaviour
{
    // Tile properties
    public RidgeMesh RidgeMesh { get; private set; }
    [field:SerializeField] public Biome Biome { get; private set; }
    [field: SerializeField] public HexCoordinates Coordinates { get; private set; }
    
    // Neighboring tiles
    private List<RidgeMesh> _neighbors = new List<RidgeMesh>(6);
    /// <summary>
    /// Gets the neighbors of this tile
    /// </summary>
    public List<RidgeMesh> Neighbors => _neighbors;

    public float debugMin;
    public float debugMax;
    
    /// <summary>
    /// Creates a new BiomeTile with the specified ridge mesh, game object, biome type and coordinates
    /// </summary>
    public void Initialize(RidgeMesh ridgeMesh, Biome biome, HexCoordinates coordinates, Material material)
    {
        RidgeMesh = ridgeMesh;
        Biome = biome;
        Coordinates = coordinates;
        
        // Add mesh filter and renderer
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = ridgeMesh.Mesh.Mesh;

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = material;
        
        // Initialize neighbors with nulls
        for (int i = 0; i < 6; i++)
        {
            _neighbors.Add(null);
        }
    }
    
    private void Update()
    {
        if (RidgeMesh != null)
        {
            (debugMin, debugMax) = RidgeMesh.GetMinMaxHeight();
            //debugNeighbour = RidgeMesh.GetNeighbours().Where(n => n != null).Select(n => n.GetCenter()).ToList();
            //debugVertex = RidgeMesh.Mesh.Vertices;
        }
    }
    
    /// <summary>
    /// Sets the neighboring tiles
    /// </summary>
    public void SetNeighbors(List<RidgeMesh> neighbors)
    {
        _neighbors = neighbors;
        // Ensure we have 6 elements (can be null)
        while (_neighbors.Count < 6)
        {
            _neighbors.Add(null);
        }
    }
    
    // Debug için OnDrawGizmos metodunu ekleyelim
    private void OnDrawGizmos()
    {
        // Tile konumunu göster //2452
        Gizmos.color = GetBiomeColor();
        Gizmos.DrawSphere(transform.position, 0.375f);
        
        // Komşuları göster
        Gizmos.color = Color.yellow;
        foreach (var neighbor in _neighbors)
        {
            if (neighbor != null)
            {
                Gizmos.DrawLine(transform.position, neighbor.GetCenter());
            }
        }
    }
    
    private Color GetBiomeColor()
    {
        switch (Biome)
        {
            case Biome.Mountain:
                return Color.grey;
            case Biome.Water:
                return Color.blue;
            case Biome.Plain:
                return Color.green;
            case Biome.Hill:
                return new Color(0.5f, 0.25f, 0);
            default:
                return Color.white;
        }
    }
}