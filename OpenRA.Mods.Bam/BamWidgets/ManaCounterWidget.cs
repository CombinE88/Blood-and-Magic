using System.Drawing;
using OpenRA.Graphics;
using OpenRA.Mods.Bam.Traits;
using OpenRA.Mods.Bam.Traits.PlayerTraits;
using OpenRA.Mods.Common.Traits;
using OpenRA.Widgets;

namespace OpenRA.Mods.Bam.BamWidgets
{
    public class ManaCounterWidget : Widget
    {
        private BamUIWidget bamUi;
        private Sprite background;
        private PlayerResources playerResources;
        private DungeonsAndDragonsExperience playerExperience;

        public ManaCounterWidget(BamUIWidget bamUi)
        {
            this.bamUi = bamUi;
            background = new Sprite(bamUi.Sheet, new Rectangle(0, 734, 160, 17), TextureChannel.RGBA);
            playerResources = bamUi.World.LocalPlayer.PlayerActor.Trait<PlayerResources>();
            playerExperience = bamUi.World.LocalPlayer.PlayerActor.Trait<DungeonsAndDragonsExperience>();
        }

        public override void Tick()
        {
            Bounds = new Rectangle(0, 5, 200, background.Bounds.Height);
        }

        public override void Draw()
        {
            // WidgetUtils.DrawRGBA(background, new float2(RenderBounds.X, RenderBounds.Y));
            // var value = 144 * (playerResources.Resources + playerResources.Cash) / 400;
            // WidgetUtils.FillRectWithColor(new Rectangle(RenderBounds.X + 8, RenderBounds.Y + 3, value, 10), Color.CornflowerBlue);
            var manaText = " :Mana";
            var manaValue = (playerResources.Resources + playerResources.Cash).ToString();

            bamUi.FontRegular.DrawTextWithContrast(manaText,
                new float2(RenderBounds.X + RenderBounds.Width - bamUi.FontRegular.Measure(manaText).X - 2,
                    RenderBounds.Y + RenderBounds.Height / 2 - bamUi.FontRegular.Measure(manaText).Y / 2),
                Color.LawnGreen,
                Color.Black, 1);

            bamUi.FontRegular.DrawTextWithContrast(manaValue,
                new float2(RenderBounds.X + RenderBounds.Width - bamUi.FontRegular.Measure(manaText).X - bamUi.FontRegular.Measure(manaValue).X - 4,
                    RenderBounds.Y + RenderBounds.Height / 2 - bamUi.FontRegular.Measure(manaValue).Y / 2),
                Color.Yellow,
                Color.Black, 1);

            var expText = "Exp: ";
            var expValue = playerExperience.Experience.ToString();

            bamUi.FontRegular.DrawTextWithContrast(expText,
                new float2(RenderBounds.X + 2, RenderBounds.Y + RenderBounds.Height / 2 - bamUi.FontRegular.Measure(expText).Y / 2),
                Color.LawnGreen,
                Color.Black, 1);

            bamUi.FontRegular.DrawTextWithContrast(expValue,
                new float2(RenderBounds.X + bamUi.FontRegular.Measure(expText).X + 2, RenderBounds.Y + RenderBounds.Height / 2 - bamUi.FontRegular.Measure(expValue).Y / 2),
                Color.Yellow,
                Color.Black, 1);
        }
    }
}