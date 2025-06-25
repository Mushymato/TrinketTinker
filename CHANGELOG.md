## Changelog

> All notable changes to this project will be documented here.
>
> The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

### 1.6.1 (wip)

#### Added
- Use "!" at first position to invert the filter on Monster and NPC anchors.
- Trigger action `mushymato.TrinketTinker_ToggleCompanion`, toggle companion visibility (the abilities still work atm).
- Allow abilities to consume fuel items from the tinker inventory.
- For hidden trinkets with a inventory, show the number of days left there.
- New C# facing API

### 1.6.0

#### Added
- TemporaryAnimatedSprite now shares some code with MMAP and can use (most of) the funny MMAP TAS features. The custom assets remain separate.
- HitTAS now takes a list of pipe (`|`) separated TAS
- With Motion `Collision: Line`, will now check walls before going towards an anchor point.
- Companions that are tied to an NPC can now breathe (if the NPC themselves can breathe).
- Updated `fr.json` thanks to Caranud
- AnimClips can now have full frame source rect overrides. This essentially allows modder to do anything with anim clip frames.
- TinkerInventory menu now has Stack and Organize buttons.
- Companions can now have an `AttachedTAS` which is a temporary animate spirte that follows them around.
- New ability `PetFarmAnimal` and corresponding anchor mode `FarmAnimal`, go to nearest farm animal and pet them.
- New anchor mode `NPC`, go to nearest NPC.

#### Changed
- AnimClips now reset to start frame when switched over, instead of attempting to keep previous frame if possible.
- Harvest abilities now have higher tool power.

### 1.5.8

#### Added

- New user config setting, global disable showing companions in events.

#### Fixed

- Trinket global inventory cleaning being incorrectly ran on farmhands as well.

### 1.5.7

#### Added

- Hitscan and Projectile can now have FacingDirectionOnly which makes them only attack monsters in the direction the companion is facing.

#### Fixed

- Alt variants swapping between 2 most valid alt variants rather than keeping 1.

### 1.5.6

#### Added

- Projectile can now have Height which are different than offset in that they do not actually become different position.
- Harvest* abilities will now show the item harvested in a short TAS over the companion, for HarvestTo Player and TinkerInventory, disable this with ShowHarvestedItem.
- Anchors now have StopRange, which defines how far away to stop moving towards target.

#### Changed

- HarvestForage now also harvest spawned items like quartz in the mines.

#### Fixed

- Hidden trinkets disappearing for 1 day when returning to menu and loading save again.
- Android sorta compatible-ish now, except for backpack related stuff which I can't fix at all.
- Make projectile hitbox based directly on the size of texture, which should hopefully make it more reliable.

### 1.5.5

#### Changed

- MoveSync now only sync movement with owner when anchor mode is owner, use MoveSyncAll for old behavior.
- Serpent alt segment layer depth changes to make them consistent, this is entirely by vibes and implementation detail subject to change

#### Fixed

- HarvestTo TrinketInventory not harvesting custom bush drops properly
- Temporary Animated Sprite now checks Condition, if given

### 1.5.4

#### Added

- Several more Harvest* abilities, and matching AnchorTarget
    - HarvestTwig & AnchorTarget Twig
    - HarvestWeed & AnchorTarget Weed
    - HarvestDigSpot * AnchorTarget DigSpot

#### Fixed

- Separated position from offset to make companions better at harvesting things at Range=0
- HarvestCrop only harvesting to player inventory
- Jittery movement on Lerp with no overlap and velocity = -2 is now wobbly movement
- Content patcher consistently apply on equipped trinkets

### 1.5.3

#### Added

- Ability ProcSyncId is like ProcSyncIndex, but uses the Id to find what to Sync.

#### Fixed

- Null handling on add item to tinker inventory
- Nop ignoring timer
- Regression on global inventory cleanup

#### Fixed

- Crash on saving with more than 10 trinkets equipped
- Duplicating base game trinket companions 2

### 1.5.2

#### Added

- HitTAS for Hitscan/Projectile, apply a TAS at the target on hit.
- HitsDelay for Hitscan/Projectile, adds a delay between hits.

#### Fixed

- Crash on saving with more than 10 trinkets equipped
- Duplicating base game trinket companions

#### Changed

- Hitscan ProcTAS now fires at the companion's position, instead of the target position, this is because HitTAS was added

### 1.5.1

#### Fixed

- Crash on day ending

### 1.5.0

#### Added

- Interact now uses a keybind, configurable in GMCM
- Inventories of equipped trinkets can now be opened with a keybind, configurable in GMCM
- Updated documentation with all 1.5.0 changes
- Add 2 new GSQ please see docs
    - `mushymato.TrinketTinker_IN_ALT_VARIANT <Input|Target> <itemId> <item count compare>`
    - `mushymato.TrinketTinker_TRINKET_HAS_ITEM <Input|Target> <itemId> <item count compare>`
- Add 1 new Item Query for specific usage
    - `mushymato.TrinketTinker_HIRE_TRINKET <trinketId>`
- Allow Proc Always to respect Condition

### 1.5.0-beta.2

#### Added

- Make sure your trinkets get unequipped if you end the day with trinketSlots=0 for some reason, unfortunately won't catch case where the trinketSlots stat changed after DayEnding
- Change ProcSound to a model with these 2 fields
    - `CueName`: sound cue to play
    - `Pitch`: list of int pitch (/2400), random one will be used
    - Old form of string still works
- HarvestTo on HarvestStone/Forage/Shakeable/Crop now takes TinkerInventory, which puts the item into the companion's inventory (if it has one)
- HarvestTo now works with ItemDrop abilities
- Nop anim clips may now have a duration
- Alt variants for companions to switch variant on the fly (just visual, no effect on the variant number)
- Chatter ability, pick a dialogue from a set of Chatter dialogue data
- ProcChatterKey, force a particular chatter key the next time a chatter ability is activated
- Ability can now check for InCombat, combat is defined as "location has monster" and player have dealt damage/taken damage in the last 10 seconds
- Change GSQ to use Input/Target, new syntax:
    - `mushymato.TrinketTinker_IS_TINKER <Input|Target|ItemId> [level] [variant]`
    - `mushymato.TrinketTinker_HAS_LEVELS <Input|Target|ItemId>`
    - `mushymato.TrinketTinker_HAS_VARIANTS <Input|Target|ItemId>`
    - `mushymato.TrinketTinker_ENABLED_TRINKET_COUNT <Input|Target|ItemId> <playerKey> [count] [trinketId]`

#### Fixed

- Made a bunch of lists in the data model nullable
- Deprecated Motions
- Draw for 36 slot trinket inventory

### 1.5.0-beta.1

#### Added

- New ability BroadcastAction, it's like action but it runs the action on multiplayer, useful with SetNpcInvisible and Host
- New Proc Interact which fires when player right clicks when overlapping with companion enough, as well as debug draw for bounding boxes.
- EquipTrinket now bans trinkets with CustomFields `mushymato.TrinketTinker/DirectEquipOnly` from entering the inventory in the first place.
- Lerp now has Velocity, -2: old behaviour, -1: match speed with farmer, 0 does not move except teleport, 1+ caps the velocity of the trinket.
- Lerp now has NoOverlap, makes this companion avoid entering the bounding box of another companion.
- Speech bubble allowed to interrupt previous speech bubble during fade out time
- New actions for equipping a hidden trinket, does not require trinketSlot to use (up to modder to gate that trigger action)
    - `mushymato.TrinketTinker_EquipHiddenTrinket <trinketId> [level] [variant] [daysDuration]`: equip trinket for `daysDuration` days, or -1 by default (unequip only with the following action)
    - `mushymato.TrinketTinker_UnequipHiddenTrinket <trinketId> [level] [variant]`: unequip trinket
    - level and variant do not support R, unlike mushymato.TrinketTinker_CREATE_TRINKET


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

- Prevent Trinket Tinker anvil output method from affecting non Trinket Tinker items.

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

