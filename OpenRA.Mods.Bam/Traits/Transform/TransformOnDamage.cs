using OpenRA.Mods.Bam.Activities;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits.Transform
{
    public class TransformOnDamageInfo : ITraitInfo
    {
        public readonly string IntoActor = null;

        public object Create(ActorInitializer init)
        {
            return new TransformOnDamage(init, this);
        }
    }

    public class TransformOnDamage : INotifyDamage
    {
        private TransformOnDamageInfo info;

        public TransformOnDamage(ActorInitializer init, TransformOnDamageInfo info)
        {
            this.info = info;
        }

        public void Damaged(Actor self, AttackInfo e)
        {
            self.QueueActivity(new AdvancedTransform(info.IntoActor, AdvancedTransformEffect.TRANSFORM));
        }
    }
}
