namespace TrinketTinker.Models.MotionArgs;

/// <summary>Orbit args</summary>
public sealed class OrbitArgs : StaticArgs
{
    /// <summary>Orbit radius, horizontal</summary>
    public float RadiusX { get; set; } = 96f;

    /// <summary>Orbit radius, vertical</summary>
    public float RadiusY { get; set; } = 40f;

    /// <summary>Y offset from shadow</summary>
    public float Height { get; set; } = 64f;

    /// <summary>Orbit period</summary>
    public double Period { get; set; } = 1000f;

    /// <inheritdoc/>
    public new bool Validate() => RadiusX > -1 && RadiusY > -1;
}
