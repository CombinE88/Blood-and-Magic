using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits.TrinketLogics
{
    public class IsTrinketInfo : ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new IsTrinket();
        }
    }

    public class IsTrinket
    {
    }
}