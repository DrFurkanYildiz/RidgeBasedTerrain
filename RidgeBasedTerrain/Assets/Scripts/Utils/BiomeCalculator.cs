using UnityEngine;

public class BiomeCalculator
{
    private float _waterThreshold = 0.2f;
    private float _plainThreshold = 0.65f;
    private float _hillThreshold = 0.75f;
    
    public Biome CalculateBiome(float minZ, float maxZ, float curZ)
    {
        float amplitude = maxZ - minZ;
        
        float waterMaxZ = minZ + amplitude * _waterThreshold;
        float plainMaxZ = minZ + amplitude * _plainThreshold;
        float hillMaxZ = minZ + amplitude * _hillThreshold;
        float mountainMaxZ = maxZ;
        
        if (minZ <= curZ && curZ <= waterMaxZ)
        {
            return Biome.Water;
        }
        else if (waterMaxZ < curZ && curZ <= plainMaxZ)
        {
            return Biome.Plain;
        }
        else if (plainMaxZ < curZ && curZ <= hillMaxZ)
        {
            return Biome.Hill;
        }
        else if (hillMaxZ < curZ && curZ <= mountainMaxZ)
        {
            return Biome.Mountain;
        }
        
        Debug.LogError("Non reachable: unknown biome");
        return Biome.Water; // Default fallback
    }
}