using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.GameData.BigCraftables;
using StardewValley.GameData.Machines;
using StardewValley;
using StardewValley.Objects.Trinkets;
using StardewValley.GameData.Shops;
using TrinketTinker.Effects;
using Force.DeepCloner;

namespace TrinketTinker.Extras
{
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
                CustomFields = null
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
            blacksmith.Items.Add(new ShopItemData()
            {
                Id = TrinketColorizerId,
                TradeItemId = "(O)336",
                TradeItemAmount = 50,
                ItemId = TrinketColorizerId,
                Condition = "PLAYER_HAS_CRAFTING_RECIPE Current Anvil"
            });
        }

        /// <summary>Setup rules for the trinket colorizer, add custom rule for anvil</summary>
        /// <param name="asset"></param>
        public static void Edit_MachineData(IAssetData asset)
        {
            IDictionary<string, MachineData> data = asset.AsDictionary<string, MachineData>().Data;
            data[TrinketColorizerQId] = new()
            {
                OutputRules = [
                    new(){
                        Id = $"{ModEntry.ModId}_Default",
                        Triggers = [
                            new(){
                                Id = $"{ModEntry.ModId}_ItemPlacedInMachine",
                                Trigger = MachineOutputTrigger.ItemPlacedInMachine,
                                RequiredCount = 1,
                                RequiredTags = ["category_trinket"]
                            }
                        ],
                        OutputItem = [
                            new(){
                                OutputMethod = $"{typeof(TrinketColorizer).AssemblyQualifiedName}:{nameof(OutputTrinketColorizer)}"
                            }
                        ]
                    }
                ],
                AdditionalConsumedItems = [
                    new(){
                        ItemId= "(O)578",
                        RequiredCount = 3,
                        InvalidCountMessage = I18n.BC_TrinketColorizer_InvalidCount()
                    }
                ],
                // InvalidItemMessage
            };
            if (data.TryGetValue("(BC)Anvil", out MachineData? anvilData))
            {
                MachineOutputRule newRule = anvilData.OutputRules.First().DeepClone();
                newRule.Id = $"{ModEntry.ModId}_Default";
                newRule.Triggers = [
                    new(){
                        Id = $"{ModEntry.ModId}_ItemPlacedInMachine",
                        Trigger = MachineOutputTrigger.ItemPlacedInMachine,
                        RequiredCount = 1,
                        RequiredTags = ["category_trinket"]
                    }
                ];
                newRule.OutputItem = [
                    new(){
                        OutputMethod = $"{typeof(TrinketColorizer).AssemblyQualifiedName}:{nameof(OutputTinkerAnvil)}"
                    }
                ];
                anvilData.OutputRules.Insert(0, newRule);
            }
        }

        /// <summary>Change trinket variant.</summary>
        /// <param name="machine"></param>
        /// <param name="inputItem"></param>
        /// <param name="probe"></param>
        /// <param name="outputData"></param>
        /// <param name="player"></param>
        /// <param name="overrideMinutesUntilReady"></param>
        /// <returns></returns>
        public static Item? OutputTrinketColorizer(SObject machine, Item inputItem, bool probe, MachineItemOutput outputData, Farmer player, out int? overrideMinutesUntilReady)
        {
            overrideMinutesUntilReady = null;
            if (inputItem is not Trinket t)
                return null;
            if (!t.GetTrinketData().CanBeReforged)
            {
                if (!probe)
                {
                    Game1.showRedMessage(Game1.content.LoadString("Strings\\1_6_Strings:Anvil_wrongtrinket"));
                }
                return null;
            }

            int previous = 0;
            if (inputItem.modData.TryGetValue(TrinketTinkerEffect.ModData_Variant, out string? previousStr))
                if (!int.TryParse(previousStr, out previous))
                    previous = 0;
            Trinket output = (Trinket)inputItem.getOne();
            if (output.GetEffect() is not TrinketTinkerEffect effect)
                return null;
            if (!effect.RerollVariant(output, previous))
            {
                if (!probe)
                {
                    player?.doEmote(40);
                }
                return null;
            }

            if (!probe)
            {
                Game1.currentLocation.playSound("metal_tap");
                DelayedAction.playSoundAfterDelay("metal_tap", 250);
                DelayedAction.playSoundAfterDelay("metal_tap", 500);
            }
            overrideMinutesUntilReady = 10;
            return output;
        }

        /// <summary>Change trinket level.</summary>
        /// <param name="machine"></param>
        /// <param name="inputItem"></param>
        /// <param name="probe"></param>
        /// <param name="outputData"></param>
        /// <param name="player"></param>
        /// <param name="overrideMinutesUntilReady"></param>
        /// <returns></returns>
        public static Item? OutputTinkerAnvil(SObject machine, Item inputItem, bool probe, MachineItemOutput outputData, Farmer player, out int? overrideMinutesUntilReady)
        {
            overrideMinutesUntilReady = null;
            if (inputItem is not Trinket t)
                return null;
            if (!t.GetTrinketData().CanBeReforged)
            {
                if (!probe)
                {
                    Game1.showRedMessage(Game1.content.LoadString("Strings\\1_6_Strings:Anvil_wrongtrinket"));
                }
                return null;
            }

            if (((Trinket)inputItem).GetEffect() is not TrinketTinkerEffect effect1)
                return null;
            Trinket output = (Trinket)inputItem.getOne();
            if (output.GetEffect() is not TrinketTinkerEffect effect2)
                return null;
            if (!effect2.RerollLevel(output, effect1.GeneralStat))
            {
                if (!probe)
                {
                    player?.doEmote(40);
                }
                return null;
            }

            if (!probe)
            {
                Game1.currentLocation.playSound("metal_tap");
                DelayedAction.playSoundAfterDelay("metal_tap", 250);
                DelayedAction.playSoundAfterDelay("metal_tap", 500);
            }
            overrideMinutesUntilReady = 10;
            return output;
        }
    }
}
