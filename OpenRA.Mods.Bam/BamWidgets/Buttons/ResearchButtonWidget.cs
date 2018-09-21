using System.Drawing;
using OpenRA.Graphics;
using OpenRA.Mods.Bam.Traits;
using OpenRA.Mods.Bam.Traits.PlayerTraits;
using OpenRA.Mods.Bam.Traits.World;
using OpenRA.Mods.Common.Traits;
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
        private Research research;


        private int poxMov;
        public bool Removing;
        private int waitTicks;
        private int undoTicks;
        private int posXMaxMov;

        public ResearchButtonWidget(
            ShowResearchButtonWidget showResearch,
            int posx,
            int posy,
            string researchItem,
            int researchTime,
            int researchCost,
            Research research,
            int waitTicks,
            int outPos)
        {
            this.showResearch = showResearch;
            this.posx = posx;
            this.posy = posy;
            this.researchItem = researchItem;
            this.researchTime = researchTime;
            this.researchCost = researchCost;
            this.research = research;

            this.waitTicks = waitTicks;
            undoTicks = waitTicks;
            poxMov = outPos;
            posXMaxMov = outPos;

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
            var playertrait = research.Researchable;
            if (playertrait != null && !playertrait.Contains(researchItem))
                disabled = false;
            else
                disabled = true;

            animation = new Animation(showResearch.ActorActions.BamUi.World, researchItem);

            HandlePosition();

            var x = pressed ? posx + poxMov + 1 : posx + poxMov;
            var y = pressed ? posy + 1 : posy;

            Bounds = new Rectangle(x, y, 75, 68);
        }

        void HandlePosition()
        {
            if (!Removing && waitTicks > 0)
            {
                waitTicks--;
                return;
            }

            if (!Removing && poxMov > 0)
                poxMov -= 30;

            if (!Removing)
            {
                return;
            }

            if (waitTicks < undoTicks)
            {
                waitTicks++;
                return;
            }

            if (poxMov < posXMaxMov)
            {
                poxMov += 30;
            }
        }

        public override void MouseEntered()
        {
            tooltip.Visible = true;
        }

        public override bool HandleMouseInput(MouseInput mi)
        {
            if (!EventBounds.Contains(mi.Location))
            {
                tooltip.Visible = true;
                return false;
            }

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
                    showResearch.SwitchResearchMenue();
                    pressed = true;
                }
                else if (research.Researchable.Contains(researchItem) || showResearch.Researching)
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