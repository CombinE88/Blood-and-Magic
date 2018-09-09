using System.Drawing;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits
{
    public class SpawnsAcolytesInfo : ITraitInfo
    {
        public readonly string Actor = "acolyte";

        public object Create(ActorInitializer init)
        {
            return new SpawnsAcolytes(init, this);
        }
    }

    public class SpawnsAcolytes : IResolveOrder, INotifyCreated
    {
        private SpawnsAcolytesInfo info;
        private PlayerResources pr;

        public SpawnsAcolytes(ActorInitializer init, SpawnsAcolytesInfo info)
        {
            this.info = info;
        }

        public void ResolveOrder(Actor self, Order order)
        {
            if (order.OrderString != "SpawnAcolyte")
                return;

            if (pr.TakeCash(self.World.Map.Rules.Actors[info.Actor].TraitInfo<ValuedInfo>().Cost))
                self.World.AddFrameEndTask(w =>
                {
                    var init = new TypeDictionary
                    {
                        new LocationInit(self.World.Map.CellContaining(self.CenterPosition + self.Info.TraitInfo<ExitInfo>().SpawnOffset)),
                        new CenterPositionInit(self.CenterPosition + self.Info.TraitInfo<ExitInfo>().SpawnOffset),
                        new OwnerInit(self.Owner),
                        new FacingInit(250)
                    };
                    var a = w.CreateActor(info.Actor, init);
                    if (a != null && !a.IsDead && a.IsInWorld)
                    {
                        var move = a.TraitOrDefault<IMove>();
                        a.QueueActivity(move.MoveIntoWorld(a, self.Location + self.Info.TraitInfo<ExitInfo>().ExitCell));
                    }
                });
        }

        public void Created(Actor self)
        {
            pr = self.Owner.PlayerActor.Trait<PlayerResources>();
        }
    }
}