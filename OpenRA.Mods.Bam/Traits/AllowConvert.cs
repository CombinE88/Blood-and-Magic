using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits
{
    public class AllowConvertInfo : ITraitInfo
    {
        public readonly string[] ConvertTo;

        public object Create(ActorInitializer init)
        {
            return new AllowConvert(init);
        }
    }

    public class AllowConvert
    {
        public AllowConvert(ActorInitializer init)
        {
        }
    }
}