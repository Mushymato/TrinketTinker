# Inventory

Trinkets can have inventory, which can be opened by "using" the trinket item. This menu can also be accessed when the trinket is equipped via a [keybinding](009-User%20Configuration.md). This inventory can accept most items except for itself and other trinkets that have inventory. Further control of what is allowed into an inventory can be achieved through `RequiredTags` and `RequiredItemCondition`.

Should the trinket item be lost, all items inside will be sent to the lost and found on the next day.

There's a number of caveats when using inventory with [EquipTrinket](004.z.200-EquipTrinket.md), refer to that page for details.

## Structure

| Property | Type | Default | Notes |
| -------- | ---- | ------- | ----- |
| `Capacity` | int | 9 | Capacity of the inventory. |
| `OpenCondition` | string | _null_ | A [game state query](https://stardewvalleywiki.com/Modding:Game_state_queries) used to check if this inventory can be opened. |
| `RequiredTags` | string | _null_ | A list of [context tags](https://stardewvalleywiki.com/Modding:Common_data_field_types#Context_tag) used to check if an item should be allowed into the inventory. Each space separated sub list of tags act as AND while the overall list act as OR. |
| `RequiredItemCondition` | string | _null_ | A [game state query](https://stardewvalleywiki.com/Modding:Game_state_queries) used to check if an item should be allowed into the inventory. The item being checked will be provided as the Input and Target. |

