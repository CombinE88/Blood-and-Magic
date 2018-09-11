using System.Collections.Generic;
using OpenRA.Mods.Bam.Activities;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits.Transform
{
    public class TransformOnMoveInfo : ITraitInfo
    {
        public readonly string IntoActor = null;

        public readonly string Cursor = "move";
        public readonly string BlockedCursor = "move-blocked";

        public object Create(ActorInitializer init)
        {
            return new TransformOnMove(init, this);
        }
    }

    public class TransformOnMove : IResolveOrder, IIssueOrder
    {
        public TransformOnMoveInfo Info;

        public TransformOnMove(ActorInitializer init, TransformOnMoveInfo info)
        {
            Info = info;
        }

        IEnumerable<IOrderTargeter> IIssueOrder.Orders { get { yield return new MoveOrderTargeter(this); } }

        Order IIssueOrder.IssueOrder(Actor self, IOrderTargeter order, Target target, bool queued)
        {
            if (order is MoveOrderTargeter)
                return new Order("Move", self, target, queued);

            return null;
        }

        public void ResolveOrder(Actor self, Order order)
        {
            if (order.OrderString != "Move")
                return;

            self.QueueActivity(new AdvancedTransform(Info.IntoActor, AdvancedTransformEffect.TRANSFORM, actor => { actor.World.IssueOrder(new Order("Move", actor, order.Target, true)); }));
        }
    }

    class MoveOrderTargeter : IOrderTargeter
    {
        private TransformOnMove transformOnMove;
        public bool TargetOverridesSelection(TargetModifiers modifiers)
        {
            return modifiers.HasModifier(TargetModifiers.ForceMove);
        }

        public MoveOrderTargeter(TransformOnMove transformOnMove)
        {
            this.transformOnMove = transformOnMove;
        }

        public string OrderID { get { return "Move"; } }
        public int OrderPriority { get { return 4; } }
        public bool IsQueued { get; protected set; }

        public bool CanTarget(Actor self, Target target, List<Actor> othersAtTarget, ref TargetModifiers modifiers, ref string cursor)
        {
            if (target.Type != TargetType.Terrain)
                return false;

            var location = self.World.Map.CellContaining(target.CenterPosition);
            IsQueued = modifiers.HasModifier(TargetModifiers.ForceQueue);

            cursor = self.World.Map.Contains(location) ? self.World.Map.GetTerrainInfo(location).CustomCursor ?? transformOnMove.Info.Cursor : transformOnMove.Info.BlockedCursor;

            return true;
        }
    }
}
