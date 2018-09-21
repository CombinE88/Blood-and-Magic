using System.Linq;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits.World
{
    public class CreateManaSpotsInfo : ITraitInfo
    {
        public readonly string TerrainType = "Manaspot";

        [ActorReference] public readonly string Actor = "manaspot";

        public object Create(ActorInitializer init)
        {
            return new CreateManaSpots(init, this);
        }
    }

    public class CreateManaSpots : INotifyCreated
    {
        private CreateManaSpotsInfo info;

        public CreateManaSpots(ActorInitializer init, CreateManaSpotsInfo info)
        {
            this.info = info;
        }

        void INotifyCreated.Created(Actor self)
        {
            foreach (var cell in self.World.Map.AllCells.Where(c => self.World.Map.GetTerrainInfo(c).Type == info.TerrainType))
            {
                var td = new TypeDictionary
                {
                    new OwnerInit("Neutral"),
                    new FacingInit(255),
                    new LocationInit(cell),
                    new CenterPositionInit(self.World.Map.CenterOfCell(cell))
                };

                self.World.AddFrameEndTask(w =>
                    w.CreateActor(true, info.Actor, td));
            }
        }
    }
}