using System.Collections.Generic;
using UnityEngine;

public class RidgeMesh : TileMesh
{
    private List<TileMesh> _neighbours = new();
    private List<Ridge> _ridges = new();

    private float _yShift = 0f;
    private float _yCompress = 1f;
    
    public RidgeMesh(Hexagon hexagon, HexMeshParams hexMeshParams) : base(hexagon, hexMeshParams)
    {
    }
}