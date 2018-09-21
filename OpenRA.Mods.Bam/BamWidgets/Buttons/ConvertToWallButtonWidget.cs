using System.Drawing;
using OpenRA.Graphics;
using OpenRA.Mods.Bam.Traits;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Widgets;

namespace OpenRA.Mods.Bam.BamWidgets.Buttons
{
    public class ConvertToWallButtonWidget : Widget
    {
        private ActorActionsWidget actorActions;
        private bool pressed;
        private Animation animation;
        private int posx;
        private int posy;
        private BamToolTipWidget tooltip;
        private ConvertAdjetantInfo convadjInfo;

        public ConvertToWallButtonWidget(ActorActionsWidget actorActions, int posx, int posy)
        {
            this.actorActions = actorActions;
            this.posx = posx;
            this.posy = posy;

            AddChild(tooltip = new BamToolTipWidget(
                this.actorActions,
                "Wall",
                20) { Visible = false });
        }

        public override void MouseEntered()
        {
            tooltip.Visible = true;
        }

        public override void Tick()
        {
            if (actorActions.Actor == null || actorActions.Actor.TraitOrDefault<ConvertAdjetant>() == null)
            {
                tooltip.Visible = false;
                return;
            }

            convadjInfo = actorActions.Actor.Info.TraitInfoOrDefault<ConvertAdjetantInfo>();

            if (actorActions.Actor != null && convadjInfo != null)
                animation = new Animation(actorActions.BamUi.World,
                    actorActions.BamUi.World.Map.Rules.Actors[convadjInfo.WallActor].TraitInfo<RenderSpritesInfo>().Image);

            var x = pressed ? posx + 1 : posx;
            var y = pressed ? posy + 1 : posy;
            Bounds = new Rectangle(x, y, 75, 68);
        }

        public override bool HandleMouseInput(MouseInput mi)
        {
            if (!EventBounds.Contains(mi.Location))
            {
                tooltip.Visible = false;
                return false;
            }

            tooltip.Visible = true;

            if (mi.Button != MouseButton.Left)
                return true;
            var pr = actorActions.BamUi.World.LocalPlayer.PlayerActor.Trait<PlayerResources>();
            if (actorActions.Actor != null && mi.Event == MouseInputEvent.Down &&
                pr.Cash + pr.Resources >= actorActions.BamUi.World.Map.Rules.Actors[convadjInfo.WallActor].TraitInfo<ValuedInfo>().Cost)
            {
                actorActions.Actor.World.IssueOrder(new Order("QuickWall", actorActions.Actor, false));
                pressed = true;
            }
            else if (mi.Event == MouseInputEvent.Up)
                pressed = false;

            return true;
        }

        public override void MouseExited()
        {
            pressed = false;
            tooltip.Visible = false;
        }

        public override void Draw()
        {
            var info = actorActions.Actor.Info.TraitInfoOrDefault<ConvertAdjetantInfo>();

            if (actorActions.Actor == null || info == null)
                return;

            if (animation != null)
            {
                animation.PlayFetchIndex(!actorActions.Actor.Trait<ConvertAdjetant>().Disabled ? "icon" : "disabled-icon", () => 0);
                WidgetUtils.DrawSHPCentered(animation.Image, new float2(RenderBounds.X, RenderBounds.Y), actorActions.BamUi.Palette);

                var text = actorActions.BamUi.World.Map.Rules.Actors[convadjInfo.WallActor].TraitInfo<ValuedInfo>().Cost.ToString();
                actorActions.BamUi.FontLarge.DrawTextWithShadow(text,
                    new float2(RenderBounds.X + 4,
                        RenderBounds.Y + RenderBounds.Height - actorActions.BamUi.FontLarge.Measure(text).Y - 2),
                    Color.CornflowerBlue, Color.DarkBlue, 2);
            }
        }
    }
}