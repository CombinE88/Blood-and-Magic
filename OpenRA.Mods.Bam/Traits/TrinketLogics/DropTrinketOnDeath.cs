using System;
using System.Collections.Generic;
using System.Linq;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits.TrinketLogics
{
    public class DropTrinketOnDeathInfo : ITraitInfo
    {
        public readonly int Probability = 80;

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

        public void Killed(Actor self, AttackInfo e)
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

            var td = new TypeDictionary
            {
                new ParentActorInit(self),
                new LocationInit(self.Location),
                new CenterPositionInit(self.CenterPosition),
                new OwnerInit(self.Owner)
            };

            if (itemToDrop != null)
                self.World.AddFrameEndTask(w => w.CreateActor(itemToDrop, td));
        }
    }
}