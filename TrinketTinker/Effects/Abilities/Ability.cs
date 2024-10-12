using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.GameData;
using StardewValley.Monsters;
using TrinketTinker.Effects.Proc;
using TrinketTinker.Models;
using TrinketTinker.Models.Mixin;

namespace TrinketTinker.Effects.Abilities
{
    /// <summary>Abstract class, proc various effects while trinket is equipped.</summary>
    public abstract class Ability<TArgs> : IAbility where TArgs : IArgs
    {
        /// <summary>Companion that owns this ability.</summary>
        protected readonly TrinketTinkerEffect e;
        /// <summary>Data for this ability.</summary>
        protected readonly AbilityData d;
        /// <summary>Ability name, default to type name.</summary>
        public readonly string Name;
        /// <summary>True if trinket data produces a valid ability.</summary>
        public bool Valid { get; set; } = false;
        /// <summary>True if trinket equiped.</summary>
        protected bool Active { get; set; }
        /// <summary>True if trinket proc timeout is not set, or elapsed.</summary>
        protected bool Allowed { get; set; }
        /// <summary>Tracks trinket proc timeout, counts down to 0 and resets to ProcTimer value is set in <see cref="AbilityData"/>.</summary>
        protected double ProcTimer { get; set; } = -1;
        /// <summary>Class dependent arguments for subclasses</summary>
        protected readonly TArgs args;

        /// <summary>Constructor</summary>
        /// <param name="effect"></param>
        /// <param name="data"></param>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public Ability(TrinketTinkerEffect effect, AbilityData data, int lvl) : base()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            if (typeof(TArgs) != typeof(NoArgs))
            {
                args = data.ParseArgs<TArgs>()!;
                if (args == null || !args.Validate())
                {
                    Valid = false;
                    return;
                }
            }
            Valid = true;
            e = effect;
            d = data;
            string clsName = data.Name == "" ? GetType().Name : data.Name;
            Name = $"{effect.Trinket.ItemId}:{clsName}[{lvl}]";
            ProcTimer = data.ProcTimer;
        }

        /// <summary>Setup the ability, when trinket is equipped.</summary>
        /// <param name="farmer"></param>
        /// <returns></returns>
        public virtual bool Activate(Farmer farmer)
        {
            if (!Active)
            {
                Active = true;
                Allowed = d.ProcOn != ProcOn.Timer;
                ProcTimer = d.ProcTimer;

                switch (d.ProcOn)
                {
                    case ProcOn.Always:
                        HandleProc(null, new(ProcOn.Always)
                        {
                            Farmer = farmer
                        });
                        break;
                    case ProcOn.Use:
                        e.EventUse += HandleProc;
                        break;
                    case ProcOn.Footstep:
                        e.EventFootstep += HandleProc;
                        break;
                    case ProcOn.ReceiveDamage:
                        e.EventReceiveDamage += HandleProc;
                        break;
                    case ProcOn.DamageMonster:
                        e.EventDamageMonster += HandleProc;
                        break;
                    case ProcOn.SlayMonster:
                        e.EventSlayMonster += HandleProc;
                        break;
                    case ProcOn.Trigger:
                        e.EventTrigger += HandleProc;
                        break;
                }

                e.EventFootstep += HandleProc;
            }
            return Active;
        }

        /// <summary>Teardown the ability, when trinket is removed.</summary>
        /// <param name="farmer"></param>
        /// <returns></returns>
        public virtual bool Deactivate(Farmer farmer)
        {
            if (Active)
            {

                switch (d.ProcOn)
                {
                    case ProcOn.Always:
                        UnProc(farmer);
                        break;
                    case ProcOn.Use:
                        e.EventUse -= HandleProc;
                        break;
                    case ProcOn.Footstep:
                        e.EventFootstep -= HandleProc;
                        break;
                    case ProcOn.ReceiveDamage:
                        e.EventReceiveDamage -= HandleProc;
                        break;
                    case ProcOn.DamageMonster:
                        e.EventDamageMonster -= HandleProc;
                        break;
                    case ProcOn.SlayMonster:
                        e.EventSlayMonster -= HandleProc;
                        break;
                    case ProcOn.Trigger:
                        e.EventTrigger -= HandleProc;
                        break;
                }
                Active = false;
                Allowed = false;
                return true;
            }
            return false;
        }

        /// <summary>Handle proc of ability</summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected virtual void HandleProc(object? sender, ProcEventArgs args)
        {
            if (Active && Allowed && args.Check(d) && ApplyEffect(args))
            {
                if (d.ProcSound != null)
                    Game1.playSound(d.ProcSound);
                foreach (TemporaryAnimatedSpriteDefinition temporarySprite in d.ProcTemporarySprites)
                {
                    TemporaryAnimatedSprite temporaryAnimatedSprite = new(
                        temporarySprite.Texture,
                        temporarySprite.SourceRect,
                        temporarySprite.Interval,
                        temporarySprite.Frames,
                        temporarySprite.Loops,
                        e.CompanionAnchor + temporarySprite.PositionOffset * 4f,
                        temporarySprite.Flicker, temporarySprite.Flip,
                        e.CompanionOwnerDrawLayer + temporarySprite.SortOffset,
                        temporarySprite.AlphaFade,
                        Utility.StringToColor(temporarySprite.Color) ?? Color.White,
                        temporarySprite.Scale * 4f,
                        temporarySprite.ScaleChange,
                        temporarySprite.Rotation,
                        temporarySprite.RotationChange
                    );
                    Game1.Multiplayer.broadcastSprites(args.LocationOrCurrent, temporaryAnimatedSprite);
                }
            }
        }

        /// <summary>Cleanup ability, if is <see cref="ProcOn.Always"/></summary>
        /// <param name="farmer"></param>
        protected virtual void UnProc(Farmer farmer)
        {
        }

        /// <summary>Applies ability effect, mark the ability as not allowed until next tick or longer.</summary>
        /// <param name="farmer"></param>
        /// <returns></returns>
        protected virtual bool ApplyEffect(ProcEventArgs proc)
        {
            Allowed = false;
            return true;
        }

        /// <summary>Update on game tick, handles the proc timer.</summary>
        /// <param name="farmer"></param>
        /// <param name="time"></param>
        /// <param name="location"></param>
        public virtual void Update(Farmer farmer, GameTime time, GameLocation location)
        {
            if (d.ProcTimer >= Constants.ONE_FRAME && !Allowed)
            {
                ProcTimer -= time.ElapsedGameTime.TotalMilliseconds;
                Allowed = ProcTimer <= 0;
            }
            else
            {
                Allowed = true;
            }
            if (d.ProcOn == ProcOn.Timer && Allowed)
            {
                HandleProc(null, new(ProcOn.Timer)
                {
                    Farmer = farmer,
                    Time = time,
                    Location = location
                });
                ProcTimer = d.ProcTimer;
            }
        }
    }
}
