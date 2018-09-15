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
        public List<Actor> selectedValidActors = new List<Actor>();
        private int posx;
        private int posy;
        private string animationString;
        private TransformToBuilding transformToBuilding;

        public TransformToBuildingButtonWidget(ActorActionsWidget actorActions, int posx, int posy, string animationString,
            TransformToBuilding transformToBuilding, HashSet<Actor> selectedValidActors)
        {
            this.actorActions = actorActions;
            this.posx = posx;
            this.posy = posy;
            this.animationString = animationString;
            this.transformToBuilding = transformToBuilding;
            this.selectedValidActors = selectedValidActors.ToList();
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

        public override bool HandleMouseInput(MouseInput mi)
        {
            if (!EventBounds.Contains(mi.Location) || selectedValidActors.Count < 4)
                return false;

            if (mi.Button != MouseButton.Left)
                return true;

            if (mi.Event == MouseInputEvent.Down)
            {
                for (int i = 0; i < selectedValidActors.Count - 1; i++)
                {
                    selectedValidActors[i].World.IssueOrder(new Order("RemoveSelf", selectedValidActors[i], false));
                }

                selectedValidActors[selectedValidActors.Count].World.IssueOrder(new Order("TransformTo", actorActions.Actor, false));

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