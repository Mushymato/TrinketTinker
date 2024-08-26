namespace TrinketTinker
{
    internal static class Constants
    {
        /// <summary>Float second of 1 frame (60fps), not accurate since it is float, only use for bounds.</summary>
        internal const float ONE_FRAME = 1000 / 60;

        /// <summary>String pattern for trinket tinker motion classes.</summary>
        internal const string MOTION_CLS = "TrinketTinker.Companions.Motions.{0}Motion, TrinketTinker";

        /// <summary>String pattern for trinket tinker ability classes</summary>
        internal const string ABILITY_CLS = "TrinketTinker.Effects.Abilities.{0}Ability, TrinketTinker";

        /// <summary>Color name, refers</summary>
        internal const string COLOR_PRISMATIC = "Prismatic";
    }
}
