using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Regular polygon base class representing geometric polygons
/// </summary>
public class RegularPolygon
{
    public Vector3 Center { get; protected set; }
    public List<Vector3> Points { get; protected set; }
    public Vector3 Normal { get; protected set; }
    
    public RegularPolygon(Vector3 center, Vector3 normal)
    {
        Center = center;
        Normal = normal;
        Points = new List<Vector3>();
    }
    
    public RegularPolygon(Vector3 center, List<Vector3> points, Vector3 normal)
    {
        Center = center;
        Points = points;
        Normal = normal;
    }
    
    public RegularPolygon(Vector3 center, float radius, int sides, float rotationOffset = 0f)
    {
        Center = center;
        Normal = Vector3.up;
        Points = new List<Vector3>(sides);
        
        for (int i = 0; i < sides; i++)
        {
            float angle = rotationOffset + i * (2 * Mathf.PI / sides);
            Points.Add(center + new Vector3(
                Mathf.Cos(angle) * radius,
                0,
                Mathf.Sin(angle) * radius
            ));
        }
    }
    
    /// <summary>
    /// Calculates if this is a valid polygon
    /// </summary>
    public virtual void Check()
    {
        // Base implementation does nothing
    }
    
    /// <summary>
    /// Sorts points in clockwise order
    /// </summary>
    public void SortPoints()
    {
        if (Points.Count <= 0) return;
        
        Vector3 v0 = Points[0] - Center;
        Points.Sort((a, b) => 
        {
            Vector3 v1 = a - Center;
            Vector3 v2 = b - Center;
            return Vector3.SignedAngle(v0, v1, Normal).CompareTo(
                Vector3.SignedAngle(v0, v2, Normal));
        });
    }
}