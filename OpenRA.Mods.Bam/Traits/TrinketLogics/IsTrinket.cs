using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits.TrinketLogics
{
    public class IsTrinketInfo : ITraitInfo
    {
        public readonly bool EffectOnPickup = false;

        public readonly bool OneTimeUse = true;

        public readonly string TrinketType = "";

        public readonly int DropChance = 50;

        public readonly string EffectSequence = "effect";

        public readonly string EffectPalette = "bam11195";

        public object Create(ActorInitializer init)
        {
            return new IsTrinket();
        }
    }

    public class IsTrinket
    {
        public IsTrinket()
        {
        }

    }
}