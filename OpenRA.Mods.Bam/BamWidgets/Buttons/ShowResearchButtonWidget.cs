using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.Bam.Traits.PlayerTraits;
using OpenRA.Mods.Bam.Traits.World;
using OpenRA.Widgets;

namespace OpenRA.Mods.Bam.BamWidgets.Buttons
{
    public class ShowResearchButtonWidget : Widget
    {
        public ActorActionsWidget ActorActions;
        private bool pressed;

        public Research Research;
        public bool Researching = false;
        public int CurrentResearchTime = 0;
        public int MaxResearchTime;
        public string ResearchItem;

        private List<ResearchButtonWidget> researchButtons = new List<ResearchButtonWidget>();

        public ShowResearchButtonWidget(ActorActionsWidget actorActions)
        {
            ActorActions = actorActions;

            var resProperty = ActorActions.BamUi.World.LocalPlayer.PlayerActor.TraitsImplementing<Research>().ToArray();
            var faction = ActorActions.BamUi.World.RenderPlayer.Faction.InternalName;
            var setfaction = resProperty.Where(r => r.Info.Faction == faction);
            Research = setfaction.First();
        }

        public override bool HandleMouseInput(MouseInput mi)
        {
            if (!EventBounds.Contains(mi.Location))
                return false;

            if (mi.Button != MouseButton.Left)
                return true;

            if (mi.Event == MouseInputEvent.Down)
            {
                if (!researchButtons.Any())
                    CreateResearchMenu();
                else if (!Researching)
                    SwitchResearchMenue();

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
                if (Research != null && CurrentResearchTime++ >= MaxResearchTime)
                {
                    var list = Research.Researchable;
                    list.Add(ResearchItem);
                    Researching = false;
                    ResearchItem = "";
                    CurrentResearchTime = 0;
                    Game.Sound.PlayNotification(
                        ActorActions.BamUi.World.Map.Rules,
                        ActorActions.BamUi.World.LocalPlayer,
                        "Speech",
                        "ResearchComplete",
                        ActorActions.BamUi.World.LocalPlayer.Faction.InternalName);
                }
            }

            Bounds = new Rectangle(5, 451, 118, 18);
        }

        public override void Draw()
        {
            var animation = new Animation(ActorActions.BamUi.World, "basic_ui");

            animation.PlayFetchIndex(pressed ? "resbutton_pressed" : "resbutton", () => 0);
            WidgetUtils.DrawSHPCentered(animation.Image, new float2(RenderBounds.X, RenderBounds.Y), ActorActions.BamUi.Palette);

            var text = "Research";
            ActorActions.BamUi.Font.DrawTextWithShadow(text,
                new float2(RenderBounds.X + RenderBounds.Width / 2 - ActorActions.BamUi.Font.Measure(text).X / 2,
                    RenderBounds.Y + RenderBounds.Height / 2 - ActorActions.BamUi.Font.Measure(text).Y / 2), Color.White, Color.DarkSlateGray, 1);

            if (Researching)
            {
                var progress = Math.Min(6 * CurrentResearchTime / MaxResearchTime, 5);

                animation.PlayFetchIndex("ui_research_bar", () => progress);
                WidgetUtils.DrawSHPCentered(animation.Image, new float2(RenderBounds.X, RenderBounds.Y + 20), ActorActions.BamUi.Palette);

                // Game.AddChatLine(Color.White, currentResearchTime + "", "" + progress);
            }
        }

        public void RemoveSingleChild(Widget child)
        {
            RemoveChild(child);
        }

        public void CreateResearchMenu()
        {
            if (Research == null)
                return;

            List<string> alreadyRes = new List<string>();
            foreach (var dict in Research.Info.PreResearched)
            {
                alreadyRes.Add(dict);
            }

            var list = Research.Info.Researchable.ToList();

            if ((55 + ActorActions.Bounds.Y + Bounds.Y + list.Count/3 * 68) < Game.Renderer.Resolution.Height)
                for (int i = 0; i < list.Count; i++)
                {
                    var con = new ResearchButtonWidget(
                        this,
                        35 + i % 3 * 75 - 100,
                        55 + 68 * (i / 3),
                        list[i].Key,
                        list[i].Value * Research.Info.TimePerCost,
                        list[i].Value,
                        Research,
                        i * 2,
                        300)
                    {
                        Visible = true
                    };
                    researchButtons.Add(con);
                }
            else
                for (int i = 0; i < list.Count; i++)
                {
                    var con = new ResearchButtonWidget(
                        this,
                        35 + i % 4 * 75 - 200,
                        55 + 68 * (i / 4),
                        list[i].Key,
                        list[i].Value * Research.Info.TimePerCost,
                        list[i].Value,
                        Research,
                        i * 2,
                        300)
                    {
                        Visible = true
                    };
                    researchButtons.Add(con);
                }

            foreach (var button in researchButtons)
            {
                AddChild(button);
            }
        }

        public void SwitchResearchMenue()
        {
            if (researchButtons.Any())
            {
                foreach (var button in researchButtons)
                {
                    button.Removing = !button.Removing;
                }
            }
        }
    }
}