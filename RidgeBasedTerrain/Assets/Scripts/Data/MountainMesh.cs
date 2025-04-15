using UnityEngine;

/// <summary>
/// Mountain mesh implementation for ridge-based peaked terrain
/// </summary>
public class MountainMesh : RidgeMesh
{
    public MountainMesh(Hexagon hex, RidgeMeshParams parameters) : base(hex, parameters) { }
    
    public override void CalculateFinalHeights(DiscreteVertexToDistance distanceMap, float diameter, int divisions)
    {
        // Use linear interpolation for mountain terrain
        CalculateRidgeBasedHeights(
            (a, b, c) => Mathf.Lerp(a, b, c),
            diameter/4, // Ridge offset - controls mountain height
            distanceMap,
            divisions
        );
    }
}