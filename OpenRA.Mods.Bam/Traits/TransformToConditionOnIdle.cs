using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits
{
    public class TransformToConditionOnIdleInfo : ITraitInfo, Requires<AdvancedTransformsInfo>
    {
        public object Create(ActorInitializer init)
        {
            return new TransformToConditionOnIdle(init, this);
        }
    }

    public class TransformToConditionOnIdle : ITick
    {
        readonly AdvancedTransforms deploy;
        private int tick = 75;

        public TransformToConditionOnIdle(ActorInitializer init, TransformToConditionOnIdleInfo info)
        {
            deploy = init.Self.Trait<AdvancedTransforms>();
        }

        void ITick.Tick(Actor self)
        {
            if (!self.IsDead && self.IsInWorld && self.IsIdle)
            {
                if (tick-- <= 0)
                {
                    deploy.DeployTransform(false);
                    tick = 75;
                }
            }
            else
                tick = 75;
        }
    }
}