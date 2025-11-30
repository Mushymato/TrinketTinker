using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.HomeRenovations;
using StardewValley.Inventories;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Objects.Trinkets;
using TrinketTinker.Companions;
using TrinketTinker.Effects;
using TrinketTinker.Models;
using TrinketTinker.Wheels;

namespace TrinketTinker.Extras;

public sealed class TinkerInventoryMenu : ItemGrabMenu
{
    private const int TEXT_M = 6;
    private const int TITLE_LM = 16;
    private const int TITLE_TM = 20;

    public Action<int>? pageMethod;

    public ClickableTextureComponent Btn_L;
    public ClickableTextureComponent Btn_R;

    public TinkerInventoryMenu(
        int actualCapacity,
        IList<Item> inventory,
        Action<int>? pageMethod,
        InventoryMenu.highlightThisItem highlightFunction,
        behaviorOnItemSelect behaviorOnItemSelectFunction,
        string message,
        behaviorOnItemSelect? behaviorOnItemGrab = null,
        bool reverseGrab = false,
        bool showReceivingMenu = true,
        bool snapToBottom = false,
        bool canBeExitedWithKey = true,
        bool playRightClickSound = true,
        bool allowRightClick = true,
        bool showOrganizeButton = true,
        int source = 0,
        Item? sourceItem = null,
        int whichSpecialButton = -1,
        object? context = null,
        ItemExitBehavior heldItemExitBehavior = ItemExitBehavior.ReturnToPlayer,
        bool allowExitWithHeldItem = false
    )
        : base(
            inventory,
            reverseGrab,
            showReceivingMenu,
            highlightFunction,
            behaviorOnItemSelectFunction,
            message,
            behaviorOnItemGrab,
            snapToBottom,
            canBeExitedWithKey,
            playRightClickSound,
            allowRightClick,
            showOrganizeButton,
            source,
            sourceItem,
            whichSpecialButton,
            context,
            heldItemExitBehavior,
            allowExitWithHeldItem
        )
    {
        this.pageMethod = pageMethod;
        // remake ItemsToGrabMenu with some specific capacity
        int rows =
            (actualCapacity < 9) ? 1
            : (actualCapacity >= 70) ? 5
            : 3;
        if (actualCapacity != 36)
        {
            int width = 64 * (actualCapacity / rows);
            ItemsToGrabMenu = new InventoryMenu(
                Game1.uiViewport.Width / 2 - width / 2,
                yPositionOnScreen + ((actualCapacity < 70) ? 64 : (-21)),
                playerInventory: false,
                inventory,
                highlightFunction,
                actualCapacity,
                rows
            );
            if (rows > 3)
            {
                yPositionOnScreen += 42;
                Reflect.Try_InventoryMenu_SetPosition(
                    base.inventory,
                    base.inventory.xPositionOnScreen,
                    base.inventory.yPositionOnScreen + 38 + 4
                );
                Reflect.Try_InventoryMenu_SetPosition(
                    ItemsToGrabMenu,
                    ItemsToGrabMenu.xPositionOnScreen - 32 + 8,
                    ItemsToGrabMenu.yPositionOnScreen
                );
                Reflect.Try_ItemGrabMenu_storageSpaceTopBorderOffset_Set(this, 20);
                trashCan.bounds.X = ItemsToGrabMenu.width + ItemsToGrabMenu.xPositionOnScreen + borderWidth * 2;
                okButton.bounds.X = ItemsToGrabMenu.width + ItemsToGrabMenu.xPositionOnScreen + borderWidth * 2;
            }
        }
        else
        {
            ItemsToGrabMenu = new InventoryMenu(
                xPositionOnScreen + 32,
                yPositionOnScreen,
                playerInventory: false,
                inventory,
                highlightFunction,
                capacity: actualCapacity
            );
        }
        // neighbour nonsense
        ItemsToGrabMenu.populateClickableComponentList();
        for (int i = 0; i < ItemsToGrabMenu.inventory.Count; i++)
        {
            if (ItemsToGrabMenu.inventory[i] != null)
            {
                ItemsToGrabMenu.inventory[i].myID += 53910;
                ItemsToGrabMenu.inventory[i].upNeighborID += 53910;
                ItemsToGrabMenu.inventory[i].rightNeighborID += 53910;
                ItemsToGrabMenu.inventory[i].downNeighborID = -7777;
                ItemsToGrabMenu.inventory[i].leftNeighborID += 53910;
                ItemsToGrabMenu.inventory[i].fullyImmutable = true;
            }
        }

        if (specialButton != null)
            specialButton = null;
        if (junimoNoteIcon != null)
            junimoNoteIcon = null;

        // more neighbour nonsense
        if (
            ItemsToGrabMenu.GetBorder(InventoryMenu.BorderSide.Right).FirstOrDefault()
            is ClickableComponent clickableComponent
        )
        {
            organizeButton?.leftNeighborID = clickableComponent.myID;
            fillStacksButton?.leftNeighborID = clickableComponent.myID;
        }

        Btn_L = new(
            new Rectangle(
                ItemsToGrabMenu.xPositionOnScreen - borderWidth - spaceToClearSideBorder - 64,
                ItemsToGrabMenu.yPositionOnScreen,
                64,
                64
            ),
            Game1.mouseCursors,
            new Rectangle(0, 256, 64, 64),
            1f,
            drawShadow: false
        );
        Btn_R = new(
            new Rectangle(
                ItemsToGrabMenu.xPositionOnScreen + ItemsToGrabMenu.width + borderWidth + spaceToClearSideBorder,
                ItemsToGrabMenu.yPositionOnScreen,
                64,
                64
            ),
            Game1.mouseCursors,
            new Rectangle(0, 192, 64, 64),
            1f,
            drawShadow: false
        );
    }

    /// <summary>Render the UI, draw the trinket item that spawned this menu</summary>
    /// <param name="b"></param>
    public override void draw(SpriteBatch b)
    {
        // compiler went derp
        Action<SpriteBatch> drawMethod = base.draw;
        if (sourceItem == null)
        {
            // should not happen, but put here just in case
            drawMethod(b);
            return;
        }

        Vector2 nameSize = Game1.dialogueFont.MeasureString(sourceItem.DisplayName);
        int sourceItemPosX = ItemsToGrabMenu.xPositionOnScreen - borderWidth - spaceToClearSideBorder + TITLE_LM;
        int sourceItemPosY =
            ItemsToGrabMenu.yPositionOnScreen
            - borderWidth
            - spaceToClearTopBorder
            + Reflect.Try_ItemGrabMenu_storageSpaceTopBorderOffset_Get(this)
            + TITLE_TM;
        if (drawBG && !Game1.options.showClearBackgrounds)
        {
            b.Draw(
                Game1.fadeToBlackRect,
                new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height),
                Color.Black * 0.5f
            );
        }
        else
        {
            b.Draw(
                Game1.fadeToBlackRect,
                new Rectangle(
                    sourceItemPosX - TEXT_M,
                    sourceItemPosY - TEXT_M,
                    (int)nameSize.X + TEXT_M * 2,
                    (int)nameSize.Y + TEXT_M * 2
                ),
                Color.Black * 0.5f
            );
        }
        string displayText = sourceItem.DisplayName;
        if ((sourceItem is Trinket trinket) && trinket.TryGetDaysLeft(out int daysDuration) && daysDuration > 0)
        {
            displayText = I18n.Effect_DaysLeft(displayText, daysDuration);
        }
        b.DrawString(Game1.dialogueFont, displayText, new(sourceItemPosX, sourceItemPosY), Color.White);
        sourceItem.drawInMenu(
            b,
            new(sourceItemPosX - TEXT_M - Game1.tileSize, sourceItemPosY - Game1.tileSize + nameSize.Y),
            1f
        );

        if (pageMethod != null && !Game1.options.snappyMenus)
        {
            Btn_L.draw(b);
            Btn_R.draw(b);
        }

        bool drawBGOrig = drawBG;
        drawBG = false;
        drawMethod(b);
        drawBG = drawBGOrig;
    }

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        if (organizeButton != null && organizeButton.containsPoint(x, y))
        {
            organizeItemsInList(ItemsToGrabMenu.actualInventory);
            Game1.playSound("Ship");
        }
        else if (pageMethod != null)
        {
            if (Btn_L.containsPoint(x, y))
            {
                pageMethod(-1);
            }
            else if (Btn_R.containsPoint(x, y))
            {
                pageMethod(1);
            }
        }
        else
        {
            base.receiveLeftClick(x, y, !destroyItemOnClick);
        }
    }
}

/// <summary>Handler for inventory, does not use mutext (yet) because each trinket has a unique global inventory</summary>
internal sealed class GlobalInventoryHandler
{
    /// <summary>Hat global inventory</summary>
    public const string GlobalHatInventory = $"{ModEntry.ModId}@Hats";

    /// <summary>Hat global inventory</summary>
    public const string ModData_HatGivenTo = $"{ModEntry.ModId}/GivenTo";

    /// <summary>
    /// Holds info about the current inventory
    /// </summary>
    /// <param name="Effect"></param>
    /// <param name="Data"></param>
    /// <param name="FullInventoryId"></param>
    internal record GIHInfo(TrinketTinkerEffect Effect, TinkerInventoryData Data, string FullInventoryId)
    {
        internal readonly Inventory TrinketInv = Game1.player.team.GetOrCreateGlobalInventory(FullInventoryId);
    }

    /// <summary>Current page</summary>
    internal readonly bool CanPage = false;
    private int page = 0;
    internal readonly List<GIHInfo> pagedInfo;

    private Inventory TrinketInv => pagedInfo[page].TrinketInv;
    private TrinketTinkerEffect Effect => pagedInfo[page].Effect;
    private TinkerInventoryData Data => pagedInfo[page].Data;
    private string FullInventoryId => pagedInfo[page].FullInventoryId;

    internal GlobalInventoryHandler(TrinketTinkerEffect effect, TinkerInventoryData data, string fullInventoryId)
    {
        CanPage = false;
        pagedInfo = [new(effect, data, fullInventoryId)];
    }

    internal GlobalInventoryHandler(Farmer owner)
    {
        pagedInfo = [];
        foreach (Trinket trinketItem in owner.trinketItems)
        {
            if (trinketItem == null)
                continue;
            if (
                trinketItem.GetEffect() is TrinketTinkerEffect effect
                && effect.CheckCanOpenInventory(owner)
                && !effect.HasEquipTrinketAbility
            )
            {
                pagedInfo.Add(new(effect, effect.Data!.Inventory!, effect.FullInventoryId!));
            }
        }
        CanPage = pagedInfo.Count > 1;
        page = pagedInfo.Count > 0 ? 0 : -1;
    }

    internal ItemGrabMenu? GetMenu()
    {
        if (page >= pagedInfo.Count)
            return null;
        if (Constants.TargetPlatform == GamePlatform.Android)
        {
            return new ItemGrabMenu(
                inventory: TrinketInv,
                reverseGrab: false,
                showReceivingMenu: true,
                highlightFunction: HighlightFunction,
                behaviorOnItemSelectFunction: BehaviorOnItemSelectFunction,
                message: FullInventoryId,
                behaviorOnItemGrab: BehaviorOnItemGrab,
                canBeExitedWithKey: true,
                sourceItem: Effect.Trinket
            );
        }
        else
        {
            return new TinkerInventoryMenu(
                Data.Capacity,
                TrinketInv,
                CanPage ? MovePage : null,
                HighlightFunction,
                BehaviorOnItemSelectFunction,
                FullInventoryId,
                BehaviorOnItemGrab,
                sourceItem: Effect.Trinket
            );
        }
    }

    private void MovePage(int count = 1)
    {
        int newPage = page + count;
        if (newPage < 0)
            page = pagedInfo.Count - 1;
        else if (newPage >= pagedInfo.Count)
            page = 0;
        else
            page = newPage;
        ItemGrabMenu menu = GetMenu()!;
        Game1.activeClickableMenu = menu;
    }

    private bool HighlightFunction(Item item)
    {
        if (item == null)
            return false;
        if (item is Trinket trinket)
        {
            if (Effect.Trinket == trinket)
                return false;
            if (trinket.GetEffect() is TrinketTinkerEffect otherEffect)
            {
                if (otherEffect.HasEquipTrinketAbility)
                    return false;
                if (Effect.HasEquipTrinketAbility && GameItemQuery.IsDirectEquipOnly(trinket))
                    return false;
            }
        }
        else if (Effect.HasEquipTrinketAbility)
        {
            return false;
        }

        return ValidForInventory(item, Data);
    }

    internal static Item? AddItem(Inventory trinketInv, int capacity, Item item)
    {
        if (item == null)
            return null;
        item.resetState();
        trinketInv.RemoveEmptySlots();
        for (int i = 0; i < trinketInv.Count; i++)
        {
            if (trinketInv[i] != null && trinketInv[i].canStackWith(item))
            {
                int amount = item.Stack - trinketInv[i].addToStack(item);
                if (item.ConsumeStack(amount) == null)
                {
                    return null;
                }
            }
        }
        if (trinketInv.Count < capacity)
        {
            trinketInv.Add(item);
            return null;
        }
        return item;
    }

    internal Item? AddItem(Item item)
    {
        return AddItem(TrinketInv, Data.Capacity, item);
    }

    private void BehaviorOnItemSelectFunction(Item item, Farmer who)
    {
        if (item == null)
            return;
        if (item.Stack == 0)
        {
            item.Stack = 1;
        }
        Item? item2 = AddItem(item);
        if (item2 != null)
        {
            who.removeItemFromInventory(item);
        }
        else
        {
            item2 = who.addItemToInventory(item2);
        }
        TrinketInv.RemoveEmptySlots();
        int num =
            (Game1.activeClickableMenu.currentlySnappedComponent != null)
                ? Game1.activeClickableMenu.currentlySnappedComponent.myID
                : (-1);
        ItemGrabMenu menu = GetMenu()!;
        Game1.activeClickableMenu = menu;
        menu.heldItem = item2;
        if (num != -1)
        {
            Game1.activeClickableMenu.currentlySnappedComponent = Game1.activeClickableMenu.getComponentWithID(num);
            Game1.activeClickableMenu.snapCursorToCurrentSnappedComponent();
        }
        return;
    }

    private void BehaviorOnItemGrab(Item item, Farmer who)
    {
        if (who.couldInventoryAcceptThisItem(item))
        {
            TrinketInv.Remove(item);
            TrinketInv.RemoveEmptySlots();
            Game1.activeClickableMenu = GetMenu();
        }
    }

    internal static void DoLockedHatInvOperation(Action<Inventory> callback)
    {
        NetMutex hatMutex = Game1.player.team.GetOrCreateGlobalInventoryMutex(GlobalHatInventory);
        hatMutex.RequestLock(() =>
        {
            Inventory hatInv = Game1.player.team.GetOrCreateGlobalInventory(GlobalHatInventory);
            callback(hatInv);
            hatMutex.ReleaseLock();
        });
    }

    internal static bool SwapHat(Farmer farmer, TrinketTinkerCompanion companion, string invId)
    {
        if (farmer.Items.Count <= farmer.CurrentToolIndex || farmer.Items[farmer.CurrentToolIndex] is not Hat newHat)
            return false;

        DoLockedHatInvOperation(
            (hatInv) =>
            {
                if (companion.GivenHat is Hat currHat)
                {
                    currHat.onDetachedFromParent();
                    hatInv.Remove(currHat);
                    companion.GivenHat = null;
                    currHat.modData.Remove(ModData_HatGivenTo);
                    Game1.createItemDebris(currHat, companion.Position, -1, farmer.currentLocation);
                }
                else
                {
                    newHat.onDetachedFromParent();
                    farmer.Items[farmer.CurrentToolIndex] = null;
                    hatInv.Add(newHat);
                    newHat.modData[ModData_HatGivenTo] = invId;
                    companion.GivenHat = newHat;
                }
            }
        );
        return true;
    }

    internal static void ApplyHat(TrinketTinkerCompanion companion, string invId)
    {
        DoLockedHatInvOperation(
            (hatInv) =>
            {
                bool hasGivenedHat = false;
                for (int i = 0; i < hatInv.Count; i++)
                {
                    if (
                        hatInv[i] is Hat currHat
                        && currHat.modData.TryGetValue(ModData_HatGivenTo, out string? hatGivenTo)
                        && hatGivenTo == invId
                    )
                    {
                        if (hasGivenedHat || !companion.CanBeGivenHat)
                        {
                            currHat.modData.Remove(ModData_HatGivenTo);
                            currHat.onDetachedFromParent();
                            hatInv[i] = null;

                            Game1.player.team.returnedDonationsMutex.RequestLock(() =>
                            {
                                Game1.player.team.returnedDonations.Add(currHat);
                                Game1.player.team.newLostAndFoundItems.Value = true;
                                Game1.player.team.returnedDonationsMutex.ReleaseLock();
                            });
                        }
                        else
                        {
                            companion.GivenHat = currHat;
                            hasGivenedHat = true;
                        }
                    }
                }
                hatInv.RemoveEmptySlots();
            }
        );
    }

    internal static void SyncHat(TrinketTinkerCompanion companion, string invId, bool hasHat)
    {
        if (hasHat)
        {
            DoLockedHatInvOperation(
                (hatInv) =>
                {
                    for (int i = 0; i < hatInv.Count; i++)
                    {
                        if (
                            hatInv[i] is Hat currHat
                            && currHat.modData.TryGetValue(ModData_HatGivenTo, out string? hatGivenTo)
                            && hatGivenTo == invId
                        )
                        {
                            companion.GivenHat = currHat;
                            return;
                        }
                    }
                }
            );
        }
        else
        {
            companion.GivenHat = null;
        }
    }

    internal static void UnapplyHat(TrinketTinkerCompanion companion)
    {
        if (companion.GivenHat == null)
            return;
        DoLockedHatInvOperation(
            (hatInv) =>
            {
                hatInv.Add(companion.GivenHat);
                companion.GivenHat = null;
            }
        );
    }

    internal static Hat? FindHat(string invId)
    {
        Inventory hatInv = Game1.player.team.GetOrCreateGlobalInventory(GlobalHatInventory);
        return (Hat?)
            hatInv.FirstOrDefault(hat =>
                hat is Hat && hat.modData.TryGetValue(ModData_HatGivenTo, out string? hatGivenTo) && hatGivenTo == invId
            );
    }

    /// <summary>
    /// Ensure empty inventories are deleted, and inaccessable inventories have their contents put into lost and found
    /// Also do a check for trinketSlots and make sure people don't end up with a trinket in slot 1/2 and trinketSlots=0
    /// </summary>
    internal static void UnreachableInventoryCleanup()
    {
        bool newLostAndFoundItems = false;
        var team = Game1.player.team;
        // check if the player somehow lost their trinketSlots stat
        bool hasTrinketSlot = Game1.player.stats.Get("trinketSlots") != 0;

        int toSkip = 0;
        if (!hasTrinketSlot)
        {
            toSkip = Math.Min(Game1.player.trinketItems.Count, ModEntry.HasWearMoreRings ? 2 : 1);
            for (int i = 0; i < toSkip; i++)
            {
                if (Game1.player.trinketItems[i] is Trinket trinketItem)
                {
                    team.returnedDonations.Add(trinketItem);
                    Game1.player.trinketItems[i] = null;
                    newLostAndFoundItems = newLostAndFoundItems || true;
                }
            }
        }

        // only run the rest on main host
        if (!Context.IsMainPlayer && Context.ScreenId == 0)
        {
            team.newLostAndFoundItems.Value = newLostAndFoundItems;
            return;
        }

        // check for missing trinkets to global inv
        HashSet<string> missingTrinketInvs = [];
        foreach (var key in team.globalInventories.Keys)
        {
            var value = team.globalInventories[key];
            if (value == null)
                continue;
            if (key.StartsWith(string.Concat(ModEntry.ModId, "/")))
            {
                value.RemoveEmptySlots();
                if (value.Count == 0)
                    team.globalInventories.Remove(key);
                else
                    missingTrinketInvs.Add(key);
            }
        }

        // check for missing given hats
        Inventory hatInv = Game1.player.team.GetOrCreateGlobalInventory(GlobalHatInventory);
        Dictionary<string, int> missingGivenHat = [];
        for (int i = 0; i < hatInv.Count; i++)
        {
            if (hatInv[i] is Hat currHat && currHat.modData.TryGetValue(ModData_HatGivenTo, out string? hatGivenTo))
            {
                missingGivenHat[hatGivenTo] = i;
            }
        }

        if (!missingTrinketInvs.Any() && !missingGivenHat.Any())
            return;

        static void UpdateMissing(
            Item item,
            HashSet<string> missingTrinketInvs,
            Dictionary<string, int> missingGivenHat
        )
        {
            if (item is Trinket trinket && trinket.GetEffect() is TrinketTinkerEffect effect)
            {
                if (effect.FullInventoryId != null)
                {
                    missingTrinketInvs.Remove(effect.FullInventoryId!);
                }
                if (effect.InventoryId != null)
                {
                    missingGivenHat.Remove(effect.InventoryId);
                }
            }
        }

        Utility.ForEachItem(
            (item) =>
            {
                UpdateMissing(item, missingTrinketInvs, missingGivenHat);
                return missingTrinketInvs.Any() || missingGivenHat.Any();
            }
        );
        foreach (Farmer farmer in Game1.getAllFarmers())
        {
            foreach (Trinket trinketItem in farmer.trinketItems.Skip(toSkip))
            {
                UpdateMissing(trinketItem, missingTrinketInvs, missingGivenHat);
            }
        }

        newLostAndFoundItems = newLostAndFoundItems || missingTrinketInvs.Any() || missingGivenHat.Any();

        foreach (string key in missingTrinketInvs)
        {
            ModEntry.Log(
                $"Destroy inaccessible trinket inventory: {key}, items will be sent to lost and found",
                LogLevel.Debug
            );
            var value = team.globalInventories[key];
            foreach (var item in value)
                team.returnedDonations.Add(item);
            team.globalInventories.Remove(key);
        }
        foreach ((string givenTo, int idx) in missingGivenHat)
        {
            ModEntry.Log(
                $"Remove orphaned hat '{hatInv[idx].QualifiedItemId}', items will be sent to lost and found",
                LogLevel.Debug
            );
            hatInv[idx].modData.Remove(ModData_HatGivenTo);
            team.returnedDonations.Add(hatInv[idx]);
            hatInv[idx] = null;
        }
        hatInv.RemoveEmptySlots();

        if (newLostAndFoundItems && team.newLostAndFoundItems.Value != newLostAndFoundItems)
        {
            Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:NewLostAndFoundItems"));
        }
        team.newLostAndFoundItems.Value = newLostAndFoundItems;
    }

    internal static bool ValidForInventory(Item item, TinkerInventoryData? data)
    {
        if (data == null)
            return true;
        if (data.RequiredTags != null && !Places.CheckContextTagFilter(item, data.RequiredTags))
            return false;
        if (
            data.RequiredItemCondition != null
            && !GameStateQuery.CheckConditions(data.RequiredItemCondition, inputItem: item, targetItem: item)
        )
            return false;
        return true;
    }

    internal static bool CanAcceptThisItem(Inventory trinketInv, int capacity, Item item)
    {
        if (item == null || item.IsRecipe || capacity <= 0)
        {
            return false;
        }
        switch (item.QualifiedItemId)
        {
            case "(O)73":
            case "(O)930":
            case "(O)102":
            case "(O)858":
            case "(O)GoldCoin":
                return true;
            default:
                trinketInv.RemoveEmptySlots();
                if (trinketInv.Count < capacity)
                    return true;
                for (int i = 0; i < capacity; i++)
                {
                    if (
                        trinketInv[i] is Item stored
                        && stored.canStackWith(item)
                        && stored.Stack + item.Stack < stored.maximumStackSize()
                    )
                        return true;
                }
                return false;
        }
    }
}
