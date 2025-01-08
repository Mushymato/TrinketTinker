## Changelog

> All notable changes to this project will be documented here.
>
> The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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

