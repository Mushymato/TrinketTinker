using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.GameData;
using TrinketTinker.Effects.Proc;
using TrinketTinker.Models;
using TrinketTinker.Models.Mixin;
using TrinketTinker.Wheels;

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
        /// <inheritdoc/>
        public bool Valid { get; protected set; } = false;
        /// <inheritdoc/>
        public event EventHandler<ProcEventArgs>? EventAbilityProc;
        /// <summary>True if trinket equiped.</summary>
        protected bool Active { get; set; }
        /// <summary>True if trinket proc timeout is not set, or elapsed.</summary>
        protected bool Allowed { get; set; }
        /// <summary>Tracks trinket proc timeout, counts down to 0 and resets to ProcTimer value is set in <see cref="AbilityData"/>.</summary>
        protected double ProcTimer { get; set; } = -1;
        /// <summary>Class dependent arguments for subclasses</summary>
        protected readonly TArgs args;

        /// <summary>Basic constructor, tries to parse arguments as the generic <see cref="IArgs"/> type.</summary>
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
            Name = $"{effect.Trinket.ItemId}:{GetType().Name}_lv{lvl}[{data.Proc}]";
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
                Allowed = d.Proc != ProcOn.Timer;
                ProcTimer = d.ProcTimer;
                switch (d.Proc)
                {
                    case ProcOn.Always:
                        ApplyEffect(new(ProcOn.Always, farmer));
                        break;
                    case ProcOn.Sync:
                        if (e.Abilities[d.ProcSyncIndex] == this)
                            throw new ArgumentException("Cannot use " + ProcOn.Sync.ToString() + " with self-referencing index " + d.ProcSyncIndex.ToString());
                        e.Abilities[d.ProcSyncIndex].EventAbilityProc += HandleProc;
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
                    case ProcOn.Warped:
                        e.EventPlayerWarped += HandleProc;
                        break;
                        // remember to add to Deactivate too
                }
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
                switch (d.Proc)
                {
                    case ProcOn.Always:
                        CleanupEffect(farmer);
                        break;
                    case ProcOn.Sync:
                        e.Abilities[d.ProcSyncIndex].EventAbilityProc -= HandleProc;
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
                    case ProcOn.Warped:
                        e.EventPlayerWarped += HandleProc;
                        break;
                        // remember to add to Activate too
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
                    Visuals.BroadcastTAS(temporarySprite, GetTASPosition(args), e.CompanionOwnerDrawLayer, args.LocationOrCurrent);

                EventAbilityProc?.Invoke(sender, args);
            }
        }

        /// <summary>Get where the on proc <see cref="TemporaryAnimatedSprite"/> should be drawn from.</summary>
        /// <returns></returns>
        protected virtual Vector2 GetTASPosition(ProcEventArgs args)
        {
            return e.CompanionAnchor ?? args.Farmer.Position;
        }

        /// <summary>Cleanup ability when trinket is unequipped, if is <see cref="ProcOn.Always"/></summary>
        /// <param name="farmer"></param>
        protected virtual void CleanupEffect(Farmer farmer)
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
            if (d.ProcTimer >= TinkerConst.ONE_FRAME && !Allowed)
            {
                ProcTimer -= time.ElapsedGameTime.TotalMilliseconds;
                Allowed = ProcTimer <= 0;
            }
            else
            {
                Allowed = true;
            }
            if (d.ProcTimer > 0 && d.Proc == ProcOn.Timer && Allowed)
            {
                HandleProc(null, new(ProcOn.Timer, farmer)
                {
                    Time = time,
                    Location = location
                });
                ProcTimer = d.ProcTimer;
            }
        }
    }
}
