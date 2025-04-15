using UnityEngine;

/// <summary>
/// A lightweight noise generation utility for procedural generation
/// Based on the FastNoise Lite library
/// </summary>
public class FastNoiseLite
{
    private System.Random random;
    private int seed;
    private float frequency = 0.01f;
    
    /// <summary>
    /// Noise type for generation algorithms
    /// </summary>
    public enum NoiseType
    {
        Perlin,
        ValueFractal,
        Simplex
    }
    
    private NoiseType noiseType = NoiseType.Perlin;
    
    /// <summary>
    /// Creates a new FastNoiseLite instance with the specified seed
    /// </summary>
    public FastNoiseLite(int seed = 1337)
    {
        this.seed = seed;
        random = new System.Random(seed);
    }
    
    /// <summary>
    /// Sets the noise type for generation
    /// </summary>
    public void SetNoiseType(NoiseType type)
    {
        noiseType = type;
    }
    
    /// <summary>
    /// Sets the frequency for noise generation
    /// </summary>
    public void SetFrequency(float freq)
    {
        frequency = freq;
    }
    
    /// <summary>
    /// Gets 2D noise at the specified coordinates
    /// </summary>
    public float GetNoise2D(float x, float z)
    {
        x *= frequency;
        z *= frequency;
        
        switch (noiseType)
        {
            case NoiseType.Perlin:
                return Mathf.PerlinNoise(x + seed, z + seed) * 2 - 1;
            case NoiseType.ValueFractal:
                return (Mathf.PerlinNoise(x + seed, z + seed) + 
                       Mathf.PerlinNoise(x * 2 + seed, z * 2 + seed) * 0.5f +
                       Mathf.PerlinNoise(x * 4 + seed, z * 4 + seed) * 0.25f) / 1.75f * 2 - 1;
            case NoiseType.Simplex:
                // A simple approximation of simplex using Perlin
                return Mathf.PerlinNoise(x + seed, z + seed) * 2 - 1;
            default:
                return 0;
        }
    }
    
    /// <summary>
    /// Gets 3D noise at the specified coordinates
    /// </summary>
    public float GetNoise3D(float x, float y, float z)
    {
        // 3D noise approximation using multiple 2D slices
        float xy = GetNoise2D(x, y);
        float yz = GetNoise2D(y, z);
        float xz = GetNoise2D(x, z);
        
        return (xy + yz + xz) / 3f;
    }
    
    /// <summary>
    /// Gets 3D noise at the specified vector position
    /// </summary>
    public float GetNoise3D(Vector3 position)
    {
        return GetNoise3D(position.x, position.y, position.z);
    }
}