using OpenRA.Mods.Bam.Activities;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits.Transform
{
    public class TransformToUnitInfo : ITraitInfo
    {
        public readonly string CapsuleActor = "capsule";

        public object Create(ActorInitializer init)
        {
            return new TransformToUnit(this);
        }
    }

    public class TransformToUnit : IResolveOrder
    {
        private TransformToUnitInfo info;
        public string IntoActor; // TODO find nicer way

        public TransformToUnit(TransformToUnitInfo info)
        {
            this.info = info;
        }

        void IResolveOrder.ResolveOrder(Actor self, Order order)
        {
            if (order.OrderString != "TransformToUnit")
                return;

            if (!self.Owner.PlayerActor.Trait<PlayerResources>().TakeCash(Game.ModData.DefaultRules.Actors[IntoActor].TraitInfo<ValuedInfo>().Cost))
                return;


            var rsi = self.Info.TraitInfo<RenderSpritesInfo>();

            self.QueueActivity(new AdvancedTransform(info.CapsuleActor, AdvancedTransformEffect.FADE, actor =>
            {
                var transformOnIdle = actor.Trait<TransformOnIdle>();
                transformOnIdle.Delay = Game.ModData.DefaultRules.Actors[IntoActor].TraitInfo<BuildableInfo>().BuildDuration;
                transformOnIdle.IntoActor = IntoActor;
                transformOnIdle.Effect = AdvancedTransformEffect.FADE;
            }));
        }
    }
}
