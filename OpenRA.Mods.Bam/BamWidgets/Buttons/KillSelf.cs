using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.Bam.Traits;
using OpenRA.Mods.Bam.Traits.TrinketLogics;
using OpenRA.Mods.Bam.Traits.UnitAbilities;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Widgets;

namespace OpenRA.Mods.Bam.BamWidgets.Buttons
{
    public class KillSelfWidget : Widget
    {
        private ActorActionsWidget actorActions;
        private bool pressed;
        private Animation animation;

        public KillSelfWidget(ActorActionsWidget actorActions)
        {
            this.actorActions = actorActions;
            animation = new Animation(actorActions.BamUi.World, "basic_ui");
        }

        public override void Tick()
        {
            var x = 0 - 58 - 30;
            var y = 0;
            Bounds = new Rectangle(x, y, 58, 30);
        }

        public override bool HandleMouseInput(MouseInput mi)
        {
            if (mi.Button != MouseButton.Left || actorActions.Actor == null)
                return true;

            if (mi.Event == MouseInputEvent.Down)
            {
                actorActions.Actor.World.IssueOrder(new Order("Terminate", actorActions.Actor, false));
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
            if (actorActions.Actor == null || actorActions.Actor.TraitOrDefault<Terminate>() == null)
                return;

            if (animation != null)
            {
                animation.PlayFetchIndex(pressed ? "ui_terminate_pressed" : "ui_terminate", () => 0);
                WidgetUtils.DrawSHPCentered(animation.Image, new float2(RenderBounds.X, RenderBounds.Y), actorActions.BamUi.Palette);
            }
        }
    }
}