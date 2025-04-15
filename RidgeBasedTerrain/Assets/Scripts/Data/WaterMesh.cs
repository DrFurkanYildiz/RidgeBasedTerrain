using UnityEngine;

/// <summary>
/// Water mesh implementation for depressions and water features
/// </summary>
public class WaterMesh : RidgeMesh
{
    public WaterMesh(Hexagon hex, RidgeMeshParams parameters) : base(hex, parameters) { }
    
    public override void CalculateFinalHeights(DiscreteVertexToDistance distanceMap, float diameter, int divisions)
    {
        // Use cosine interpolation for water terrain
        CalculateRidgeBasedHeights(
            Cosrp, 
            -diameter/4, // Negative offset to create depressions
            distanceMap,
            divisions
        );
    }
    
    // Cosine interpolation function
    private float Cosrp(float a, float b, float mu)
    {
        float mu2 = (1 - Mathf.Cos(mu * Mathf.PI)) / 2;
        return a * (1 - mu2) + b * mu2;
    }
}