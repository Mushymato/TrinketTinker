## Changelog

> All notable changes to this project will be documented here.
>
> The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

### 1.5.0-beta.1

#### Added

- Add new ability BroadcastAction, it's like action but it runs the action on multiplayer, useful with SetNpcInvisible and Host
- Add new Proc Interact which fires when player right clicks when overlapping with companion enough, as well as debug draw for bounding boxes.
- EquipTrinket now bans `mushymato.TrinketTinker/IndirectEquip` from entering the inventory in the first place.
- Lerp now has Velocity, -2: old behaviour, -1: match speed with farmer, 0 does not move except teleport, 1+ caps the velocity of the trinket.

### 1.5.0-beta.0

#### Added

- Allow HarvestShakeable to target larger bushes (but not walnut bush), handle BushBloomMod integration
- New game state queries
    - `mushymato.TrinketTinker_IS_TINKER [level] [variant]`: check the input item is a trinket with tinker data, then check if the item is of some level and variant. Compare operators can be used, one of `>1`, `<1`, `>=1`, `<=1`, `!=1`.
    - `mushymato.TrinketTinker_HAS_LEVELS`: check the input item is a trinket with tinker data, then check if the input item has any unlocked levels.
    - `mushymato.TrinketTinker_HAS_VARIANTS`: check the input item is a trinket with tinker data, then check if the input item has any unlocked variants.
    - `mushymato.TrinketTinker_ENABLED_TRINKET_COUNT <playerKey> [count] [trinketId]`: Count number of trinket of particular ID (either the optional trinketId or inputItem) equipped and activated, and compare it to a number.
- MachineOutputItem CustomData `mushymato.TrinketTinker/Increment`, allows upgrading a trinket's level or variant by X amount
- Trinket companion/effects can be silenced with `EnableCondition` on `TinkerData`, essentially making them do nothing on equip.
- Trinket can now have an inventory via `Inventory` on `TinkerData`, "use" the trinket item to open this inventory.
- EquipTrinket ability, equips trinkets inside the inventory.
    - Trinkets can be banned from this ability by setting `mushymato.TrinketTinker/DirectEquipOnly` to `"T"` or any non null value.
    - Trinkets equipped this way will have modData `mushymato.TrinketTinker/IndirectEquip` set to `"T"`.
- ActionAbility: support for `Actions` (list of actions), `ActionEnd` (action to run at removal for AlwaysProc), and `ActionsEnd` (list of end actions)
- TriggerActionContext from ActionAbility now use `mushymato.TrinketTinker/Action` as name and pass these fields via CustomFields:
    - `mushymato.TrinketTinker/Owner`: trinket owner (Farmer)
    - `mushymato.TrinketTinker/Trinket`: trinket item (Trinket)
    - `mushymato.TrinketTinker/Data`: AbilityData (TrinketTinker.Models.AbilityData)
    - `mushymato.TrinketTinker/Position`: companion position including offset (Vector2)
- GameStateQueryContext from ability proc check now provides the trinket item as inputItem and targetItem, along with
    - `mushymato.TrinketTinker/Data`: AbilityData (TrinketTinker.Models.AbilityData)
    - `mushymato.TrinketTinker/Position`: companion position including offset (Vector2)
- LerpMotion Velocity argument
    - Limits velocity to some constant float
    - When velocity is -1, match velocity to player movement speed
    - Default: velocity is -2 or lower, regular Lerp

#### Fixed

- Some abilities did not apply due to an incorrect check for max level

### 1.4.5

#### Fixed

- Error with filtering for certain types of crops (ginger?).

### 1.4.4

#### Added

- Draw debug mode that shows the sprite index of the companion on screen. Toggle with command tt_draw_debug.

#### Fixed

- Companions not appearing in volcano and farm buildings.

### 1.4.3

#### Added

- New HarvestShakeable ability to shake trees bushes and fruit trees.
- New Shakeable anchor target.
- New Nop ability that does nothing, but can be used for purpose of proc effects.
- Anchors can now specify a list of RequiredAbilities. If set, the anchor only activates if the trinket has ability of matching AbilityClass at the current level. Some mode dependent default values are provided.
- Abilities can define ProcSyncDelay for how much time should pass between its proc and any follow up abilities.
- Crop and Forage Anchors can now specify context tag items to ignore.
- HarvestCrop and HarvestForage can now specify context tag items to ignore.
- fr.json by by [Caranud](https://next.nexusmods.com/profile/Caranud)

### 1.4.2

#### Fixed

- Hopefully fix a crash after some events, very strange.

### 1.4.1

#### Added

- Add support for randomized speech bubbles.
- Add "Swim" anim clip key for when the player is swimming.

#### Fixed

- Companions duplicating when farmhand is exiting an event.

### 1.4.0

#### Added

- Add support for randomized anim clips.
- Add speech bubble feature.
- Allow one shot clips to pause movement.
- Changed perching clip to behave by static motion rules (check against player facing direction), rather than lerp motion rules (check against companion facing).

#### Fixed

- AbilityProc clips not playing in multiplayer.

### 1.3.0

#### Added

- Additional HarvestTo field for Harvest type abilities to determine where the harvested item go (inventory, debris, none).
- New field Filter on Anchors and on Hitscan/Projectile ability. If set, the enemy types listed will not be targeted.

#### Fixed

- Lerp MoveSync companions moving when a weapon is swung. They are now prevented from moving while a tool is being used. Also applies to check for perching.

### 1.2.1

#### Fixed

- Prevent trinket tinker anvil output method from affecting non trinket tinker items.

### 1.2.0

#### Added

- New "Homing" argument on projectile to make projectile recheck target midflight.

#### Fixed

- Projectile used wrong target point, change to bounding box center.

### 1.1.0

#### Fixed

- Update for SDV 1.6.14, add new "sourceChange" argument in ItemQueryContext.

### 1.0.2

#### Fixed

- Correctly invalidate Data/Trinkets whenever the Tinker asset gets invalidated.

### 1.0.1

#### Fixed

- Add workaround for issue where `TrinketEffectClass` ends up being null.

### 1.0.0

#### Added

- Implement all the things.

