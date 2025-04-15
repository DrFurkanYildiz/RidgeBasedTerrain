using System.Collections.Generic;
using UnityEngine;

public class HexGridMap : MonoBehaviour
{
    [Header("Hex Settings")]
    [SerializeField] private int size = 5;
    [SerializeField] private int divisions = 3;
    [SerializeField] private float diameter = 1.0f;
    [SerializeField] private bool frameState = true;
    [SerializeField] private float frameOffset = 0.05f;
    [SerializeField] private Material material;
    [SerializeField] private ClipOptions clipOptions;

    private List<List<Tile>> tilesLayout = new();
    protected List<List<Vector3Int>> colRowLayout;

    private void Start()
    {
        InitColRowLayout();
        InitHexMesh();
    }

    private void InitHexMesh()
    {
        tilesLayout.Clear();

        for (int row = 0; row < colRowLayout.Count; row++)
        {
            var tileRow = new List<Tile>();
            for (int col = 0; col < colRowLayout[row].Count; col++)
            {
                //var hexagon = new Hexagon();
                var hexParams = new HexMeshParams()
                {
                    Diameter = diameter,
                    Divisions = divisions,
                    FrameState = frameState,
                    FrameOffset = frameOffset,
                    ClipOptions = clipOptions
                };

                //var tileMesh = new TileMesh(hexagon, hexParams);
                
                
                GameObject tileObj = new GameObject($"Tile_{row}_{col}");
                tileObj.transform.SetParent(transform);
                var tile = tileObj.AddComponent<Tile>();

                var offset = new OffsetCoordinates(row, col);
                //var position = HexUtils.CubeToWorld(HexUtils.OffsetToCube(offset), diameter);
                var position = PointyHexToPixel3D(offset, diameter);
                //tile.Initialize(tileMesh, position, material, offset);
                tileRow.Add(tile);
            }
            
            tilesLayout.Add(tileRow);
        }
    }
    
    protected virtual void InitColRowLayout()
    {
        colRowLayout = GetOffsetCoordsLayout(size, size);
    }

    private static List<List<Vector3Int>> GetOffsetCoordsLayout(int height, int width)
    {
        // İki boyutlu liste oluştur
        var colRowLayout = new List<List<Vector3Int>>(height);
        
        for (int row = 0; row < height; ++row)
        {
            // Her satır için yeni bir liste oluştur
            var rowList = new List<Vector3Int>(width);
            
            for (int col = 0; col < width; ++col)
            {
                // Vector3i yerine Unity'nin Vector3Int'ini kullan
                rowList.Add(new Vector3Int(row, 0, col));
            }
            
            colRowLayout.Add(rowList);
        }
        
        return colRowLayout;
    }
/*
    private static Vector3 PointyHexToPixel3D(OffsetCoordinates offset, float diameter)
    {
        var x = diameter * (Mathf.Sqrt(3) * offset.row + Mathf.Sqrt(3)/2 * offset.col);
        var z = diameter * (3f/2 * offset.col);
        return new Vector3(x, 0, z);
    }
    */
    private static Vector3 PointyHexToPixel3D(OffsetCoordinates offset, float diameter)
    {
        var hexWidth = diameter * Mathf.Sqrt(3);  // Genişlik (2 * SmallRadius)
        var hexHeight = diameter * 1.5f;          // Yükseklik (1.5 * diameter)
        
        var x = (offset.col + (offset.row % 2) * 0.5f) * (hexWidth / 2f);
        var z = offset.row * (hexHeight / 2f);
        
        return new Vector3(x, 0, z);
    }
}