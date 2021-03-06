using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.Bam.Traits;
using OpenRA.Mods.Bam.Traits.Player;
using OpenRA.Mods.Bam.Traits.World;
using OpenRA.Mods.Common.Traits;
using OpenRA.Widgets;

namespace OpenRA.Mods.Bam.BamWidgets.Buttons
{
    public class ShowResearchButtonWidget : Widget
    {
        public ActorActionsWidget ActorActions;
        private bool pressed;
        public bool ShowResearch;

        public Research research;
        public bool Researching = false;
        public int currentResearchTime = 0;
        public int MaxResearchTime;
        public string ResearchItem;


        private List<ResearchButtonWidget> ResearchButtons = new List<ResearchButtonWidget>();

        public ShowResearchButtonWidget(ActorActionsWidget actorActions)
        {
            ActorActions = actorActions;

            var resProperty = ActorActions.BamUi.World.LocalPlayer.PlayerActor.TraitsImplementing<Research>().ToArray();
            var faction = ActorActions.BamUi.World.RenderPlayer.Faction.InternalName;
            var setfaction = resProperty.Where(r => r.Info.Faction == faction);
            research = setfaction.FirstOrDefault();
        }

        public override bool HandleMouseInput(MouseInput mi)
        {
            if (!EventBounds.Contains(mi.Location))
                return false;

            if (mi.Button != MouseButton.Left)
                return true;

            if (mi.Event == MouseInputEvent.Down)
            {
                ShowResearch = !ShowResearch;

                if (ShowResearch && !ResearchButtons.Any())
                    CreateResearchMenu();
                else if (!ShowResearch && ResearchButtons.Any())
                    RemoveResearchMenu();

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

        public override void Tick()
        {
            if (Researching)
            {
                if (research != null && currentResearchTime++ >= MaxResearchTime)
                {
                    var list = research.Researchable;
                    list.Add(ResearchItem);
                    Researching = false;
                    ResearchItem = "";
                    currentResearchTime = 0;
                    Game.Sound.PlayNotification(
                        ActorActions.BamUi.World.Map.Rules,
                        ActorActions.BamUi.World.LocalPlayer,
                        "Speech",
                        "ResearchComplete",
                        ActorActions.BamUi.World.LocalPlayer.Faction.InternalName
                    );
                }
            }

            Bounds = new Rectangle(85, 426, 76, 24);
        }

        public override void Draw()
        {
            //120.0-1 76x24
            var animation = new Animation(ActorActions.BamUi.World, "basic_ui");

            animation.PlayFetchIndex(pressed ? "resbutton_pressed" : "resbutton", () => 0);
            WidgetUtils.DrawSHPCentered(animation.Image, new float2(RenderBounds.X, RenderBounds.Y), ActorActions.BamUi.Palette);

            animation.PlayFetchIndex("resbutton_pressed", () => 0);
            WidgetUtils.DrawSHPCentered(animation.Image, new float2(RenderBounds.X - 76, RenderBounds.Y), ActorActions.BamUi.Palette);

            var text = "Research";
            ActorActions.BamUi.Font.DrawTextWithShadow(text,
                new float2(RenderBounds.X + RenderBounds.Width / 2 - ActorActions.BamUi.Font.Measure(text).X / 2,
                    RenderBounds.Y + RenderBounds.Height / 2 - ActorActions.BamUi.Font.Measure(text).Y / 2), Color.White, Color.DarkSlateGray, 1);

            var text2 = ActorActions.BamUi.World.LocalPlayer.PlayerActor.Info.HasTraitInfo<DungeonsAndDragonsExperienceInfo>()
                ? ActorActions.BamUi.World.LocalPlayer.PlayerActor.Trait<DungeonsAndDragonsExperience>().Experience.ToString()
                : "0";

            ActorActions.BamUi.Font.DrawTextWithShadow(text2,
                new float2(RenderBounds.X - 76 + RenderBounds.Width / 2 - ActorActions.BamUi.Font.Measure(text2).X / 2,
                    RenderBounds.Y + RenderBounds.Height / 2 - ActorActions.BamUi.Font.Measure(text2).Y / 2), Color.White, Color.DarkSlateGray, 1);

            if (Researching)
            {
                var progress = Math.Min(6 * currentResearchTime / MaxResearchTime, 5);

                animation.PlayFetchIndex("ui_research_bar", () => progress);
                WidgetUtils.DrawSHPCentered(animation.Image, new float2(RenderBounds.X - 76 + 20, RenderBounds.Y + 316), ActorActions.BamUi.Palette);

                //Game.AddChatLine(Color.White, currentResearchTime + "", "" + progress);
            }
        }

        public void CreateResearchMenu()
        {
            if (research == null)
                return;

            List<string> alreadyRes = new List<string>();
            foreach (var dict in research.Info.PreResearched)
            {
                alreadyRes.Add(dict);
            }

            var list = research.Info.Researchable.ToList();

            for (int i = 0; i < list.Count; i++)
            {
                var con = new ResearchButtonWidget
                (
                    this,
                    -76 + i % 2 * 75, 24 + 68 * (i / 2),
                    list[i].Key,
                    list[i].Value * research.Info.TimePerCost,
                    list[i].Value
                );
                ResearchButtons.Add(con);
            }

            foreach (var button in ResearchButtons)
            {
                AddChild(button);
            }
        }

        public void RemoveResearchMenu()
        {
            if (ResearchButtons.Any())
                foreach (var button in ResearchButtons)
                {
                    RemoveChild(button);
                    button.Removed();
                }

            ResearchButtons = new List<ResearchButtonWidget>();
        }
    }
}