using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits
{
    public class AllowTransfromInfo : ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new AllowTransfrom(init, this);
        }
    }

    public class AllowTransfrom
    {
        public AllowTransfrom(ActorInitializer init, AllowTransfromInfo info)
        {
        }
    }
}