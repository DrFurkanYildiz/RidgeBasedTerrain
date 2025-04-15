using UnityEngine;

/// <summary>
/// A utility for discretizing vertices for normal calculations
/// </summary>
public static class VertexToNormalDiscretizer
{
    /// <summary>
    /// Converts a continuous vertex position to a discrete grid point
    /// </summary>
    public static Vector3Int GetDiscreteVertex(Vector3 point, float discretizationStep)
    {
        return new Vector3Int(
            Mathf.RoundToInt(point.x / discretizationStep),
            Mathf.RoundToInt(point.y / discretizationStep),
            Mathf.RoundToInt(point.z / discretizationStep)
        );
    }
}