
> All examples are assumed to live under content patcher

# [Tinker](#tab/tinker)

```json

```

# [Trinket & Sprites](#tab/other)

```json
// Load texture
{
  "Action": "Load",
  "Target": "Mods/{{ModId}}/MyTrinket",
  "FromFile": "sprites/{{TargetWithoutPath}}.png"
},
// Edit Data/Trinkets 
{
  "Action": "EditData",
  "Target": "Data/Trinkets",
  "Entries": {
    "{{ModId}}_MyTrinket": {
      // Trinket ID, gives qualified ID of (TR){{ModId}}_MyTrinket
      "Id": "{{ModId}}_MyTrinket",
      // Display name (with i18n)
      "DisplayName": "{{i18n:MyTrinket.DisplayName}}",
      // Description
      "Description": "{{i18n:MyTrinket.Description}}",
      // Path to asset texture load target
      "Texture": "Mods/{{ModId}}/MyTrinket",
      // Sheet index (with 16x16 sprite size)
      "SheetIndex": 0,
      // Add trinket to random drop pool once player attains combat mastery
      // Can still add other ways to acquire (e.g. shops, machine outputs)
      "DropsNaturally": true,
      // Allow trinket to reroll stats on the anvil
      "CanBeReforged": true,
    },
  }
}
```