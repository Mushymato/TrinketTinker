namespace TrinketTinker.Models
{
    public enum ProcOn
    {
        Always = 0, // this is Apply + Unapply
        Use = 1,
        Footstep = 2,
        ReceiveDamage = 3,
        DamageMonster = 4,
        SecondElapsed = 5
    }
    public class AbilityData
    {
        public string Name = "";
        public string? AbilityClass { get; set; } = null;
        public ProcOn ProcOn = new();
        public string? Condition { get; set; } = null;
        public int DamageThreshold { get; set; } = -1;
        public Dictionary<string, string> Args { get; set; } = new();
    }
}