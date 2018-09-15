using OpenRA.Mods.Bam.Activities;
using OpenRA.Mods.Bam.Traits.TrinketLogics;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Effects;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits
{
    public class TransformAfterTimeInfo : ITraitInfo
    {
        public readonly string Notification = "UnitReady";

        [Desc("Offset to spawn the transformed actor relative to the current cell.")]
        public readonly CVec Offset = CVec.Zero;

        [Desc("Facing that the actor must face before transforming.")]
        public readonly int Facing = 96;

        [Desc("Sounds to play when transforming.")]
        public readonly string[] TransformSounds = { };

        [Desc("Notification to play when transforming.")]
        public readonly string TransformNotification = null;

        public readonly bool NotifyBuildComplete = true;

        public object Create(ActorInitializer init)
        {
            return new TransformAfterTime(init, this);
        }
    }

    public class TransformAfterTime : ITick
    {
        private TransformAfterTimeInfo info;

        public bool Transforming = false;
        public string IntoActor;
        public int Time;

        public int Ticker;

        public TransformAfterTime(ActorInitializer init, TransformAfterTimeInfo info)
        {
            this.info = info;
        }

        void ITick.Tick(Actor self)
        {
            if (Transforming && Ticker++ >= Time)
            {
                self.World.AddFrameEndTask(w =>
                    w.Add(new SpriteEffect(
                        self.CenterPosition,
                        w,
                        self.Info.TraitInfo<RenderSpritesInfo>().Image,
                        "transform_reverse",
                        self.Info.TraitInfo<RenderSpritesInfo>().PlayerPalette + self.Owner.InternalName)));

                self.QueueActivity(new AdvancedTransform(self, IntoActor)
                {
                    Offset = info.Offset,
                    Facing = info.Facing,
                    Sounds = info.TransformSounds,
                    Notification = info.TransformNotification,
                    NotifyBuildComplete = info.NotifyBuildComplete,
                    Trinket = self.Info.HasTraitInfo<CanHoldTrinketInfo>() ? self.Trait<CanHoldTrinket>().HoldsTrinket : null
                });

                var player = self.World.LocalPlayer;
                Game.Sound.PlayNotification(self.World.Map.Rules, player, "Speech", info.Notification, self.Owner.Faction.InternalName);

                Transforming = false;
            }
        }
    }
}