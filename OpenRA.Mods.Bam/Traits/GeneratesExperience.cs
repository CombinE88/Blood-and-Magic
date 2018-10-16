using OpenRA.Mods.Bam.Traits.PlayerTraits;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits
{
    public class GeneratesExperienceInfo : ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new GeneratesExperience(init);
        }
    }

    public class GeneratesExperience : INotifyKilled, INotifyDamage
    {
        private DungeonsAndDragonsExperience exp;

        public GeneratesExperience(ActorInitializer init)
        {
            exp = init.Self.Info.HasTraitInfo<DungeonsAndDragonsExperienceInfo>() ? exp = init.Self.Trait<DungeonsAndDragonsExperience>() : null;
        }

        void INotifyKilled.Killed(Actor self, AttackInfo e)
        {
            if (exp != null && e.Attacker != null && !self.Owner.IsAlliedWith(e.Attacker.Owner))
            {
                if (self.Info.HasTraitInfo<ValuedInfo>())
                    e.Attacker.Owner.PlayerActor.Trait<DungeonsAndDragonsExperience>().AddCash(self.Info.TraitInfo<ValuedInfo>().Cost);
            }
        }

        void INotifyDamage.Damaged(Actor self, AttackInfo e)
        {
            if (e.Attacker != null
                && !e.Attacker.IsDead
                && e.Attacker.Owner.PlayerActor.Info.HasTraitInfo<DungeonsAndDragonsExperienceInfo>()
                && !e.Attacker.Owner.IsAlliedWith(self.Owner))
                e.Attacker.Owner.PlayerActor.Trait<DungeonsAndDragonsExperience>().AddCash(e.Damage.Value > 0 ? e.Damage.Value * 3 : 0);
        }
    }
}