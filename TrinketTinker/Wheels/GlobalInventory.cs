using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace TrinketTinker.Wheels;

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

/// <summary>Helper methods dealing with inventory</summary>
internal static class GlobalInventory
{
    internal static TinkerInventoryMenu GetMenu(string inventoryId, int actualCapacity)
    {
        Console.WriteLine($"GetItemGrabMenu: {inventoryId}");
        // IList<Item> inventory, bool reverseGrab, bool showReceivingMenu, InventoryMenu.highlightThisItem highlightFunction, behaviorOnItemSelect behaviorOnItemSelectFunction, string message, behaviorOnItemSelect behaviorOnItemGrab = null, bool snapToBottom = false, bool canBeExitedWithKey = false, bool playRightClickSound = true, bool allowRightClick = true, bool showOrganizeButton = false, int source = 0, Item sourceItem = null, int whichSpecialButton = -1, object context = null, ItemExitBehavior heldItemExitBehavior = ItemExitBehavior.ReturnToPlayer, bool allowExitWithHeldItem = false
        return new TinkerInventoryMenu(
            actualCapacity,
            Game1.player.team.GetOrCreateGlobalInventory(inventoryId), //inventory
            reverseGrab: false,
            showReceivingMenu: true,
            InventoryMenu.highlightAllItems,
            behaviorOnItemSelectFunction,
            "Inventory.GetItemGrabMenu",
            behaviorOnItemGrab,
            snapToBottom: false,
            canBeExitedWithKey: true,
            playRightClickSound: true,
            allowRightClick: true,
            showOrganizeButton: true
        );
    }

    private static void behaviorOnItemGrab(Item item, Farmer who)
    {
        ModEntry.Log($"behaviorOnItemGrab({item.QualifiedItemId}, {who.UniqueMultiplayerID})");
        return;
    }

    internal static void behaviorOnItemSelectFunction(Item item, Farmer who)
    {
        ModEntry.Log($"behaviorOnItemSelectFunction({item.QualifiedItemId}, {who.UniqueMultiplayerID})");
        return;
    }
}
