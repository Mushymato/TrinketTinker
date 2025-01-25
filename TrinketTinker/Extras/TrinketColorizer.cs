using Force.DeepCloner;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.BigCraftables;
using StardewValley.GameData.Machines;
using StardewValley.GameData.Shops;
using StardewValley.Objects.Trinkets;
using TrinketTinker.Effects;

namespace TrinketTinker.Extras;

public static class TrinketColorizer
{
    public const string TrinketColorizerTextureFile = "assets/trinketcolorizer.png";
    public static string TrinketColorizerTexture => $"Mods/{ModEntry.ModId}/TrinketColorizer";
    public static string TrinketColorizerId => $"{ModEntry.ModId}_TrinketColorizer";
    public static string TrinketColorizerQId => $"(BC){ModEntry.ModId}_TrinketColorizer";

    public static void OnAssetRequested(AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo(TrinketColorizerTexture))
        {
            e.LoadFromModFile<Texture2D>(TrinketColorizerTextureFile, AssetLoadPriority.Exclusive);
        }
        if (e.Name.IsEquivalentTo("Data/BigCraftables"))
        {
            e.Edit(Edit_BigCraftables);
        }
        if (e.Name.IsEquivalentTo("Data/Shops"))
        {
            e.Edit(Edit_Shops_Blacksmith);
        }
        if (e.Name.IsEquivalentTo("Data/Machines"))
        {
            e.Edit(Edit_MachineData);
        }
    }

    /// <summary>Add the trinket colorizer big craftable</summary>
    /// <param name="asset"></param>
    public static void Edit_BigCraftables(IAssetData asset)
    {
        IDictionary<string, BigCraftableData> data = asset.AsDictionary<string, BigCraftableData>().Data;
        data[TrinketColorizerId] = new()
        {
            Name = TrinketColorizerId,
            DisplayName = I18n.BC_TrinketColorizer_Name(),
            Description = I18n.BC_TrinketColorizer_Description(),
            Price = 50,
            Fragility = 0,
            CanBePlacedOutdoors = true,
            CanBePlacedIndoors = true,
            IsLamp = false,
            Texture = TrinketColorizerTexture,
            SpriteIndex = 0,
            ContextTags = [ModEntry.ModId],
            CustomFields = null,
        };
    }

    /// <summary>Add colorizer trade-in at clint's</summary>
    /// <param name="asset"></param>
    public static void Edit_Shops_Blacksmith(IAssetData asset)
    {
        // Prefered to do this instead of crafting recipe since shops have conditions
        // data[TrinketColorizerId] = $"336 50/UNUSED/{TrinketColorizerId}/true/l 2/";
        IDictionary<string, ShopData> data = asset.AsDictionary<string, ShopData>().Data;
        ShopData blacksmith = data["Blacksmith"];
        blacksmith.Items.Add(
            new ShopItemData()
            {
                Id = TrinketColorizerId,
                TradeItemId = "(O)336",
                TradeItemAmount = 50,
                ItemId = TrinketColorizerId,
                Condition = "PLAYER_HAS_CRAFTING_RECIPE Current Anvil",
            }
        );
    }

    /// <summary>Setup rules for the trinket colorizer, add custom rule for anvil</summary>
    /// <param name="asset"></param>
    public static void Edit_MachineData(IAssetData asset)
    {
        IDictionary<string, MachineData> data = asset.AsDictionary<string, MachineData>().Data;
        data[TrinketColorizerQId] = new()
        {
            OutputRules =
            [
                new()
                {
                    Id = $"{ModEntry.ModId}_Default",
                    Triggers =
                    [
                        new()
                        {
                            Id = $"{ModEntry.ModId}_ItemPlacedInMachine",
                            Trigger = MachineOutputTrigger.ItemPlacedInMachine,
                            RequiredCount = 1,
                            RequiredTags = ["category_trinket"],
                            Condition = GameItemQuery.GameStateQuery_HAS_VARIANTS,
                        },
                    ],
                    OutputItem =
                    [
                        new()
                        {
                            Id = $"{ModEntry.ModId}_Reroll",
                            ItemId = $"{GameItemQuery.ItemQuery_CREATE_TRINKET} DROP_IN_ID ? R",
                        },
                    ],
                    MinutesUntilReady = 10,
                },
            ],
            AdditionalConsumedItems =
            [
                new()
                {
                    ItemId = "(O)749",
                    RequiredCount = 15,
                    InvalidCountMessage = I18n.BC_TrinketColorizer_InvalidCount(),
                },
            ],
            LoadEffects =
            [
                new()
                {
                    Id = $"{ModEntry.ModId}_Default",
                    Sounds =
                    [
                        new() { Id = "metal_tap", Delay = 0 },
                        new() { Id = "metal_tap", Delay = 250 },
                        new() { Id = "metal_tap", Delay = 500 },
                    ],
                },
            ],
            // InvalidItemMessage
        };
        if (data.TryGetValue("(BC)Anvil", out MachineData? anvilData))
        {
            MachineOutputRule newRule = anvilData.OutputRules.First().DeepClone();
            newRule.Id = $"{ModEntry.ModId}_Default";
            newRule.Triggers =
            [
                new()
                {
                    Id = $"{ModEntry.ModId}_ItemPlacedInMachine",
                    Trigger = MachineOutputTrigger.ItemPlacedInMachine,
                    RequiredCount = 1,
                    RequiredTags = ["category_trinket"],
                    Condition = GameItemQuery.GameStateQuery_HAS_LEVELS,
                },
            ];
            newRule.OutputItem =
            [
                new()
                {
                    Id = $"{ModEntry.ModId}_Reroll",
                    ItemId = $"{GameItemQuery.ItemQuery_CREATE_TRINKET} DROP_IN_ID R ?",
                },
            ];
            newRule.MinutesUntilReady = 10;
            anvilData.OutputRules.Insert(0, newRule);
            anvilData.LoadEffects ??= [];
            anvilData.LoadEffects.Add(
                new()
                {
                    Id = $"{ModEntry.ModId}_Default",
                    Condition = GameItemQuery.GameStateQuery_HAS_LEVELS,
                    Sounds =
                    [
                        new() { Id = "metal_tap", Delay = 0 },
                        new() { Id = "metal_tap", Delay = 250 },
                        new() { Id = "metal_tap", Delay = 500 },
                    ],
                }
            );
        }
    }
}
