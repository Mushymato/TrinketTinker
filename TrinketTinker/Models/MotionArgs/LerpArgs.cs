using TrinketTinker.Models.Mixin;

namespace TrinketTinker.Models.MotionArgs;

/// <summary>Lerp args</summary>
public class LerpArgs : IArgs
{
    /// <summary>Min distance from anchor, the companion does not move until they are this far from the anchor.</summary>
    public float Min { get; set; } = 96f;

    /// <summary>Max distance from anchor, if the companion is farther away than this, teleport.</summary>
    public float Max { get; set; } = 1280f;

    /// <summary>Lerp rate in miliseconds</summary>
    public double Rate { get; set; } = 400f;

    /// <summary>Pause between lerp retargeting, in ms</summary>
    public float Pause { get; set; } = 0f;

    /// <summary>Stop moving as soon as the player stops moving.</summary>
    public bool MoveSync { get; set; } = false;

    /// <summary>While within the minimum range, randomly move around a little bit.</summary>
    public float Jitter { get; set; } = 0f;

    /// <inheritdoc/>
    public bool Validate() => Min < Max;
}
