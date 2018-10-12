using System.Linq;
using OpenRA.Mods.Bam.Traits.PlayerTraits;
using OpenRA.Mods.Bam.Traits.RPGTraits;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Effects;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits.TrinketLogics
{
    public class CanHoldTrinketInfo : ITraitInfo
    {
        public readonly string[] CannotUse = { };

        [Desc("Which sprite body to modify.")] public readonly string Body = "body";

        public object Create(ActorInitializer init)
        {
            return new CanHoldTrinket(init, this);
        }
    }

    public class CanHoldTrinket : IResolveOrder, ITick, INotifyKilled
    {
        public Actor HoldsTrinket;
        public Actor IgnoreTrinket;
        private Actor self;
        public int ExtraDamage;
        public int ExtraArmor;
        public int ExtraSpeed;
        private CanHoldTrinketInfo info;

        public CanHoldTrinket(ActorInitializer init, CanHoldTrinketInfo info)
        {
            self = init.Self;
            this.info = info;
        }

        public void DropTrinket(Actor self)
        {
            if (HoldsTrinket == null)
                return;

            var trinketInfo = HoldsTrinket.Info.Name;

            var build = self.World.WorldActor.Trait<BuildingInfluence>().GetBuildingAt(self.Location);
            var newTrinket = self.World.FindActorsInCircle(self.CenterPosition, new WDist(125)).FirstOrDefault(a => a.Info.HasTraitInfo<IsTrinketInfo>());

            var findPos = self.World.Map.FindTilesInCircle(self.Location, 2, false).ToArray();
            var findEmpty = findPos.Where(c => self.World.Map.GetTerrainInfo(c).Type != "Clear" && self.World.Map.GetTerrainInfo(c).Type != "Wall").ToArray();
            var findEmptyActor = findEmpty.Where(c =>
                self.World.FindActorsInCircle(self.World.Map.CenterOfCell(c), new WDist(265)).All(a => a.TraitOrDefault<IsTrinket>() == null)
                && self.World.WorldActor.Trait<BuildingInfluence>().GetBuildingAt(c) == null).ToArray();

            var position = build == null && newTrinket == null ? self.Location : self.ClosestCell(findEmptyActor);

            self.World.AddFrameEndTask(w =>
            {
                IgnoreTrinket = w.CreateActor(trinketInfo, new TypeDictionary
                {
                    new LocationInit(position),
                    new CenterPositionInit(self.World.Map.CenterOfCell(position)),
                    new OwnerInit("Neutral"),
                    new FacingInit(255)
                });
            });

            HoldsTrinket.Dispose();
            HoldsTrinket = null;
        }

        public void ResolveOrder(Actor self, Order order)
        {
            if (order.OrderString != "DropTrinket" && order.OrderString != "UseTrinket")
                return;

            if (self == null || self.IsDead || !self.IsInWorld)
                return;

            switch (order.OrderString)
            {
                case "DropTrinket":
                    DropTrinket(self);
                    break;

                case "UseTrinket":
                    if (!self.IsDead && self.IsInWorld)
                        EffectClick();
                    break;
            }
        }

        void ITick.Tick(Actor self)
        {
            if (!self.IsInWorld || self.IsDead)
                return;

            var newTrinket = self.World.FindActorsInCircle(self.CenterPosition, new WDist(125)).FirstOrDefault(a => a.Info.HasTraitInfo<IsTrinketInfo>());

            if (newTrinket == null && IgnoreTrinket != null)
                IgnoreTrinket = null;

            if (HoldsTrinket == null)
            {
                ExtraArmor = 0;
                ExtraDamage = 0;
                ExtraSpeed = 0;

                var manaShotoer = self.TraitOrDefault<ManaShooter>();
                if (manaShotoer != null)
                    manaShotoer.ExtraModifier = 100;
            }

            if (newTrinket == null)
            {
                if (HoldsTrinket != null)
                {
                    var trinketInfoOld = HoldsTrinket.Info.TraitInfoOrDefault<IsTrinketInfo>();
                    if (trinketInfoOld != null && trinketInfoOld.ContiniusEffect && !info.CannotUse.Contains(trinketInfoOld.TrinketType))
                        ContiniusEffect(trinketInfoOld);
                }

                return;
            }

            if (newTrinket.TraitOrDefault<IsTrinket>() != null && newTrinket.Info.TraitInfo<IsTrinketInfo>().EffectOnPickup)
            {
                EffectOnPickup(newTrinket.Info.TraitInfo<IsTrinketInfo>(), newTrinket);
                return;
            }

            if (newTrinket == IgnoreTrinket || HoldsTrinket != null)
            {
                return;
            }

            HoldsTrinket = newTrinket;
            self.World.Remove(newTrinket);

            var trinketInfo = HoldsTrinket.Info.TraitInfoOrDefault<IsTrinketInfo>();
            if (trinketInfo != null && trinketInfo.ContiniusEffect && !info.CannotUse.Contains(trinketInfo.TrinketType))
                ContiniusEffect(trinketInfo);
        }

        void EffectClick()
        {
            if (HoldsTrinket != null)
            {
                var trinketinfo = HoldsTrinket.Info.TraitInfoOrDefault<IsTrinketInfo>();
                if (trinketinfo != null && !info.CannotUse.Contains(trinketinfo.TrinketType))
                    switch (trinketinfo.TrinketType)
                    {
                        case "healsalve":

                            if (
                                !self.IsDead
                                && self.IsInWorld
                                && self.Info.TraitInfoOrDefault<HealthInfo>() != null
                                && self.TraitOrDefault<DungeonsAndDragonsStats>() != null)
                            {
                                self.InflictDamage(self, new Damage(-1 * (self.Trait<Health>().MaxHP - self.Trait<Health>().HP), new BitSet<DamageType>("Healing")));
                                var trinket = HoldsTrinket;
                                HoldsTrinket = null;
                                IgnoreTrinket = null;

                                self.World.AddFrameEndTask(w =>
                                    w.Add(new SpriteEffect(
                                        self.CenterPosition,
                                        w,
                                        trinket.Info.TraitInfo<RenderSpritesInfo>().Image,
                                        trinketinfo.EffectSequence,
                                        trinketinfo.EffectPalette)));

                                Game.Sound.Play(SoundType.World, trinketinfo.Sound, self.CenterPosition);

                                if (trinketinfo.OneTimeUse)
                                    trinket.Dispose();
                            }

                            break;

                        case "water":

                            if (
                                !self.IsDead
                                && self.IsInWorld
                                && self.Info.TraitInfoOrDefault<HealthInfo>() != null
                                && self.TraitOrDefault<DungeonsAndDragonsStats>() != null)
                            {
                                self.InflictDamage(self, new Damage(-1 * 10, new BitSet<DamageType>("Healing")));
                                var trinket = HoldsTrinket;
                                HoldsTrinket = null;
                                IgnoreTrinket = null;

                                self.World.AddFrameEndTask(w =>
                                    w.Add(new SpriteEffect(
                                        self.CenterPosition,
                                        w,
                                        trinket.Info.TraitInfo<RenderSpritesInfo>().Image,
                                        trinketinfo.EffectSequence,
                                        trinketinfo.EffectPalette)));

                                Game.Sound.Play(SoundType.World, trinketinfo.Sound, self.CenterPosition);

                                if (trinketinfo.OneTimeUse)
                                    trinket.Dispose();
                            }

                            break;

                        case "meat":

                            if (
                                !self.IsDead
                                && self.IsInWorld
                                && self.Info.TraitInfoOrDefault<HealthInfo>() != null
                                && self.TraitOrDefault<DungeonsAndDragonsStats>() != null)
                            {
                                self.InflictDamage(self, new Damage(-1 * 15, new BitSet<DamageType>("Healing")));
                                var trinket = HoldsTrinket;
                                HoldsTrinket = null;
                                IgnoreTrinket = null;

                                self.World.AddFrameEndTask(w =>
                                    w.Add(new SpriteEffect(
                                        self.CenterPosition,
                                        w,
                                        trinket.Info.TraitInfo<RenderSpritesInfo>().Image,
                                        trinketinfo.EffectSequence,
                                        trinketinfo.EffectPalette)));

                                Game.Sound.Play(SoundType.World, trinketinfo.Sound, self.CenterPosition);

                                if (trinketinfo.OneTimeUse)
                                    trinket.Dispose();
                            }

                            break;

                        case "kit":

                            var actors = self.World.FindActorsInCircle(self.CenterPosition, WDist.FromCells(4))
                                .Where(a =>
                                    !a.IsDead
                                    && a.IsInWorld
                                    && a.Owner.IsAlliedWith(self.Owner)
                                    && a.Info.TraitInfoOrDefault<BuildingInfo>() != null
                                    && a.TraitOrDefault<Health>() != null
                                    && a.Trait<Health>().HP < a.Trait<Health>().MaxHP);

                            Actor closest = null;
                            if (actors.Any())
                            {
                                closest = actors.First();

                                closest.InflictDamage(self, new Damage(-1 * 100, new BitSet<DamageType>("Healing")));

                                var trinket = HoldsTrinket;
                                HoldsTrinket = null;
                                IgnoreTrinket = null;

                                foreach (var cell in closest.Trait<Building>().OccupiedCells())
                                {
                                    self.World.AddFrameEndTask(w =>
                                        w.Add(new SpriteEffect(
                                            self.World.Map.CenterOfCell(cell.First),
                                            w,
                                            trinket.Info.TraitInfo<RenderSpritesInfo>().Image,
                                            trinketinfo.EffectSequence,
                                            trinketinfo.EffectPalette)));
                                }

                                Game.Sound.Play(SoundType.World, trinketinfo.Sound, self.CenterPosition);

                                if (trinketinfo.OneTimeUse)
                                    trinket.Dispose();
                            }

                            break;
                    }
            }
        }

        void EffectOnPickup(IsTrinketInfo trinketInfo, Actor trinket)
        {
            if (!info.CannotUse.Contains(trinketInfo.TrinketType))
            {
                switch (trinketInfo.TrinketType)
                {
                    case "manaorb":

                        self.Owner.PlayerActor.Trait<PlayerResources>().GiveCash(60);
                        Game.Sound.Play(SoundType.World, trinketInfo.Sound, self.CenterPosition);
                        if (trinketInfo.ShowEffect)
                            self.World.AddFrameEndTask(w =>
                                w.Add(new SpriteEffect(
                                    self.CenterPosition,
                                    w,
                                    trinket.Info.TraitInfo<RenderSpritesInfo>().Image,
                                    trinketInfo.EffectSequence,
                                    trinketInfo.EffectPalette)));
                        if (trinketInfo.OneTimeUse)
                            trinket.Dispose();
                        break;
                    case "thome":
                        self.Owner.PlayerActor.Trait<DungeonsAndDragonsExperience>().AddCash(150);
                        Game.Sound.Play(SoundType.World, trinketInfo.Sound, self.CenterPosition);
                        if (trinketInfo.ShowEffect)
                            self.World.AddFrameEndTask(w =>
                                w.Add(new SpriteEffect(
                                    self.CenterPosition,
                                    w,
                                    trinket.Info.TraitInfo<RenderSpritesInfo>().Image,
                                    trinketInfo.EffectSequence,
                                    trinketInfo.EffectPalette)));
                        if (trinketInfo.OneTimeUse)
                            trinket.Dispose();
                        break;
                    case "map":
                        var random = new CPos(self.World.SharedRandom.Next(1, self.World.Map.Bounds.Size.Width),
                            self.World.SharedRandom.Next(1, self.World.Map.Bounds.Size.Height));
                        var td = new TypeDictionary
                        {
                            new LocationInit(random),
                            new OwnerInit(self.Owner),
                            new FacingInit(255)
                        };
                        self.World.AddFrameEndTask(w =>
                            w.CreateActor("camera", td));
                        if (trinketInfo.OneTimeUse)
                            trinket.Dispose();
                        break;
                }
            }
        }

        void ContiniusEffect(IsTrinketInfo trinketInfo)
        {
            if (!info.CannotUse.Contains(trinketInfo.TrinketType))
                switch (trinketInfo.TrinketType)
                {
                    case "mantle":
                        ExtraArmor = 2;
                        break;
                    case "boots":
                        ExtraSpeed = 1;
                        break;
                    case "gauntlet":
                        ExtraDamage = 1;
                        break;
                    case "coat":
                        ExtraArmor = 1;
                        break;
                    case "locket":
                        var manaShotoer = self.TraitOrDefault<ManaShooter>();
                        if (manaShotoer != null)
                            manaShotoer.ExtraModifier = 66;
                        break;
                }
        }

        void INotifyKilled.Killed(Actor self, AttackInfo e)
        {
            DropTrinket(self);
        }
    }
}