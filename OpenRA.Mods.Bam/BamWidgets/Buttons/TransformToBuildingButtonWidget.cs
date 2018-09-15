using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.Bam.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Widgets;

namespace OpenRA.Mods.Bam.BamWidgets.Buttons
{
    public class TransformToBuildingButtonWidget : Widget
    {
        private ActorActionsWidget actorActions;
        private bool pressed;
        private Animation animation;
        private ActorInfo actorInfo;
        public HashSet<Actor> SelectedValidActors = new HashSet<Actor>();

        public TransformToBuildingButtonWidget(ActorActionsWidget actorActions)
        {
            this.actorActions = actorActions;
        }

        public override void Tick()
        {
            if (actorActions.Actor == null)
                return;

            string actorString = null;
            if (actorActions.Actor.Trait<TransformToBuilding>().StandsOnBuilding)
                actorString = actorActions.Actor.Info.TraitInfo<TransformToBuildingInfo>().IntoBuilding;

            actorInfo = actorActions.BamUi.World.Map.Rules.Actors[actorString];

            var seq = actorActions.BamUi.World.Map.Rules.Sequences;
            var name = actorActions.Actor.Owner.Faction.Name;

            if (actorInfo != null && actorInfo.HasTraitInfo<RenderSpritesInfo>())
                animation = new Animation(actorActions.BamUi.World, actorInfo.TraitInfo<RenderSpritesInfo>().GetImage(actorInfo, seq, name));

            var x = pressed ? 11 : 10;
            var y = pressed ? 450 + 1 : 450;
            Bounds = new Rectangle(x, y, 75, 68);
        }

        public override bool HandleMouseInput(MouseInput mi)
        {
            if (!EventBounds.Contains(mi.Location) || SelectedValidActors.Count < 4)
                return false;

            if (mi.Button != MouseButton.Left)
                return true;

            if (mi.Event == MouseInputEvent.Down)
            {
                SelectedValidActors.First().World.IssueOrder(new Order("TransformTo", actorActions.Actor, false));
                foreach (var actor in SelectedValidActors)
                {
                    actor.World.IssueOrder(new Order("RemoveSelf", actor, false));
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
        }
    }
}