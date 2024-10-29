# [Item Queries](https://stardewvalleywiki.com/Modding:Item_queries)

These are useful for populating shops and machine output rules.

## mushymato.TrinketTinker_CREATE_TRINKET

```
mushymato.TrinketTinker_CREATE_TRINKET <trinket id> [level] [variant]
```

Create a new trinket item. If the trinket has tinker data, set level and variant. Useful for producing a trinket at specified level/variant.

Setting level or variant to "R" will randomize the level or variant.

## mushymato.TrinketTinker_CREATE_TRINKET_ALL_VARIANTS

```
mushymato.TrinketTinker_CREATE_TRINKET_ALL_VARIANTS <trinket id> [level]
```

Create all variants of a trinket (with tinker data). Useful for making a shop sell all variants of a trinket instead of making the player roll for them.

Setting level to "R" will randomize the level once, and then set trinkets created by this query to that level.
