using UnityEngine;

/// <summary>
/// Utility functions for creating hexagons
/// </summary>
public static class HexagonFactory
{
    /// <summary>
    /// Creates a hexagon at the specified position with given diameter
    /// </summary>
    public static Hexagon MakeHexagonAtPosition(Vector3 position, float diameter)
    {
        return new Hexagon(position, diameter);
    }
    
    /// <summary>
    /// Creates a hexagon at the origin with given diameter
    /// </summary>
    public static Hexagon MakeHexagonAtOrigin(float diameter)
    {
        return MakeHexagonAtPosition(Vector3.zero, diameter);
    }
    
    /// <summary>
    /// Creates a unit hexagon (diameter=1) at the origin
    /// </summary>
    public static Hexagon MakeUnitHexagon()
    {
        return MakeHexagonAtOrigin(1f);
    }
}