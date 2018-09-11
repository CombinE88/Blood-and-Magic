using System.Drawing;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.Bam.BamWidgets;
using OpenRA.Mods.Bam.Traits.Mana;
using OpenRA.Widgets;

namespace OpenRA.Mods.Bam.Widgets.Buttons
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
            Bounds = new Rectangle(0, 264, background.Bounds.Width, background.Bounds.Height);
        }

        public override bool HandleMouseInput(MouseInput mi)
        {
            if (!EventBounds.Contains(mi.Location))
                return false;

            if (mi.Button != MouseButton.Left)
                return true;

            if (mi.Event == MouseInputEvent.Down)
            {
                var actors = actorActions.ActorGroup.Where(a => a.Info.HasTraitInfo<ManaGeneratorInfo>()).ToArray();

                foreach (var actor in actors)
                    actor.World.IssueOrder(new Order("ShootMana", actor, false));

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
            var manaStorage = actorActions.Actor.Trait<ManaStorage>();
            var progress = 128 * manaStorage.Current / manaStorage.Capacity;

            WidgetUtils.DrawRGBA(pressed ? backgroundDown : background, new float2(RenderBounds.X, RenderBounds.Y));
            WidgetUtils.FillRectWithColor(new Rectangle(RenderBounds.X + 26, RenderBounds.Y + 18, progress, 10), Color.RoyalBlue);

            var text = "Transferer";
            actorActions.BamUi.Font.DrawTextWithContrast(text,
                new float2(RenderBounds.X + RenderBounds.Width / 4 - actorActions.BamUi.Font.Measure(text).X / 2, RenderBounds.Y + 2), Color.White, Color.Black, 1);

            var text2 = "Mana: " + manaStorage.Current;
            actorActions.BamUi.Font.DrawTextWithContrast(text2,
                new float2(RenderBounds.X + RenderBounds.Width / 2 - actorActions.BamUi.Font.Measure(text2).X / 2,
                    RenderBounds.Y + 5 + RenderBounds.Height / 2 - actorActions.BamUi.Font.Measure(text2).Y / 2), Color.White,
                Color.DarkBlue, 1);
        }
    }
}
