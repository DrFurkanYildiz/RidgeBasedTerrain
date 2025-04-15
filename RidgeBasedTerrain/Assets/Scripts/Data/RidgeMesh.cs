using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// The RidgeMesh class represents a hexagonal mesh with ridge-based terrain generation capabilities.
/// </summary>
public class RidgeMesh : TileMesh, IRidgeBased
{
    // Hexagon parameters
    protected Hexagon _hexagon;
    public Hexagon GetHexagon => _hexagon;
    protected Mesh _mesh;
    protected int _id;
    protected float _diameter;
    protected int _divisions;
    protected bool _frameState;
    protected float _frameOffset;
    protected Material _material;
    
    // Terrain parameters
    protected FastNoiseLite _plainNoise;
    protected FastNoiseLite _ridgeNoise;
    protected List<Ridge> _ridges = new List<Ridge>();
    protected List<TileMesh> _neighbours = new List<TileMesh>();
    
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
        _hexagon = hexagon;
        _id = parameters.HexMeshParams.Id;
        _diameter = parameters.HexMeshParams.Diameter;
        _divisions = parameters.HexMeshParams.Divisions;
        _frameState = parameters.HexMeshParams.FrameState;
        _frameOffset = parameters.HexMeshParams.FrameOffset;
        _material = parameters.HexMeshParams.Material;
        
        _plainNoise = parameters.PlainNoise;
        _ridgeNoise = parameters.RidgeNoise;
        
        // Initialize mesh
        CreateMesh();
        
        // Initialize processor based on orientation - Plane is default for this implementation
        _processor = new FlatMeshProcessor();
    }

    /// <summary>
    /// Creates the initial mesh from the hexagon
    /// </summary>
    protected virtual void CreateMesh()
    {
        _mesh = new Mesh();
        _mesh.name = $"RidgeMesh_{_id}";
        
        // Calculate vertices, triangles, and other mesh data
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        
        // Generate a triangulated mesh of the hexagon
        TriangulateHexagon(vertices, triangles);
        
        // Add frame if needed
        if (_frameState)
        {
            AddFrame(vertices, triangles);
        }
        
        // Set mesh data
        _mesh.vertices = vertices.ToArray();
        _mesh.triangles = triangles.ToArray();
        
        // Calculate additional mesh data
        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();
        CalculateUVs();
    }
    
    /// <summary>
    /// Triangulates a hexagon into mesh data
    /// </summary>
    protected void TriangulateHexagon(List<Vector3> vertices, List<int> triangles)
    {
        float hexRadius = _diameter / 2f;
        Vector3 center = _hexagon.Center;
        List<Vector3> cornerPoints = _hexagon.Points;
        
        // Subdivide the hexagon based on divisions parameter
        float zStep = hexRadius / _divisions;
        float halfZ = zStep / 2;
        float xStep = zStep * Mathf.Sqrt(3) / 2;

        float zInitEven = _diameter / 2f;
        float zInitOdd = zInitEven - halfZ;
        int triCount = _divisions * 2 + (_divisions * 2 - 1);

        for (int layer = 0; layer < _divisions; layer++, triCount -= 2)
        {
            float zEven = zInitEven - (layer * halfZ);
            float zOdd = zInitOdd - (layer * halfZ);

            for (int i = 0; i < triCount; i++)
            {
                if ((i & 1) == 1) // Odd triangle
                {
                    Vector3 v1 = new Vector3(xStep * layer + xStep, 0, zOdd);
                    Vector3 v2 = new Vector3(xStep * layer, 0, zOdd - halfZ);
                    Vector3 v3 = new Vector3(xStep * layer + xStep, 0, zOdd - zStep);
                    
                    int baseIndex = vertices.Count;
                    vertices.Add(v1);
                    vertices.Add(v3);
                    vertices.Add(v2);
                    
                    triangles.Add(baseIndex);
                    triangles.Add(baseIndex + 1);
                    triangles.Add(baseIndex + 2);
                    
                    zOdd -= zStep;
                }
                else // Even triangle
                {
                    Vector3 v1 = new Vector3(xStep * layer, 0, zEven);
                    Vector3 v3 = new Vector3(xStep * layer, 0, zEven - zStep);
                    Vector3 v2 = new Vector3(xStep * layer + xStep, 0, zEven - halfZ);
                    
                    int baseIndex = vertices.Count;
                    vertices.Add(v1);
                    vertices.Add(v2);
                    vertices.Add(v3);
                    
                    triangles.Add(baseIndex);
                    triangles.Add(baseIndex + 1);
                    triangles.Add(baseIndex + 2);
                    
                    zEven -= zStep;
                }
            }
        }

        // Transform vertices to be centered around the hexagon center
        for (int i = 0; i < vertices.Count; i++)
        {
            // Transform local coordinates to world space relative to hexagon center
            vertices[i] = center + vertices[i] - new Vector3(_diameter / 4, 0, 0);
        }
    }
    
    /// <summary>
    /// Adds a frame around the hexagon
    /// </summary>
    protected void AddFrame(List<Vector3> vertices, List<int> triangles)
    {
        List<Vector3> cornerPoints = _hexagon.Points;
        
        for (int i = 0; i < 6; i++)
        {
            Vector3 a = cornerPoints[i];
            Vector3 b = cornerPoints[(i + 1) % 6];
            Vector3 c = a + Vector3.down * _frameOffset;
            Vector3 d = b + Vector3.down * _frameOffset;
            
            int baseIndex = vertices.Count;
            vertices.AddRange(new[] { b, c, a, b, d, c });
            
            triangles.Add(baseIndex);
            triangles.Add(baseIndex + 1);
            triangles.Add(baseIndex + 2);
            
            triangles.Add(baseIndex + 3);
            triangles.Add(baseIndex + 4);
            triangles.Add(baseIndex + 5);
        }
    }
    
    /// <summary>
    /// Calculates UVs for the mesh
    /// </summary>
    protected void CalculateUVs()
    {
        Vector3[] vertices = _mesh.vertices;
        Vector2[] uvs = new Vector2[vertices.Length];
        
        float radius = _diameter / 2f;
        float smallRadius = Mathf.Sqrt(3) * radius / 2f;
        
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 v = vertices[i];
            float u = (v.x + smallRadius) / (2 * smallRadius);
            float v_coord = (v.z + radius) / (2 * radius);
            uvs[i] = new Vector2(u, v_coord);
        }
        
        _mesh.uv = uvs;
    }
    
    #region TileMesh Implementations
    public override int GetId() => _id;
    public override Mesh Mesh => _mesh;
    #endregion

    #region IRidgeBased Implementations
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
    public virtual void CalculateInitialHeights()
    {
        Vector3 normal = Vector3.up;
        _initialVertices = _mesh.vertices.Clone() as Vector3[];

        Vector3[] vertices = _mesh.vertices;
        _processor.CalculateInitialHeights(vertices, _plainNoise, ref _minHeight, ref _maxHeight, normal);
        _mesh.vertices = vertices;
        _mesh.RecalculateBounds();
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

        Vector3[] vertices = _mesh.vertices;
        Vector3 center = _hexagon.Center;

        vertices = _processor.ShiftCompress(vertices, _yShift, _yCompress, center.y);

        _mesh.vertices = vertices;
        _mesh.RecalculateBounds();
    }

    /// <summary>
    /// Calculates ridge-based heights
    /// </summary>
    public void CalculateRidgeBasedHeights(
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
                RidgeMesh ridgeMesh = neighbor as RidgeMesh;
                if (ridgeMesh != null)
                {
                    neighboursCornerPoints.AddRange(ridgeMesh._hexagon.Points);
                }
            }
        }

        Vector3[] vertices = _mesh.vertices;
        
        float diameter = _diameter;
        float radius = diameter / 2f;
        
        // Create a regular polygon from hexagon for calculations
    RegularPolygon basePolygon = _hexagon;
    
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
            Vector3Int discretePoint = GetDiscreteVertex(point, diameter / (divisions * 2));
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
        float noise = (_ridgeNoise != null) ? 
            Mathf.Abs(_ridgeNoise.GetNoise2D(v.x, v.z)) * 0.289f : 0;
        
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
    
    _mesh.vertices = vertices;
    _mesh.RecalculateBounds();
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
    }

    /// <summary>
    /// Calculates and stores distances from corner points to borders
    /// </summary>
    public void CalculateCornerPointsDistancesToBorder(DiscreteVertexToDistance distanceMap, int divisions)
    {
        float diameter = _diameter;
        
        // Discretize points based on divisions
        Func<Vector3, Vector3Int> divisioned = point => 
            VertexToNormalDiscretizer.GetDiscreteVertex(point, diameter / (divisions * 2));
        
        // Collect neighbor corner points
        List<Vector3> neighboursCornerPoints = new List<Vector3>();
        foreach (var neighbor in _neighbours)
        {
            if (neighbor != null)
            {
                RidgeMesh ridgeMesh = neighbor as RidgeMesh;
                if (ridgeMesh != null)
                {
                    neighboursCornerPoints.AddRange(ridgeMesh._hexagon.Points);
                }
            }
        }
        
        // Calculate distances for each corner point
        List<Vector3> cornerPoints = _hexagon.Points;
        
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
    #endregion

    /// <summary>
    /// Gets the set of borders to exclude based on neighbor placement
    /// </summary>
    protected HashSet<int> GetExcludeBorderSet()
    {
        HashSet<int> result = new HashSet<int>();
        
        List<Vector3> cornerPoints = _hexagon.Points;
        int size = cornerPoints.Count;
        float approxR = _diameter / 2f;
        
        for (int i = 0; i < size; ++i)
        {
            Vector3 p1 = cornerPoints[i];
            Vector3 p2 = cornerPoints[(i + 1) % size];
            Vector3 mid = (p1 + p2) / 2;
            
            bool shouldExclude = _neighbours.Any(tileMesh => {
                if (tileMesh == null)
                    return false;
                
                var neighborCenter = (tileMesh as RidgeMesh)?._hexagon.Center ?? Vector3.zero;
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
        return _hexagon.Center;
    }

    /// <summary>
    /// Gets neighboring tile meshes
    /// </summary>
    public List<TileMesh> GetNeighbours()
    {
        return _neighbours.Where(n => n != null).ToList();
    }

    /// <summary>
    /// Sets neighboring tile meshes
    /// </summary>
    public void SetNeighbours(List<TileMesh> neighbours)
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

    /// <summary>
    /// Recalculates normals for the mesh
    /// </summary>
    public void RecalculateNormals()
    {
        _mesh.RecalculateNormals();
    }

    /// <summary>
    /// Updates all mesh data except vertices
    /// </summary>
    public void RecalculateAllExceptVertices()
    {
        _mesh.RecalculateNormals();
        _mesh.RecalculateTangents();
        _mesh.RecalculateBounds();
    }
}