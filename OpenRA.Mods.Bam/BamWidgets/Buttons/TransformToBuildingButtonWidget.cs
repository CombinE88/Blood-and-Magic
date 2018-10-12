using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.Bam.Traits;
using OpenRA.Mods.Common.Traits;
using OpenRA.Widgets;

namespace OpenRA.Mods.Bam.BamWidgets.Buttons
{
    public class TransformToBuildingButtonWidget : Widget
    {
        private ActorActionsWidget actorActions;
        private bool pressed;
        private Animation animation;
        public List<Actor> SelectedValidActors = new List<Actor>();
        private int posx;
        private int posy;
        private string animationString;
        private bool visibleText;

        public TransformToBuildingButtonWidget(ActorActionsWidget actorActions, int posx, int posy, string animationString, HashSet<Actor> selectedValidActors)
        {
            this.actorActions = actorActions;
            this.posx = posx;
            this.posy = posy;
            this.animationString = animationString;
            this.SelectedValidActors = selectedValidActors.ToList();
        }

        public override void Tick()
        {
            animation = new Animation(actorActions.BamUi.World, animationString);

            if (actorActions.Actor == null)
                return;

            var x = pressed ? posx + 1 : posx;
            var y = pressed ? posy + 1 : posy;
            Bounds = new Rectangle(x, y, 75, 68);
        }

        public override void MouseEntered()
        {
            visibleText = true;
        }

        public override bool HandleMouseInput(MouseInput mi)
        {
            if (!EventBounds.Contains(mi.Location))
                return false;

            if (mi.Button != MouseButton.Left)
                return true;

            if (mi.Event == MouseInputEvent.Down)
            {
                Visible = false;
                actorActions.RemoveTransformmenu();

                actorActions.BamUi.World.IssueOrder(new Order("TransformTo-" + animationString, SelectedValidActors.Last(), false));

                for (int i = 0; i < 3; i++)
                {
                    actorActions.BamUi.World.IssueOrder(new Order("RemoveSelf", SelectedValidActors[i], false));
                }

                pressed = true;
            }

            if (mi.Event == MouseInputEvent.Up)
            {
                pressed = false;
            }

            return true;
        }

        public override void MouseExited()
        {
            pressed = false;
            visibleText = false;
        }

        public override void Draw()
        {
            if (actorActions.Actor == null)
                return;

            if (animation != null)
            {
                animation.PlayFetchIndex("icon", () => 0);
                WidgetUtils.DrawSHPCentered(animation.Image, new float2(RenderBounds.X, RenderBounds.Y), actorActions.BamUi.Palette);
            }

            if (visibleText)
            {
                var text = "Enables: ";
                foreach (var varString in actorActions.BamUi.World.Map.Rules.Actors[animationString].TraitInfo<AllowConvertInfo>().ConvertTo)
                {
                    text = text + actorActions.BamUi.World.Map.Rules.Actors[varString].TraitInfo<TooltipInfo>().Name + ", ";
                }

                actorActions.BamUi.Font.DrawTextWithContrast(text,
                    new float2(RenderBounds.X - actorActions.BamUi.Font.Measure(text).X - 1,
                        RenderBounds.Y + RenderBounds.Height - actorActions.BamUi.Font.Measure(text).Y - 1), Color.White,
                    Color.DarkBlue, 1);
            }
        }
    }
}