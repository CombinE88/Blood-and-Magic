using System.Drawing;
using OpenRA.Graphics;
using OpenRA.Mods.Bam.Traits;
using OpenRA.Mods.Bam.Traits.RPGTraits;
using OpenRA.Mods.Bam.Traits.World;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Traits;
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
        private BamToolTipWidget tooltip;

        public ConvertToButtonWidget(ActorActionsWidget actorActions, int posx, int posy, string animationString, string actorString)
        {
            this.actorActions = actorActions;
            this.posx = posx;
            this.posy = posy;
            this.animationString = animationString;
            this.actorString = actorString;

            AddChild(tooltip = new BamToolTipWidget(
                this.actorActions,
                actorActions.BamUi.World.Map.Rules.Actors[this.actorString].TraitInfo<TooltipInfo>().Name,
                actorActions.BamUi.World.Map.Rules.Actors[this.actorString].TraitInfo<ValuedInfo>().Cost,
                0,
                0,
                actorActions.BamUi.World.Map.Rules.Actors[this.actorString].TraitInfo<DungeonsAndDragonsStatsInfo>().Damage,
                actorActions.BamUi.World.Map.Rules.Actors[this.actorString].TraitInfo<DungeonsAndDragonsStatsInfo>().Armor,
                actorActions.BamUi.World.Map.Rules.Actors[this.actorString].TraitInfo<DungeonsAndDragonsStatsInfo>().Speed,
                false,
                true) { Visible = false });
        }

        public override void Tick()
        {
            if (actorActions.Actor == null || actorActions.Actor.Trait<ConvertAdjetant>() == null)
            {
                tooltip.Visible = false;
                return;
            }

            var playertraits = actorActions.BamUi.World.LocalPlayer.PlayerActor.TraitsImplementing<Research>();
            var contains = false;
            foreach (var trait in playertraits)
            {
                if (trait.Researchable.Contains(actorString))
                {
                    contains = true;
                    break;
                }
            }

            disabled = !contains;

            actorString = null;
            if (actorActions.Actor.Info.HasTraitInfo<ConvertAdjetantInfo>() && actorActions.Actor.TraitOrDefault<ConvertAdjetant>().TransformEnabler != null)
                actorString = animationString;

            actorInfo = actorActions.BamUi.World.Map.Rules.Actors[actorString];
            if (actorInfo != null && actorInfo.HasTraitInfo<RenderSpritesInfo>())
                animation = new Animation(actorActions.BamUi.World,
                    actorInfo.TraitInfo<RenderSpritesInfo>().GetImage(actorInfo, actorActions.BamUi.World.Map.Rules.Sequences, actorActions.Actor.Owner.Faction.Name));

            var x = pressed ? posx + 1 : posx;
            var y = pressed ? posy + 1 : posy;
            Bounds = new Rectangle(x, y, 75, 68);
        }

        public override bool HandleMouseInput(MouseInput mi)
        {
            if (!EventBounds.Contains(mi.Location))
            {
                tooltip.Visible = false;
                return false;
            }

            tooltip.Visible = true;
            if (disabled)
                return false;
            if (mi.Button != MouseButton.Left)
                return true;
            var pr = actorActions.BamUi.World.LocalPlayer.PlayerActor.Trait<PlayerResources>();
            if (actorActions.Actor != null && mi.Event == MouseInputEvent.Down &&
                pr.Cash + pr.Resources >= actorActions.BamUi.World.Map.Rules.Actors[actorString].TraitInfo<ValuedInfo>().Cost)
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
            tooltip.Visible = false;
        }

        public override void Draw()
        {
            if (actorActions.Actor == null)
                return;
            if (animation != null)
            {
                animation.PlayFetchIndex(disabled ? "disabled-icon" : "icon", () => 0);
                WidgetUtils.DrawSHPCentered(animation.Image, new float2(RenderBounds.X, RenderBounds.Y), actorActions.BamUi.Palette);

                var text = actorActions.BamUi.World.Map.Rules.Actors[actorString].TraitInfo<ValuedInfo>().Cost.ToString();
                actorActions.BamUi.FontLarge.DrawTextWithShadow(text,
                    new float2(RenderBounds.X + 4,
                        RenderBounds.Y + RenderBounds.Height - actorActions.BamUi.FontLarge.Measure(text).Y - 2),
                    Color.CornflowerBlue, Color.DarkBlue, 2);
            }
        }
    }
}