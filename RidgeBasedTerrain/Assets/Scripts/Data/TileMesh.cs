using UnityEngine;

public class TileMesh
{
    public HexMesh HexMesh { get; }
    
    public TileMesh(Hexagon hexagon, HexMeshParams hexMeshParams)
    {
        HexMesh = new HexMesh(hexagon, hexMeshParams);
    }
}