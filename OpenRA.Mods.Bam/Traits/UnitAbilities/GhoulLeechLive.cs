using System.Linq;
using OpenRA.Mods.Bam.Traits.RPGTraits;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits.UnitAbilities
{
    public class GhoulLeechLiveInfo : ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new GhoulLeechLive();
        }
    }

    public class GhoulLeechLive : INotifyAppliedDamage
    {
        void INotifyAppliedDamage.AppliedDamage(Actor self, Actor damaged, AttackInfo e)
        {
            if (damaged != null
                && damaged.TraitOrDefault<DungeonsAndDragonsStats>() != null
                && damaged.Info.TraitInfo<DungeonsAndDragonsStatsInfo>().Attributes.Contains("alive")
                && !self.IsDead
                && self.IsInWorld)
                self.InflictDamage(self, new Damage(-e.Damage.Value, new BitSet<DamageType>("Healing")));
        }
    }
}