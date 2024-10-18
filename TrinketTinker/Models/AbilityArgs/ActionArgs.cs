using TrinketTinker.Models.Mixin;

namespace TrinketTinker.Models.AbilityArgs
{
    /// <summary>(trigger) action arguments</summary>
    public sealed class ActionArgs : IArgs
    {
        public string? Action = null;
        public bool Validate()
        {
            return Action != null;
        }
    }
}
