using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits.UnitAbilities
{
    public class TerminateInfo : ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new Terminate(init);
        }
    }

    public class Terminate : IResolveOrder
    {
        public Terminate(ActorInitializer init)
        {
        }

        void IResolveOrder.ResolveOrder(Actor self, Order order)
        {
            if (order.OrderString != "Terminate")
                return;

            self.CancelActivity();
            self.Kill(self, new BitSet<DamageType>("Death"));
        }
    }
}