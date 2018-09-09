using OpenRA.Mods.Bam.Activities;
using OpenRA.Mods.Bam.Traits.TrinketLogics;
using OpenRA.Mods.Common.Activities;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits
{
    public class TransformAfterTimeInfo : ITraitInfo
    {
        public readonly int Time = 200;

        public readonly string IntoActor = null;

        [Desc("Offset to spawn the transformed actor relative to the current cell.")]
        public readonly CVec Offset = CVec.Zero;

        [Desc("Facing that the actor must face before transforming.")]
        public readonly int Facing = 96;

        [Desc("Sounds to play when transforming.")]
        public readonly string[] TransformSounds = { };

        [Desc("Notification to play when transforming.")]
        public readonly string TransformNotification = null;

        public object Create(ActorInitializer init)
        {
            return new TransformAfterTime(init, this);
        }
    }

    public class TransformAfterTime : ITick
    {
        private TransformAfterTimeInfo info;

        private bool transforming = false;

        public int Ticker;

        public TransformAfterTime(ActorInitializer init, TransformAfterTimeInfo info)
        {
            this.info = info;
        }

        void ITick.Tick(Actor self)
        {
            if (!transforming && Ticker++ >= info.Time)
            {
                transforming = true;
                self.QueueActivity(new AdvancedTransform(self, info.IntoActor)
                {
                    Offset = info.Offset,
                    Facing = info.Facing,
                    Sounds = info.TransformSounds,
                    Notification = info.TransformNotification,
                    Trinket = self.Info.HasTraitInfo<CanHoldTrinketInfo>() ? self.Trait<CanHoldTrinket>().HoldsTrinket : null
                });
            }
        }
    }
}