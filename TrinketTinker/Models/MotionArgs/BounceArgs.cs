namespace TrinketTinker.Models.MotionArgs;

/// <summary>Bounce args</summary>
public sealed class BounceArgs : LerpArgs
{
    /// <summary>Bounce height</summary>
    public float MaxHeight { get; set; } = 96f;

    /// <summary>Deform when hitting the ground</summary>
    public float Squash { get; set; } = 0f;

    /// <summary>Period of bounce, in ms</summary>
    public double Period { get; set; } = 400f;
}
