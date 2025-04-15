/// <summary>
/// Parameters for ridge mesh generation
/// </summary>
public class RidgeMeshParams
{
    public HexMeshParams HexMeshParams { get; set; }
    public FastNoiseLite PlainNoise { get; set; }
    public FastNoiseLite RidgeNoise { get; set; }
}