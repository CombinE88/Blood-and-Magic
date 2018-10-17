using System.Linq;
using OpenRA.GameRules;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits
{
    public class SoulFireInfo : ITraitInfo, IRulesetLoaded
    {
        public readonly int Seconds = 5;
        public readonly int DamageTick = 1;

        [WeaponReference, FieldLoader.Require, Desc("Default weapon to use for explosion if ammo/payload is loaded.")]
        public readonly string Weapon = null;

        public WeaponInfo WeaponInfo { get; private set; }

        public object Create(ActorInitializer init)
        {
            return new SoulFire(this);
        }

        public void RulesetLoaded(Ruleset rules, ActorInfo ai)
        {
            if (!string.IsNullOrEmpty(Weapon))
            {
                WeaponInfo weapon;
                var weaponToLower = Weapon.ToLowerInvariant();
                if (!rules.Weapons.TryGetValue(weaponToLower, out weapon))
                {
                    throw new YamlException("Weapons Ruleset does not contain an entry '{0}'".F(weaponToLower));
                }

                WeaponInfo = weapon;
            }
        }
    }

    public class SoulFire : ITick
    {
        private SoulFireInfo info;
        private int max;
        public int Ticker;

        public SoulFire(SoulFireInfo info)
        {
            this.info = info;
            max = this.info.Seconds * 25;
        }

        public void Tick(Actor self)
        {
            if (Ticker < max)
            {
                Ticker++;
                if (Ticker % (info.DamageTick * 25) == 0)
                {
                    var weapon = info.WeaponInfo;
                    if (weapon == null)
                        return;

                    if (weapon.Report != null && weapon.Report.Any())
                        Game.Sound.Play(SoundType.World, weapon.Report.Random(self.World.SharedRandom), self.CenterPosition);

                    // Use .FromPos since this actor is killed. Cannot use Target.FromActor
                    weapon.Impact(Target.FromPos(self.CenterPosition), self, Enumerable.Empty<int>());
                }
            }
            else if (Ticker == max)
            {
                self.Dispose();
                Ticker = max + 1;
            }
        }
    }
}