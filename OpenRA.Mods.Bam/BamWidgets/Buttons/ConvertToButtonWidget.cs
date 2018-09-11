using System.Drawing;
using OpenRA.Graphics;
using OpenRA.Mods.Bam.Traits;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Widgets;

namespace OpenRA.Mods.Bam.BamWidgets.Buttons
{
    public class ConvertToButtonWidget : Widget
    {
        private ActorActionsWidget actorActions;
        private bool pressed;
        private Animation animation;
        private ActorInfo actorInfo;
        private bool disabled;
        private int posx;
        private int posy;
        private string animationString;
        private string actorString;


        public ConvertToButtonWidget(ActorActionsWidget actorActions, int posx, int posy, bool disabled, string animationString, string actorString)
        {
            this.actorActions = actorActions;
            this.posx = posx;
            this.posy = posy;
            this.disabled = disabled;
            this.animationString = animationString;
            this.actorString = actorString;
        }

        public override void Tick()
        {
            if (actorActions.Actor == null)
                return;

            string actorString = null;
            if (actorActions.Actor.Info.HasTraitInfo<ConvertAdjetantInfo>() && actorActions.Actor.TraitOrDefault<ConvertAdjetant>().TransformEnabler != null)
                actorString = animationString;

            actorInfo = actorActions.BamUi.World.Map.Rules.Actors[actorString];

            if (actorInfo != null && actorInfo.HasTraitInfo<RenderSpritesInfo>())
                animation = new Animation(actorActions.BamUi.World, actorInfo.TraitInfo<RenderSpritesInfo>().GetImage
                (
                    actorInfo,
                    actorActions.BamUi.World.Map.Rules.Sequences,
                    actorActions.Actor.Owner.Faction.Name
                ));

            var x = pressed ? posx + 1 : posx;
            var y = pressed ? posy + 1 : posy;
            Bounds = new Rectangle(x, y, 75, 68);
        }

        public override bool HandleMouseInput(MouseInput mi)
        {
            if (!EventBounds.Contains(mi.Location) || !disabled)
                return false;

            if (mi.Button != MouseButton.Left)
                return true;

            if (mi.Event == MouseInputEvent.Down)
            {
                actorActions.Actor.World.IssueOrder(new Order("Convert-" + actorString, actorActions.Actor, false));
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
                animation.PlayFetchIndex(!disabled ? "disabled-icon" : "icon", () => 0);
                WidgetUtils.DrawSHPCentered(animation.Image, new float2(RenderBounds.X, RenderBounds.Y), actorActions.BamUi.Palette);
            }
        }
    }
}