using Microsoft.Xna.Framework;

namespace TrinketTinker.Models.MotionArgs;

/// <summary>Orbit args</summary>
public sealed class RelativeArgs : StaticArgs
{
    /// <summary>Offset (Down)</summary>
    public Vector2? OffsetD { get; set; } = null;

    /// <summary>Offset (Right)</summary>
    public Vector2? OffsetR { get; set; } = null;

    /// <summary>Offset (Up)</summary>
    public Vector2? OffsetU { get; set; } = null;

    /// <summary>Offset (Left)</summary>
    public Vector2? OffsetL { get; set; } = null;

    /// <summary>Layer Depth Offset (Down)</summary>
    public float? LayerD { get; set; } = null;

    /// <summary>Layer Depth Offset (Right)</summary>
    public float? LayerR { get; set; } = null;

    /// <summary>Layer Depth Offset (Up)</summary>
    public float? LayerU { get; set; } = null;

    /// <summary>Layer Depth Offset (Left)</summary>
    public float? LayerL { get; set; } = null;

    /// <inheritdoc/>
    public new bool Validate() => OffsetD != null && OffsetR != null && OffsetL != null && OffsetU != null;
}
