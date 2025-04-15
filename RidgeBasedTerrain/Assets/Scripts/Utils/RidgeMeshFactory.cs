using UnityEngine;

/// <summary>
/// Factory methods for creating ridge meshes
/// </summary>
public static class RidgeMeshFactory
{
    public static RidgeMesh CreateRidgeMesh(Biome biome, Hexagon hexagon, RidgeMeshParams parameters)
    {
        switch (biome)
        {
            case Biome.Mountain:
                return new MountainMesh(hexagon, parameters);
            case Biome.Plain:
                return new PlainMesh(hexagon, parameters);
            case Biome.Hill:
                return new HillMesh(hexagon, parameters);
            case Biome.Water:
                return new WaterMesh(hexagon, parameters);
            default:
                Debug.LogError("Unknown biome type");
                return new PlainMesh(hexagon, parameters);
        }
    }
}