using System.Drawing;
using OpenRA.Graphics;
using OpenRA.Widgets;

namespace OpenRA.Mods.Bam.BamWidgets
{
    public class BamUIWidget : Widget
    {
        private static int Border = 26;

        public World World;
        public WorldRenderer WorldRenderer;
        public SpriteFont Font;
        public PaletteReference Palette;

        public Sheet Sheet;
        private Sprite rightBar;

        [ObjectCreator.UseCtor]
        public BamUIWidget(World world, WorldRenderer worldRenderer)
        {
            World = world;
            WorldRenderer = worldRenderer;
            Game.Renderer.Fonts.TryGetValue("Bold", out Font);
            Palette = WorldRenderer.Palette("BamPlayer" + World.LocalPlayer.InternalName);

            Sheet = new Sheet(SheetType.BGRA, Game.ModData.DefaultFileSystem.Open("uibits/chromebam.png"));

            var sheet = new Sheet(SheetType.BGRA, Game.ModData.DefaultFileSystem.Open("uibits/mainbar.png"));
            rightBar = new Sprite(sheet, new Rectangle(0, 89, 180 + Border, 249), TextureChannel.RGBA);

            AddChild(new ManaCounterWidget(this));
            AddChild(new ActorActionsWidget(this));
        }

        public override bool HandleMouseInput(MouseInput mi)
        {
            return new Rectangle(RenderBounds.X - Border, RenderBounds.Y, RenderBounds.Width + Border, RenderBounds.Height).Contains(mi.Location);
        }

        public override void Tick()
        {
            Bounds = new Rectangle(Game.Renderer.Resolution.Width - rightBar.Bounds.Width + Border, 0, rightBar.Bounds.Width - Border, Game.Renderer.Resolution.Height);
        }

        public override void Draw()
        {
            for (var y = 0; y < RenderBounds.Height; y += rightBar.Bounds.Height)
                WidgetUtils.DrawRGBA(rightBar, new float2(RenderBounds.X - Border, y));
        }
    }
}