using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Inventories;
using StardewValley.Menus;
using StardewValley.Objects.Trinkets;
using TrinketTinker.Effects;
using TrinketTinker.Models;
using TrinketTinker.Wheels;

namespace TrinketTinker.Extras;

public sealed class TinkerInventoryMenu : ItemGrabMenu
{
    const int TEXT_M = 6;
    const int TITLE_LM = 16;
    const int TITLE_TM = 20;

    public TinkerInventoryMenu(
        int actualCapacity,
        IList<Item> inventory,
        bool reverseGrab,
        bool showReceivingMenu,
        InventoryMenu.highlightThisItem highlightFunction,
        behaviorOnItemSelect behaviorOnItemSelectFunction,
        string message,
        behaviorOnItemSelect? behaviorOnItemGrab = null,
        bool snapToBottom = false,
        bool canBeExitedWithKey = false,
        bool playRightClickSound = true,
        bool allowRightClick = true,
        bool showOrganizeButton = false,
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
                base.inventory.SetPosition(base.inventory.xPositionOnScreen, base.inventory.yPositionOnScreen + 38 + 4);
                ItemsToGrabMenu.SetPosition(
                    ItemsToGrabMenu.xPositionOnScreen - 32 + 8,
                    ItemsToGrabMenu.yPositionOnScreen
                );
                storageSpaceTopBorderOffset = 20;
                trashCan.bounds.X =
                    ItemsToGrabMenu.width + ItemsToGrabMenu.xPositionOnScreen + IClickableMenu.borderWidth * 2;
                okButton.bounds.X =
                    ItemsToGrabMenu.width + ItemsToGrabMenu.xPositionOnScreen + IClickableMenu.borderWidth * 2;
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
        // more neighbour nonsense
        if (
            ItemsToGrabMenu.GetBorder(InventoryMenu.BorderSide.Right).FirstOrDefault()
            is ClickableComponent clickableComponent
        )
        {
            if (organizeButton != null)
            {
                organizeButton.leftNeighborID = clickableComponent.myID;
            }

            if (specialButton != null)
            {
                specialButton.leftNeighborID = clickableComponent.myID;
            }

            if (fillStacksButton != null)
            {
                fillStacksButton.leftNeighborID = clickableComponent.myID;
            }

            if (junimoNoteIcon != null)
            {
                junimoNoteIcon.leftNeighborID = clickableComponent.myID;
            }
        }
    }

    /// <summary>Render the UI, draw the trinket item that spawned this menu</summary>
    /// <param name="b"></param>
    public override void draw(SpriteBatch b)
    {
        // compiler went derp
        Action<SpriteBatch> drawMethod = base.draw;
        if (sourceItem != null)
        {
            Vector2 nameSize = Game1.dialogueFont.MeasureString(sourceItem.DisplayName);
            int sourceItemPosX = ItemsToGrabMenu.xPositionOnScreen - borderWidth - spaceToClearSideBorder + TITLE_LM;
            int sourceItemPosY =
                ItemsToGrabMenu.yPositionOnScreen
                - borderWidth
                - spaceToClearTopBorder
                + storageSpaceTopBorderOffset
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
            b.DrawString(Game1.dialogueFont, sourceItem.DisplayName, new(sourceItemPosX, sourceItemPosY), Color.White);
            sourceItem.drawInMenu(
                b,
                new(sourceItemPosX - TEXT_M - Game1.tileSize, sourceItemPosY - Game1.tileSize + nameSize.Y),
                1f
            );
            bool drawBGOrig = drawBG;
            drawBG = false;
            drawMethod(b);
            drawBG = drawBGOrig;
        }
        else
        {
            drawMethod(b);
        }
    }
}

/// <summary>Handler for inventory, does not use mutext (yet) because each trinket has a unique global inventory</summary>
internal sealed class GlobalInventoryHandler
{
    internal record GlobalInventoryHandlerInfo

    /// <summary>Global inventory for this trinket</summary>
    private readonly Inventory trinketInv;
    private TrinketTinkerEffect effect;
    private TinkerInventoryData data;
    private string inventoryId;

    /// <summary>Current page, if this handler is not paged, this value is -1</summary>
    private int page;
    internal List<Trinket>? pagedTrinkets = null;

    internal GlobalInventoryHandler(TrinketTinkerEffect effect, TinkerInventoryData data, string inventoryId)
    {
        this.effect = effect;
        this.data = data;
        this.inventoryId = inventoryId;
        page = -1;
        trinketInv = Game1.player.team.GetOrCreateGlobalInventory(inventoryId);
    }

    internal GlobalInventoryHandler(Farmer farmer)
    {

    }


    internal TinkerInventoryMenu GetMenu()
    {
        return new TinkerInventoryMenu(
            data.Capacity,
            trinketInv,
            reverseGrab: false,
            showReceivingMenu: true,
            HighlightFunction,
            BehaviorOnItemSelectFunction,
            inventoryId,
            BehaviorOnItemGrab,
            snapToBottom: false,
            canBeExitedWithKey: true,
            playRightClickSound: true,
            allowRightClick: false,
            showOrganizeButton: false,
            sourceItem: effect.Trinket
        );
    }

    private bool HighlightFunction(Item item)
    {
        if (item is Trinket trinket)
        {
            if (effect.Trinket == trinket)
                return false;
            if (trinket.GetEffect() is TrinketTinkerEffect otherEffect)
            {
                if (otherEffect.HasEquipTrinketAbility)
                    return false;
                if (
                    effect.HasEquipTrinketAbility
                    && (
                        trinket
                            .GetTrinketData()
                            ?.CustomFields?.TryGetValue(
                                TinkerConst.CustomFields_DirectEquipOnly,
                                out string? directOnly
                            ) ?? false
                    )
                    && directOnly != null
                )
                    return false;
            }
        }
        else if (effect.HasEquipTrinketAbility)
        {
            return false;
        }
        if (data.RequiredTags != null && !Places.CheckContextTagFilter(item, data.RequiredTags))
            return false;
        if (
            data.RequiredItemCondition != null
            && !GameStateQuery.CheckConditions(data.RequiredItemCondition, inputItem: item, targetItem: item)
        )
            return false;
        return true;
    }

    internal Item? AddItem(Item item)
    {
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
        if (trinketInv.Count < data.Capacity)
        {
            trinketInv.Add(item);
            return null;
        }
        return item;
    }

    private void BehaviorOnItemSelectFunction(Item item, Farmer who)
    {
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
        trinketInv.RemoveEmptySlots();
        int num =
            (Game1.activeClickableMenu.currentlySnappedComponent != null)
                ? Game1.activeClickableMenu.currentlySnappedComponent.myID
                : (-1);
        TinkerInventoryMenu menu = GetMenu();
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
            trinketInv.Remove(item);
            trinketInv.RemoveEmptySlots();
            Game1.activeClickableMenu = GetMenu();
        }
    }

    /// <summary>
    /// Ensure empty inventories are deleted, and inaccessable inventories have their contents put into lost and found
    /// Also do a check for trinketSlots and make sure people don't end up with a trinket in slot 1/2 and trinketSlots=0
    /// </summary>
    internal static void DayEndingCleanup()
    {
        var team = Game1.player.team;
        bool newLostAndFoundItems = false;
        // check if the player somehow lost their trinketSlots stat
        bool hasTrinketSlot = Game1.player.stats.Get("trinketSlots") != 0;

        int toSkip = 0;
        if (!hasTrinketSlot)
        {
            toSkip = ModEntry.HasWearMoreRings ? 2 : 1;
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
        // check for missing trinkets to global inv
        HashSet<string> missingTrinketInvs = [];
        foreach (var key in team.globalInventories.Keys)
        {
            var value = team.globalInventories[key];
            if (value == null)
                continue;
            value.RemoveEmptySlots();
            if (key.StartsWith($"{ModEntry.ModId}/"))
            {
                if (value.Count == 0)
                    team.globalInventories.Remove(key);
                else
                    missingTrinketInvs.Add(key);
            }
        }
        if (!missingTrinketInvs.Any())
            return;
        Utility.ForEachItem(
            (item) =>
            {
                if (
                    item is Trinket trinket
                    && trinket.GetEffect() is TrinketTinkerEffect effect
                    && effect.InventoryId != null
                )
                    missingTrinketInvs.Remove(effect.FullInventoryId!);
                return missingTrinketInvs.Any();
            }
        );
        foreach (Trinket trinketItem in Game1.player.trinketItems.Skip(toSkip))
        {
            if (
                trinketItem != null
                && trinketItem.GetEffect() is TrinketTinkerEffect effect
                && effect.InventoryId != null
            )
                missingTrinketInvs.Remove(effect.FullInventoryId!);
        }
        newLostAndFoundItems = newLostAndFoundItems || missingTrinketInvs.Any();
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
        team.newLostAndFoundItems.Value = newLostAndFoundItems;
    }

    internal bool CanAcceptThisItem(Item item)
    {
        if (item == null)
        {
            return false;
        }
        if (item.IsRecipe)
        {
            return true;
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
                if (trinketInv.Count < data.Capacity)
                    return true;
                for (int i = 0; i < data.Capacity; i++)
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
