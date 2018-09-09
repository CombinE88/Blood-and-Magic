using System.Drawing;
using System.Linq;
using System.Threading;
using OpenRA.Graphics;
using OpenRA.Mods.Bam.Traits;
using OpenRA.Widgets;

namespace OpenRA.Mods.Bam.BamWidgets.Buttons
{
    public class ManaSendButtonWidget : Widget
    {
        private ActorActionsWidget actorActions;
        private Sprite background;
        private Sprite backgroundDown;
        private bool pressed;

        public ManaSendButtonWidget(ActorActionsWidget actorActions)
        {
            this.actorActions = actorActions;
            background = new Sprite(actorActions.BamUi.Sheet, new Rectangle(152, 818, 180, 35), TextureChannel.RGBA);
            backgroundDown = new Sprite(actorActions.BamUi.Sheet, new Rectangle(152, 853, 180, 35), TextureChannel.RGBA);
        }

        public override void Tick()
        {
            Bounds = new Rectangle(0, 68, background.Bounds.Width, background.Bounds.Height);
        }

        public override bool HandleMouseInput(MouseInput mi)
        {
            if (!EventBounds.Contains(mi.Location))
                return false;

            if (mi.Button != MouseButton.Left)
                return true;

            if (mi.Event == MouseInputEvent.Down)
            {
                var actors = actorActions.ActorGroup.Where(a => a.Info.HasTraitInfo<ManaShooterInfo>()).ToArray();

                foreach (var actor in actors)
                    actor.Trait<ManaShooter>().ShootMana();

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
            var currentMana = actorActions.Actor.Trait<ManaShooter>().CurrentStorage;
            var maxMana = actorActions.Actor.Info.TraitInfo<ManaShooterInfo>().MaxStorage;
            var progress = 128 * currentMana / maxMana;

            WidgetUtils.DrawRGBA(pressed ? backgroundDown : background, new float2(RenderBounds.X, RenderBounds.Y));
            WidgetUtils.FillRectWithColor(new Rectangle(RenderBounds.X + 26, RenderBounds.Y + 18, progress, 10), Color.RoyalBlue);
            actorActions.BamUi.Font.DrawText("Transferer", new float2(RenderBounds.X + 10, RenderBounds.Y + 4), Color.Chocolate);
        }
    }
}