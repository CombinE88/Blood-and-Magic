using System.Linq;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits.TrinketLogics
{
    public class DropTrinketOnDeathInfo : ITraitInfo
    {
        public readonly int Probability = 80;

        public readonly string[] IgnoreTerrain = { "Clear", "Water", "Walls" };

        public object Create(ActorInitializer init)
        {
            return new DropTrinketOnDeath(init, this);
        }
    }

    public class DropTrinketOnDeath : INotifyKilled
    {
        private DropTrinketOnDeathInfo info;
        private Actor self;

        public DropTrinketOnDeath(ActorInitializer init, DropTrinketOnDeathInfo info)
        {
            this.info = info;
            self = init.Self;
        }

        void INotifyKilled.Killed(Actor self, AttackInfo e)
        {
            if (self.World.SharedRandom.Next(0, 100) >= info.Probability)
                return;

            var allItems = self.World.Map.Rules.Actors.Where(a => a.Value.TraitInfoOrDefault<IsTrinketInfo>() != null).ToList();

            if (!allItems.Any())
                return;

            var totalChance = 0;
            string itemToDrop = null;

            foreach (var item in allItems)
            {
                totalChance += item.Value.TraitInfo<IsTrinketInfo>().DropChance;
            }

            var choosRandom = self.World.SharedRandom.Next(0, totalChance);

            foreach (var item in allItems)
            {
                choosRandom -= item.Value.TraitInfo<IsTrinketInfo>().DropChance;
                if (choosRandom <= 0)
                {
                    itemToDrop = item.Value.Name;
                    break;
                }
            }

            self.World.AddFrameEndTask(w =>
            {
                var build = self.World.FindActorsInCircle(self.CenterPosition, new WDist(265))
                    .FirstOrDefault(a => !a.Equals(self) && a.Trait<Building>() != null && a.Info.TraitInfo<BuildingInfo>().UnpathableTiles(self.Location).Any());
                var newTrinket = self.World.FindActorsInCircle(self.CenterPosition, new WDist(125)).FirstOrDefault(a => a.Info.HasTraitInfo<IsTrinketInfo>());

                var findPos = self.World.Map.FindTilesInCircle(self.Location, 2, false).ToArray();
                var findEmpty = findPos.Where(c => info.IgnoreTerrain.Contains(self.World.Map.GetTerrainInfo(c).Type)).ToArray();
                var findEmptyActor = findEmpty.Where(c =>
                    self.World.FindActorsInCircle(self.World.Map.CenterOfCell(c), new WDist(265)).All(a => a.TraitOrDefault<IsTrinket>() == null)
                    && self.World.WorldActor.Trait<BuildingInfluence>().GetBuildingAt(c) == null).ToArray();

                var position = build == null && newTrinket == null ? self.Location : self.ClosestCell(findEmptyActor);

                var td = new TypeDictionary
                {
                    new ParentActorInit(self),
                    new LocationInit(position),
                    new CenterPositionInit(w.Map.CenterOfCell(position)),
                    new OwnerInit(self.Owner)
                };

                if (itemToDrop != null && position != null)
                    w.CreateActor(itemToDrop, td);
            });
        }
    }
}