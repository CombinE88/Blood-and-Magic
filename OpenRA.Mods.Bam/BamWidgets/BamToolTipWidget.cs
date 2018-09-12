using System.Drawing;
using OpenRA.Graphics;
using OpenRA.Widgets;

namespace OpenRA.Mods.Bam.BamWidgets
{
    public class BamToolTipWidget : Widget
    {
        private ActorActionsWidget actorActions;
        private string name;
        private int cost;
        private int researcost;
        private int researchtime;
        private int attack;
        private int armor;
        private int speed;
        private bool showRes;
        private bool showStats;
        private Animation anim;
        private int xWidth;

        public BamToolTipWidget
        (
            ActorActionsWidget actorActions,
            string name = "",
            int cost = 0,
            int researcost = 0,
            int researchtime = 0,
            int attack = 0,
            int armor = 0,
            int speed = 0,
            bool showRes = false,
            bool showStats = false
        )
        {
            this.actorActions = actorActions;
            this.name = name;
            this.cost = cost;
            this.researcost = researcost;
            this.researchtime = researchtime;
            this.attack = attack;
            this.armor = armor;
            this.speed = speed;
            this.showRes = showRes;
            this.showStats = showStats;

            anim = new Animation(actorActions.BamUi.World, "basic_ui");

            anim.PlayFetchIndex("ui_tooltipbar", () => 0);
            xWidth = anim.Image.Bounds.Width;
            Bounds = new Rectangle(0 - xWidth, 0, xWidth, 10);
        }

        public override void Draw()
        {
            anim.PlayFetchIndex("ui_tooltipbar", () => 0);

            var x = anim.Image.Bounds.Width;
            var y = anim.Image.Bounds.Height;

            WidgetUtils.DrawSHPCentered(anim.Image, new float2(RenderBounds.X, RenderBounds.Y), actorActions.BamUi.Palette);
            WidgetUtils.DrawSHPCentered(anim.Image, new float2(RenderBounds.X, RenderBounds.Y + y), actorActions.BamUi.Palette);
            WidgetUtils.DrawSHPCentered(anim.Image, new float2(RenderBounds.X, RenderBounds.Y + y * 2), actorActions.BamUi.Palette);

            if (showStats)
            {
                anim.PlayFetchIndex("ui_tooltipbar", () => 0);
                WidgetUtils.DrawSHPCentered(anim.Image, new float2(RenderBounds.X, RenderBounds.Y + y * 3), actorActions.BamUi.Palette);
                WidgetUtils.DrawSHPCentered(anim.Image, new float2(RenderBounds.X, RenderBounds.Y + y * 4), actorActions.BamUi.Palette);

                for (int i = 0; i < attack; i++)
                {
                    anim.PlayFetchIndex("damage", () => 0);
                    WidgetUtils.DrawSHPCentered(anim.Image, new float2(RenderBounds.X + 5 + i * 12, RenderBounds.Y + y * 2), actorActions.BamUi.Palette);
                }

                for (int i = 0; i < armor; i++)
                {
                    anim.PlayFetchIndex("armor", () => 0);
                    WidgetUtils.DrawSHPCentered(anim.Image, new float2(RenderBounds.X + 5 + i * 12, RenderBounds.Y + y * 3), actorActions.BamUi.Palette);
                }

                for (int i = 0; i < speed; i++)
                {
                    anim.PlayFetchIndex("speed", () => 0);
                    WidgetUtils.DrawSHPCentered(anim.Image, new float2(RenderBounds.X + 5 + i * 12, RenderBounds.Y + y * 4), actorActions.BamUi.Palette);
                }

                var text = cost.ToString();
                actorActions.BamUi.Font.DrawTextWithShadow(text,
                    new float2(RenderBounds.X + RenderBounds.Width - actorActions.BamUi.Font.Measure(text).X - 2, RenderBounds.Y + 2),
                    Color.YellowGreen, Color.DarkSlateGray, 1);
            }

            if (showRes)
            {
                anim.PlayFetchIndex("ui_tooltipbar", () => 0);
                WidgetUtils.DrawSHPCentered(anim.Image, new float2(RenderBounds.X, RenderBounds.Y + y * 3), actorActions.BamUi.Palette);

                anim.PlayFetchIndex("ui_research_bar", () => 5);
                WidgetUtils.DrawSHPCentered(anim.Image, new float2(RenderBounds.X + 5, RenderBounds.Y + y * 3 - 5), actorActions.BamUi.Palette);

                var text = researcost.ToString();
                actorActions.BamUi.Font.DrawTextWithShadow(text,
                    new float2(RenderBounds.X + RenderBounds.Width - actorActions.BamUi.Font.Measure(text).X - 2, RenderBounds.Y + 2),
                    Color.YellowGreen, Color.DarkSlateGray, 1);

                var text2 = researchtime / 25 + " Seconds";
                actorActions.BamUi.Font.DrawTextWithShadow(text2,
                    new float2(RenderBounds.X + RenderBounds.Width / 2 - actorActions.BamUi.Font.Measure(text2).X, RenderBounds.Y + y * 2),
                    Color.YellowGreen, Color.DarkSlateGray, 1);
            }

            actorActions.BamUi.FontLarge.DrawTextWithShadow(name, new float2(RenderBounds.X + 2, RenderBounds.Y + 2), Color.YellowGreen, Color.DarkSlateGray,
                1);
        }
    }
}