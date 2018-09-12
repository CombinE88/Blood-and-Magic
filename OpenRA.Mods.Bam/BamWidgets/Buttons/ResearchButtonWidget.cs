using System.Collections.Generic;
using System.Drawing;
using OpenRA.Graphics;
using OpenRA.Mods.Bam.Traits;
using OpenRA.Mods.Bam.Traits.Player;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Widgets;

namespace OpenRA.Mods.Bam.BamWidgets.Buttons
{
    public class ResearchButtonWidget : Widget
    {
        private ShowResearchButtonWidget showResearch;
        private bool pressed;
        private Animation animation;
        private bool disabled;
        private int posx;
        private int posy;
        private string researchItem;
        private int researchTime;
        private int ResearchCost;
        private BamToolTipWidget tooltip;

        public ResearchButtonWidget(ShowResearchButtonWidget showResearch, int posx, int posy, string researchItem, bool disabled, int ResearchTime, int ResearchCost)
        {
            this.showResearch = showResearch;
            this.posx = posx;
            this.posy = posy;
            this.researchItem = researchItem;
            this.disabled = disabled;
            researchTime = ResearchTime;
            this.ResearchCost = ResearchCost;

            AddChild(tooltip = new BamToolTipWidget
            (
                this.showResearch.ActorActions,
                this.showResearch.ActorActions.BamUi.World.Map.Rules.Actors[researchItem].TraitInfo<TooltipInfo>().Name,
                0,
                ResearchCost,
                researchTime,
                0,
                0,
                0,
                true,
                false
            ) {Visible = false});
        }

        public override void Tick()
        {
            animation = new Animation(showResearch.ActorActions.BamUi.World, researchItem);

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

            if (mi.Event == MouseInputEvent.Down)
            {
                if (!showResearch.Researching && showResearch.ActorActions.BamUi.World.RenderPlayer.PlayerActor.Trait<DungeonsAndDragonsExperience>().TakeCash(ResearchCost))
                {
                    showResearch.ResearchItem = researchItem;
                    showResearch.MaxResearchTime = researchTime;
                    showResearch.Researching = true;
                    showResearch.RemoveResearchMenu();
                    showResearch.ShowResearch = false;
                    pressed = true;
                }
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
            if (animation != null)
            {
                animation.PlayFetchIndex(disabled ? "disabled-icon" : "icon", () => 0);
                WidgetUtils.DrawSHPCentered(animation.Image, new float2(RenderBounds.X, RenderBounds.Y), showResearch.ActorActions.BamUi.Palette);
            }
        }
    }
}