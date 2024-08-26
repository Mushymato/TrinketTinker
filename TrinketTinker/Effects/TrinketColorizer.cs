using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.GameData.BigCraftables;
using StardewValley.GameData.Machines;
using StardewValley;
using StardewValley.Objects.Trinkets;
using SObject = StardewValley.Object;

namespace TrinketTinker.Effects
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
                e.Edit(Edit_BigCraftables, AssetEditPriority.Early);
            }
            if (e.Name.IsEquivalentTo("Data/Machines"))
            {
                e.Edit(Edit_MachineData, AssetEditPriority.Early);
            }
        }

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

        public static void Edit_MachineData(IAssetData asset)
        {
            IDictionary<string, MachineData> data = asset.AsDictionary<string, MachineData>().Data;
            data[TrinketColorizerQId] = new()
            {
                OutputRules = [
                    new(){
                        Id = "Default",
                        Triggers = [
                            new(){
                                Id = "ItemPlacedInMachine",
                                Trigger = MachineOutputTrigger.ItemPlacedInMachine,
                                RequiredCount = 1
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
                ]
            };
        }

        /// <summary>
        /// Change trinket color
        /// </summary>
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

            Trinket output = (Trinket)inputItem.getOne();
            if (output.GetEffect() is not TrinketTinkerEffect effect)
                return null;
            if (!effect.RerollVariant(output))
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
