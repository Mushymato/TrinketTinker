# Utility

Extra non-trinket features provided by this mod.

See sub pages for more specific topics.

## [Data/Location](https://stardewvalleywiki.com/Modding:Location_data) CustomFields

Aside from conditions defined on a particular trinket, it's also possible to disable trinket features for a whole location using CustomFields.

```
"mushymato.TrinketTinker/disableAbilities": true|false
```
Disable trinket abilities while owner is in the location (except for always active abilities).

```
"mushymato.TrinketTinker/disableCompanions": true|false
```
Disable companion display while owner is in the location. Their position updates continue.

## Console Commands

### tt_draw_debug

This command toggles drawing of companion sprite index and bounding box of both the companion and the farmer ()

This command toggles some debug drawing options:
- Companion sprite index: a sprite index will be drawn over the companion as they animate. This sprite index is positioned at the companion's "position".
- Companion bounding box: a magenta box is drawn around the companion, showing the zone used for [Interact proc](004-Ability.md) type and for [NoOverlap Lerp motion](003.z.000-Lerp.md).

### tt_unequip_trinket

Force unequip all trinkets and send the unequipped trinkets to lost and found. Mainly useful if a mod fails to properly remove their trinkets.
