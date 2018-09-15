using System.Collections.Generic;
using System.Drawing;
using OpenRA.Graphics;
using OpenRA.Mods.Bam.Traits;
using OpenRA.Mods.Bam.Traits.Player;
using OpenRA.Mods.Bam.Traits.World;
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
        private bool disabled = true;
        private int posx;
        private int posy;
        private string researchItem;
        private int researchTime;
        private int researchCost;
        private BamToolTipWidget tooltip;

        public ResearchButtonWidget(ShowResearchButtonWidget showResearch, int posx, int posy, string researchItem, int researchTime, int researchCost)
        {
            this.showResearch = showResearch;
            this.posx = posx;
            this.posy = posy;
            this.researchItem = researchItem;
            this.researchTime = researchTime;
            this.researchCost = researchCost;

            AddChild(tooltip = new BamToolTipWidget(
                this.showResearch.ActorActions,
                this.showResearch.ActorActions.BamUi.World.Map.Rules.Actors[researchItem].TraitInfo<TooltipInfo>().Name,
                0,
                researchCost,
                this.researchTime,
                0,
                0,
                0,
                true) { Visible = false });
        }

        public override void Tick()
        {
            var playertrait = showResearch.ActorActions.BamUi.World.RenderPlayer.PlayerActor.TraitOrDefault<Research>().Researchable;
            if (playertrait != null && !playertrait.Contains(researchItem))
                disabled = false;
            else
                disabled = true;

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
                if (!showResearch.Researching && showResearch.ActorActions.BamUi.World.RenderPlayer.PlayerActor.Trait<DungeonsAndDragonsExperience>().Experience >= researchCost)
                {
                    showResearch.ActorActions.BamUi.World.IssueOrder(new Order("ExpRemove-" + researchItem, showResearch.ActorActions.BamUi.World.LocalPlayer.PlayerActor, false));
                    pressed = true;

                    showResearch.ResearchItem = researchItem;
                    showResearch.MaxResearchTime = researchTime;
                    showResearch.Researching = true;
                    showResearch.RemoveResearchMenu();
                    showResearch.ShowResearch = false;
                    pressed = true;
                }
                else if (showResearch.ActorActions.BamUi.World.RenderPlayer.PlayerActor.Trait<Research>().Researchable.Contains(researchItem) || showResearch.Researching)
                {
                    Game.Sound.PlayNotification(
                        showResearch.ActorActions.BamUi.World.Map.Rules,
                        showResearch.ActorActions.BamUi.World.LocalPlayer,
                        "Speech",
                        "AlreadyResearched",
                        showResearch.ActorActions.BamUi.World.LocalPlayer.Faction.InternalName);
                }
                else if (showResearch.ActorActions.BamUi.World.RenderPlayer.PlayerActor.Trait<DungeonsAndDragonsExperience>().Experience < researchCost)
                {
                    Game.Sound.PlayNotification(
                        showResearch.ActorActions.BamUi.World.Map.Rules,
                        showResearch.ActorActions.BamUi.World.LocalPlayer,
                        "Speech",
                        "LowExp",
                        showResearch.ActorActions.BamUi.World.LocalPlayer.Faction.InternalName);
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