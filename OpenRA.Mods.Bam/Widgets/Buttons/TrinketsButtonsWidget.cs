using System.Drawing;
using OpenRA.Graphics;
using OpenRA.Mods.Bam.Traits.Trinkets;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Widgets;

namespace OpenRA.Mods.Bam.BamWidgets.Buttons
{
    public class TrinketButtonsWidget : Widget
    {
        private ActorActionsWidget actorActions;
        private bool pressed;
        private Animation animation;
        private ActorInfo actorInfo;

        public TrinketButtonsWidget(ActorActionsWidget actorActions)
        {
            this.actorActions = actorActions;
        }

        public override void Tick()
        {
            if (actorActions.Actor == null)
                return;

            string actorString = null;
            if (actorActions.Actor.Trait<CanHoldTrinket>().Current != null)
                actorString = actorActions.Actor.Trait<CanHoldTrinket>().Current.Info.Name;

            actorInfo = actorActions.BamUi.World.Map.Rules.Actors[actorString];

            if (actorInfo != null && actorInfo.HasTraitInfo<RenderSpritesInfo>())
                animation = new Animation(actorActions.BamUi.World, actorInfo.TraitInfo<RenderSpritesInfo>().GetImage
                (
                    actorInfo,
                    actorActions.BamUi.World.Map.Rules.Sequences,
                    actorActions.Actor.Owner.Faction.Name
                ));

            var x = pressed ? 1 : 0;
            var y = pressed ? 352 + 1 : 352;
            Bounds = new Rectangle(x, y, 76, 51);
        }

        public override bool HandleMouseInput(MouseInput mi)
        {
            if (mi.Button != MouseButton.Left)
                return true;

            if (mi.Event == MouseInputEvent.Down)
            {
                actorActions.Actor.World.IssueOrder(new Order("UseTrinket", actorActions.Actor, false));
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
        }

        public override void Draw()
        {
            if (actorActions.Actor == null)
                return;

            if (animation != null)
            {
                animation.PlayFetchIndex("icon", () => 0);
                WidgetUtils.DrawSHPCentered(animation.Image, new float2(RenderBounds.X, RenderBounds.Y + 17), actorActions.BamUi.Palette);
            }
        }
    }
}
