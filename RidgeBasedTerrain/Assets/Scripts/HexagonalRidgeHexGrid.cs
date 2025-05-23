﻿using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Hexagonal grid of hexagonal tiles
/// </summary>
public class HexagonalRidgeHexGrid : RidgeHexGrid
{
    [Header("Hexagonal Grid")]
    [SerializeField] private int size = 3;
    
    /// <summary>
    /// Generates positions for a hexagonal grid of hexagons
    /// </summary>
    protected override List<Vector3> GenerateHexPositions()
    {
        List<Vector3> positions = new List<Vector3>();

        float radius = diameter / 2f;
        float width  = Mathf.Sqrt(3f) * radius;
        float height = diameter;

        float horiz = width;                     // tam genişlik
        float vert  = height * 0.75f;            // ¾ * yükseklik

        for (int q = -size; q <= size; q++)
        {
            int rMin = Mathf.Max(-size, -q - size);
            int rMax = Mathf.Min(size, -q + size);

            for (int r = rMin; r <= rMax; r++)
            {
                float x = horiz * (q + r / 2f);
                float z = vert  * r;
                positions.Add(new Vector3(x, 0f, z));
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
                    if (_coordinatesToHexagon.TryGetValue(neighborCoords, out RidgeMesh neighborMesh))
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
    private BiomeTile FindTileForMesh(RidgeMesh mesh)
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