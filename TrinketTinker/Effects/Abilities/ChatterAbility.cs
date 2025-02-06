using System.Reflection;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.TokenizableStrings;
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
    private Dictionary<string, ChatterLinesData> chatter = null!;
    private NPC speakerNPC = null!;
    private readonly FieldInfo dialogueField = typeof(NPC).GetField(
        "dialogue",
        BindingFlags.NonPublic | BindingFlags.Instance
    )!;
    private readonly FieldInfo portraitField = typeof(NPC).GetField(
        "portrait",
        BindingFlags.NonPublic | BindingFlags.Instance
    )!;

    private string GetText(string text)
    {
        // object[] subs = args.Substitutions?.ToArray() ?? [];
        if (Game1.content.IsValidTranslationKey(text))
            return Game1.content.LoadString(text);
        return TokenParser.ParseText(text) ?? "CHATTER ERROR";
    }

    public override bool Activate(Farmer farmer)
    {
        if (base.Activate(farmer))
        {
            chatter = e.Data?.Chatter!;
            if (chatter == null)
                return false;
            speakerNPC = new(null, Vector2.Zero, "", 0, "???", null, eventActor: false) { displayName = "???" };
        }
        return Active;
    }

    protected override void CleanupEffect(Farmer farmer)
    {
        chatter = null!;
        speakerNPC = null!;
        base.CleanupEffect(farmer);
    }

    protected override bool ApplyEffect(ProcEventArgs proc)
    {
        // choose chatter data, either from next chatter key proc'd by ability or by conds
        if (e.NextChatterKey == null || !chatter.TryGetValue(e.NextChatterKey, out ChatterLinesData? foundLines))
        {
            if (
                chatter
                    .OrderByDescending((kv) => kv.Value.Priority)
                    .FirstOrDefault(
                        (kv) =>
                            (args.ChatterPrefix == null || kv.Key.StartsWith(args.ChatterPrefix))
                            && GameStateQuery.CheckConditions(kv.Value.Condition, proc.GSQContext)
                    )
                is KeyValuePair<string, ChatterLinesData> foundLinesKV
            )
                foundLines = foundLinesKV.Value;
            else
                return false;
        }
        e.NextChatterKey = null;
        if (foundLines == null)
            return false;

        string chosen = Random.Shared.ChooseFrom(foundLines.Lines);
        // draw the dialogue
        ChatterSpeaker? speaker = e.CompanionSpeaker;
        if (speaker != null && speaker.Portrait.Value != null)
        {
            speakerNPC.displayName = speaker.DisplayName;
            portraitField.SetValue(speakerNPC, speaker.Portrait.Value);
        }
        else
        {
            speakerNPC.displayName = "???";
            portraitField.SetValue(speakerNPC, null);
        }

        if (foundLines.Responses != null)
        {
            Dictionary<string, string> TranslatedResponses = foundLines
                .Responses.Select((kv) => new KeyValuePair<string, string>(kv.Key, GetText(kv.Value)))
                .ToDictionary((kv) => kv.Key, (kv) => kv.Value);
            dialogueField.SetValue(speakerNPC, TranslatedResponses);
        }
        Game1.DrawDialogue(new(speakerNPC, chosen, GetText(chosen)));
        return base.ApplyEffect(proc);
    }
}
