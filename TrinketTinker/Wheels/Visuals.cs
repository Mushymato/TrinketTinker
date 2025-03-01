using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData;

namespace TrinketTinker.Wheels;

/// <summary>Helper methods for visual effects</summary>
internal static class Visuals
{
    internal const float LAYER_OFFSET = 2E-06f;

    /// <summary>
    /// Get a monogame color from string.
    /// Supports <see cref="TinkerConst.COLOR_PRISMATIC"/> for animated color.
    /// The default color is White (#FFFFFF).
    /// </summary>
    /// <param name="color">Color string</param>
    /// <param name="isConstant">Indicates that this is not animated, no need to update.</param>
    /// <param name="invert">Invert the RGB components.</param>
    /// <returns></returns>
    public static Color GetSDVColor(string? colorStr, out bool isConstant, bool invert = false)
    {
        Color result;
        if (colorStr == TinkerConst.COLOR_PRISMATIC)
        {
            isConstant = false;
            result = Utility.GetPrismaticColor();
        }
        else
        {
            isConstant = true;
            result = Utility.StringToColor(colorStr) ?? Color.White;
        }
        if (invert)
            return new Color(result.PackedValue ^ 0x00FFFFFF);
        return result;
    }

    /// <summary>Broadcast a <see cref="TemporaryAnimatedSprite"/> from <see cref="TemporaryAnimatedSpriteDefinition"/> to all players</summary>
    /// <param name="tasDef">TAS definition</param>
    /// <param name="position">origin of TAS</param>
    /// <param name="drawLayer">base draw layer</param>
    /// <param name="location">sprite broadcast location</param>
    /// <param name="duration"></param>
    /// <param name="rotation"></param>
    public static void BroadcastTAS(
        TemporaryAnimatedSpriteDefinition tasDef,
        Vector2 position,
        float drawLayer,
        GameLocation location,
        float? duration = null,
        float? rotation = null
    )
    {
        if (!GameStateQuery.CheckConditions(tasDef.Condition, location: location))
            return;
        // TemporaryAnimatedSprite temporaryAnimatedSprite = new(
        TemporaryAnimatedSprite temporaryAnimatedSprite = TemporaryAnimatedSprite.GetTemporaryAnimatedSprite(
            tasDef.Texture,
            tasDef.SourceRect,
            tasDef.Interval,
            tasDef.Frames,
            (duration != null) ? (int)(duration / (tasDef.Frames * tasDef.Interval)) : tasDef.Loops,
            position + tasDef.PositionOffset * 4f,
            tasDef.Flicker,
            tasDef.Flip,
            drawLayer + tasDef.SortOffset,
            tasDef.AlphaFade,
            Utility.StringToColor(tasDef.Color) ?? Color.White,
            tasDef.Scale * 4f,
            tasDef.ScaleChange,
            rotation ?? tasDef.Rotation,
            tasDef.RotationChange
        );
        Game1.Multiplayer.broadcastSprites(location, temporaryAnimatedSprite);
    }

    /// <summary>Broadcast TAS using the string id</summary>
    /// <param name="tasId"></param>
    /// <param name="position"></param>
    /// <param name="drawLayer"></param>
    /// <param name="location"></param>
    /// <param name="duration"></param>
    /// <param name="rotation"></param>
    /// <returns></returns>
    public static bool BroadcastTAS(
        string tasId,
        Vector2 position,
        float drawLayer,
        GameLocation location,
        float? duration = null,
        float? rotation = null
    )
    {
        if (AssetManager.TASData.TryGetValue(tasId, out TemporaryAnimatedSpriteDefinition? tasDef))
        {
            BroadcastTAS(tasDef, position, drawLayer, location, duration, rotation);
            return true;
        }
        return false;
    }

    /// <summary>Broadcast a list of TAS using their string ids. Warn and remove any invalid TAS ids.</summary>
    /// <param name="tasIds"></param>
    /// <param name="position"></param>
    /// <param name="drawLayer"></param>
    /// <param name="duration"></param>
    /// <param name="rotation"></param>
    public static void BroadcastTASList(
        List<string> tasIds,
        Vector2 position,
        float drawLayer,
        GameLocation location,
        float? duration = null,
        float? rotation = null
    )
    {
        HashSet<string> invalidTASIds = [];
        foreach (string tasId in tasIds)
        {
            if (AssetManager.TASData.TryGetValue(tasId, out TemporaryAnimatedSpriteDefinition? tasDef))
            {
                BroadcastTAS(tasDef, position, drawLayer, location, duration, rotation);
            }
            else
            {
                invalidTASIds.Add(tasId);
            }
        }
        if (invalidTASIds.Count > 0)
        {
            ModEntry.LogOnce(
                $"No {AssetManager.TASAsset} entry found for: {string.Join(',', invalidTASIds)}",
                LogLevel.Warn
            );
            tasIds.RemoveAll(invalidTASIds.Contains);
        }
    }

    public static void BroadcastItemGetTAS(Item? item, GameLocation location, Vector2 position, Vector2 offset)
    {
        if (item == null)
            return;
        Vector2 startPos = position + offset;
        TemporaryAnimatedSprite temporaryAnimatedSprite =
            new(
                null,
                Rectangle.Empty,
                750f,
                1,
                0,
                startPos,
                flicker: false,
                flipped: false,
                position.Y / 10000f,
                0.005f,
                Color.White,
                1f,
                -0.005f,
                0f,
                0f
            );
        temporaryAnimatedSprite.CopyAppearanceFromItemId(item?.QualifiedItemId);
        temporaryAnimatedSprite.motion.Y = -1f;
        temporaryAnimatedSprite.layerDepth = 1f - Game1.random.Next(100) / 10000f;
        temporaryAnimatedSprite.delayBeforeAnimationStart = Game1.random.Next(350);
        Game1.Multiplayer.broadcastSprites(location, temporaryAnimatedSprite);
    }

    /// <summary>Quadratic ease out function</summary>
    /// <param name="a">Starting value</param>
    /// <param name="b">Ending value</param>
    /// <param name="t">Progress</param>
    /// <returns></returns>
    public static float EaseOut(float a, float b, float t)
    {
        return a + (1 - MathF.Pow(1 - t, 2)) * (b - a);
    }

    /// <summary>Stop drawing companions while game is paused. Doesn't seem to do anything though?</summary>
    /// <returns></returns>
    public static bool ShouldDraw()
    {
        return !(Game1.paused || Game1.HostPaused);
    }

    /// <summary>Take X and Y of a <see cref="Vector3"/> as a new <see cref="Vector2"/></summary>
    /// <param name="vec3"></param>
    /// <returns></returns>
    public static Vector2 AsVec2(this Vector3 vec3)
    {
        return new(vec3.X, vec3.Y);
    }

    /// <summary>Obtain <see cref="Vector3"/> from <see cref="Vector2"/> with rotation as the Z component.</summary>
    /// <param name="vec2"></param>
    /// <param name="rot"></param>
    /// <returns></returns>
    public static Vector3 WithRot(this Vector2 vec2, float rot)
    {
        return new Vector3(vec2.X, vec2.Y, rot);
    }
}
