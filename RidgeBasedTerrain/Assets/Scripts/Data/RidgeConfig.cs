/// <summary>
/// Configuration parameters for ridge generation
/// </summary>
public class RidgeConfig
{
    public float VariationMinBound { get; set; } = 0.0f;
    public float VariationMaxBound { get; set; } = 0.02f;
    public float TopRidgeOffset { get; set; } = 0.1f;
    public float BottomRidgeOffset { get; set; } = -0.075f;
}