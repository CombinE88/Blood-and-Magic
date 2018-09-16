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

using System.Linq;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
    public class DamagedByTrapsInfo : ITraitInfo
    {
        public readonly int Damage = 2;

        public readonly int Delay = 12;

        public readonly BitSet<DamageType> DamageTypes = default(BitSet<DamageType>);

        [FieldLoader.Require] [Desc("Terrain names to trigger the condition.")]
        public readonly string[] TerrainTypes = { };

        public object Create(ActorInitializer init)
        {
            return new DamagedByTraps(init, this);
        }
    }

    public class DamagedByTraps : ITick
    {
        readonly TileSet tileSet;
        private DamagedByTrapsInfo info;
        private int _delay;


        public DamagedByTraps(ActorInitializer init, DamagedByTrapsInfo info)
        {
            this.info = info;
            tileSet = init.World.Map.Rules.TileSet;
        }


        void ITick.Tick(Actor self)
        {
            var ground = self.World.Map.GetTerrainInfo(self.Location).Type;

            if (!info.TerrainTypes.Contains(ground))
            {
                _delay = 0;
                return;
            }

            if (_delay++ >= info.Delay)
            {
                _delay = 0;
                self.InflictDamage(self, new Damage(info.Damage, info.DamageTypes));
            }
        }
    }
}