using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for calculating point to line distances using vector multiplication
/// </summary>
public class PointToLineDistance_VectorMultBased
{
    private HashSet<int> _excludeBorderSet;
    private Vector3[] _points;
    
    /// <summary>
    /// Creates a new PointToLineDistance_VectorMultBased calculator
    /// </summary>
    public PointToLineDistance_VectorMultBased(HashSet<int> excludeBorderSet, Vector3[] points)
    {
        _excludeBorderSet = excludeBorderSet ?? new HashSet<int>();
        _points = points;
    }
    
    /// <summary>
    /// Calculates the minimum distance from a point to any of the lines
    /// </summary>
    public float Calc(Vector3 point)
    {
        float minDistance = float.MaxValue;
        
        for (int i = 0; i < _points.Length; i++)
        {
            if (_excludeBorderSet.Contains(i))
            {
                continue;
            }
            
            Vector3 p1 = _points[i];
            Vector3 p2 = _points[(i + 1) % _points.Length];
            
            Vector3 line = p2 - p1;
            Vector3 pToLine = point - p1;
            
            // Cross product magnitude divided by line length gives distance to line
            float distance = Vector3.Cross(line, pToLine).magnitude / line.magnitude;
            minDistance = Mathf.Min(minDistance, distance);
        }
        
        return minDistance;
    }
}