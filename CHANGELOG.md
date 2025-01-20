## Changelog

> All notable changes to this project will be documented here.
>
> The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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

