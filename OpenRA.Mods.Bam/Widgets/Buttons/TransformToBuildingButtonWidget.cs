using System;
using System.Drawing;
using OpenRA.Graphics;
using OpenRA.Mods.Bam.BamWidgets;
using OpenRA.Mods.Bam.Traits.Transform;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Widgets;

namespace OpenRA.Mods.Bam.Widgets.Buttons
{
    public class TransformToBuildingButtonWidget : Widget
    {
        private ActorActionsWidget actorActions;
        private bool pressed;
        public String TransformInto = "barracks";

        public TransformToBuildingButtonWidget(ActorActionsWidget actorActions)
        {
            this.actorActions = actorActions;
        }

        public override void Tick()
        {
            Bounds = new Rectangle(pressed ? 11 : 10, pressed ? 450 + 1 : 450, 75, 68);
        }

        public override bool HandleMouseInput(MouseInput mi)
        {
            if (!EventBounds.Contains(mi.Location) || mi.Button != MouseButton.Left)
                return false;

            if (mi.Event == MouseInputEvent.Down)
            {
                actorActions.Actor.Trait<TransformToBuilding>().IntoActor = TransformInto;
                actorActions.Actor.World.IssueOrder(new Order("TransformToBuilding", actorActions.Actor, false));
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
            var actorInfo = actorActions.BamUi.World.Map.Rules.Actors[TransformInto];

            var animation = new Animation(actorActions.BamUi.World, actorInfo.TraitInfo<RenderSpritesInfo>().GetImage
            (
                actorInfo,
                actorActions.BamUi.World.Map.Rules.Sequences,
                actorActions.Actor.Owner.Faction.Name
            ));

            animation.PlayFetchIndex("icon", () => 0);
            WidgetUtils.DrawSHPCentered(animation.Image, new float2(RenderBounds.X, RenderBounds.Y), actorActions.BamUi.Palette);
        }
    }
}
