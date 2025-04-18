using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// The RidgeMesh class represents a hexagonal mesh with ridge-based terrain generation capabilities.
/// </summary>
public abstract class RidgeMesh
{
    // Hexagon parameters
    public HexMesh Mesh { get; }
    protected int _id;
    private HexMeshParams _hexMeshParams;
    public Hexagon GetHexagon { get; }

    // Terrain parameters
    protected FastNoiseLite _plainNoise;
    protected FastNoiseLite _ridgeNoise;
    protected List<Ridge> _ridges = new List<Ridge>();
    protected List<RidgeMesh> _neighbours = new List<RidgeMesh>();

    // Height tracking
    protected float _minHeight = float.MaxValue;
    protected float _maxHeight = float.MinValue;
    protected float _yShift = 0.0f;
    protected float _yCompress = 1.0f;

    // Initial vertices
    protected Vector3[] _initialVertices;

    // Processor for mesh calculations
    protected MeshProcessor _processor;

    /// <summary>
    /// Initializes a new instance of the RidgeMesh class.
    /// </summary>
    protected RidgeMesh(Hexagon hexagon, RidgeMeshParams parameters)
    {
        GetHexagon = hexagon;
        _id = parameters.HexMeshParams.Id;
        _hexMeshParams = parameters.HexMeshParams;
        _plainNoise = parameters.PlainNoise;
        _ridgeNoise = parameters.RidgeNoise;

        // Initialize mesh
        Mesh = new HexMesh(hexagon, parameters.HexMeshParams);
        // Initialize processor based on orientation - Plane is default for this implementation
        _processor = new FlatMeshProcessor();
    }

    /// <summary>
    /// Gets the minimum and maximum height of the mesh
    /// </summary>
    public (float min, float max) GetMinMaxHeight() => (_minHeight, _maxHeight);

    /// <summary>
    /// Sets shift and compress values for height adjustments
    /// </summary>
    public void SetShiftCompress(float yShift, float yCompress)
    {
        _yShift = yShift;
        _yCompress = yCompress;
    }

    /// <summary>
    /// Calculates initial heights based on noise
    /// </summary>
    public void CalculateInitialHeights()
    {
        Vector3 normal = Vector3.up;
        _initialVertices = Mesh.Vertices.ToArray().Clone() as Vector3[];

        Vector3[] vertices = Mesh.Vertices.ToArray();
        _processor.CalculateInitialHeights(vertices, _plainNoise, ref _minHeight, ref _maxHeight, normal);
        Mesh.Vertices = vertices.ToList();
        Mesh.UpdateMesh();
    }

    /// <summary>
    /// Performs shift and compress operations on mesh vertices
    /// </summary>
    protected void ShiftCompress()
    {
        if (_processor == null)
        {
            Debug.LogError("Processor of RidgeMesh object is null");
            return;
        }

        Vector3[] vertices = Mesh.Vertices.ToArray();
        Vector3 center = GetHexagon.Center;

        vertices = _processor.ShiftCompress(vertices, _yShift, _yCompress, center.y);

        Mesh.Vertices = vertices.ToList();
        Mesh.UpdateMesh();
    }

    /// <summary>
    /// Calculates ridge-based heights
    /// </summary>
    protected void CalculateRidgeBasedHeights(
        Func<float, float, float, float> interpolationFunc,
        float ridgeOffset,
        DiscreteVertexToDistance distanceMap,
        int divisions)
    {
        ShiftCompress();

        // Collect neighbors' corner points
        List<Vector3> neighboursCornerPoints = new List<Vector3>();
        foreach (var neighbor in _neighbours)
        {
            if (neighbor != null)
            {
                neighboursCornerPoints.AddRange(neighbor.GetHexagon.Points);
            }
        }

        Vector3[] vertices = Mesh.Vertices.ToArray();

        // Create a regular polygon from hexagon for calculations
        RegularPolygon basePolygon = GetHexagon;

        // Calculate ridge-based heights
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 v = vertices[i];
            Vector3 center = basePolygon.Center;

            // Find closest ridge point
            Vector3 closestRidgePoint = FindClosestRidgePoint(_ridges, new Vector2(v.x, v.z));

            // Calculate distance to border
            float distanceToBorder = CalculateDistanceToBorder(v, basePolygon, GetExcludeBorderSet());

            // Check neighbor points for minimum distance
            foreach (var point in neighboursCornerPoints)
            {
                Vector3Int discretePoint = GetDiscreteVertex(point, _hexMeshParams.Diameter / (divisions * 2));
                if (distanceMap.ContainsKey(discretePoint))
                {
                    float pointDistance = Vector2.Distance(
                        new Vector2(v.x, v.z),
                        new Vector2(point.x, point.z)
                    );

                    distanceToBorder = Mathf.Min(
                        distanceToBorder,
                        pointDistance + distanceMap[discretePoint]
                    );
                }
            }

            // Calculate distance to ridge projection
            float distanceToRidgeProjection = Vector2.Distance(
                new Vector2(closestRidgePoint.x, closestRidgePoint.z),
                new Vector2(v.x, v.z)
            );

            float approxEnd = closestRidgePoint.y;

            // Interpolation parameter
            float t = distanceToBorder / (distanceToBorder + distanceToRidgeProjection);

            // Apply noise
            float noise = (_ridgeNoise != null) ? Mathf.Abs(_ridgeNoise.GetNoise2D(v.x, v.z)) * 0.289f : 0;

            float tPerlin = 0;
            if (!Mathf.Approximately(distanceToBorder, 0.0f))
            {
                tPerlin = 1 - (ridgeOffset - v.y * 2) / ridgeOffset;
            }

            // Calculate final height
            float newY = center.y + interpolationFunc(
                v.y,
                approxEnd,
                t
            );

            newY -= Mathf.Lerp(0, noise, tPerlin);

            // Update vertex
            vertices[i] = new Vector3(v.x, newY, v.z);

            // Update min/max heights
            _minHeight = Mathf.Min(_minHeight, newY);
            _maxHeight = Mathf.Max(_maxHeight, newY);
        }

        Mesh.Vertices = vertices.ToList();
        Mesh.UpdateMesh();
    }

    private Vector3Int GetDiscreteVertex(Vector3 point, float step)
    {
        return new Vector3Int(
            Mathf.RoundToInt(point.x / step),
            Mathf.RoundToInt(point.y / step),
            Mathf.RoundToInt(point.z / step)
        );
    }

    private float CalculateDistanceToBorder(Vector3 point, RegularPolygon polygon, HashSet<int> excludeBorderSet)
    {
        float minDistance = float.MaxValue;
        List<Vector3> cornerPoints = polygon.Points;

        for (int i = 0; i < cornerPoints.Count; i++)
        {
            // Skip excluded borders
            if (excludeBorderSet != null && excludeBorderSet.Contains(i))
            {
                continue;
            }

            Vector3 p1 = cornerPoints[i];
            Vector3 p2 = cornerPoints[(i + 1) % cornerPoints.Count];

            // Calculate distance from point to line segment (p1-p2)
            Vector3 lineDir = p2 - p1;
            Vector3 pointVec = point - p1;

            float lineLength = lineDir.magnitude;
            Vector3 lineNormalized = lineDir / lineLength;

            // Project point onto line
            float projection = Vector3.Dot(pointVec, lineNormalized);

            // Distance calculation based on projection
            float distance;
            if (projection < 0)
            {
                // Point projects before start of line segment
                distance = Vector3.Distance(point, p1);
            }
            else if (projection > lineLength)
            {
                // Point projects after end of line segment
                distance = Vector3.Distance(point, p2);
            }
            else
            {
                // Point projects onto line segment
                Vector3 projectedPoint = p1 + lineNormalized * projection;
                distance = Vector3.Distance(point, projectedPoint);
            }

            minDistance = Mathf.Min(minDistance, distance);
        }

        return minDistance;
    }

    private Vector3 FindClosestRidgePoint(List<Ridge> ridges, Vector2 point)
    {
        if (ridges == null || ridges.Count == 0)
        {
            return new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        }

        Vector3 closest = Vector3.zero;
        float minDistance = float.MaxValue;

        foreach (Ridge ridge in ridges)
        {
            foreach (Vector3 ridgePoint in ridge.GetPoints())
            {
                float dist = Vector2.Distance(
                    new Vector2(ridgePoint.x, ridgePoint.z),
                    point
                );

                if (dist < minDistance)
                {
                    minDistance = dist;
                    closest = ridgePoint;
                }
            }
        }

        return closest;
    }

    /// <summary>
    /// Calculates final heights - must be implemented by child classes
    /// </summary>
    public virtual void CalculateFinalHeights(DiscreteVertexToDistance distanceMap, float diameter, int divisions)
    {
        // Base implementation just performs shift compress
        ShiftCompress();
        Mesh.UpdateMesh();
    }

    /// <summary>
    /// Calculates and stores distances from corner points to borders
    /// </summary>
    public void CalculateCornerPointsDistancesToBorder(DiscreteVertexToDistance distanceMap, int divisions)
    {
        // Discretize points based on divisions
        Func<Vector3, Vector3Int> divisioned = point =>
            VertexToNormalDiscretizer.GetDiscreteVertex(point, _hexMeshParams.Diameter / (divisions * 2));

        // Collect neighbor corner points
        List<Vector3> neighboursCornerPoints = new List<Vector3>();
        foreach (var neighbor in _neighbours)
        {
            if (neighbor != null)
            {
                RidgeMesh ridgeMesh = neighbor as RidgeMesh;
                if (ridgeMesh != null)
                {
                    neighboursCornerPoints.AddRange(ridgeMesh.GetHexagon.Points);
                }
            }
        }

        // Calculate distances for each corner point
        List<Vector3> cornerPoints = GetHexagon.Points;

        PointToLineDistance_VectorMultBased calculator =
            new PointToLineDistance_VectorMultBased(GetExcludeBorderSet(), cornerPoints.ToArray());

        foreach (var v in cornerPoints)
        {
            float distanceToBorder = calculator.Calc(v);

            // Check for minimum distance considering neighbors
            foreach (var point in neighboursCornerPoints)
            {
                float pointDistance = Vector2.Distance(
                    new Vector2(v.x, v.z),
                    new Vector2(point.x, point.z)
                );

                Vector3Int discretePoint = divisioned(point);
                if (distanceMap.ContainsKey(discretePoint))
                {
                    distanceToBorder = Mathf.Min(
                        distanceToBorder,
                        pointDistance + distanceMap[discretePoint]
                    );
                }
            }

            // Update distance in the map
            Vector3Int discreteV = divisioned(v);
            if (distanceMap.ContainsKey(discreteV))
            {
                distanceToBorder = Mathf.Min(distanceToBorder, distanceMap[discreteV]);
            }

            distanceMap[discreteV] = distanceToBorder;
        }
    }

    /// <summary>
    /// Gets the set of borders to exclude based on neighbor placement
    /// </summary>
    private HashSet<int> GetExcludeBorderSet()
    {
        HashSet<int> result = new HashSet<int>();

        List<Vector3> cornerPoints = GetHexagon.Points;
        int size = cornerPoints.Count;
        float approxR = _hexMeshParams.Diameter / 2f;

        for (int i = 0; i < size; ++i)
        {
            Vector3 p1 = cornerPoints[i];
            Vector3 p2 = cornerPoints[(i + 1) % size];
            Vector3 mid = (p1 + p2) / 2;

            bool shouldExclude = _neighbours.Any(tileMesh =>
            {
                if (tileMesh == null)
                    return false;

                var neighborCenter = (tileMesh as RidgeMesh)?.GetHexagon.Center ?? Vector3.zero;
                return Vector3.Distance(mid, neighborCenter) < (approxR * 1.1f);
            });

            if (shouldExclude)
            {
                result.Add(i);
            }
        }

        return result;
    }

    /// <summary>
    /// Gets the center of the mesh
    /// </summary>
    public Vector3 GetCenter()
    {
        return GetHexagon.Center;
    }

    /// <summary>
    /// Gets neighboring tile meshes
    /// </summary>
    public List<RidgeMesh> GetNeighbours()
    {
        return _neighbours;
    }

    /// <summary>
    /// Sets neighboring tile meshes
    /// </summary>
    public void SetNeighbours(List<RidgeMesh> neighbours)
    {
        _neighbours = neighbours;
    }

    /// <summary>
    /// Sets ridge data for this mesh
    /// </summary>
    public void SetRidges(List<Ridge> ridges)
    {
        _ridges = ridges;
    }

    public List<Ridge> GetRidges()
    {
        return _ridges;
    }
    
    public override bool Equals(object obj) => obj is RidgeMesh other && _id == other._id;
    public override int GetHashCode() => _id;
}