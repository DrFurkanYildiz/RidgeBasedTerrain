using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Hexagon class representing a 6-sided regular polygon
/// </summary>
public class Hexagon : RegularPolygon
{
    /// <summary>
    /// Creates a hexagon with specific center, points, and normal
    /// </summary>
    public Hexagon(Vector3 center, List<Vector3> points, Vector3 normal) 
        : base(center, points, normal) { }
    
    /// <summary>
    /// Creates a hexagon with specific center and normal
    /// </summary>
    public Hexagon(Vector3 center, Vector3 normal) 
        : base(center, normal) { }
    
    /// <summary>
    /// Creates a hexagon with specific center and diameter
    /// </summary>
    public Hexagon(Vector3 center, float diameter) 
        : base(center, Vector3.up)
    {
        Points = CalculatePoints(center, diameter);
    }
    
    /// <summary>
    /// Validates that this is a hexagon by checking point count
    /// </summary>
    public override void Check()
    {
        if (Points.Count != 6)
        {
            Debug.LogError("Hexagon has != 6 points");
        }
    }
    
    /// <summary>
    /// Calculates hexagon points based on center and diameter
    /// </summary>
    public static List<Vector3> CalculatePoints(Vector3 center, float diameter)
    {
        List<Vector3> result = new List<Vector3>(6);
        float radius = diameter / 2f;
        float angleOffset = -Mathf.PI / 6f; // -30 degrees starting angle
        
        for (int i = 0; i < 6; i++)
        {
            float angle = angleOffset + i * Mathf.PI / 3f; // 60 degree intervals
            result.Add(center + new Vector3(
                Mathf.Cos(angle) * radius,
                0,
                Mathf.Sin(angle) * radius
            ));
        }
        
        return result;
    }
}