using System.Drawing;
using OpenRA.Graphics;
using OpenRA.Mods.Bam.BamWidgets;
using OpenRA.Mods.Bam.Traits.UnitAbilities;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Widgets;

namespace OpenRA.Mods.Common.Warheads
{
    public class SecondAbilityButtonWidget : Widget
    {
        private ActorActionsWidget actorActions;
        private string text;
        private Animation anim;
        private bool pressed;

        public SecondAbilityButtonWidget(ActorActionsWidget actorActions)
        {
            this.actorActions = actorActions;
            anim = new Animation(actorActions.BamUi.World, "basic_ui");
            text = "Arbort transformation";
        }

        public override void Tick()
        {
            Bounds = new Rectangle(0, 338, 180, 34);
        }

        public override bool HandleMouseInput(MouseInput mi)
        {
            if (!EventBounds.Contains(mi.Location))
                return false;

            if (mi.Button != MouseButton.Left)
                return true;

            if (mi.Event == MouseInputEvent.Down)
            {
                actorActions.Actor.World.IssueOrder(new Order("AbortConvert", actorActions.Actor, false));
                pressed = true;
            }
            else if (mi.Event == MouseInputEvent.Up)
                pressed = false;

            return true;
        }

        public override void MouseExited()
        {
            pressed = false;
        }

        public override void Draw()
        {
            anim.PlayFetchIndex(pressed ? "ui_convertbackground_pressed" : "ui_convertbackground", () => 0);
            WidgetUtils.DrawSHPCentered(anim.Image, new float2(RenderBounds.X, RenderBounds.Y), actorActions.BamUi.Palette);

            actorActions.BamUi.Font.DrawTextWithShadow(text, new float2(RenderBounds.X + RenderBounds.Width / 4 - actorActions.BamUi.Font.Measure(text).X / 2, RenderBounds.Y - 2),
                Color.White, Color.Gray, 1);
        }
    }
}