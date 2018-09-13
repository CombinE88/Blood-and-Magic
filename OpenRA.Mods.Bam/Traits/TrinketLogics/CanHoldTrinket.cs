using System.Linq;
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
        public object Create(ActorInitializer init)
        {
            return new CanHoldTrinket(init, this);
        }
    }

    public class CanHoldTrinket : IResolveOrder, ITick
    {
        public Actor HoldsTrinket;
        private Actor ignoreTrinket;
        private Actor self;

        public CanHoldTrinket(ActorInitializer init, CanHoldTrinketInfo info)
        {
            self = init.Self;
        }

        void DropTrinket(Actor self)
        {
            if (HoldsTrinket == null)
                return;

            var trinketInfo = HoldsTrinket.Info.Name;

            self.World.AddFrameEndTask(w =>
            {
                ignoreTrinket = w.CreateActor(trinketInfo, new TypeDictionary
                {
                    new LocationInit(self.World.Map.CellContaining(self.CenterPosition)),
                    new CenterPositionInit(self.CenterPosition),
                    new OwnerInit("Neutral"),
                    new FacingInit(255)
                });
            });

            HoldsTrinket.Dispose();
            HoldsTrinket = null;
        }

        public void ResolveOrder(Actor self, Order order)
        {
            if (order.OrderString != "dropItem" && order.OrderString != "UseTrinket")
                return;

            switch (order.OrderString)
            {
                case "DropItem":
                    DropTrinket(self);
                    break;
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

            if (newTrinket == null && ignoreTrinket != null)
                ignoreTrinket = null;

            if (newTrinket == null || HoldsTrinket != null || newTrinket == ignoreTrinket)
                return;

            HoldsTrinket = newTrinket;
            self.World.Remove(newTrinket);
            if (HoldsTrinket.Info.TraitInfo<IsTrinketInfo>().EffectOnPickup)
                EffectOnPickup();

            if (HoldsTrinket.Info.TraitInfo<IsTrinketInfo>().ContiniusEffect)
                ContiniusEffect();
        }


        public void EffectClick()
        {
            var trinketinfo = HoldsTrinket.Info.TraitInfo<IsTrinketInfo>();
            switch (trinketinfo.TrinketType)
            {
                case "healsalve":

                    if (!self.IsDead && self.IsInWorld && self.Info.HasTraitInfo<HealthInfo>())
                    {
                        self.InflictDamage(self, new Damage(-1 * (self.Trait<Health>().MaxHP - self.Trait<Health>().HP)));
                        var trinket = HoldsTrinket;
                        HoldsTrinket = null;
                        ignoreTrinket = null;

                        self.World.AddFrameEndTask(w =>
                            w.Add(new SpriteEffect(
                                self.CenterPosition,
                                w,
                                trinket.Info.TraitInfo<RenderSpritesInfo>().Image,
                                trinketinfo.EffectSequence,
                                trinketinfo.EffectPalette)));

                        if (trinketinfo.OneTimeUse)
                            trinket.Dispose();
                    }

                    break;
                case "masonmix":

                    break;
            }
        }

        public void EffectOnPickup()
        {
            var trinketinfo = HoldsTrinket.Info.TraitInfo<IsTrinketInfo>();
            switch (trinketinfo.TrinketType)
            {
                case "manaorb":

                    self.Owner.PlayerActor.Trait<PlayerResources>().GiveCash(60);
                    var trinket = HoldsTrinket;
                    HoldsTrinket = null;
                    ignoreTrinket = null;
                    trinket.Dispose();
                    break;
            }
        }

        public void ContiniusEffect()
        {
            var trinketinfo = HoldsTrinket.Info.TraitInfo<IsTrinketInfo>();
            switch (trinketinfo.TrinketType)
            {
                case "mightmantle":

                    self.Trait<DungeonsAndDragonsStats>().ModifiedArmor += 1;
                    break;
            }
        }
    }
}