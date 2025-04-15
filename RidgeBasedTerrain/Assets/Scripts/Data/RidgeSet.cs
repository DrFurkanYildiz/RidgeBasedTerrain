using System.Collections.Generic;
using UnityEngine;

public class RidgeSet
{
    private List<Ridge> _ridges = new List<Ridge>();
    private RidgeConfig _config;
    
    public RidgeSet(RidgeConfig config)
    {
        _config = config;
    }
    
    public void CreateSingle(RidgeMesh mesh, float offset)
    {
        Vector3 center = mesh.GetCenter();
        Vector3 normal = Vector3.up; // For plane orientation
        
        // Create a slight ridge along the mesh
        Vector3 tangent = (mesh.GetHexagon.Points[0] - center).normalized;
        Vector3 c = center + normal * offset;
        Vector3 start = c - tangent * 0.02f;
        Vector3 end = c + tangent * 0.02f;
        
        // Create ridge points
        Ridge ridge = new Ridge(start, end);
        List<Vector3> points = new List<Vector3>();
        
        int piecesNum = 2;
        Vector3 dist = end - start;
        
        for (int i = 0; i <= piecesNum; i++)
        {
            points.Add(start + (i * dist / piecesNum));
        }
        
        ridge.SetPoints(points);
        _ridges.Add(ridge);
    }
    
    public void CreateDfsRandom(List<RidgeMesh> meshes, float offset, int divisions)
    {
        // Ridge connections using a maker
        RidgeSetMaker maker = new RidgeSetMaker(meshes);
        List<RidgeConnection> connections = maker.Construct(offset);
        
        // Random number generation for variations
        System.Random random = new System.Random(0); // Fixed seed for deterministic results
        
        // Fracture parameters
        int fractureNum = random.Next(0, 4);
        int connectionPiecesNum = fractureNum + 1;
        
        // Compute ridge segments from connections
        List<(Vector3, Vector3)> bounds = new List<(Vector3, Vector3)>();
        Vector3[] displacementXyz = new Vector3[connectionPiecesNum];
        
        for (int k = 0; k < connections.Count; k++)
        {
            var (lhsVertex, rhsVertex) = connections[k].Get();
            Vector3 lhsCoord = lhsVertex.Coord;
            Vector3 rhsCoord = rhsVertex.Coord;
            
            // Calculate tangent vectors for variation
            Vector3 tangentDir = Vector3.Cross(rhsCoord - lhsCoord, lhsVertex.Normal).normalized;
            Vector3[] tangents = new Vector3[] { tangentDir, -tangentDir };
            
            Vector3 distance = rhsCoord - lhsCoord;
            
            // Create ridge segments with variation
            for (int i = 0; i < connectionPiecesNum; i++)
            {
                Vector3 a = lhsCoord + (i * distance / connectionPiecesNum);
                Vector3 b = lhsCoord + ((i + 1) * distance / connectionPiecesNum);
                
                bounds.Add((a, b));
                
                // Add randomized displacement
                float randomness = Random.Range(_config.VariationMinBound, _config.VariationMaxBound);
                Vector3 randomVector = randomness * ((i & 1) == 1 ? tangents[1] : tangents[0]);
                displacementXyz[i] = randomVector;
            }
            
            // Apply displacements to connecting points
            for (int i = 1 + connectionPiecesNum * k; i < connectionPiecesNum * (k + 1); i++)
            {
                var current = bounds[i];
                var previous = bounds[i - 1];
                
                // Add displacement to current start point and previous end point
                Vector3 displacement = displacementXyz[(i - 1) % connectionPiecesNum];
                bounds[i] = (current.Item1 + displacement, current.Item2);
                bounds[i - 1] = (previous.Item1, previous.Item2 + displacement);
            }
        }

        // Create Ridge objects from bounds
        int piecesNum = divisions * 2;
        foreach (var (start, end) in bounds)
        {
            Ridge ridge = new Ridge(start, end);
            List<Vector3> points = new List<Vector3>();
            Vector3 distance = end - start;
            
            for (int i = 0; i <= piecesNum; i++)
            {
                points.Add(start + (i * distance / piecesNum));
            }
            
            ridge.SetPoints(points);
            _ridges.Add(ridge);
        }
    }
    
    public List<Ridge> GetRidges()
    {
        return _ridges;
    }
}