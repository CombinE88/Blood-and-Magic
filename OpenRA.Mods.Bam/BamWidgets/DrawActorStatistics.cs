using System.Drawing;
using OpenRA.Graphics;
using OpenRA.Mods.Bam.Traits;
using OpenRA.Mods.Bam.Traits.TrinketLogics;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Widgets;

namespace OpenRA.Mods.Bam.BamWidgets
{
    public class DrawActorStatisticsWidget : Widget
    {
        private ActorActionsWidget actorActions;
        private bool show = false;
        [Translate] private string text = "";

        public DrawActorStatisticsWidget(ActorActionsWidget actorActions)
        {
            this.actorActions = actorActions;
        }

        public override void MouseEntered()
        {
            show = true;
        }

        public override void MouseExited()
        {
            show = false;
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

                if (!show)
                    return;

                var traitInfo = actorActions.AllActor.Info.TraitInfoOrDefault<IsTrinketInfo>();
                if (traitInfo != null)
                {
                    text = traitInfo.Description.Replace("\\n", "\n");
                    var textSize = actorActions.BamUi.Font.Measure(text);

                    actorActions.BamUi.Font.DrawTextWithShadow(text,
                        new float2(RenderBounds.X + -textSize.X - 35, RenderBounds.Y + RenderBounds.Height - textSize.Y - 2),
                        Color.White, Color.Black, 2);
                }
            }
        }
    }
}