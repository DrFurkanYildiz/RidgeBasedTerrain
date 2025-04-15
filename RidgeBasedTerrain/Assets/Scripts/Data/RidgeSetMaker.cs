using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Helper class for creating ridge connections between mesh tiles
/// </summary>
public class RidgeSetMaker
{
    private List<RidgeMesh> _meshes;
    private HashSet<RidgeMesh> _visited = new HashSet<RidgeMesh>();
    
    public RidgeSetMaker(List<RidgeMesh> meshes)
    {
        _meshes = meshes;
    }
    
    /// <summary>
    /// Counts the number of unvisited neighboring meshes
    /// </summary>
    private int UnvisitedNeighboursCount(RidgeMesh mesh)
    {
        return UnvisitedNeighbours(mesh).Count;
    }
    
    /// <summary>
    /// Gets a list of unvisited neighboring meshes
    /// </summary>
    private List<RidgeMesh> UnvisitedNeighbours(RidgeMesh mesh)
    {
        List<RidgeMesh> unvisited = new List<RidgeMesh>();
        var allNeighbours = mesh.GetNeighbours();
        
        foreach (var tileMesh in allNeighbours)
        {
            RidgeMesh ridgeMesh = tileMesh as RidgeMesh;
            if (ridgeMesh != null && !_visited.Contains(ridgeMesh) && _meshes.Contains(ridgeMesh))
            {
                unvisited.Add(ridgeMesh);
            }
        }
        
        return unvisited;
    }
    
    /// <summary>
    /// Constructs ridge connections between meshes
    /// </summary>
    public List<RidgeConnection> Construct(float offset)
    {
        System.Random random = new System.Random(0); // Fixed seed for consistency
        
        // Sort meshes by number of unvisited neighbors (increasing)
        _meshes.Sort((lhs, rhs) => UnvisitedNeighboursCount(lhs).CompareTo(UnvisitedNeighboursCount(rhs)));
        
        List<RidgeConnection> result = new List<RidgeConnection>();
        int iterations = 35; // Number of connection attempts
        int left = 0;
        int right = _meshes.Count - 1;
        
        while (iterations > 0 && left < right)
        {
            RidgeMesh leastNeighborsMesh = _meshes[left];
            RidgeMesh mostNeighborsMesh = _meshes[right];
            
            RidgeMesh current = leastNeighborsMesh;
            
            while (current != mostNeighborsMesh)
            {
                List<RidgeMesh> neighbors = UnvisitedNeighbours(current);
                if (neighbors.Count == 0)
                {
                    break;
                }
                
                // Randomly select the next mesh to connect to
                int nextIdx = random.Next(0, neighbors.Count);
                RidgeMesh next = neighbors[nextIdx];
                
                // Create connection between current and next mesh
                Vector3 curCenter = current.GetCenter();
                Vector3 curNormal = Vector3.up; // Assuming plane orientation
                Vector3 curPosition = curCenter + curNormal * offset;
                
                Vector3 nextCenter = next.GetCenter();
                Vector3 nextNormal = Vector3.up; // Assuming plane orientation
                Vector3 nextPosition = nextCenter + nextNormal * offset;
                
                result.Add(new RidgeConnection(
                    new RidgeVertex(curPosition, curNormal),
                    new RidgeVertex(nextPosition, nextNormal)
                ));
                
                _visited.Add(current);
                current = next;
            }
            
            iterations--;
            left++;
            right--;
        }
        
        return result;
    }
}