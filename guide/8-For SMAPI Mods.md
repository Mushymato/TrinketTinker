# For SMAPI Mods

Trinket Tinker has no API at the moment as it is primarily meant to be interacted with a content editing framework like Content Patcher.

Here are some well-defined ways SMAPI mods can interface with Trinket Tinker.

## Custom Trigger Action in Ability Action

As mentioned in the [Action ability](4.z.100-Action.md), `TriggerActionContext.CustomFields` has custom values are provided for usage with custom actions.

| Key | Type | Notes |
| --- | ---- | ----- |
| `mushymato.TrinketTinker/Trinket` | `StardewValley.Objects.Trinkets.Trinket` | The trinket which owns the ability that ran this action. |
| `mushymato.TrinketTinker/Owner` | `StardewValley.Farmer` | The farmer who equipped the trinket. |
| `mushymato.TrinketTinker/Position` | `Microsoft.Xna.Framework.Vector2` | The position of the companion, or _null_ if there is no companion. |
| `mushymato.TrinketTinker/Data` | [`TrinketTinker.Models.AbilityData`](~/api/TrinketTinker.Models.AbilityData.yml) | The trinket ability data model, this is not converted by pintail so you must use reflection to access any fields, potentially fragile. |

Once again, [BroadcastAction](4.z.101-BroadcastAction.md) do not benefit from these custom fields.

## Trigger Action

The action `mushymato.TrinketTinker_ProcTrinket <trinket id>` can be run with `TriggerActionManager.TryRunAction` to activate an equipped trinket's `Proc=Trigger` ablities.
The `TriggerContext` provided to the action will be passed through to any action ran by [Action ability](guide/4.z.100-Action.md). If `TriggerContext.CustomFields` is not null, trinket tinker will fill in the aformentioned custom fields for the `TriggerContext`.

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

- [TinkerData](1-Tinker.md)
    - `EnableCondition`
    - `AbilityUnlockConditions`
    - `VariantUnlockConditions`
- [Ability](4-Ability.md)
    - `Condition`
- [Chatter](4.z.201-Chatter.md)
    - `Condition`

## Implementing Entirely new Motions/Abilities

While it's possible to do this by hard DLL reference, it's not recommended as implementation details may change at the author's digression.

## Compatibility

Trinkets are equippped onto the player by appending to `Farmer.trinketItems`, which is a list of trinkets.


