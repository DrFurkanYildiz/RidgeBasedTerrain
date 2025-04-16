using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// A grid of hexagonal tiles that use ridge-based terrain generation
/// </summary>
public class RidgeHexGrid : MonoBehaviour
{
    [Header("Grid Settings")] [SerializeField]
    protected float diameter = 10f;

    [SerializeField] private int divisions = 10;
    [SerializeField] private bool frameState = false;
    [SerializeField] private float frameOffset = 0f;

    [Header("Ridge Parameters")] [SerializeField]
    private float ridgeVariationMinBound = 0.0f;

    [SerializeField] private float ridgeVariationMaxBound = 0.02f;
    [SerializeField] private float ridgeTopOffset = 0.1f;
    [SerializeField] private float ridgeBottomOffset = -0.075f;

    [Header("Biome Parameters")] [SerializeField]
    private float biomesHillLevelRatio = 0.7f;

    [SerializeField] private float biomesPlainHillGain = 0.1f;

    [Header("Noise")] 
    [SerializeField] private FastNoiseLite biomesNoise;
    [SerializeField] private FastNoiseLite plainNoise;
    [SerializeField] private FastNoiseLite ridgeNoise;
/*
    [Header("Textures")] [SerializeField] 
    private Texture2D plainTexture;
    [SerializeField] private Texture2D hillTexture;
    [SerializeField] private Texture2D waterTexture;
    [SerializeField] private Texture2D mountainTexture;
*/
    [Header("Appearance")] [SerializeField]
    private bool smoothNormals = false;

    [SerializeField] private Material terrainMaterial;

    // Ridge generation config
    private RidgeConfig _ridgeConfig = new RidgeConfig();

    // Collection of all meshes by type
    private List<RidgeGroup> _mountainGroups = new List<RidgeGroup>();
    private List<RidgeGroup> _waterGroups = new List<RidgeGroup>();
    private List<RidgeGroup> _plainGroups = new List<RidgeGroup>();
    private List<RidgeGroup> _hillGroups = new List<RidgeGroup>();

    // Tiles and management
    protected List<List<BiomeTile>> _tilesLayout = new List<List<BiomeTile>>();
    protected Dictionary<HexCoordinates, RidgeMesh> _coordinatesToHexagon = new Dictionary<HexCoordinates, RidgeMesh>();
    private DiscreteVertexToDistance _distanceMap = new DiscreteVertexToDistance();

    // Default heights for min/max
    private float _minHeight = float.MaxValue;
    private float _maxHeight = float.MinValue;

    private void Start()
    {
        InitializeConfiguration();
        GenerateGrid();
    }

    /// <summary>
    /// Initializes configuration parameters
    /// </summary>
    private void InitializeConfiguration()
    {
        // Set ridge configuration
        _ridgeConfig.VariationMinBound = ridgeVariationMinBound;
        _ridgeConfig.VariationMaxBound = ridgeVariationMaxBound;
        _ridgeConfig.TopRidgeOffset = ridgeTopOffset;
        _ridgeConfig.BottomRidgeOffset = ridgeBottomOffset;
    }

    /// <summary>
    /// Generates the complete hex grid with biomes and terrain features
    /// </summary>
    public void GenerateGrid()
    {
        // Clear any existing tiles
        ClearGrid();

        // Create the grid layout
        List<Vector3> positions = GenerateHexPositions();

        // Calculate biome distribution using noise
        Dictionary<int, float> altitudes = CalculateAltitudes(positions);

        // Create tiles with mesh instances
        CreateTiles(positions, altitudes);

        // Assign cube coordinates map
        AssignCubeCoordinatesMap();

        // Group tiles by biome and initialize ridge groups
        InitializeBiomeGroups();

        // Calculate terrain heights
        PrepareHeightsCalculation();
        CalculateFinalHeights();

        // Calculate smooth normals if enabled
        CalculateNormals();
    }

    /// <summary>
    /// Clear all existing tiles and children objects
    /// </summary>
    private void ClearGrid()
    {
        _tilesLayout.Clear();
        _coordinatesToHexagon.Clear();
        _distanceMap.Clear();

        // Clear child game objects
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// Generate positions for hexagonal grid
    /// </summary>
    protected virtual List<Vector3> GenerateHexPositions()
    {
        // Implement in derived class based on layout type
        return new List<Vector3>();
    }

    /// <summary>
    /// Calculate biome distribution altitudes using noise
    /// </summary>
    private Dictionary<int, float> CalculateAltitudes(List<Vector3> positions)
    {
        Dictionary<int, float> altitudes = new Dictionary<int, float>();

        for (int i = 0; i < positions.Count; i++)
        {
            Vector3 pos = positions[i];
            float heightValue = 0f;

            if (biomesNoise != null)
            {
                heightValue = biomesNoise.GetNoise3D(pos);
            }

            altitudes[i] = heightValue;
        }

        return altitudes;
    }

    /// <summary>
    /// Creates tile objects with their mesh instances
    /// </summary>
    private void CreateTiles(List<Vector3> positions, Dictionary<int, float> altitudes)
    {
        // Find min/max heights for biome distribution
        float minZ = altitudes.Values.Min();
        float maxZ = altitudes.Values.Max();

        BiomeCalculator biomeCalculator = new BiomeCalculator();
        _tilesLayout.Clear();
        _tilesLayout.Add(new List<BiomeTile>());

        for (int i = 0; i < positions.Count; i++)
        {
            // Determine biome type
            Biome biome = biomeCalculator.CalculateBiome(minZ, maxZ, altitudes[i]);

            // Create material instance
            Material material = null;
            if (terrainMaterial != null)
            {
                material = new Material(terrainMaterial);
/*
                // Assign textures
                if (waterTexture != null) material.SetTexture("_WaterTexture", waterTexture);
                if (plainTexture != null) material.SetTexture("_PlainTexture", plainTexture);
                if (hillTexture != null) material.SetTexture("_HillTexture", hillTexture);
                if (mountainTexture != null) material.SetTexture("_MountainTexture", mountainTexture);
*/
                // Set terrain parameters
                material.SetFloat("_TopOffset", _ridgeConfig.TopRidgeOffset);
                material.SetFloat("_BottomOffset", _ridgeConfig.BottomRidgeOffset);
                material.SetFloat("_HillLevelRatio", biomesHillLevelRatio);
            }

            // Create hexagon mesh at position
            Vector3 position = positions[i];
            Hexagon hex = new Hexagon(position, diameter);

            // Create hex mesh parameters
            HexMeshParams hexParams = new HexMeshParams
            {
                Diameter = diameter,
                Divisions = divisions,
                FrameState = frameState,
                FrameOffset = frameOffset,
                Material = material
            };

            // Create ridge mesh parameters
            RidgeMeshParams ridgeParams = new RidgeMeshParams
            {
                HexMeshParams = hexParams,
                PlainNoise = plainNoise,
                RidgeNoise = ridgeNoise
            };

            // Create the appropriate ridge mesh based on biome
            var ridgeMesh = RidgeMeshFactory.CreateRidgeMesh(biome, hex, ridgeParams);

            // Create hex coordinates for this tile
            HexCoordinates coords = HexCoordinates.FromPosition(position, diameter);
            
            
            // Create tile object
            GameObject tileObj = new GameObject($"Tile_{i}");
            var tile = tileObj.AddComponent<BiomeTile>();
            tileObj.transform.SetParent(transform);
            tileObj.transform.position = position;

            tile.Initialize(ridgeMesh, biome, coords, material);
            _tilesLayout[0].Add(tile);
        }
    }

    /// <summary>
    /// Maps cube coordinates to hex tiles for neighbor lookup
    /// </summary>
    private void AssignCubeCoordinatesMap()
    {
        _coordinatesToHexagon.Clear();

        foreach (var row in _tilesLayout)
        {
            foreach (var tile in row)
            {
                _coordinatesToHexagon[tile.Coordinates] = tile.RidgeMesh;
            }
        }
    }

    /// <summary>
    /// Initializes biome groups based on tile placement
    /// </summary>
    private void InitializeBiomeGroups()
    {
        _mountainGroups.Clear();
        _waterGroups.Clear();
        _plainGroups.Clear();
        _hillGroups.Clear();

        // Group tiles by biome type
        List<List<RidgeMesh>> mountainGroups = CollectBiomeGroups(Biome.Mountain);
        List<List<RidgeMesh>> waterGroups = CollectBiomeGroups(Biome.Water);
        List<List<RidgeMesh>> plainGroups = CollectBiomeGroups(Biome.Plain);
        List<List<RidgeMesh>> hillGroups = CollectBiomeGroups(Biome.Hill);

        // Create ridge groups
        foreach (var group in mountainGroups)
        {
            _mountainGroups.Add(new RidgeGroup(group, new RidgeSet(_ridgeConfig)));
        }

        foreach (var group in waterGroups)
        {
            _waterGroups.Add(new RidgeGroup(group, new RidgeSet(_ridgeConfig)));
        }

        foreach (var group in plainGroups)
        {
            _plainGroups.Add(new RidgeGroup(group));
        }

        foreach (var group in hillGroups)
        {
            _hillGroups.Add(new RidgeGroup(group));
        }
    }

    /// <summary>
    /// Collects and groups ridge meshes by biome type
    /// </summary>
    protected virtual List<List<RidgeMesh>> CollectBiomeGroups(Biome biome)
    {
        // Implementation in derived classes for specific layout types
        return new List<List<RidgeMesh>>();
    }

    /// <summary>
    /// Calculates neighbors for ridge meshes
    /// </summary>
    private void CalculateNeighbors(List<RidgeMesh> group)
    {
        // Helper function to check if a mesh is in the group
        bool MemberOfGroup(RidgeMesh ridgeMesh)
        {
            return group.Contains(ridgeMesh);
        }

        foreach (var row in _tilesLayout)
        {
            foreach (var tile in row)
            {
                BiomeTile biomeTile = tile;
                RidgeMesh ridgeMesh = biomeTile.RidgeMesh;

                // Skip if this mesh is not in the current group
                if (!group.Contains(ridgeMesh))
                {
                    continue;
                }

                // Get cube coordinates and calculate neighbors
                HexCoordinates cubeCurrent = biomeTile.Coordinates;
                List<RidgeMesh> hexagonNeighbors = new List<RidgeMesh>(6) { null, null, null, null, null, null };
                List<HexCoordinates> neighborsCoords = cubeCurrent.GetNeighbors();

                // Assign neighbors
                for (int i = 0; i < neighborsCoords.Count; i++)
                {
                    HexCoordinates n = neighborsCoords[i];

                    if (_coordinatesToHexagon.TryGetValue(n, out RidgeMesh neighborMesh))
                    {
                        //RidgeMesh castedNeighbor = neighborMesh as RidgeMesh;

                        // Only assign neighbors from the same group
                        if (neighborMesh != null && MemberOfGroup(neighborMesh) && MemberOfGroup(ridgeMesh))
                        {
                            hexagonNeighbors[i] = neighborMesh;
                        }
                    }
                }

                // Set neighbors on the tile
                biomeTile.SetNeighbors(hexagonNeighbors);
            }
        }
    }

    /// <summary>
    /// Assigns calculated neighbors to ridge mesh objects
    /// </summary>
    private void AssignNeighbors(List<RidgeMesh> group)
    {
        foreach (var row in _tilesLayout)
        {
            foreach (var tile in row)
            {
                BiomeTile biomeTile = tile;
                RidgeMesh ridgeMesh = biomeTile.RidgeMesh;

                // Skip if this mesh is not in the current group
                if (!group.Contains(ridgeMesh))
                {
                    continue;
                }

                // Set the neighbors from the tile onto the ridge mesh
                ridgeMesh.SetNeighbours(biomeTile.Neighbors);
            }
        }
    }

    /// <summary>
    /// Initializes ridge generation for the specified groups
    /// </summary>
    private void InitRidges(List<RidgeGroup> groups, float ridgeOffset)
    {
        foreach (var group in groups)
        {
            group.InitRidges(_distanceMap, ridgeOffset, divisions);
        }
    }

    /// <summary>
    /// Prepares height calculation by setting up initial heights
    /// </summary>
    private void PrepareHeightsCalculation()
    {
        // Calculate neighbors for each biome group
        foreach (var group in GetAllGroups())
        {
            CalculateNeighbors(group.GetMeshes());
            AssignNeighbors(group.GetMeshes());
        }

        // Initialize ridges for mountain and water groups
        InitRidges(_mountainGroups, _ridgeConfig.TopRidgeOffset);
        InitRidges(_waterGroups, _ridgeConfig.BottomRidgeOffset);

        // Calculate initial heights for all meshes
        _minHeight = float.MaxValue;
        _maxHeight = float.MinValue;

        // Calculate initial heights function
        Action<List<RidgeMesh>> calculateInitial = (group) =>
        {
            foreach (var mesh in group)
            {
                mesh.CalculateInitialHeights();
                (float meshMinY, float meshMaxY) = mesh.GetMinMaxHeight();
                _minHeight = Mathf.Min(_minHeight, meshMinY);
                _maxHeight = Mathf.Max(_maxHeight, meshMaxY);
            }
        };

        // Apply to all groups
        foreach (var group in GetAllGroups())
        {
            group.ForEachMesh(calculateInitial);
        }

        // Calculate compression factors
        float amplitude = _maxHeight - _minHeight;
        float compressionFactor = biomesPlainHillGain / amplitude;

        // Apply shift and compression
        Action<List<RidgeMesh>> shiftCompress = (group) =>
        {
            foreach (var mesh in group)
            {
                mesh.SetShiftCompress(-_minHeight, compressionFactor);
            }
        };

        // Apply to all groups
        foreach (var group in GetAllGroups())
        {
            group.ForEachMesh(shiftCompress);
        }
    }

    /// <summary>
    /// Calculates final heights for all tiles
    /// </summary>
    private void CalculateFinalHeights()
    {
        foreach (var row in _tilesLayout)
        {
            foreach (var tile in row)
            {
                RidgeMesh mesh = tile.RidgeMesh;

                // Calculate final heights based on biome and ridge configuration
                mesh.CalculateFinalHeights(_distanceMap, diameter, divisions);
                mesh.RecalculateNormals();
            }
        }
    }

    /// <summary>
    /// Calculates normals for all tiles (smooth or flat)
    /// </summary>
    private void CalculateNormals()
    {
        if (smoothNormals)
        {
            SmoothShadesProcessor processor = new SmoothShadesProcessor(GetAllMeshes());
            processor.CalculateNormals(true);
        }
        else
        {
            // Recalculate flat normals for each mesh
            foreach (var row in _tilesLayout)
            {
                foreach (var tile in row)
                {
                    tile.RidgeMesh.RecalculateNormals();
                }
            }
        }
    }

    /// <summary>
    /// Gets all meshes in the grid
    /// </summary>
    private List<RidgeMesh> GetAllMeshes()
    {
        List<RidgeMesh> meshes = new List<RidgeMesh>();

        foreach (var row in _tilesLayout)
        {
            foreach (var tile in row)
            {
                meshes.Add(tile.RidgeMesh);
            }
        }

        return meshes;
    }

    #region IRidgeBased Implementation

    /// <summary>
    /// Gets all ridge groups of all biome types
    /// </summary>
    public List<RidgeGroup> GetAllGroups()
    {
        List<RidgeGroup> result = new List<RidgeGroup>();
        result.AddRange(_mountainGroups);
        result.AddRange(_waterGroups);
        result.AddRange(_plainGroups);
        result.AddRange(_hillGroups);
        return result;
    }

    /// <summary>
    /// Prints information about biome group distribution
    /// </summary>
    public void PrintBiomes()
    {
        Debug.Log("\nBiome Groups:");

        foreach (var group in _mountainGroups)
        {
            Debug.Log($"Mountain group of size {group.GetMeshes().Count}");
        }

        foreach (var group in _waterGroups)
        {
            Debug.Log($"Water group of size {group.GetMeshes().Count}");
        }

        foreach (var group in _hillGroups)
        {
            Debug.Log($"Hill group of size {group.GetMeshes().Count}");
        }

        foreach (var group in _plainGroups)
        {
            Debug.Log($"Plain group of size {group.GetMeshes().Count}");
        }
    }

    #endregion
}