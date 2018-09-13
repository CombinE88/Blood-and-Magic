using System.Drawing;
using OpenRA.Graphics;
using OpenRA.Mods.Bam.SpriteLoaders;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Widgets;
using OpenRA.Widgets;

namespace OpenRA.Mods.Bam.BamWidgets
{
    public class ManaCounterWidget : Widget
    {
        private BamUIWidget bamUi;
        private Sprite background;
        private PlayerResources playerResources;

        public ManaCounterWidget(BamUIWidget bamUi)
        {
            this.bamUi = bamUi;
            background = new Sprite(bamUi.Sheet, new Rectangle(0, 734, 160, 17), TextureChannel.RGBA);
            playerResources = bamUi.World.LocalPlayer.PlayerActor.Trait<PlayerResources>();
        }

        public override void Tick()
        {
            Bounds = new Rectangle(0, 5, background.Bounds.Width, background.Bounds.Height);
        }

        public override void Draw()
        {
            WidgetUtils.DrawRGBA(background, new float2(RenderBounds.X, RenderBounds.Y));


            var value = 144 * (playerResources.Resources + playerResources.Cash) / 400;
            WidgetUtils.FillRectWithColor(new Rectangle(RenderBounds.X + 8, RenderBounds.Y + 3, value, 10), Color.CornflowerBlue);

            var text = "Mana: " + (playerResources.Resources + playerResources.Cash);
            bamUi.Font.DrawTextWithContrast(text,
                new float2(RenderBounds.X + RenderBounds.Width / 2 - bamUi.Font.Measure(text).X / 2, RenderBounds.Y + RenderBounds.Height / 2 - bamUi.Font.Measure(text).Y / 2),
                Color.White,
                Color.DarkBlue, 1);
        }
    }
}