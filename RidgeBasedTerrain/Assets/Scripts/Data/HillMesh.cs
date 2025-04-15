using UnityEngine;
/// <summary>
/// Hill mesh implementation for elevated terrain
/// </summary>
public class HillMesh : RidgeMesh
{
    public HillMesh(Hexagon hex, RidgeMeshParams parameters) : base(hex, parameters) { }
    
    public override void CalculateFinalHeights(DiscreteVertexToDistance distanceMap, float diameter, int divisions)
    {
        ShiftCompress();
        
        // Apply hill heights transformation
        Vector3[] vertices = _mesh.vertices;
        _processor.CalculateHillHeights(vertices, diameter/4, diameter/2, GetCenter());
        _mesh.vertices = vertices;
        
        // Update min/max heights
        _minHeight += _yShift;
        _minHeight *= _yCompress;
        _minHeight += GetCenter().y;

        _maxHeight += _yShift;
        _maxHeight *= _yCompress;
        _maxHeight += GetCenter().y;
    }
}