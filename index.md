---
_layout: landing
---

# Trinket Tinker

This is a framework for creating Stardew Valley 1.6 trinkets that can have advanced abilities using just content patcher.
If you are looking to make mods using this framework, [start here](guide/trinkets.md). The rest of this page is musings about design and future updates.


### Design

- Logic around companion and how they move is fully separate from the trinket's abilities, allowing easier mix and match.
- Variant system to give companion different sprites for the same motion pattern, color masks are also supported.
- Ability operate on a proc condition -> effect system, which allows easier code reuse.
- Avoided using harmony for all this, no specific motivation for this, just seeing if I could.


### Future

- If you got any motion or ability ideas/requests feel free to reach out to me, I am especially willing to implement if you can describe the desired use case.
    - discord: chu2.718281828459045235360287471, on the main Stardew Valley discord.
    - nexus: mushymato
    - github (I dont check DMs here): mushymato
- Want to make something like [spine](https://esotericsoftware.com/spine-in-depth) to let people rig sprites, something like that probably deserve its own mod though.
- Companion don't have any kind of AI beyond picking which position to follow and does not care about pathfinding, There is some inherent conflict in giving a game entity that is a "follower" to the player too much agency. Unsure what I want to achieve here.


