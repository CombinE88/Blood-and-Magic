using System.Drawing;
using OpenRA.Graphics;
using OpenRA.Mods.Bam.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Widgets;

namespace OpenRA.Mods.Bam.BamWidgets.Buttons
{
    public class SpawnGolemWidget : Widget
    {
        private ActorActionsWidget actorActions;
        private bool pressed;
        private Animation animation;
        private ActorInfo actorInfo;

        public SpawnGolemWidget(ActorActionsWidget actorActions)
        {
            this.actorActions = actorActions;
        }

        public override void Tick()
        {
            if (actorActions.Actor == null)
                return;

            string actorString = null;
            if (actorActions.Actor.Info.HasTraitInfo<SpawnsAcolytesInfo>())
                actorString = actorActions.Actor.Info.TraitInfo<SpawnsAcolytesInfo>().Actor;

            actorInfo = actorActions.BamUi.World.Map.Rules.Actors[actorString];

            if (actorInfo != null && actorInfo.HasTraitInfo<RenderSpritesInfo>())
                animation = new Animation(actorActions.BamUi.World,
                    actorInfo.TraitInfo<RenderSpritesInfo>().GetImage(actorInfo, actorActions.BamUi.World.Map.Rules.Sequences, actorActions.Actor.Owner.Faction.Name));

            var x = pressed ? 11 : 10;
            var y = pressed ? 450 + 1 : 450;
            Bounds = new Rectangle(x, y, 75, 68);
        }

        public override bool HandleMouseInput(MouseInput mi)
        {
            if (!EventBounds.Contains(mi.Location))
                return false;

            if (mi.Button != MouseButton.Left)
                return true;

            if (mi.Event == MouseInputEvent.Down)
            {
                actorActions.Actor.World.IssueOrder(new Order("SpawnAcolyte", actorActions.Actor, false));
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
            if (actorActions.Actor == null)
                return;

            if (animation != null)
            {
                animation.PlayFetchIndex("icon", () => 0);
                WidgetUtils.DrawSHPCentered(animation.Image, new float2(RenderBounds.X, RenderBounds.Y), actorActions.BamUi.Palette);

                var text = "40";
                actorActions.BamUi.FontLarge.DrawTextWithShadow(text,
                    new float2(RenderBounds.X + 4,
                        RenderBounds.Y + RenderBounds.Height - actorActions.BamUi.FontLarge.Measure(text).Y - 2),
                    Color.CornflowerBlue, Color.DarkBlue, 2);
            }
        }
    }
}