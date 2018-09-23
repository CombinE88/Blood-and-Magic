using OpenRA.Mods.Bam.Traits.Activities;
using OpenRA.Mods.Bam.Traits.TrinketLogics;
using OpenRA.Mods.Common.Effects;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits.UnitAbilities
{
    public class AbortConvertInfo : ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new AbortConvert(init);
        }
    }

    public class AbortConvert : IResolveOrder
    {
        public AbortConvert(ActorInitializer init)
        {
        }

        void IResolveOrder.ResolveOrder(Actor self, Order order)
        {
            if (order.OrderString != "AbortConvert")
                return;

            AbortTransformation(self);
        }

        public void AbortTransformation(Actor self)
        {
            self.World.AddFrameEndTask(w =>
                w.Add(new SpriteEffect(
                    self.CenterPosition,
                    w,
                    self.Info.TraitInfo<RenderSpritesInfo>().Image,
                    "transform_reverse",
                    self.Info.TraitInfo<RenderSpritesInfo>().PlayerPalette + self.Owner.InternalName)));

            self.CancelActivity();
            self.QueueActivity(new AdvancedTransform(self, "acolyte")
            {
                Trinket = self.Info.HasTraitInfo<CanHoldTrinketInfo>() ? self.Trait<CanHoldTrinket>().HoldsTrinket : null
            });
            Game.Sound.Play(SoundType.World, "7284.wav", self.CenterPosition);
        }
    }
}