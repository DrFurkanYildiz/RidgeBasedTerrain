using UnityEngine;

/// <summary>
/// Vertex representation for ridge connections
/// </summary>
public class RidgeVertex
{
    public Vector3 Coord { get; private set; }
    public Vector3 Normal { get; private set; }
    
    public RidgeVertex(Vector3 coord, Vector3 normal)
    {
        Coord = coord;
        Normal = normal;
    }
}