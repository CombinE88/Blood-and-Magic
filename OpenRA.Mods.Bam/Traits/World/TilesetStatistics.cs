using System.Collections.Generic;
using System.Linq;
using OpenRA.Mods.Common.AI;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits.World
{
    public class TilesetInformations
    {
        public readonly string Tilename = "";
        public readonly string IconSequence = "";
        public readonly int Damage = 0;
        public readonly int Armor = 0;
        public readonly int Speed = 0;

        public TilesetInformations(MiniYaml yaml)
        {
            FieldLoader.Load(this, yaml);
        }
    }

    public class TilesetStatisticsInfo : ITraitInfo
    {
        [FieldLoader.LoadUsing("LoadTilesetInformations")]
        public readonly List<TilesetInformations> TilesetInformations = new List<TilesetInformations>();

        static object LoadTilesetInformations(MiniYaml yaml)
        {
            var ret = new List<TilesetInformations>();
            foreach (var d in yaml.Nodes)
                if (d.Key.Split('@')[0] == "TilesetInformations")
                    ret.Add(new TilesetInformations(d.Value));

            return ret;
        }

        public object Create(ActorInitializer init)
        {
            return new TilesetStatistics(init, this);
        }
    }

    public class TilesetStatistics
    {
        public TilesetStatistics(ActorInitializer init, TilesetStatisticsInfo info)
        {
        }
    }
}