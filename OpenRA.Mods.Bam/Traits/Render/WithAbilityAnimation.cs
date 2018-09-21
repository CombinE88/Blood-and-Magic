using System.Linq;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits.Render
{
    public class WithAbilityAnimationInfo : ConditionalTraitInfo
    {
        [Desc("Displayed while attacking.")] [SequenceReference]
        public readonly string Sequence = "damage";

        [Desc("Which sprite body to modify.")] public readonly string Body = "body";

        public override object Create(ActorInitializer init)
        {
            return new WithAbilityAnimation(init, this);
        }

        public override void RulesetLoaded(Ruleset rules, ActorInfo ai)
        {
            var matches = ai.TraitInfos<WithSpriteBodyInfo>().Count(w => w.Name == Body);
            if (matches != 1)
                throw new YamlException("WithMoveAnimation needs exactly one sprite body with matching name.");

            base.RulesetLoaded(rules, ai);
        }
    }

    public class WithAbilityAnimation : ConditionalTrait<WithAbilityAnimationInfo>, INotifyCreated
    {
        private WithSpriteBody wsb;
        private WithAbilityAnimationInfo info;
        private Actor self;

        public WithAbilityAnimation(ActorInitializer init, WithAbilityAnimationInfo info) : base(info)
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
    }
}