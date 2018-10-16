using System;
using System.Collections.Generic;
using System.Linq;
using OpenRA.Mods.Bam.Traits.TrinketLogics;
using OpenRA.Mods.Bam.Traits.World;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits.RPGTraits
{
    public class TerrainBonus
    {
        public readonly string Tilename = "";
        public readonly int Damage = 0;
        public readonly int Armor = 0;
        public readonly int Speed = 0;

        public TerrainBonus(MiniYaml yaml)
        {
            FieldLoader.Load(this, yaml);
        }
    }

    public class DungeonsAndDragonsStatsInfo : ITraitInfo
    {
        public readonly int Armor = 0;
        public readonly int Damage = 0;
        public readonly int Speed = 0;

        public readonly string[] PartialProtection = {};
        public readonly string[] FullProtection = {};
        public readonly string[] IgnoresAbilites = {};
        public readonly bool PartialReverted = false;

        [Desc("What is this. possibilities are: Humanoid, Alive, Nature, Holy, Evil")]
        public readonly string[] Attributes = { "Alive", "Humanoid" };

        public readonly string[] IgnoresNegativeTerrainEffects = { "" };
        public readonly bool Flying = false;

        [FieldLoader.LoadUsing("LoadTerrainBonus")]
        public readonly List<TerrainBonus> TerrainBonus = new List<TerrainBonus>();

        static object LoadTerrainBonus(MiniYaml yaml)
        {
            var ret = new List<TerrainBonus>();
            foreach (var d in yaml.Nodes)
                if (d.Key.Split('@')[0] == "TerrainBonus")
                    ret.Add(new TerrainBonus(d.Value));

            return ret;
        }

        public readonly bool CanbeModified = true;

        public object Create(ActorInitializer init)
        {
            return new DungeonsAndDragonsStats(init, this);
        }
    }

    public class DungeonsAndDragonsStats : ITick, IFirepowerModifier, ISpeedModifier, IReloadModifier
    {
        public int Armor;
        public int Damage;
        public int Speed;

        public int ModifiedArmor;
        public int ModifiedDamage;
        public int ModifiedSpeed;
        public DungeonsAndDragonsStatsInfo Info;
        private Actor self;

        public DungeonsAndDragonsStats(ActorInitializer init, DungeonsAndDragonsStatsInfo info)
        {
            Armor = info.Armor;
            Damage = info.Damage;
            Speed = info.Speed;
            ModifiedArmor = info.Armor;
            ModifiedDamage = info.Damage;
            ModifiedSpeed = info.Speed;

            Info = info;
            self = init.Self;
        }

        void ITick.Tick(Actor self)
        {
            if (!self.IsInWorld || self.IsDead || !Info.CanbeModified)
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

            ModifyValues(tilesetInformations, self);
        }

        private void ModifyValues(TilesetInformations tileInfo, Actor self)
        {
            if (!Info.Flying)
            {
                ModifiedArmor = Armor + (Info.IgnoresNegativeTerrainEffects.Contains(tileInfo.Tilename)
                                    ? Math.Max(tileInfo.Armor, 0)
                                    : tileInfo.Armor);

                ModifiedDamage = Damage + (Info.IgnoresNegativeTerrainEffects.Contains(tileInfo.Tilename)
                                     ? Math.Max(tileInfo.Damage, 0)
                                     : tileInfo.Damage);

                ModifiedSpeed = Speed + (Info.IgnoresNegativeTerrainEffects.Contains(tileInfo.Tilename)
                                    ? Math.Max(tileInfo.Speed, 0)
                                    : tileInfo.Speed);

                var selfBonus = Info.TerrainBonus.FirstOrDefault(t => t.Tilename == tileInfo.Tilename);
                if (selfBonus != null)
                {
                    ModifiedArmor += selfBonus.Armor;
                    ModifiedDamage += selfBonus.Damage;
                    ModifiedSpeed += selfBonus.Speed;
                }
            }

            var trinketTrati = self.TraitOrDefault<CanHoldTrinket>();
            if (trinketTrati != null)
            {
                ModifiedArmor += trinketTrati.ExtraArmor;
                ModifiedDamage += trinketTrati.ExtraDamage;
                ModifiedSpeed += trinketTrati.ExtraSpeed;
            }
        }

        int IFirepowerModifier.GetFirepowerModifier()
        {
            return 100 * ModifiedDamage;
        }

        int ISpeedModifier.GetSpeedModifier()
        {
            return 100 + ModifiedSpeed * 25;
        }

        int IReloadModifier.GetReloadModifier()
        {
            return 100 - self.World.SharedRandom.Next(0, 5);
        }
    }
}