using System.Drawing;
using OpenRA.Graphics;
using OpenRA.Mods.Bam.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Widgets;

namespace OpenRA.Mods.Bam.BamWidgets
{
    public class DrawActorStatisticsWidget : Widget
    {
        private ActorActionsWidget actorActions;

        public DrawActorStatisticsWidget(ActorActionsWidget actorActions)
        {
            this.actorActions = actorActions;
        }

        public override void Tick()
        {
            Bounds = new Rectangle(4, 183, 75, 68);
        }

        public override void Draw()
        {
            if (actorActions.AllActor != null)
            {
                var palette = actorActions.BamUi.WorldRenderer.Palette("BamPlayer" + actorActions.AllActor.Owner.InternalName);
                var animation = new Animation(actorActions.BamUi.World, actorActions.AllActor.Trait<RenderSprites>().GetImage(actorActions.AllActor));
                animation.PlayFetchIndex("icon", () => 0);
                WidgetUtils.DrawSHPCentered(animation.Image, new float2(RenderBounds.X, RenderBounds.Y), palette);
            }
        }
    }
}