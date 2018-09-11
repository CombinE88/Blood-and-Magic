using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits.Trinkets
{
    public class TrinketInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new Trinket(); }
    }

    public class Trinket {}
}
