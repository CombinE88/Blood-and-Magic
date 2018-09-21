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

        public BamToolTipWidget(
            ActorActionsWidget actorActions,
            string name = "",
            int cost = 0,
            int researcost = 0,
            int researchtime = 0,
            int attack = 0,
            int armor = 0,
            int speed = 0,
            bool showRes = false,
            bool showStats = false)
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
            Bounds = new Rectangle(0 - 192 - 10, 0, 192, 86);
        }

        public override void Draw()
        {
            // Background Sheet Toolip
            var radarsheet = new Sheet(SheetType.BGRA, Game.ModData.DefaultFileSystem.Open("uibits/radarbg.png"));

            var x = anim.Image.Bounds.Width;
            var y = anim.Image.Bounds.Height;

            if (showStats)
            {
                var radarBG = new Sprite(radarsheet, new Rectangle(0, 150, 192, 68), TextureChannel.RGBA);
                WidgetUtils.DrawRGBA(radarBG, new float2(RenderBounds.X, RenderBounds.Y));

                for (int i = 0; i < attack; i++)
                {
                    anim.PlayFetchIndex("damage", () => 0);
                    WidgetUtils.DrawSHPCentered(anim.Image, new float2(RenderBounds.X + 48 + i * 12, RenderBounds.Y + 18), actorActions.BamUi.Palette);

                    actorActions.BamUi.Font.DrawTextWithShadow(attack.ToString(), new float2(RenderBounds.X + 30, RenderBounds.Y + 18), Color.Azure, Color.DarkSlateGray, 1);
                }

                for (int i = 0; i < armor; i++)
                {
                    anim.PlayFetchIndex("armor", () => 0);
                    WidgetUtils.DrawSHPCentered(anim.Image, new float2(RenderBounds.X + 48 + i * 12, RenderBounds.Y + 34), actorActions.BamUi.Palette);

                    actorActions.BamUi.Font.DrawTextWithShadow(armor.ToString(), new float2(RenderBounds.X + 30, RenderBounds.Y + 34), Color.Azure, Color.DarkSlateGray, 1);
                }

                for (int i = 0; i < speed; i++)
                {
                    anim.PlayFetchIndex("speed", () => 0);
                    WidgetUtils.DrawSHPCentered(anim.Image, new float2(RenderBounds.X + 48 + i * 12, RenderBounds.Y + 50), actorActions.BamUi.Palette);

                    actorActions.BamUi.Font.DrawTextWithShadow(speed.ToString(), new float2(RenderBounds.X + 30, RenderBounds.Y + 50), Color.Azure, Color.DarkSlateGray, 1);
                }

                var manaCost = cost.ToString();
                var manaLabel = "Mana: ";

                actorActions.BamUi.Font.DrawTextWithShadow(manaCost,
                    new float2(RenderBounds.X + RenderBounds.Width - actorActions.BamUi.Font.Measure(manaCost).X - 30, RenderBounds.Y),
                    Color.Yellow, Color.Black, 2);

                actorActions.BamUi.Font.DrawTextWithShadow(manaLabel,
                    new float2(RenderBounds.X + RenderBounds.Width - actorActions.BamUi.Font.Measure(manaCost).X - actorActions.BamUi.Font.Measure(manaLabel).X - 30,
                        RenderBounds.Y),
                    Color.LawnGreen, Color.Black, 2);
            }

            if (showRes)
            {
                var radarBG = new Sprite(radarsheet, new Rectangle(0, 218, 192, 35), TextureChannel.RGBA);
                WidgetUtils.DrawRGBA(radarBG, new float2(RenderBounds.X, RenderBounds.Y));

                var expCost = researcost.ToString();
                var expLabel = "EXP: ";

                actorActions.BamUi.Font.DrawTextWithShadow(expCost,
                    new float2(RenderBounds.X + RenderBounds.Width - actorActions.BamUi.Font.Measure(expCost).X - 30, RenderBounds.Y),
                    Color.Yellow, Color.Black, 2);

                actorActions.BamUi.Font.DrawTextWithShadow(expLabel,
                    new float2(RenderBounds.X + RenderBounds.Width - actorActions.BamUi.Font.Measure(expCost).X - actorActions.BamUi.Font.Measure(expLabel).X - 30, RenderBounds.Y),
                    Color.LawnGreen, Color.Black, 2);

                var text2 = researchtime / 25 + " Seconds";
                actorActions.BamUi.Font.DrawTextWithShadow(text2,
                    new float2(RenderBounds.X + 84, RenderBounds.Y + 17),
                    Color.LawnGreen, Color.Black, 2);
            }

            if (!showRes && !showStats)
            {
                var top = new Sprite(radarsheet, new Rectangle(0, 150, 191, 16), TextureChannel.RGBA);
                WidgetUtils.DrawRGBA(top, new float2(RenderBounds.X, RenderBounds.Y));
                var bottom = new Sprite(radarsheet, new Rectangle(0, 215, 191, 3), TextureChannel.RGBA);
                WidgetUtils.DrawRGBA(bottom, new float2(RenderBounds.X, RenderBounds.Y + 16));
            }

            actorActions.BamUi.Font.DrawTextWithShadow(name, new float2(RenderBounds.X + 26, RenderBounds.Y), Color.LawnGreen, Color.Black,
                2);
        }
    }
}