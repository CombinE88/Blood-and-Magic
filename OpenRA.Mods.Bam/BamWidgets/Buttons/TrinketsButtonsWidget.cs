using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.Bam.Traits;
using OpenRA.Mods.Bam.Traits.TrinketLogics;
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
            if (actorActions.Actor.Trait<CanHoldTrinket>().HoldsTrinket != null)
                actorString = actorActions.Actor.Trait<CanHoldTrinket>().HoldsTrinket.Info.Name;

            actorInfo = actorActions.BamUi.World.Map.Rules.Actors[actorString];

            if (actorInfo != null && actorInfo.HasTraitInfo<RenderSpritesInfo>())
                animation = new Animation(actorActions.BamUi.World, actorInfo.TraitInfo<RenderSpritesInfo>().GetImage
                (
                    actorInfo,
                    actorActions.BamUi.World.Map.Rules.Sequences,
                    actorActions.Actor.Owner.Faction.Name
                ));

            var x = pressed ? 1 : 0;
            var y = pressed ? 369 + 1 : 369;
            Bounds = new Rectangle(x, y, 76, 51);
        }

        public override bool HandleMouseInput(MouseInput mi)
        {
            if (mi.Button != MouseButton.Left)
                return true;

            if (mi.Event == MouseInputEvent.Down)
            {
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