using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Extensions;
using TrinketTinker.Effects.Support;
using TrinketTinker.Models;
using TrinketTinker.Models.AbilityArgs;

namespace TrinketTinker.Effects.Abilities;

/// <summary>Trigger a dialogue box from matching <see cref="ChatterLinesData"/></summary>
/// <param name="effect"></param>
/// <param name="data"></param>
/// <param name="lvl"></param>
public sealed class ChatterAbility(TrinketTinkerEffect effect, AbilityData data, int lvl)
    : Ability<ChatterArgs>(effect, data, lvl)
{
    private Dictionary<string, ChatterLinesData>? chatter = null;
    private readonly HashSet<string> spoken = [];

    public override bool Activate(Farmer farmer)
    {
        chatter = e.Data?.Chatter;
        if (chatter == null)
            return false;
        return base.Activate(farmer);
    }

    protected override void CleanupEffect(Farmer farmer)
    {
        chatter = null;
        spoken.Clear();
        base.CleanupEffect(farmer);
    }

    protected override bool ApplyEffect(ProcEventArgs proc)
    {
        // choose line
        if (
            chatter!
                .OrderByDescending((kv) => kv.Value.Priority)
                .FirstOrDefault(
                    (kv) =>
                        (args.ChatterPrefix == null || kv.Key.StartsWith(args.ChatterPrefix))
                        && GameStateQuery.CheckConditions(kv.Value.Condition, proc.GSQContext)
                )
            is KeyValuePair<string, ChatterLinesData> foundLines
        )
        {
            string chosen = Random.Shared.ChooseFrom(foundLines.Value.Lines);
            ModEntry.Log(chosen);
            // draw the dialogue
            ChatterSpeaker? speaker = e.CompanionSpeaker;
            NPC? speakerNPC = null;
            if (speaker != null && speaker.Portrait.Value != null)
            {
                //new AnimatedSprite("Characters\\Abigail", 0, 16, 16)
                speakerNPC = new(null, Vector2.Zero, "", 0, "???", speaker.Portrait.Value, eventActor: false)
                {
                    displayName = speaker.DisplayName,
                };
            }
            Game1.DrawDialogue(
                new Dialogue(speakerNPC, chosen, Game1.content.LoadString(chosen, args.Substitutions.ToArray()))
            );
            return base.ApplyEffect(proc);
        }
        ModEntry.Log("no line picked");
        return false;
    }
}
