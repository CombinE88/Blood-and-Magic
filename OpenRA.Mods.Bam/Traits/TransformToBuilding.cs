using System.Drawing;
using System.Linq;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits
{
    public class TransformToBuildingInfo : ITraitInfo
    {
        public readonly string IntoBuilding = "building1";

        public object Create(ActorInitializer init)
        {
            return new TransformToBuilding(init, this);
        }
    }

    public class TransformToBuilding : ITick, IResolveOrder
    {
        public Actor Buildingbelow;
        public bool StandsOnBuilding = false;
        private TransformToBuildingInfo info;

        public TransformToBuilding(ActorInitializer init, TransformToBuildingInfo info)
        {
            this.info = info;
        }

        void ITick.Tick(Actor self)
        {
            if (self == null || self.IsDead || !self.IsInWorld)
                return;

            var cellstandingOn = self.Location;

            var build = self.World.WorldActor.Trait<BuildingInfluence>().GetBuildingAt(cellstandingOn);
            Buildingbelow = build != null && build.Info.HasTraitInfo<AllowTransfromInfo>() ? build : null;
            StandsOnBuilding = build != null;
        }

        void IResolveOrder.ResolveOrder(Actor self, Order order)
        {
            if (order.OrderString != "TransformTo" && order.OrderString != "RemoveSelf")
                return;

            if (order.OrderString != "TransformTo")
                self.World.AddFrameEndTask(w =>
                {
                    var init = new TypeDictionary
                    {
                        new LocationInit(Buildingbelow.Location),
                        new OwnerInit(self.Owner)
                    };
                    var a = w.CreateActor(info.IntoBuilding, init);

                    self.Dispose();
                });
            else if (order.OrderString != "RemoveSelf")
                self.Dispose();
        }
    }
}