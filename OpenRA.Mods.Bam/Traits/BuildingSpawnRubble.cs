using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits
{
    public class BuildingSpawnRubbleInfo : ITraitInfo, Requires<BuildingInfo>
    {
        public readonly string[] Actors = { "" };

        public object Create(ActorInitializer init)
        {
            return new BuildingSpawnRubble(init, this);
        }
    }

    public class BuildingSpawnRubble : INotifyKilled
    {
        private Pair<CPos, SubCell>[] footprints;
        private BuildingSpawnRubbleInfo info;

        public BuildingSpawnRubble(ActorInitializer init, BuildingSpawnRubbleInfo info)
        {
            footprints = init.Self.Trait<Building>().OccupiedCells();
            this.info = info;
        }

        void INotifyKilled.Killed(Actor self, AttackInfo e)
        {
            foreach (var cell in footprints)
            {
                var td = new TypeDictionary
                {
                    new OwnerInit("Neutral"),
                    new LocationInit(cell.First),
                    new CenterPositionInit(self.World.Map.CenterOfCell(cell.First))
                };

                self.World.AddFrameEndTask(w =>
                    w.CreateActor(true, info.Actors[self.World.SharedRandom.Next(0, info.Actors.Length)], td));
            }
        }
    }
}