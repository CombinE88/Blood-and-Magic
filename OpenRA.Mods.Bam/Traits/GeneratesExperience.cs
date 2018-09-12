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

    public class GeneratesExperience : INotifyDamage, INotifyKilled
    {
        private DungeonsAndDragonsExperience exp;

        public GeneratesExperience(ActorInitializer init)
        {
            exp = init.Self.Info.HasTraitInfo<DungeonsAndDragonsExperienceInfo>() ? exp = init.Self.Trait<DungeonsAndDragonsExperience>() : null;
        }

        public void Damaged(Actor self, AttackInfo e)
        {
            if (exp != null && !self.IsDead)
                exp.AddCash(e.Damage.Value / 2);
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