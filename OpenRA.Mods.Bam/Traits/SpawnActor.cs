using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits
{
    public class SpawnActorInfo : ITraitInfo
    {
        public readonly string Actor = null;

        public object Create(ActorInitializer init)
        {
            return new SpawnActor(init, this);
        }
    }

    public class SpawnActor : IResolveOrder
    {
        private SpawnActorInfo info;

        public SpawnActor(ActorInitializer init, SpawnActorInfo info)
        {
            this.info = info;
        }

        public void ResolveOrder(Actor self, Order order)
        {
            if (order.OrderString != "SpawnActor")
                return;

            if (!self.Owner.PlayerActor.Trait<PlayerResources>().TakeCash(self.World.Map.Rules.Actors[info.Actor].TraitInfo<ValuedInfo>().Cost))
                return;

            self.World.AddFrameEndTask(world =>
            {
                var init = new TypeDictionary
                {
                    new LocationInit(self.World.Map.CellContaining(self.CenterPosition + self.Info.TraitInfo<ExitInfo>().SpawnOffset)),
                    new CenterPositionInit(self.CenterPosition + self.Info.TraitInfo<ExitInfo>().SpawnOffset),
                    new OwnerInit(self.Owner)
                };

                var newActor = world.CreateActor(info.Actor, init);
                var move = newActor.TraitOrDefault<IMove>();
                newActor.QueueActivity(move.MoveIntoWorld(newActor, self.Location + self.Info.TraitInfo<ExitInfo>().ExitCell));
            });
        }
    }
}
