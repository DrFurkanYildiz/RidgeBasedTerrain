using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A discrete vertex to normals mapping for smooth normal calculations
/// </summary>
public class DiscreteVertexToNormals : Dictionary<Vector3Int, List<Vector3>> { }

/// <summary>
/// Utility class for calculating and applying smooth normals across mesh groups
/// </summary>
public class SmoothShadesProcessor
{
    private List<TileMesh> _meshes;
    
    /// <summary>
    /// Creates a new SmoothShadesProcessor for the specified meshes
    /// </summary>
    public SmoothShadesProcessor(List<TileMesh> meshes)
    {
        _meshes = meshes;
    }
    
    /// <summary>
    /// Calculates and applies normals to the meshes
    /// </summary>
    public void CalculateNormals(bool smoothNormals)
    {
        if (smoothNormals)
        {
            CalculateSmoothNormals();
        }
        else
        {
            CalculateFlatNormals();
        }
        
        // Update all meshes
        UpdateMeshes();
    }
    
    /// <summary>
    /// Updates all meshes in the processor
    /// </summary>
    private void UpdateMeshes()
    {
        foreach (TileMesh mesh in _meshes)
        {
            if (mesh is RidgeMesh ridgeMesh)
            {
                ridgeMesh.RecalculateNormals();
            }
        }
    }
    
    /// <summary>
    /// Calculates flat normals (per-face) for all meshes
    /// </summary>
    private void CalculateFlatNormals()
    {
        foreach (TileMesh mesh in _meshes)
        {
            if (mesh is RidgeMesh ridgeMesh)
            {
                ridgeMesh.RecalculateNormals();
            }
        }
    }
    
    /// <summary>
    /// Calculates smooth normals for all meshes
    /// </summary>
    private void CalculateSmoothNormals()
    {
        // First collect all vertex normals
        List<DiscreteVertexToNormals> vertexGroups = new List<DiscreteVertexToNormals>();
        
        foreach (TileMesh mesh in _meshes)
        {
            if (mesh is RidgeMesh ridgeMesh)
            {
                vertexGroups.Add(GetDiscreteVertexToNormals(ridgeMesh));
            }
        }
        
        // Process smooth normals
        MakeSmoothNormals(vertexGroups);
    }
    
    /// <summary>
    /// Gets a mapping of discrete vertices to their normals
    /// </summary>
    private DiscreteVertexToNormals GetDiscreteVertexToNormals(RidgeMesh mesh)
    {
        Mesh unityMesh = mesh.Mesh;
        Vector3[] vertices = unityMesh.vertices;
        Vector3[] normals = unityMesh.normals;
        
        DiscreteVertexToNormals result = new DiscreteVertexToNormals();
        float discretizationStep = 0.0001f; // Small step for precision
        
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3Int discreteVertex = VertexToNormalDiscretizer.GetDiscreteVertex(vertices[i], discretizationStep);
            
            if (!result.ContainsKey(discreteVertex))
            {
                result[discreteVertex] = new List<Vector3>();
            }
            
            // Store a reference to the normal
            result[discreteVertex].Add(normals[i]);
        }
        
        return result;
    }
    
    /// <summary>
    /// Makes normal vectors smooth by averaging normals at shared vertices
    /// </summary>
    private void MakeSmoothNormals(List<DiscreteVertexToNormals> vertexGroups)
    {
        DiscreteVertexToNormals all = new DiscreteVertexToNormals();
        
        // Combine all vertex groups
        foreach (var group in vertexGroups)
        {
            foreach (var kvp in group)
            {
                if (!all.ContainsKey(kvp.Key))
                {
                    all[kvp.Key] = new List<Vector3>();
                }
                
                all[kvp.Key].AddRange(kvp.Value);
            }
        }
        
        // Average normals for each vertex
        foreach (var kvp in all)
        {
            var normals = kvp.Value;
            Vector3 averagedNormal = Vector3.zero;
            
            foreach (var normal in normals)
            {
                averagedNormal += normal;
            }
            
            averagedNormal.Normalize();
            
            // Apply the averaged normal to all instances
            foreach (var normal in normals)
            {
                normal.Set(averagedNormal.x, averagedNormal.y, averagedNormal.z);
            }
        }
    }
}