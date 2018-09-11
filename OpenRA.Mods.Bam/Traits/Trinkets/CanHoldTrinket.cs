using System.Linq;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits.Trinkets
{
    public class CanHoldTrinketInfo : ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new CanHoldTrinket();
        }
    }

    public class CanHoldTrinket : ITick, IResolveOrder
    {
        public Actor Current;
        public Actor IgnorePickup;

        void IResolveOrder.ResolveOrder(Actor self, Order order)
        {
            if (order.OrderString != "DropTrinket")
                return;

            Drop(self);
        }

        public void Drop(Actor self)
        {
            if (Current == null)
                return;

            var trinketInfo = Current.Info.Name;

            self.World.AddFrameEndTask(world =>
            {
                IgnorePickup = world.CreateActor(trinketInfo, new TypeDictionary
                {
                    new LocationInit(self.World.Map.CellContaining(self.CenterPosition)),
                    new OwnerInit("Neutral")
                });
            });

            Current.Dispose();
            Current = null;
        }

        void ITick.Tick(Actor self)
        {
            if (!self.IsInWorld || self.IsDead)
                return;

            var newTrinket = self.World.FindActorsInCircle(self.CenterPosition, new WDist(125)).FirstOrDefault(a => a.Info.HasTraitInfo<TrinketInfo>());

            if (IgnorePickup != newTrinket)
                IgnorePickup = null;

            if (newTrinket == null || Current != null || newTrinket == IgnorePickup)
                return;

            Current = newTrinket;
            self.World.Remove(newTrinket);
        }
    }
}
