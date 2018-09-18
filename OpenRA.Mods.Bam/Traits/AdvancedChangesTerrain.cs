#region Copyright & License Information

/*
 * Copyright 2007-2018 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */

#endregion

using System.Collections.Generic;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
    [Desc("Modifies the terrain type underneath the actors location.")]
    class AdvancedChangesTerrainInfo : ITraitInfo, Requires<BuildingInfo>
    {
        [FieldLoader.Require] public readonly string TerrainType = null;

        public object Create(ActorInitializer init)
        {
            return new AdvancedChangesTerrain(this);
        }
    }

    class AdvancedChangesTerrain : INotifyAddedToWorld, INotifyRemovedFromWorld
    {
        readonly AdvancedChangesTerrainInfo info;
        private Dictionary<CPos, byte> previousTerrains = new Dictionary<CPos, byte>();

        public AdvancedChangesTerrain(AdvancedChangesTerrainInfo info)
        {
            this.info = info;
        }

        void INotifyAddedToWorld.AddedToWorld(Actor self)
        {
            var footprintcells = self.Info.TraitInfo<BuildingInfo>().Footprint;
            var map = self.World.Map;
            foreach (var cell in footprintcells)
            {
                var terrain = map.Rules.TileSet.GetTerrainIndex(info.TerrainType);
                previousTerrains.Add(self.Location + cell.Key, map.CustomTerrain[self.Location + cell.Key]);
                map.CustomTerrain[self.Location + cell.Key] = terrain;
            }
        }

        void INotifyRemovedFromWorld.RemovedFromWorld(Actor self)
        {
            foreach (var cell in previousTerrains)
            {
                var map = self.World.Map;
                map.CustomTerrain[cell.Key] = cell.Value;
            }
        }
    }
}