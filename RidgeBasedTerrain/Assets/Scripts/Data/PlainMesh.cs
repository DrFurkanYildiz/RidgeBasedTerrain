/// <summary>
/// Plain mesh implementation for basic terrain
/// </summary>
public class PlainMesh : RidgeMesh
{
    public PlainMesh(Hexagon hex, RidgeMeshParams parameters) : base(hex, parameters) { }
    
    public override void CalculateFinalHeights(DiscreteVertexToDistance distanceMap, float diameter, int divisions)
    {
        ShiftCompress();
        
        // Update min/max heights
        _minHeight += _yShift;
        _minHeight *= _yCompress;
        _minHeight += GetCenter().y;

        _maxHeight += _yShift;
        _maxHeight *= _yCompress;
        _maxHeight += GetCenter().y;
    }
}