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
    public List<Vector3> debugNeigbour = new();
    /// <summary>
    /// Gets the neighbors of this tile
    /// </summary>
    public List<RidgeMesh> Neighbors => _neighbors;
    
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


        debugNeigbour = neighbors.Where(n => n != null ).Select(n => n.GetCenter()).ToList();
    }
}