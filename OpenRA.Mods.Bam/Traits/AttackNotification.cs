using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits
{
    public class AttackNotificationInfo : ITraitInfo
    {
        [Desc("The audio notification type to play.")]
        [FieldLoader.RequireAttribute] public string Notifications = "";

        public bool RadarPings = true;

        public object Create(ActorInitializer init) { return new AttackNotification(); }
    }

    public class AttackNotification { }
}
