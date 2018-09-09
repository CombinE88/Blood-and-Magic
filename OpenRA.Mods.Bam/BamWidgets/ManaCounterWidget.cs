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
            Bounds = new Rectangle(0, 30, background.Bounds.Width, background.Bounds.Height);
        }

        public override void Draw()
        {
            WidgetUtils.DrawRGBA(background, new float2(RenderBounds.X, RenderBounds.Y));
            bamUi.Font.DrawText("Mana: " + (playerResources.Resources + playerResources.Cash), new float2(RenderBounds.X + 10, RenderBounds.Y + 4), Color.Chocolate);
        }
    }
}