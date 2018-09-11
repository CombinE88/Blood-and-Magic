using System;
using System.Linq;
using OpenRA.Mods.Bam.Traits.World;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits.RPGTraits
{
    public class DungeonsAndDragonsStatsInfo : ITraitInfo
    {
        public readonly int Armor = 0;
        public readonly int Damage = 0;
        public readonly int Speed = 0;

        public readonly bool CanbeModified = true;

        public object Create(ActorInitializer init)
        {
            return new DungeonsAndDragonsStats(init, this);
        }
    }

    public class DungeonsAndDragonsStats : ITick, IFirepowerModifier, ISpeedModifier
    {
        public int Armor;
        public int Damage;
        public int Speed;

        public int ModifiedArmor;
        public int ModifiedDamage;
        public int ModifiedSpeed;
        private DungeonsAndDragonsStatsInfo info;

        public DungeonsAndDragonsStats(ActorInitializer init, DungeonsAndDragonsStatsInfo info)
        {
            Armor = info.Armor;
            Damage = info.Damage;
            Speed = info.Speed;
            ModifiedArmor = info.Armor;
            ModifiedDamage = info.Damage;
            ModifiedSpeed = info.Speed;

            this.info = info;
        }

        public void Tick(Actor self)
        {
            if (!self.IsInWorld || self.IsDead || !info.CanbeModified)
                return;

            var getTileBelow = self.World.Map.GetTerrainInfo(self.Location).Type;


            var getModifiersInfo = self.World.WorldActor.Info.TraitInfo<TilesetStatisticsInfo>();

            var tilesetList = getModifiersInfo.TilesetInformations;
            var tilesetInformations = tilesetList.FirstOrDefault(t => t.Tilename == getTileBelow);

            if (tilesetInformations == null)
            {
                ModifiedArmor = Armor;
                ModifiedDamage = Damage;
                ModifiedSpeed = Speed;
                return;
            }

            ModifiedArmor = Armor + tilesetInformations.Armor;
            ModifiedDamage = Damage + tilesetInformations.Damage;
            ModifiedSpeed = Speed + tilesetInformations.Speed;
        }

        public int GetFirepowerModifier()
        {
            return 100 * ModifiedDamage;
        }

        public int GetSpeedModifier()
        {
            return 100 + ModifiedSpeed*15;
        }
    }
}