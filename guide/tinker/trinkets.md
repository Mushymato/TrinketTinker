# Trinkets

> [!NOTE]
> The following page mainly covers how trinkets are added in vanilla game, regardless of whether TrinketTinker is being used.
> It is required to add trinkets through this method before extending it with TrinketTinker features.

Trinkets can be added with by editing `Data/Trinkets`, generally with [content patcher](https://github.com/Pathoschild/StardewMods/tree/stable/ContentPatcher).

Annotated Example:
[!code.json[](guide/tinker/cp-example.json)]

> [!TIP]
> Refer to content patcher docs for more details about [EditData](https://github.com/Pathoschild/StardewMods/blob/stable/ContentPatcher/docs/author-guide/action-load.md) and [Load](https://github.com/Pathoschild/StardewMods/blob/stable/ContentPatcher/docs/author-guide/action-load.md)

### TrinketEffectClass

This is the entry point to using TrinketTinker