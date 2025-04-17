using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Mountain mesh implementation for ridge-based peaked terrain
/// </summary>
public class MountainMesh : RidgeMesh
{
    public MountainMesh(Hexagon hex, RidgeMeshParams parameters) : base(hex, parameters) { }
    
    // MountainMesh içine debug kodu ekleyin
    // MountainMesh.cs içinde, CalculateFinalHeights metoduna değişiklik yapın
    // MountainMesh.cs içinde CalculateFinalHeights metodunda
    public override void CalculateFinalHeights(DiscreteVertexToDistance distanceMap, float diameter, int divisions)
    {
        Debug.Log($"MountainMesh has {_ridges.Count} ridges");
    
        // Orijinal ridge listesini kopyala
        var ridgesCopy = new List<Ridge>(_ridges);
    
        // Döngüyü kopyalanan liste üzerinde çalıştır
        for (int i = 0; i < ridgesCopy.Count; i++)
        {
            Ridge ridge = ridgesCopy[i];
        
            // Mevcut ridge'in özelliklerini alalım
            Vector3 start = ridge.Start;
            Vector3 end = ridge.End;
        
            // Bitiş noktasının yüksekliğini değiştirelim
            end.y += UnityEngine.Random.Range(2.0f, 5.0f);
        
            // Yeni ridge'i orijinal listeye ata
            _ridges[i] = new Ridge(start, end, 12);
        }
    
        if (_ridges.Count > 0)
        {
            Debug.Log($"After modification - First ridge start: {_ridges[0].Start.y}, end: {_ridges[0].End.y}");
        }
    
        // Use linear interpolation for mountain terrain
        CalculateRidgeBasedHeights(
            (a, b, c) => Mathf.Lerp(a, b, c),
            diameter, // Daha belirgin dağlar için
            distanceMap,
            divisions
        );
    }
}