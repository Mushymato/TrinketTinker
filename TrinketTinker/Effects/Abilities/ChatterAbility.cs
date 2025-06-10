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
    private const string QQQ = "???";
    private Dictionary<string, ChatterLinesData> chatter = null!;
    private NPC speakerNPC = null!;
    private Dialogue? chosenDialogue = null;
    private readonly FieldInfo dialogueField = typeof(NPC).GetField(
        "dialogue",
        BindingFlags.NonPublic | BindingFlags.Instance
    )!;
    private readonly FieldInfo portraitField = typeof(NPC).GetField(
        "portrait",
        BindingFlags.NonPublic | BindingFlags.Instance
    )!;

    private static string GetText(string text)
    {
        if (Game1.content.IsValidTranslationKey(text))
            return Game1.content.LoadString(text);
        return TokenParser.ParseText(text) ?? "CHATTER ERROR";
    }

    public override bool Activate(Farmer farmer)
    {
        chatter = e.Data?.Chatter!;
        if (chatter == null)
            return false;
        speakerNPC = new(null, Vector2.Zero, "", 0, QQQ, null, eventActor: false) { displayName = QQQ };
        if (!base.Activate(farmer))
        {
            chatter = null!;
            speakerNPC = null!;
            return false;
        }
        return true;
    }

    protected override void CleanupEffect(Farmer farmer)
    {
        chatter = null!;
        speakerNPC = null!;
        base.CleanupEffect(farmer);
    }

    protected override bool ApplyEffect(ProcEventArgs proc)
    {
        if (Game1.activeClickableMenu != null)
            return false;
        if (!chosenDialogue?.isDialogueFinished() ?? false)
        {
            Game1.DrawDialogue(chosenDialogue);
            return base.ApplyEffect(proc);
        }
        chosenDialogue = null;
        // choose chatter data, either from next chatter key proc'd by ability or by conds
        if (e.NextChatterKey == null || !chatter.TryGetValue(e.NextChatterKey, out ChatterLinesData? foundLines))
        {
            if (
                chatter
                    .OrderByDescending((kv) => kv.Value?.Priority ?? 0)
                    .Where(
                        (kv) =>
                            (args.ChatterPrefix == null || kv.Key.StartsWith(args.ChatterPrefix ?? ""))
                            && GameStateQuery.CheckConditions(kv.Value.Condition, proc.GSQContext)
                            && kv.Value.Lines != null
                    )
                    is IEnumerable<KeyValuePair<string, ChatterLinesData>> foundLinesKV
                && foundLinesKV.Any()
            )
                foundLines = Random.Shared.ChooseFrom(foundLinesKV.ToList()).Value;
            else
                return false;
        }
        e.NextChatterKey = null;
        if (foundLines == null)
            return false;

        string chosen = Random.Shared.ChooseFrom(foundLines.Lines);
        // draw the dialogue
        ChatterSpeaker? speaker = e.CompanionSpeaker;
        if (speaker != null && speaker.PortraitTx2D.Value != null)
        {
            portraitField.SetValue(speakerNPC, speaker.PortraitTx2D.Value);
            if (speaker.NPC != null)
            {
                speakerNPC.Name = speaker.NPC;
                speakerNPC.displayName = null;
            }
            else
            {
                speakerNPC.Name = QQQ;
                speakerNPC.displayName = speaker.DisplayName ?? QQQ;
            }
        }
        else
        {
            speakerNPC.Name = QQQ;
            speakerNPC.displayName = QQQ;
            portraitField.SetValue(speakerNPC, null);
        }

        if (foundLines.Responses != null)
        {
            Dictionary<string, string> TranslatedResponses = foundLines
                .Responses.Select((kv) => new KeyValuePair<string, string>(kv.Key, GetText(kv.Value)))
                .ToDictionary((kv) => kv.Key, (kv) => kv.Value);
            dialogueField.SetValue(speakerNPC, TranslatedResponses);
        }
        chosenDialogue = new(speakerNPC, chosen, GetText(chosen));
        Game1.DrawDialogue(chosenDialogue);
        return base.ApplyEffect(proc);
    }
}
