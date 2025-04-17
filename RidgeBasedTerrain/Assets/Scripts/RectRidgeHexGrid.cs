using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Rectangular grid of hexagonal tiles
/// </summary>
public class RectRidgeHexGrid : RidgeHexGrid
{
    [Header("Rectangular Grid")] [SerializeField]
    private int height = 5;

    [SerializeField] private int width = 5;
    [SerializeField] private bool clipped = false;

    /// <summary>
    /// Generates positions for a rectangular grid of hexagons
    /// </summary>
    protected override List<Vector3> GenerateHexPositions()
    {
        List<Vector3> positions = new List<Vector3>();
        float radius = diameter / 2f;

        // Tam yatay merkez mesafesi
        float horizontalSpacing = diameter * Mathf.Sqrt(3f) / 2f;

        // Tam dikey merkez mesafesi
        float verticalSpacing = radius * 1.5f;

        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                float xPos = col * horizontalSpacing;
                float zPos = row * verticalSpacing;

                // Tek satırları sağa kaydır (0.5 * yatay mesafe)
                if (row % 2 == 1)
                {
                    xPos += horizontalSpacing / 2;
                }

                positions.Add(new Vector3(xPos, 0f, zPos));
            }
        }

        return positions;
    }
    
    protected override List<List<RidgeMesh>> CollectBiomeGroups(Biome biome)
    {
        List<BiomeTile> biomeTiles = new List<BiomeTile>();

        foreach (var row in _tilesLayout)
        {
            foreach (var tile in row)
            {
                if (tile.Biome == biome)
                {
                    biomeTiles.Add(tile);
                }
            }
        }
        
        Dictionary<RidgeMesh, int> meshIds = new Dictionary<RidgeMesh, int>();
        for (int i = 0; i < biomeTiles.Count; i++)
        {
            meshIds[biomeTiles[i].RidgeMesh] = i;
        }

        // DisjointSet<int> kullanacağız, doğrudan RidgeMesh yerine
        DisjointSet<int> disjointSet = new DisjointSet<int>();

        // First pass: add all IDs
        foreach (var id in meshIds.Values)
        {
            disjointSet.MakeSet(id);
        }

        foreach (var tile in biomeTiles)
        {
            HexCoordinates coords = tile.Coordinates;
            List<HexCoordinates> neighbors = coords.GetNeighbors();
            int tileId = meshIds[tile.RidgeMesh];

            foreach (var neighborCoords in neighbors)
            {
                if (_coordinatesToHexagon.TryGetValue(neighborCoords, out RidgeMesh neighborMesh))
                {
                    BiomeTile neighborTile = FindTileForMesh(neighborMesh);

                    if (neighborTile != null && neighborTile.Biome == biome)
                    {
                        // Komşu mesh'in ID'sini bul
                        if (meshIds.TryGetValue(neighborTile.RidgeMesh, out int neighborId))
                        {
                            // Birleştirme işlemini ID'ler üzerinden yap
                            disjointSet.Union(tileId, neighborId);
                        }
                    }
                }
            }
        }

        // DisjointSet'in gruplarını al
        List<List<int>> idGroups = disjointSet.GetGroups();

        // ID gruplarını RidgeMesh gruplarına dönüştür
        List<List<RidgeMesh>> result = new List<List<RidgeMesh>>();

        foreach (var idGroup in idGroups)
        {
            List<RidgeMesh> meshGroup = new List<RidgeMesh>();

            foreach (var id in idGroup)
            {
                // ID'ye karşılık gelen RidgeMesh'i bul
                foreach (var pair in meshIds)
                {
                    if (pair.Value == id)
                    {
                        meshGroup.Add(pair.Key);
                        break;
                    }
                }
            }

            if (meshGroup.Count > 0)
            {
                result.Add(meshGroup);
            }
        }

        return result;
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
                if (tile.RidgeMesh.Equals(mesh))
                {
                    return tile;
                }
            }
        }

        return null;
    }
}