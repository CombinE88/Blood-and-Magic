using System.Drawing;
using System.Linq;
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
        public Actor Actor;

        public SpawnGolemWidget(ActorActionsWidget actorActions)
        {
            this.actorActions = actorActions;
        }

        public override void Tick()
        {
            if (Actor == null)
                return;

            string actorString = null;
            if (Actor.Info.HasTraitInfo<SpawnsAcolytesInfo>())
                actorString = Actor.Info.TraitInfo<SpawnsAcolytesInfo>().Actor;

            actorInfo = actorActions.BamUi.World.Map.Rules.Actors[actorString];

            if (actorInfo != null && actorInfo.HasTraitInfo<RenderSpritesInfo>())
                animation = new Animation(actorActions.BamUi.World,
                    actorInfo.TraitInfo<RenderSpritesInfo>().GetImage(actorInfo, actorActions.BamUi.World.Map.Rules.Sequences, Actor.Owner.Faction.Name));

            var x = pressed ? 1 + 100 : 100;
            var y = pressed ? 377 + 1 : 377;
            Bounds = new Rectangle(x, y, 75, 68);
        }

        public override bool HandleMouseInput(MouseInput mi)
        {
            if (Actor == null)
                return false;

            if (!EventBounds.Contains(mi.Location))
                return false;

            if (mi.Button != MouseButton.Left)
                return true;

            if (mi.Event == MouseInputEvent.Down)
            {
                Actor.World.IssueOrder(new Order("SpawnAcolyte", Actor, false));
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
            if (Actor == null)
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