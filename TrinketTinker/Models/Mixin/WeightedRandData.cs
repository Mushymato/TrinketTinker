using StardewValley;
using StardewValley.Extensions;

namespace TrinketTinker.Models.Mixin;

public abstract class WeightedRandData
{
    /// <summary>If true, this will actually do nothing (nop). For use with random</summary>
    public bool Nop { get; set; } = false;

    /// <summary>Weight of randomization, higher number is more likely.</summary>
    public int RandomWeight { get; set; } = 1;

    /// <summary>Condition to check before clip can be chosen</summary>
    public string? Condition { get; set; } = null;

    // implementation details
    private static int GCD(IEnumerable<int> weights)
    {
        return weights.Aggregate(GCD);
    }

    private static int GCD(int a, int b)
    {
        return b == 0 ? a : GCD(b, a % b);
    }

    protected List<WeightedRandData>? randomExtra = null;

    protected WeightedRandData? randSelected = null;

    protected WeightedRandData? PickRandBase(Random rand, Farmer owner)
    {
        if (randomExtra == null || randomExtra.Count == 0)
        {
            if (GameStateQuery.CheckConditions(Condition, player: owner, random: rand))
            {
                randSelected = this;
                return this;
            }
            else
            {
                randSelected = null;
                return null;
            }
        }
        List<int> weights = [RandomWeight];
        weights.AddRange(randomExtra.Select((clip) => clip.RandomWeight));
        int gcd = GCD(weights);
        List<WeightedRandData> randWeightedList = [];
        if (GameStateQuery.CheckConditions(Condition, player: owner, random: rand))
        {
            for (int i = 0; i < RandomWeight / gcd; i++)
            {
                randWeightedList.Add(this);
            }
        }
        foreach (WeightedRandData clip in randomExtra)
        {
            if (GameStateQuery.CheckConditions(clip.Condition, player: owner, random: rand))
            {
                for (int i = 0; i < clip.RandomWeight / gcd; i++)
                {
                    randWeightedList.Add(clip);
                }
            }
        }
        if (randWeightedList.Count == 0)
        {
            randSelected = null;
            return null;
        }
        randSelected = rand.ChooseFrom(randWeightedList);
        return randSelected;
    }
}
