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

## Draw debug mode

To help with debugging animations, use `tt_draw_debug` to enable/disable drawing of companion sprite index.
