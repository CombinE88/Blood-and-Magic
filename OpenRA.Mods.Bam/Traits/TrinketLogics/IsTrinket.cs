using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits.TrinketLogics
{
    public class IsTrinketInfo : ITraitInfo
    {
        public readonly bool EffectOnPickup = false;

        public readonly bool OneTimeUse = true;

        public readonly bool ContiniusEffect = false;

        public readonly string TrinketType = "";

        public readonly int DropChance = 50;

        public readonly string EffectSequence = "effect";

        public readonly bool ShowEffect = true;

        public readonly string EffectPalette = "bam11195";

        public readonly string Sound = null;

        [Desc("Text shown in the production tooltip.")]
        [Translate] public readonly string Description = "";

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