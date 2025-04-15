using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Processor for volume meshes
/// </summary>
public class VolumeMeshProcessor : MeshProcessor
{
    private Vector3[] _initialVertices;
    
    /// <summary>
    /// Creates a new VolumeMeshProcessor with initial vertices
    /// </summary>
    public VolumeMeshProcessor(Vector3[] initialVertices)
    {
        _initialVertices = initialVertices;
    }
    
    /// <summary>
    /// Shifts and compresses vertices based on their directions
    /// </summary>
    public override Vector3[] ShiftCompress(Vector3[] vertices, float shift, float compress, float offset)
    {
        Vector3[] result = new Vector3[vertices.Length];
        
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 dir = _initialVertices[i].normalized;
            result[i] = _initialVertices[i] + dir * 
                (vertices[i].magnitude - _initialVertices[i].magnitude) * compress;
        }
        
        return result;
    }
    
    /// <summary>
    /// Calculates initial heights using 3D noise
    /// </summary>
    public override void CalculateInitialHeights(Vector3[] vertices, FastNoiseLite noise, ref float minHeight, ref float maxHeight, Vector3 normal)
    {
        // Normalize distances
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] += vertices[i].normalized * (1 - vertices[i].magnitude);
        }
        
        // Apply noise
        for (int i = 0; i < vertices.Length; i++)
        {
            float n = noise != null ? 
                noise.GetNoise3D(vertices[i].x, vertices[i].y, vertices[i].z) : 0.0f;
            
            Vector3 old = vertices[i];
            vertices[i] += n * vertices[i].normalized;
            
            float heightDiff = vertices[i].magnitude - old.magnitude;
            minHeight = Mathf.Min(minHeight, heightDiff);
            maxHeight = Mathf.Max(maxHeight, heightDiff);
        }
    }
    
    /// <summary>
    /// Calculates hill heights for volume meshes
    /// </summary>
    public override void CalculateHillHeights(Vector3[] vertices, float r, float R, Vector3 center)
    {
        Func<float, float> t = distToCenterAxis => (r - Mathf.Min(r, distToCenterAxis)) / r;
        
        for (int i = 0; i < vertices.Length; i++)
        {
            float distToCenterAxis = Vector3.Cross(center, vertices[i] - center).magnitude / center.magnitude;
            
            Vector3 normal = vertices[i].normalized;
            vertices[i] += normal * Mathf.Lerp(0.0f, R / 4.0f, t(distToCenterAxis * 1.2f));
        }
    }
    
    /// <summary>
    /// Calculates ridge-based heights for volume meshes
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
            if (Vector3.Distance(ridge.Start, center) < 2 * ridgeOffset ||
                Vector3.Distance(ridge.End, center) < 2 * ridgeOffset)
            {
                ridgePoints.AddRange(ridge.GetPoints());
            }
        }
        
        // Calculate heights for each vertex
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 v = _initialVertices[i];
            
            // Calculate distance to border
            PointToLineDistance_VectorMultBased calculator = 
                new PointToLineDistance_VectorMultBased(excludeBorderSet, basePoly.Points.ToArray());
            
            float distanceToBorder = calculator.Calc(v);
            
            // Check neighbor corners for minimum distance
            foreach (var point in neighborCornerPoints)
            {
                Vector3 normalized = point.normalized;
                float pointDistance = (v - normalized).magnitude;
                
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
            Vector3? closestRidgePoint = FindClosestRidgePoint(ridgePoints, v);
            
            if (!closestRidgePoint.HasValue)
            {
                Debug.LogError("Can't find closest ridge point, yield initial point");
                result[i] = vertices[i];
                continue;
            }
            
            Vector3 crp = closestRidgePoint.Value;
            
            // Calculate distance to ridge projection
            float distanceToRidgeProjection = Vector3.Cross(v, crp).magnitude / crp.magnitude;
            
            // Calculate interpolation parameter
            Func<float, float, float> t = (toBorder, toProjection) => 
                toBorder / (toBorder + toProjection);
            
            Vector3 direction = v.normalized * (crp.magnitude - v.magnitude > 0 ? 1 : -1);
            float length = (crp - v).magnitude;
            float tResult = t(distanceToBorder, distanceToRidgeProjection);
            
            // Apply noise
            float noise = ridgeNoise != null ? 
                Mathf.Abs(ridgeNoise.GetNoise3D(vertices[i].x, vertices[i].y, vertices[i].z)) : 0;
            
            tResult -= Mathf.Lerp(0.0f, noise, tResult);
            
            // Update result
            result[i] = vertices[i] + tResult * direction * length;
        }
        
        return result;
    }
    
    /// <summary>
    /// Finds the closest ridge point to the given 3D position
    /// </summary>
    private Vector3? FindClosestRidgePoint(List<Vector3> ridgePoints, Vector3 point)
    {
        if (ridgePoints.Count == 0)
        {
            return null;
        }
        
        Vector3 closest = ridgePoints[0];
        float minDistance = Vector3.Distance(closest, point);
        
        foreach (var rp in ridgePoints)
        {
            float dist = Vector3.Distance(rp, point);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = rp;
            }
        }
        
        return closest;
    }
}