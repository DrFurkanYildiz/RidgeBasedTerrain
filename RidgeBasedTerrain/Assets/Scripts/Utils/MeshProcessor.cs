using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract processor for mesh height calculations
/// </summary>
public abstract class MeshProcessor
{
    /// <summary>
    /// Shifts and compresses mesh vertices
    /// </summary>
    public abstract Vector3[] ShiftCompress(Vector3[] vertices, float shift, float compress, float offset);
    
    /// <summary>
    /// Calculates initial heights using noise
    /// </summary>
    public abstract void CalculateInitialHeights(Vector3[] vertices, FastNoiseLite noise, ref float minHeight, ref float maxHeight, Vector3 normal);
    
    /// <summary>
    /// Calculates hill heights by applying a hill/dome function
    /// </summary>
    public abstract void CalculateHillHeights(Vector3[] vertices, float r, float R, Vector3 center);
    
    /// <summary>
    /// Calculates heights based on ridge features
    /// </summary>
    public abstract Vector3[] CalculateRidgeBasedHeights(
        Vector3[] vertices,
        RegularPolygon basePoly,
        Ridge[] ridges,
        Vector3[] neighborCornerPoints,
        float R,
        HashSet<int> excludeBorderSet,
        float diameter,
        int divisions,
        DiscreteVertexToDistance distanceMap,
        FastNoiseLite ridgeNoise,
        float ridgeOffset,
        Func<float, float, float, float> interpolationFunc,
        ref float minHeight,
        ref float maxHeight);
}

/// <summary>
/// Processor for flat mesh terrain
/// </summary>
public class FlatMeshProcessor : MeshProcessor
{
    /// <summary>
    /// Shifts and compresses vertices along the Y axis
    /// </summary>
    public override Vector3[] ShiftCompress(Vector3[] vertices, float shift, float compress, float offset)
    {
        Vector3[] result = new Vector3[vertices.Length];
        
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 p = vertices[i];
            p.y += shift;
            p.y *= compress;
            p.y += offset;
            result[i] = p;
        }
        
        return result;
    }
    
    /// <summary>
    /// Calculates initial heights using noise
    /// </summary>
    public override void CalculateInitialHeights(Vector3[] vertices, FastNoiseLite noise, ref float minHeight, ref float maxHeight, Vector3 normal)
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            float n = noise != null ? noise.GetNoise2D(vertices[i].x, vertices[i].z) : 0.0f;
            vertices[i] += n * normal;
            minHeight = Mathf.Min(minHeight, vertices[i].y);
            maxHeight = Mathf.Max(maxHeight, vertices[i].y);
        }
    }
    
    /// <summary>
    /// Calculates hill heights
    /// </summary>
    public override void CalculateHillHeights(Vector3[] vertices, float r, float R, Vector3 center)
    {
        Func<float, float> t = distToCenterAxis => (r - Mathf.Min(r, distToCenterAxis)) / r;
        
        for (int i = 0; i < vertices.Length; i++)
        {
            float distToCenterAxis = Vector2.Distance(
                Vector2.zero, 
                new Vector2(vertices[i].x, vertices[i].z)
            );
            
            vertices[i].y *= Mathf.Lerp(1.0f, 3.0f, t(distToCenterAxis));
        }
    }
    
    /// <summary>
    /// Calculates ridge-based heights
    /// </summary>
    public override Vector3[] CalculateRidgeBasedHeights(
        Vector3[] vertices,
        RegularPolygon basePoly,
        Ridge[] ridges,
        Vector3[] neighborCornerPoints,
        float R,
        HashSet<int> excludeBorderSet,
        float diameter,
        int divisions,
        DiscreteVertexToDistance distanceMap,
        FastNoiseLite ridgeNoise,
        float ridgeOffset,
        Func<float, float, float, float> interpolationFunc,
        ref float minHeight,
        ref float maxHeight)
    {
        Vector3[] result = new Vector3[vertices.Length];
        Func<Vector3, Vector3Int> divisioned = point =>
            VertexToNormalDiscretizer.GetDiscreteVertex(point, diameter / (divisions * 2));
        
        Vector3 center = basePoly.Center;
        
        // Collect ridge points near the base center
        List<Vector3> ridgePoints = new List<Vector3>();
        foreach (var ridge in ridges)
        {
            if (Vector3.Distance(ridge.Start, center) < 2 * R ||
                Vector3.Distance(ridge.End, center) < 2 * R)
            {
                ridgePoints.AddRange(ridge.GetPoints());
            }
        }
        
        // Calculate heights for each vertex
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 v = vertices[i];
            
            // Calculate distance to border
            PointToLineDistance_VectorMultBased calculator = 
                new PointToLineDistance_VectorMultBased(excludeBorderSet, basePoly.Points.ToArray());
            
            float distanceToBorder = calculator.Calc(new Vector3(v.x, 0, v.z));
            
            // Check neighbor corners for minimum distance
            foreach (var point in neighborCornerPoints)
            {
                float pointDistance = Vector2.Distance(
                    new Vector2(v.x, v.z),
                    new Vector2(point.x, point.z)
                );
                
                Vector3Int discretePoint = divisioned(point);
                if (distanceMap.ContainsKey(discretePoint))
                {
                    distanceToBorder = Mathf.Min(
                        distanceToBorder, 
                        pointDistance + distanceMap[discretePoint]
                    );
                }
            }
            
            // Find closest ridge point
            Vector3 closestRidgePoint = FindClosestRidgePoint(ridgePoints, new Vector2(v.x, v.z));
            
            // Calculate distance to ridge projection
            float distanceToRidgeProjection = Vector2.Distance(
                new Vector2(closestRidgePoint.x, closestRidgePoint.z),
                new Vector2(v.x, v.z)
            );
            
            float approxEnd = closestRidgePoint.y;
            
            // Interpolation parameter
            Func<float, float, float> t = (toBorder, toProjection) => 
                toBorder / (toBorder + toProjection);
            
            // Apply noise
            float noise = ridgeNoise != null ? 
                Mathf.Abs(ridgeNoise.GetNoise2D(v.x, v.z)) * 0.289f : 0;
            
            Func<float, float> tPerlin = y => {
                if (Mathf.Approximately(distanceToBorder, 0.0f))
                    return 0.0f;
                return 1 - (ridgeOffset - y * 2) / ridgeOffset;
            };
            
            // Calculate final height
            float newY = center.y + interpolationFunc(
                v.y, 
                approxEnd,
                t(distanceToBorder, distanceToRidgeProjection)
            );
            
            newY -= Mathf.Lerp(0, noise, tPerlin(newY));
            
            // Update result
            result[i] = new Vector3(v.x, newY, v.z);
            
            // Update min/max heights
            minHeight = Mathf.Min(minHeight, newY);
            maxHeight = Mathf.Max(maxHeight, newY);
        }
        
        return result;
    }
    
    /// <summary>
    /// Finds the closest ridge point to the given 2D position
    /// </summary>
    private Vector3 FindClosestRidgePoint(List<Vector3> ridgePoints, Vector2 point)
    {
        if (ridgePoints.Count == 0)
        {
            return new Vector3(
                float.MaxValue,
                float.MaxValue,
                float.MaxValue
            );
        }
        
        Vector3 closest = ridgePoints[0];
        float minDistance = Vector2.Distance(
            new Vector2(closest.x, closest.z),
            point
        );
        
        foreach (var rp in ridgePoints)
        {
            float dist = Vector2.Distance(
                new Vector2(rp.x, rp.z),
                point
            );
            
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = rp;
            }
        }
        
        return closest;
    }
}