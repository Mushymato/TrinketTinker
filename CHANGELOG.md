## Changelog

> All notable changes to this project will be documented here.
>
> The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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

