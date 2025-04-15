using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Rectangular grid of hexagonal tiles
/// </summary>
public class RectRidgeHexGrid : RidgeHexGrid
{
    [Header("Rectangular Grid")]
    [SerializeField] private int height = 5;
    [SerializeField] private int width = 5;
    [SerializeField] private bool clipped = false;
    
    /// <summary>
    /// Generates positions for a rectangular grid of hexagons
    /// </summary>
    protected override List<Vector3> GenerateHexPositions()
    {
        List<Vector3> positions = new List<Vector3>();
        float hexRadius = diameter / 2f;
        float hexHeight = hexRadius * Mathf.Sqrt(3);
        
        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                float xPos = col * (hexRadius * 1.5f);
                float zPos = row * hexHeight;
                
                // Offset odd rows
                if (row % 2 == 1)
                {
                    xPos += hexRadius * 0.75f;
                }
                
                positions.Add(new Vector3(xPos, 0f, zPos));
            }
        }
        
        return positions;
    }
    
    /// <summary>
    /// Collects and groups ridge meshes by biome type
    /// </summary>
    protected override List<List<RidgeMesh>> CollectBiomeGroups(Biome biome)
    {
        // Use a union-find data structure to group connected components
        DisjointSet<RidgeMesh> disjointSet = new DisjointSet<RidgeMesh>();
        
        // First pass: add all meshes of this biome to the set
        foreach (var row in _tilesLayout)
        {
            foreach (var tile in row)
            {
                if (tile.Biome == biome)
                {
                    disjointSet.MakeSet(tile.RidgeMesh);
                }
            }
        }
        
        // Second pass: union adjacent tiles of the same biome
        foreach (var row in _tilesLayout)
        {
            foreach (var tile in row)
            {
                if (tile.Biome != biome)
                {
                    continue;
                }
                
                HexCoordinates coords = tile.Coordinates;
                List<HexCoordinates> neighbors = coords.GetNeighbors();
                
                foreach (var neighborCoords in neighbors)
                {
                    if (_coordinatesToHexagon.TryGetValue(neighborCoords, out TileMesh neighborMesh))
                    {
                        BiomeTile neighborTile = FindTileForMesh(neighborMesh);
                        if (neighborTile != null && neighborTile.Biome == biome)
                        {
                            disjointSet.Union(tile.RidgeMesh, neighborTile.RidgeMesh);
                        }
                    }
                }
            }
        }
        
        // Get the groups
        return disjointSet.GetGroups();
    }
    
    /// <summary>
    /// Finds a BiomeTile for a given mesh
    /// </summary>
    private BiomeTile FindTileForMesh(TileMesh mesh)
    {
        foreach (var row in _tilesLayout)
        {
            foreach (var tile in row)
            {
                if (tile.RidgeMesh == mesh)
                {
                    return tile;
                }
            }
        }
        
        return null;
    }
}