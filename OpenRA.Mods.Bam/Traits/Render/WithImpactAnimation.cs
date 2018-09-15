using System.Linq;
using OpenRA.Mods.Common.Activities;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits.Render
{
    public class WithImpactAnimationInfo : ConditionalTraitInfo
    {
        [Desc("Displayed while attacking.")] [SequenceReference]
        public readonly string Sequence = "damage";

        [Desc("Which sprite body to modify.")] public readonly string Body = "body";

        public override object Create(ActorInitializer init)
        {
            return new WithImpactAnimation(init, this);
        }

        public override void RulesetLoaded(Ruleset rules, ActorInfo ai)
        {
            var matches = ai.TraitInfos<WithSpriteBodyInfo>().Count(w => w.Name == Body);
            if (matches != 1)
                throw new YamlException("WithMoveAnimation needs exactly one sprite body with matching name.");

            base.RulesetLoaded(rules, ai);
        }
    }

    public class WithImpactAnimation : ConditionalTrait<WithImpactAnimationInfo>, INotifyDamage, INotifyCreated
    {
        private WithSpriteBody wsb;
        private WithImpactAnimationInfo info;
        private Actor self;

        public WithImpactAnimation(ActorInitializer init, WithImpactAnimationInfo info) : base(info)
        {
            this.info = info;
            self = init.Self;
        }

        public void PlayManaAnimation(Actor self)
        {
            if (IsTraitDisabled)
                return;

            if (wsb.DefaultAnimation.CurrentSequence.Name == "idle"
                || wsb.DefaultAnimation.CurrentSequence.Name == "stand"
                || wsb.DefaultAnimation.CurrentSequence.Name == "run"
                || wsb.DefaultAnimation.CurrentSequence.Name == "aim")
            {
                wsb.PlayCustomAnimation(self, info.Sequence, () => { });
            }
        }

        void INotifyCreated.Created(Actor self)
        {
            wsb = self.TraitsImplementing<WithSpriteBody>().Single(w => w.Info.Name == Info.Body);
        }

        void INotifyDamage.Damaged(Actor self, AttackInfo e)
        {
            if (!self.IsDead && self.IsInWorld && !e.Damage.DamageTypes.Contains("Healing"))
            {
                self.Trait<Mobile>().Facing = (self.World.Map.CenterOfCell(e.Attacker.Location) - self.CenterPosition).Yaw.Facing;
                PlayManaAnimation(self);
            }
        }
    }
}