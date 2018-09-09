using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits
{
    public class AllowConvertInfo : ITraitInfo
    {
        public readonly string[] ConvertTo = {"capsule.warrior"};

        public object Create(ActorInitializer init)
        {
            return new AllowConvert(init, this);
        }
    }

    public class AllowConvert
    {
        public AllowConvert(ActorInitializer init, AllowConvertInfo info)
        {
        }
    }
}