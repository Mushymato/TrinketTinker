namespace TrinketTinker.Models.MotionArgs
{
    /// <summary>Orbit radius</summary>
    public class OrbitArgs : StaticArgs
    {
        /// <summary>Orbit radius, horizontal</summary>
        public float RadiusX { get; set; } = -1;
        /// <summary>Orbit radius, vertical</summary>
        public float RadiusY { get; set; } = -1;
        /// <inheritdoc/>
        public new bool Validate() => RadiusX > -1 && RadiusY > -1;
    }
}