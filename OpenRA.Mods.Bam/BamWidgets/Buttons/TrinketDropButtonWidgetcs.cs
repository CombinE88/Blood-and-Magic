using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.Bam.Traits;
using OpenRA.Mods.Bam.Traits.TrinketLogics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Widgets;

namespace OpenRA.Mods.Bam.BamWidgets.Buttons
{
    public class TrinketDropButtonWidget : Widget
    {
        private ActorActionsWidget actorActions;
        private bool pressed;
        private Sprite background;
        private Sprite backgroundDown;

        public TrinketDropButtonWidget(ActorActionsWidget actorActions)
        {
            this.actorActions = actorActions;

            background = new Sprite(actorActions.BamUi.Sheet, new Rectangle(948, 91, 76, 18), TextureChannel.RGBA);
            backgroundDown = new Sprite(actorActions.BamUi.Sheet, new Rectangle(948, 109, 76, 18), TextureChannel.RGBA);
        }

        public override void Tick()
        {
            if (actorActions.Actor == null)
                return;

            Bounds = new Rectangle(0, 403, background.Bounds.Width, background.Bounds.Height);
        }

        public override bool HandleMouseInput(MouseInput mi)
        {
            if (mi.Button != MouseButton.Left)
                return true;

            if (mi.Event == MouseInputEvent.Down && actorActions.Actor.IsIdle)
            {
                actorActions.Actor.World.IssueOrder(new Order("dropItem", actorActions.Actor, false));
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

            WidgetUtils.DrawRGBA(pressed ? backgroundDown : background, new float2(RenderBounds.X, RenderBounds.Y));
            actorActions.BamUi.Font.DrawTextWithShadow("Drop", new float2(RenderBounds.X + 10, RenderBounds.Y + 4), Color.YellowGreen, Color.DarkSlateGray, 1);
        }
    }
}