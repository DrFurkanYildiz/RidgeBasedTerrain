using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ridge class that represents a terrain ridge feature used in procedural terrain generation
/// </summary>
public class Ridge
{
    // Start and end points of the ridge
    private Vector3 _start;
    private Vector3 _end;
    
    // Control points for bezier curve
    private Vector3 _control1;
    private Vector3 _control2;
    
    // Points along the ridge
    private List<Vector3> _points = new List<Vector3>();
    
    // Resolution of the ridge (number of segments)
    private int _resolution;
    
    /// <summary>
    /// Start point of the ridge
    /// </summary>
    public Vector3 Start => _start;
    
    /// <summary>
    /// End point of the ridge
    /// </summary>
    public Vector3 End => _end;
    
    /// <summary>
    /// Creates a straight ridge between two points
    /// </summary>
    public Ridge(Vector3 start, Vector3 end, int resolution = 10)
    {
        _start = start;
        _end = end;
        _resolution = resolution;
        
        // Generate default control points (for a slight curve)
        _control1 = Vector3.Lerp(start, end, 0.33f) + Vector3.up * (end.y - start.y) * 0.5f;
        _control2 = Vector3.Lerp(start, end, 0.66f) + Vector3.up * (end.y - start.y) * 0.5f;
        
        CalculatePoints();
    }
    
    /// <summary>
    /// Creates a curved ridge using bezier control points
    /// </summary>
    public Ridge(Vector3 start, Vector3 end, Vector3 control1, Vector3 control2, int resolution = 10)
    {
        _start = start;
        _end = end;
        _control1 = control1;
        _control2 = control2;
        _resolution = resolution;
        
        CalculatePoints();
    }
    
    /// <summary>
    /// Calculates points along the ridge using bezier interpolation
    /// </summary>
    private void CalculatePoints()
    {
        _points.Clear();
        
        for (int i = 0; i <= _resolution; i++)
        {
            float t = i / (float)_resolution;
            Vector3 point = BezierPoint(t);
            _points.Add(point);
        }
    }
    
    /// <summary>
    /// Calculates a point on the cubic bezier curve at parameter t
    /// </summary>
    private Vector3 BezierPoint(float t)
    {
        float u = 1f - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;
        
        Vector3 p = uuu * _start;
        p += 3 * uu * t * _control1;
        p += 3 * u * tt * _control2;
        p += ttt * _end;
        
        return p;
    }
    
    /// <summary>
    /// Sets the ridge points manually
    /// </summary>
    public void SetPoints(List<Vector3> points)
    {
        _points = new List<Vector3>(points);
    }
    
    /// <summary>
    /// Gets the points along the ridge
    /// </summary>
    public List<Vector3> GetPoints()
    {
        return new List<Vector3>(_points);
    }
    
    /// <summary>
    /// Gets a point on the ridge at parameter t (0 to 1)
    /// </summary>
    public Vector3 GetPointAt(float t)
    {
        t = Mathf.Clamp01(t);
        return BezierPoint(t);
    }
    
    /// <summary>
    /// Elevates the ridge by the specified amount
    /// </summary>
    public void ElevateRidge(float amount)
    {
        _start.y += amount;
        _end.y += amount;
        _control1.y += amount;
        _control2.y += amount;
        
        // Recalculate points with new heights
        CalculatePoints();
    }
    
    /// <summary>
    /// Draws the ridge using Gizmos for visualization in the editor
    /// </summary>
    public void DrawGizmo(Color color)
    {
        Gizmos.color = color;
        
        // Draw bezier curve segments
        for (int i = 0; i < _points.Count - 1; i++)
        {
            Gizmos.DrawLine(_points[i], _points[i + 1]);
        }
        
        // Draw control points
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(_start, 0.1f);
        Gizmos.DrawSphere(_end, 0.1f);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(_control1, 0.075f);
        Gizmos.DrawSphere(_control2, 0.075f);
        
        // Draw control lines
        Gizmos.color = new Color(1, 1, 0, 0.5f);
        Gizmos.DrawLine(_start, _control1);
        Gizmos.DrawLine(_control2, _end);
    }
}