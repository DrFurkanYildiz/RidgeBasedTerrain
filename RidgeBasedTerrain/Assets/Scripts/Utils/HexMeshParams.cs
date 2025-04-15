using UnityEngine;

/// <summary>
/// Hex mesh parameters
/// </summary>
public class HexMeshParams
{
    public int Id { get; set; } = 0;
    public float Diameter { get; set; } = 10f;
    public bool FrameState { get; set; } = false;
    public float FrameOffset { get; set; } = 0f;
    public Material Material { get; set; }
    public int Divisions { get; set; } = 3;
    public ClipOptions ClipOptions { get; set; } = new ClipOptions();
}