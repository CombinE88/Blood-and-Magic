using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits
{
    public class TransformToConditionOnIdleInfo : ITraitInfo, Requires<TransformsInfo>
    {
        public object Create(ActorInitializer init)
        {
            return new TransformToConditionOnIdle(init, this);
        }
    }

    public class TransformToConditionOnIdle : ITick
    {
        readonly Transforms deploy;
        private int tick = 25;

        public TransformToConditionOnIdle(ActorInitializer init, TransformToConditionOnIdleInfo info)
        {
            deploy = init.Self.Trait<Transforms>();
        }

        public void Tick(Actor self)
        {
            if (!self.IsDead && self.IsInWorld && self.IsIdle)
                if (tick-- <= 0)
                {
                    deploy.DeployTransform(false);
                    tick = 25;
                }
        }
    }
}