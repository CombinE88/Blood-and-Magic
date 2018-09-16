using System.Drawing;
using System.Linq;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Effects;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits
{
    public class TransformToBuildingInfo : ITraitInfo
    {
        public readonly string[] Factions = { "good" };

        public readonly string IntoBuilding = "barracks";


        public readonly string Image = "explosion";
        public readonly string EffectSequence = "smokecloud_effect";
        public readonly string EffectPalette = "bam11195";

        public object Create(ActorInitializer init)
        {
            return new TransformToBuilding(init, this);
        }
    }

    public class TransformToBuilding : ITick, IResolveOrder
    {
        public Actor Buildingbelow;
        public bool StandsOnBuilding = false;
        public TransformToBuildingInfo Info;

        public TransformToBuilding(ActorInitializer init, TransformToBuildingInfo info)
        {
            Info = info;
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
            if (order.OrderString != "TransformTo-" + Info.IntoBuilding && order.OrderString != "RemoveSelf")
                return;

            if (order.OrderString == "RemoveSelf")
            {
                self.CancelActivity();
                self.QueueActivity(new RemoveSelf());
            }

            if (order.OrderString.Contains("TransformTo-" + Info.IntoBuilding))
            {
                var location = Buildingbelow.Location;
                var ownerSelf = self.Owner;
                self.CancelActivity();
                self.QueueActivity(new RemoveSelf());

                self.World.AddFrameEndTask(w =>
                {
                    var init = new TypeDictionary
                    {
                        new LocationInit(location),
                        new OwnerInit(ownerSelf)
                    };

                    var actor = w.CreateActor(Info.IntoBuilding, init);

                    foreach (var cell in actor.Info.TraitInfo<BuildingInfo>().Tiles(actor.Location))
                    {
                        w.Add(new SpriteEffect(
                            actor.World.Map.CenterOfCell(cell),
                            w,
                            Info.Image,
                            Info.EffectSequence,
                            Info.EffectPalette));
                    }
                });
            }
        }
    }
}