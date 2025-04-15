using System;
using System.Collections.Generic;

/// <summary>
/// A collection of RidgeMesh objects that form a coherent terrain group
/// </summary>
public class RidgeGroup
{
    private List<RidgeMesh> _meshes;
    private RidgeSet _ridgeSet;
    
    /// <summary>
    /// Creates a new ridge group with a ridge set for generating ridges
    /// </summary>
    public RidgeGroup(List<RidgeMesh> meshes, RidgeSet ridgeSet)
    {
        _meshes = meshes;
        _ridgeSet = ridgeSet;
    }
    
    /// <summary>
    /// Creates a new ridge group without a ridge set (for biomes without ridges)
    /// </summary>
    public RidgeGroup(List<RidgeMesh> meshes)
    {
        _meshes = meshes;
        _ridgeSet = null;
    }
    
    /// <summary>
    /// Gets the list of meshes in this group
    /// </summary>
    public List<RidgeMesh> GetMeshes()
    {
        return _meshes;
    }
    
    /// <summary>
    /// Apply an action to all meshes in the group
    /// </summary>
    public void ForEachMesh(Action<List<RidgeMesh>> action)
    {
        action(_meshes);
    }
    
    /// <summary>
    /// Initialize ridge generation and calculate connections between meshes
    /// </summary>
    public void InitRidges(DiscreteVertexToDistance distanceMap, float offset, int divisions)
    {
        if (_ridgeSet == null)
        {
            return; // No ridges for this group
        }
        
        if (_meshes.Count > 1)
        {
            // Create ridge network for connected meshes
            _ridgeSet.CreateDfsRandom(_meshes, offset, divisions);
        }
        else if (_meshes.Count == 1)
        {
            // Create a single ridge for an isolated mesh
            _ridgeSet.CreateSingle(_meshes[0], offset);
        }
        
        // Assign generated ridges to all meshes in the group
        AssignRidges();
        
        // Calculate corner points distances to borders for all meshes
        CalculateCornerPointsDistancesToBorder(distanceMap, divisions);
    }
    
    /// <summary>
    /// Assigns the ridges to all meshes in the group
    /// </summary>
    private void AssignRidges()
    {
        if (_ridgeSet == null)
        {
            return;
        }
        
        List<Ridge> ridges = _ridgeSet.GetRidges();
        
        // Convert to ridge pointers
        List<Ridge> ridgePointers = new List<Ridge>();
        foreach (var ridge in ridges)
        {
            ridgePointers.Add(ridge);
        }
        
        // Assign to all meshes in the group
        foreach (var mesh in _meshes)
        {
            mesh.SetRidges(ridgePointers);
        }
    }
    
    /// <summary>
    /// Calculates and stores distances from corner points to borders for all meshes
    /// </summary>
    private void CalculateCornerPointsDistancesToBorder(DiscreteVertexToDistance distanceMap, int divisions)
    {
        foreach (var mesh in _meshes)
        {
            mesh.CalculateCornerPointsDistancesToBorder(distanceMap, divisions);
        }
    }
}