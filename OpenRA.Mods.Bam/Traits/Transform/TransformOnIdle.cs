using OpenRA.Mods.Bam.Activities;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits.Transform
{
    public class TransformOnIdleInfo : ITraitInfo
    {
        public readonly int Delay = 75;

        public readonly string IntoActor = null;

        public object Create(ActorInitializer init)
        {
            return new TransformOnIdle(init, this);
        }
    }

    public class TransformOnIdle : ITick
    {
        public int Delay;
        public string IntoActor;
        public AdvancedTransformEffect Effect = AdvancedTransformEffect.TRANSFORM;
        public int Ticker { get; private set; }

        public TransformOnIdle(ActorInitializer init, TransformOnIdleInfo info)
        {
            Delay = info.Delay;
            IntoActor = info.IntoActor;
        }

        void ITick.Tick(Actor self)
        {
            if (self.IsIdle)
            {
                Ticker++;

                if (Ticker >= Delay)
                    self.QueueActivity(new AdvancedTransform(IntoActor, Effect));
            }
            else
                Ticker = 0;
        }
    }
}
