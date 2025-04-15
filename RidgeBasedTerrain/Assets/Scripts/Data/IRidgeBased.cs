/// <summary>
/// Interface for ridge-based mesh generation functionality.
/// </summary>
public interface IRidgeBased
{
    /// <summary>
    /// Gets the min and max height of the mesh.
    /// </summary>
    (float min, float max) GetMinMaxHeight();
    
    /// <summary>
    /// Sets shift and compress values for height adjustments.
    /// </summary>
    void SetShiftCompress(float yShift, float yCompress);
    
    /// <summary>
    /// Calculates initial heights based on noise.
    /// </summary>
    void CalculateInitialHeights();
    
    /// <summary>
    /// Calculates final heights based on ridge parameters.
    /// </summary>
    void CalculateFinalHeights(DiscreteVertexToDistance distanceMap, float diameter, int divisions);
    
    /// <summary>
    /// Calculates distances from corner points to borders.
    /// </summary>
    void CalculateCornerPointsDistancesToBorder(DiscreteVertexToDistance distanceMap, int divisions);
}