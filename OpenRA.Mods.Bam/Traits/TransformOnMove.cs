using System.Collections.Generic;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits
{
    public class TransformOnMoveInfo : ITraitInfo, Requires<AdvancedTransformsInfo>
    {
        public object Create(ActorInitializer init)
        {
            return new TransformOnMove(init, this);
        }
    }

    public class TransformOnMove : IResolveOrder, INotifyTransform
    {
        readonly AdvancedTransforms deploy;
        private Order order;

        public TransformOnMove(ActorInitializer init, TransformOnMoveInfo info)
        {
            deploy = init.Self.Trait<AdvancedTransforms>();
        }

        public void ResolveOrder(Actor self, Order order)
        {
            if (order.OrderString == "Move")
            {
                this.order = order;
                if (self != null && !self.IsDead && self.IsInWorld)
                    deploy.DeployTransform(false);
            }
        }

        public void BeforeTransform(Actor self)
        {
        }

        public void OnTransform(Actor self)
        {
        }

        public void AfterTransform(Actor toActor)
        {
            if (order != null && toActor != null && !toActor.IsDead && toActor.IsInWorld)
            {
                toActor.QueueActivity(new Move(toActor, order.TargetLocation, WDist.FromCells(2), null, true));
            }
        }
    }
}