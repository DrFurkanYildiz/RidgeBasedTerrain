﻿using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a hexagonal tile in the terrain grid with a specific biome type
/// </summary>
public class BiomeTile
{
    // Tile properties
    public RidgeMesh RidgeMesh { get; private set; }
    public GameObject GameObject { get; private set; }
    public Biome Biome { get; private set; }
    public HexCoordinates Coordinates { get; private set; }
    
    // Neighboring tiles
    private List<TileMesh> _neighbors = new List<TileMesh>(6);
    
    /// <summary>
    /// Gets the neighbors of this tile
    /// </summary>
    public List<TileMesh> Neighbors => _neighbors;
    
    /// <summary>
    /// Creates a new BiomeTile with the specified ridge mesh, game object, biome type and coordinates
    /// </summary>
    public BiomeTile(RidgeMesh ridgeMesh, GameObject gameObject, Biome biome, HexCoordinates coordinates)
    {
        RidgeMesh = ridgeMesh;
        GameObject = gameObject;
        Biome = biome;
        Coordinates = coordinates;
        
        // Initialize neighbors with nulls
        for (int i = 0; i < 6; i++)
        {
            _neighbors.Add(null);
        }
    }
    
    /// <summary>
    /// Sets the neighboring tiles
    /// </summary>
    public void SetNeighbors(List<TileMesh> neighbors)
    {
        _neighbors = neighbors;
        
        // Ensure we have 6 elements (can be null)
        while (_neighbors.Count < 6)
        {
            _neighbors.Add(null);
        }
    }
}