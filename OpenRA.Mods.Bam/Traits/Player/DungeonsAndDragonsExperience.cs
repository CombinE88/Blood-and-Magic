using OpenRA.Mods.Bam.Traits.World;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits.Player
{
    public class DungeonsAndDragonsExperienceInfo : ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new DungeonsAndDragonsExperience();
        }
    }

    public class DungeonsAndDragonsExperience : ISync, IResolveOrder
    {
        [Sync] public int Experience;

        void TakeCash(int num)
        {
            Experience -= num;
        }

        public void AddCash(int num)
        {
            Experience = Experience + num;
        }

        void IResolveOrder.ResolveOrder(Actor self, Order order)
        {
            if (!order.OrderString.Contains("ExpRemove-"))
                return;

            var researchItem = order.OrderString.Replace("ExpRemove-", "");

            TakeCash(self.Owner.PlayerActor.Info.TraitInfo<ResearchInfo>().Researchable[researchItem]);
        }
    }
}