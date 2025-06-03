# Appendix

A.k.a the section where the author go on and on out about design choices and other info of -2 relevance to actually using this framework. If you are not a nerd you can stop reading here.

The designed layed out here pertains specifically to Trinket Tinker. It is not the sole answer to extending trinkets, or sole answer to a system of companions. It would be very cool to see other framework mods implement their own flavor of trinkets/companions.

## Features that made this mod Viable

Trinkets are cool 1.6 feature indeed, but it has some special traits that makes it a far easier to extend than previous game systems that described equipment: the (ab)use of reflection to fetch a `TrinketEffectClass` that drives the actual functionality of a trinket. This degree of separation is vital because `TrinketEffectClass` is never serialized to the save file, making it safe to subclass. This is why Trinket Tinker does not need Harmony (the library used to modify game code), as it can simply politely "ask" the game to use it's code by presenting a custom `TrinketEffectClass` to be fetched by reflection.

Besides the effect class, another convenient entity is the `StardewValley.Companions.Companion` class. It is a lightweight class that does not descend from `Characters` but still implememnts `INetObject<NetFields>` and carry info about which player owns it and what position in the world it's currently at. Through subclassing `Companion`, Trinket Tinker has access to net fields without taking on the burden of the entire `Characters` class, and get free netsync'd position out of it. It too is never serialized to the save (although there are some persistent problems with duplicate vanilla companions).

## Scheme

The two core features of a trinket is `Motion`, which represents the companion and how it moves, and `Abilities` which represents what it does. All other kinds of data are auxiliary and simply serve one or both of these features. Although they are "core", both features are optional in a way: you can have a trinket that has no abilities and only a companion follower, and you can have a trinket that has no companions but still activate abilities as usual.

### Motion

Because there's no assumption about the intended purpose of the trinket companion, the `Motion` (and it's supplemental data in `Variants`) is very flexible and imposes only a few rules about how the sprite sheet should be structured. Initially, this framework is geared towards creating non-human critters with entirely custom sprite sheets. As the usecase of a NPC-esque companion came up, features were added later to help support asset reuse and pretending to be an NPC. Another emergent feature is `AnimClips` which was added on after the default motion modes were finalized, but if this framework were to be redone, `AnimClips` is perhaps the preferable way of defining animations in general as it describes animations explictly, rather than rely on magic implict rules that require an entire documentation page to explain. Even in current version it is completely possible to create a companion that only has motion via `AnimClips`.

A notable choice is the absense of true collision, this is because the author played enough skyrim to see the followers get stuck on 1cm ledges. Collision will never be truly real for trinket tinker, because the path finder of stardew is both very cursed and gird based while the player does not move along grids.

### Abilities

The ability system is essentially a live and fast acting version of `Data/TriggerActions` that only activates when trinket is equipped, with pre-defined set of abilities to use based on the initial goal of "replicate most if not all vanilla trinkets" and a number of useful shared behaviors that ride along with the ability's activation (proc). It is another beneficiary of 1.6: many useful bits of code are now usable anywhere via Game State Queries and Trigger Action Actions, and now it becomes quite easy for one mod to define custom behavior that other mods can leverage. In Trinket Tinker this takes the form of the `"Action"` ability class to run arbitrary Trigger Action Actions with, and the `"Condition"` fields provided in a number of places that can be controlled via Game State Queries.



