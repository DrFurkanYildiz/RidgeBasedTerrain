﻿using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for tile meshes in the terrain grid
/// </summary>
public abstract class TileMesh
{
    /// <summary>
    /// Gets the unique identifier of this tile mesh
    /// </summary>
    public abstract int GetId();
    
    /// <summary>
    /// Gets the Unity mesh object
    /// </summary>
    public abstract Mesh Mesh { get; }
}


/*
using UnityEngine;

public class TileMesh
{
    public HexMesh HexMesh { get; }
    
    public TileMesh(Hexagon hexagon, HexMeshParams hexMeshParams)
    {
        HexMesh = new HexMesh(hexagon, hexMeshParams);
    }
}
*/