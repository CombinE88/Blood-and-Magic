using System.Drawing;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Widgets;

namespace OpenRA.Mods.Bam.BamWidgets
{
    public class BamUIWidget : Widget
    {
        public readonly RadarPings RadarPings;

        private static int border = 26;

        public World World;
        public WorldRenderer WorldRenderer;
        public SpriteFont Font;
        public SpriteFont FontRegular;
        public SpriteFont FontLarge;
        public PaletteReference Palette;

        public Sheet Sheet;
        private Sprite rightBar;
        public Sheet RightbarSheet;
        private Sprite bottom;

        [ObjectCreator.UseCtor]
        public BamUIWidget(World world, WorldRenderer worldRenderer)
        {
            World = world;
            WorldRenderer = worldRenderer;
            RadarPings = world.WorldActor.Trait<RadarPings>();

            Game.Renderer.Fonts.TryGetValue("Small", out Font);
            Game.Renderer.Fonts.TryGetValue("Regular", out FontRegular);
            Game.Renderer.Fonts.TryGetValue("MediumBold", out FontLarge);
            Palette = WorldRenderer.Palette("BamPlayer" + World.LocalPlayer.InternalName);

            Sheet = new Sheet(SheetType.BGRA, Game.ModData.DefaultFileSystem.Open("uibits/chromebam.png"));

            RightbarSheet = new Sheet(SheetType.BGRA, Game.ModData.DefaultFileSystem.Open("uibits/mainbar.png"));
            rightBar = new Sprite(RightbarSheet, new Rectangle(0, 89, 200 + border, 249), TextureChannel.RGBA);
            bottom = new Sprite(RightbarSheet, new Rectangle(0, 338, 256 + border, 9), TextureChannel.RGBA);

            AddChild(new ManaCounterWidget(this));
            AddChild(new ActorActionsWidget(this));
        }

        public override bool HandleMouseInput(MouseInput mi)
        {
            return new Rectangle(RenderBounds.X - border, RenderBounds.Y, RenderBounds.Width + border, RenderBounds.Height).Contains(mi.Location);
        }

        public override void Tick()
        {
            Bounds = new Rectangle(Game.Renderer.Resolution.Width - rightBar.Bounds.Width + border, 0, rightBar.Bounds.Width - border, 450);
        }

        public override void Draw()
        {
            for (var y = 0; y < RenderBounds.Height; y += rightBar.Bounds.Height)
                WidgetUtils.DrawRGBA(rightBar, new float2(RenderBounds.X - border, y));

            WidgetUtils.DrawRGBA(bottom, new float2(RenderBounds.X - border, 490));
        }
    }
}