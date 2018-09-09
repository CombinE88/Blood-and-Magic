using System.Collections.Generic;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits
{
    public class TransformToConditionOnDamageInfo: ITraitInfo, Requires<AdvancedTransformsInfo>
    {
        public object Create(ActorInitializer init)
        {
            return new TransformToConditionOnDamage(init, this);
        }
    }

    public class TransformToConditionOnDamage: INotifyDamage
    {
        readonly AdvancedTransforms deploy;

        public TransformToConditionOnDamage(ActorInitializer init, TransformToConditionOnDamageInfo info)
        {
            deploy = init.Self.Trait<AdvancedTransforms>();
        }

        public void Damaged(Actor self, AttackInfo e)
        {
            if (!self.IsDead && self.IsInWorld)
                deploy.DeployTransform(false);
        }
    }
}