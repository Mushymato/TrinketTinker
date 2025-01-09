using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace TrinketTinker.Companions.Motions;

public interface IMotion
{
    /// <summary>Random used for anim clips</summary>
    Random ClipRand { get; set; }

    /// <summary>Random used for speech bubbles</summary>
    Random SpeechRand { get; set; }

    /// <summary>Set an oneshot clip, to play once until end</summary>
    /// <param name="clipKey"></param>
    void SetOneshotClip(string? clipKey);

    /// <summary>Set an override clip, to play instead of normal directional animation</summary>
    /// <param name="clipKey"></param>
    void SetOverrideClip(string? clipKey);

    /// <summary>Set a speech bubble to call, usually on ability proc.</summary>
    /// <returns>offset</returns>
    void SetSpeechBubble(string? speechBubbleKey);

    /// <summary>Initialize motion, setup light source if needed.</summary>
    /// <param name="farmer"></param>
    void Initialize(Farmer farmer);

    /// <summary>Cleanup motion, remove light source.</summary>
    /// <param name="farmer"></param>
    void Cleanup();

    /// <summary>Update light source when owner changes location</summary>
    void OnOwnerWarp();

    /// <summary>Changes the position of the anchor that the companion moves relative to.</summary>
    /// <param name="time"></param>
    /// <param name="location"></param>
    void UpdateAnchor(GameTime time, GameLocation location);

    /// <summary>Update info that should change every tick, for owner only. Netfield changes should happen here.</summary>
    /// <param name="time"></param>
    /// <param name="location"></param>
    void UpdateLocal(GameTime time, GameLocation location);

    /// <summary>Update info that should change every tick, for all game instances in multiplayer.</summary>
    /// <param name="time"></param>
    /// <param name="location"></param>
    void UpdateGlobal(GameTime time, GameLocation location);

    /// <summary>Reposition the lightsource.</summary>
    /// <param name="time"></param>
    /// <param name="location"></param>
    void UpdateLightSource(GameTime time, GameLocation location);

    /// <summary>Draws the companion, for all game instances in multiplayer.</summary>
    /// <param name="b"></param>
    void Draw(SpriteBatch b);

    /// <summary>Get position offset of motion</summary>
    /// <returns>offset</returns>
    Vector2 GetOffset();
}
