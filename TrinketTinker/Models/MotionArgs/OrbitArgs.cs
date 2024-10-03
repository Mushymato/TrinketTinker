namespace TrinketTinker.Models.MotionArgs
{
    /// <summary>Orbit radius</summary>
    public class OrbitArgs : StaticArgs
    {
        public float RadiusX { get; set; } = -1;
        public float RadiusY { get; set; } = -1;
        public new bool Validate() => RadiusX > -1 && RadiusY > -1;
    }
}