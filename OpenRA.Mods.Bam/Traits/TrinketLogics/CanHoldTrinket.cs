using System.Linq;
using OpenRA.Mods.Common;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits.TrinketLogics
{
    public class CanHoldTrinketInfo : ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new CanHoldTrinket();
        }
    }

    public class CanHoldTrinket : IResolveOrder, ITick
    {
        public Actor HoldsTrinket;
        private Actor ignoreTrinket;

        void DropTrinket(Actor self)
        {
            if (HoldsTrinket == null)
                return;

            var trinketInfo = HoldsTrinket.Info.Name;

            self.World.AddFrameEndTask(w =>
            {
                ignoreTrinket = w.CreateActor(trinketInfo, new TypeDictionary
                {
                    new LocationInit(self.World.Map.CellContaining(self.CenterPosition)),
                    new CenterPositionInit(self.CenterPosition),
                    new OwnerInit("Neutral"),
                    new FacingInit(255)
                });
            });

            HoldsTrinket.Dispose();
            HoldsTrinket = null;
        }

        public void ResolveOrder(Actor self, Order order)
        {
            if (order.OrderString != "dropItem")
                return;

            DropTrinket(self);
        }

        void ITick.Tick(Actor self)
        {
            if (!self.IsInWorld || self.IsDead)
                return;

            var newTrinket = self.World.FindActorsInCircle(self.CenterPosition, new WDist(125)).FirstOrDefault(a => a.Info.HasTraitInfo<IsTrinketInfo>());

            if (newTrinket == null && ignoreTrinket != null)
                ignoreTrinket = null;

            if (newTrinket == null || HoldsTrinket != null || newTrinket == ignoreTrinket)
                return;

            HoldsTrinket = newTrinket;
            self.World.Remove(newTrinket);
        }
    }
}