using OpenRA.Mods.Bam.Traits.Player;
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

    public class GeneratesExperience : INotifyDamage, INotifyAttack, INotifyBuildComplete, INotifyKilled
    {
        private DungeonsAndDragonsExperience exp;

        public GeneratesExperience(ActorInitializer init)
        {
            exp = init.Self.Info.HasTraitInfo<DungeonsAndDragonsExperienceInfo>() ? exp = init.Self.Trait<DungeonsAndDragonsExperience>() : null;
        }

        public void Damaged(Actor self, AttackInfo e)
        {
            if (!self.IsDead)
                exp.AddCash(e.Damage.Value / 2);
        }

        public void BuildingComplete(Actor self)
        {
            if (exp != null)
            {
                if (self.Info.HasTraitInfo<ValuedInfo>())
                    exp.AddCash(self.Info.TraitInfo<ValuedInfo>().Cost / 2);
            }
        }

        public void Attacking(Actor self, Target target, Armament a, Barrel barrel)
        {
            if (exp != null && target.Actor != null && !self.Owner.IsAlliedWith(target.Actor.Owner))
            {
                if (self.Info.HasTraitInfo<ValuedInfo>())
                    exp.AddCash(self.Info.TraitInfo<ValuedInfo>().Cost / 2);
            }
        }

        public void PreparingAttack(Actor self, Target target, Armament a, Barrel barrel)
        {
        }

        public void Killed(Actor self, AttackInfo e)
        {
            if (exp != null && e.Attacker != null && !self.Owner.IsAlliedWith(e.Attacker.Owner))
            {
                if (self.Info.HasTraitInfo<ValuedInfo>())
                    e.Attacker.Owner.PlayerActor.Trait<DungeonsAndDragonsExperience>().AddCash(self.Info.TraitInfo<ValuedInfo>().Cost * 2);
            }
        }
    }
}