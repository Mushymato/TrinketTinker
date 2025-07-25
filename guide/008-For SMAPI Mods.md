# For SMAPI Mods

Trinket Tinker has no API at the moment as it is primarily meant to be interacted with a content editing framework like Content Patcher.

Still, Trinket Tinker may not have implemented everything you need, here are ways SMAPI mods can interface with Trinket Tinker.

## Custom Trigger Action in Ability Action

As mentioned in the [Action ability](004.z.100-Action.md), `TriggerActionContext.CustomFields` has custom values are provided for usage with custom actions.

| Key | Type | Notes |
| --- | ---- | ----- |
| `mushymato.TrinketTinker/Trinket` | `StardewValley.Objects.Trinkets.Trinket` | The trinket which owns the ability that ran this action. |
| `mushymato.TrinketTinker/Owner` | `StardewValley.Farmer` | The farmer who equipped the trinket. |
| `mushymato.TrinketTinker/Position` | `Microsoft.Xna.Framework.Vector2` | The position of the companion, or _null_ if there is no companion. |
| `mushymato.TrinketTinker/PosOff` | `Microsoft.Xna.Framework.Vector2` | The position of the companion plus the visual offset, or _null_ if there is no companion. |
| `mushymato.TrinketTinker/Data` | [`TrinketTinker.Models.AbilityData`](~/api/TrinketTinker.Models.AbilityData.yml) | The trinket ability data model, this is not converted by pintail so you must use reflection to access any fields, fragile. |

These custom fields provide a way for C# mods to implement their own ability effects as actions, which can then be put into Trinket Tinker.

Once again, [BroadcastAction](004.z.101-BroadcastAction.md) do not benefit from these custom fields.

An example implementation:

### C\# Side
```cs
internal sealed class ModEntry : Mod
{
    public override void Entry(IModHelper helper)
    {
        // ... other Entry stuff ...

        TriggerActionManager.RegisterAction(
            "author.ModName_SpecialAction",
            DoMySpecialAction
        );
    }

    // handler for your action
    private static bool DoMySpecialAction(string[] args, TriggerActionContext context, out string error)
    {
        if (context.CustomFields?.TryGetValue("mushymato.TrinketTinker/Position", out object? vectObj) ?? false)
        {
            Vector2 position = (Vector2)vectObj;
            // do things given the companion position here
        }
        return true;
    }
}
```

### Content Side

```json
// Under "Abilities"
{
    "AbilityClass": "Action",
    "Proc": "Timer",
    "ProcTimer": 250,
    "Args": {
        "Action": "author.ModName_SpecialAction arg1 arg2 arg3"
    },
}
```


## Trigger Action

The action `mushymato.TrinketTinker_ProcTrinket <trinket id>` can be run with `TriggerActionManager.TryRunAction` to activate an equipped trinket's `Proc=Trigger` ablities.
The `TriggerContext` provided to the action will be passed through to any action ran by [Action ability](004.z.100-Action.md). If `TriggerContext.CustomFields` is not null, Trinket Tinker will fill in the aformentioned custom fields for the `TriggerContext`.

To ensure CustomFields is not null, call the action like this:
```cs
CachedAction action = TriggerActionManager.ParseAction($"mushymato.TrinketTinker_ProcTrinket {DesiredTrinketId}");
TriggerActionContext context = new($"{YourModId}_WhateverSuffix", [], null, []);
if (!TriggerActionManager.TryRunAction(action, context, out string error, out Exception _))
{
    // Do any error handling/logging
}
```

## Game State Query Context

To have greater control over trinket abilities, you can define custom Game State Queries for use with your trinkets.

The following Condition fields get the trinket item as both the Input and Target items on their `GameStateQueryContext`.

- [Ability](004-Ability.md)
    - `Condition`
- [Chatter](005.1-Chatter.md)
    - `Condition`
- [Inventory](005.0-Inventory.md)
    - `OpenCondition`
- [TinkerData](001-Tinker.md)
    - `EnableCondition`
    - `AbilityUnlockConditions`
    - `VariantUnlockConditions`

Other cases of Condition either do not provide this, or only use something else for Input and Target items (e.g. [Inventory.RequiredItemCondition](005.0-Inventory.md))

## Implementing Entirely new Motions/Abilities

While it's possible to do this by hard DLL reference, it's not recommended as implementation details may change at the author's digression.

## Compatibility

Trinkets are equipped onto the player by appending to `Farmer.trinketItems`, which is a list of trinkets. Normally this list only ever has 1 trinket, but Trinket Tinker will add indirectly equipped trinkets to the list as well.

These trinkets have mod data set to:
- `mushymato.TrinketTinker/IndirectEquip` = `"T"`
    - All indirectly equipped trinkets has this field set.
- `mushymato.TrinketTinker/HiddenEquip` = `"integer"`, if this trinket is equipped using [mushymato.TrinketTinker_EquipHiddenTrinket](007.2-Actions.md) then this value is the number of days left before the trinket will be

Please leave my 

Special compatiblity is done for `bcmpinc.WearMoreRings`, indirect trinkets will be inserted after trinket 2.


