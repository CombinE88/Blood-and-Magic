using System.Linq;
using OpenRA.Mods.Bam.Traits.Player;
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

    public class CanHoldTrinket : IResolveOrder, ITick, INotifyKilled, INotifyCreated
    {
        public Actor HoldsTrinket;
        public Actor IgnoreTrinket;
        private Actor self;
        public int ExtraDamage;
        public int ExtraArmor;
        public int ExtraSpeed;
        private CanHoldTrinketInfo info;
        private WithSpriteBody wsb;

        public CanHoldTrinket(ActorInitializer init, CanHoldTrinketInfo info)
        {
            self = init.Self;
            this.info = info;
        }

        void INotifyCreated.Created(Actor self)
        {
            wsb = self.TraitsImplementing<WithSpriteBody>().First(w => w.Info.Name == info.Body);
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

            if (position != null)
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
                    {
                        var wsb = this.self.Trait<WithSpriteBody>();
                        if (wsb.DefaultAnimation.CurrentSequence.Name == "idle"
                            || wsb.DefaultAnimation.CurrentSequence.Name == "stand"
                            || wsb.DefaultAnimation.CurrentSequence.Name == "run"
                            || wsb.DefaultAnimation.CurrentSequence.Name == "aim")
                        {
                            DropTrinket(self);
                        }

                        break;
                    }
                    case "UseTrinket":
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
            }


            if (newTrinket == null)
            {
                return;
            }

            if (newTrinket.Trait<IsTrinket>() != null && newTrinket.Info.TraitInfo<IsTrinketInfo>().EffectOnPickup)
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

            if (trinketInfo != null && trinketInfo.ContiniusEffect)
                ContiniusEffect(trinketInfo);
        }

        void EffectClick()
        {
            var trinketinfo = HoldsTrinket.Info.TraitInfo<IsTrinketInfo>();
            if (!info.CannotUse.Contains(trinketinfo.TrinketType))
                switch (trinketinfo.TrinketType)
                {
                    case "healsalve":

                        if (
                            !self.IsDead
                            && self.IsInWorld
                            && self.Info.TraitInfoOrDefault<HealthInfo>() != null
                            && self.TraitOrDefault<DungeonsAndDragonsStats>() != null)
                        {
                            self.InflictDamage(self, new Damage(-1 * (self.Trait<Health>().MaxHP - self.Trait<Health>().HP)));
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
                            self.InflictDamage(self, new Damage(-1 * 10));
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
                }
        }

        void INotifyKilled.Killed(Actor self, AttackInfo e)
        {
            DropTrinket(self);
        }
    }
}