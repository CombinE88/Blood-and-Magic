using System;
using System.Linq;
using OpenRA.Mods.Bam.Activities;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits.Transform
{
    public class TransformToBuildingInfo : ITraitInfo
    {
        public readonly string BuildAreaActor = "buildarea";

        public object Create(ActorInitializer init)
        {
            return new TransformToBuilding(this);
        }
    }

    public class TransformToBuilding : IResolveOrder
    {
        private TransformToBuildingInfo info;
        public string IntoActor; // TODO find nicer way

        public TransformToBuilding(TransformToBuildingInfo info)
        {
            this.info = info;
        }

        public bool CanTransform(Actor self)
        {
            var building = self.World.WorldActor.Trait<BuildingInfluence>().GetBuildingAt(self.Location);

            if (building == null || building.Info.Name != info.BuildAreaActor)
                return false;

            foreach (var tile in building.Info.TraitInfo<BuildingInfo>().Tiles(building.Location))
            {
                var actors = self.World.FindActorsInCircle(self.World.Map.CenterOfCell(tile), WDist.Zero).ToArray();

                if (actors.Length != 1 || actors[0].Owner != self.Owner || !actors[0].Info.HasTraitInfo<TransformToBuildingInfo>())
                    return false;
            }

            return true;
        }

        void IResolveOrder.ResolveOrder(Actor self, Order order)
        {
            if (order.OrderString != "TransformToBuilding")
                return;

            if (!CanTransform(self))
                return;

            var building = self.World.WorldActor.Trait<BuildingInfluence>().GetBuildingAt(self.Location);

            foreach (var tile in building.Info.TraitInfo<BuildingInfo>().Tiles(building.Location))
                self.World.FindActorsInCircle(self.World.Map.CenterOfCell(tile), WDist.Zero).First().Dispose();

            self.QueueActivity(new AdvancedTransform(IntoActor, AdvancedTransformEffect.NONE));
        }
    }
}
