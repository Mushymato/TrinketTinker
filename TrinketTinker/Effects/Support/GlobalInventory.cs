using StardewModdingAPI;
using StardewValley;
using StardewValley.Inventories;
using StardewValley.Menus;
using StardewValley.Objects.Trinkets;
using TrinketTinker.Models;
using TrinketTinker.Wheels;

namespace TrinketTinker.Effects.Support;

internal class TinkerInventoryMenu : ItemGrabMenu
{
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
        int num = (actualCapacity >= 70) ? 5 : 3;
        if (actualCapacity < 9)
        {
            num = 1;
        }
        int num2 = 64 * (actualCapacity / num);
        ItemsToGrabMenu = new InventoryMenu(
            Game1.uiViewport.Width / 2 - num2 / 2,
            yPositionOnScreen + ((actualCapacity < 70) ? 64 : (-21)),
            playerInventory: false,
            inventory,
            highlightFunction,
            actualCapacity,
            num
        );
        if (num > 3)
        {
            yPositionOnScreen += 42;
            base.inventory.SetPosition(base.inventory.xPositionOnScreen, base.inventory.yPositionOnScreen + 38 + 4);
            ItemsToGrabMenu.SetPosition(ItemsToGrabMenu.xPositionOnScreen - 32 + 8, ItemsToGrabMenu.yPositionOnScreen);
            storageSpaceTopBorderOffset = 20;
            trashCan.bounds.X =
                ItemsToGrabMenu.width + ItemsToGrabMenu.xPositionOnScreen + IClickableMenu.borderWidth * 2;
            okButton.bounds.X =
                ItemsToGrabMenu.width + ItemsToGrabMenu.xPositionOnScreen + IClickableMenu.borderWidth * 2;
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
}

/// <summary>Handler for inventory, does not use mutext (yet) because each trinket has a unique global inventory</summary>
internal sealed class GlobalInventoryHandler(TrinketTinkerEffect effect, TinkerInventoryData data, string inventoryId)
{
    /// <summary>Global inventory for this trinket</summary>
    private readonly Inventory trinketInv = Game1.player.team.GetOrCreateGlobalInventory(inventoryId);

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
            showOrganizeButton: false
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
                if (otherEffect.InventoryId != null)
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
        if (data.RequiredTags != null && !Places.CheckContextTagFilter(item, data.RequiredTags))
            return false;
        if (
            data.RequiredItemCondition != null
            && !GameStateQuery.CheckConditions(data.RequiredItemCondition, inputItem: item)
        )
            return false;
        return true;
    }

    private Item? AddItem(Item item)
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

    /// <summary>Ensure empty inventories are deleted, and inaccessable inventories have their contents put into lost and found</summary>
    internal static void DayEndingCleanup()
    {
        HashSet<string> missingTrinketInvs = [];
        var team = Game1.player.team;
        foreach (var key in team.globalInventories.Keys)
        {
            var value = team.globalInventories[key];
            if (value == null)
                continue;
            value.RemoveEmptySlots();
            if (key.StartsWith(ModEntry.ModId))
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
        foreach (Farmer farmer in Game1.getAllFarmers())
        {
            foreach (Trinket trinketItem in farmer.trinketItems)
            {
                if (
                    trinketItem != null
                    && trinketItem.GetEffect() is TrinketTinkerEffect effect
                    && effect.InventoryId != null
                )
                    missingTrinketInvs.Remove(effect.FullInventoryId!);
            }
        }
        team.newLostAndFoundItems.Value = missingTrinketInvs.Any();
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
    }
}
