# Tinker

To make a trinket use TrinketTinker features, add a new entry to the custom asset `mushymato.TrinketTinker/Tinker`.
The key used must match the __unqualified ID__ of the trinket.

When a `mushymato.TrinketTinker/Tinker` entry exists, the `TrinketEffectClass` will be set to `TrinketTinker.Effects.TrinketTinkerEffect` from this mod.

## Structure

| Property | Type | Default | Notes |
| -------- | ---- | ------- | ----- |
| `MinLevel` | int | 1 | Changes the level value that will replace `{0}` in `DisplayName`. |
| `Variants` | [List\<VariantData\>](2-Variant.md) | _empty_ | Defines the sprites of the companion. |
| `Motions` | [List\<MotionData\>](3-Motion.md) | _empty_ | Defines how the companion moves. |
| `Abilities` | [List\<AbilityData\>](4-Ability.md) | _empty_ | Defines what effects are activated, and when. |

## Notes

- Technically all fields here are optional, but in that case there'd be little point to using this framework at all.
- To display a companion, at least 1 Variant and 1 Motion must be defined.
- To have the trinket do thins, at least 1 Ability must be defined.
- At the moment, there's no reason to have more than 1 Motion, but this may change in the future.
