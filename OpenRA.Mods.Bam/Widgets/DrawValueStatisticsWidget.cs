using System.Drawing;
using OpenRA.Mods.Bam.BamWidgets;
using OpenRA.Widgets;

namespace OpenRA.Mods.Bam.Widgets
{
    public class DrawValueStatisticsWidget : Widget
    {
        private ActorActionsWidget actorActions;

        public DrawValueStatisticsWidget(ActorActionsWidget actorActions)
        {
            this.actorActions = actorActions;
        }

        public override void Tick()
        {
            Bounds = new Rectangle(85, 183, 100, 100);
        }

        public override void Draw()
        {
        }
    }
}
