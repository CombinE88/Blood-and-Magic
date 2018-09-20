using System.Drawing;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Widgets;
using OpenRA.Mods.Kknd.Widgets.Ingame;
using OpenRA.Widgets;

namespace OpenRA.Mods.Bam.BamWidgets
{
    public class BamUIWidget : Widget
    {
        private static int border = 26;

        public World World;
        public WorldRenderer WorldRenderer;
        public SpriteFont Font;
        public SpriteFont FontLarge;
        public PaletteReference Palette;

        public readonly RadarPings RadarPings;

        public Sheet Sheet;
        private Sprite rightBar;
        public Sheet RightbarSheet;

        [ObjectCreator.UseCtor]
        public BamUIWidget(World world, WorldRenderer worldRenderer)
        {
            World = world;
            WorldRenderer = worldRenderer;
            RadarPings = world.WorldActor.Trait<RadarPings>();

            Game.Renderer.Fonts.TryGetValue("Small", out Font);
            Game.Renderer.Fonts.TryGetValue("MediumBold", out FontLarge);
            Palette = WorldRenderer.Palette("BamPlayer" + World.LocalPlayer.InternalName);

            Sheet = new Sheet(SheetType.BGRA, Game.ModData.DefaultFileSystem.Open("uibits/chromebam.png"));

            RightbarSheet = new Sheet(SheetType.BGRA, Game.ModData.DefaultFileSystem.Open("uibits/mainbar.png"));
            rightBar = new Sprite(RightbarSheet, new Rectangle(0, 89, 200 + border, 249), TextureChannel.RGBA);

            CreateBackground();

            AddChild(new ManaCounterWidget(this));
            AddChild(new ActorActionsWidget(this));
        }

        public override bool HandleMouseInput(MouseInput mi)
        {
            return new Rectangle(RenderBounds.X - border, RenderBounds.Y, RenderBounds.Width + border, RenderBounds.Height).Contains(mi.Location);
        }

        public override void Tick()
        {
            Bounds = new Rectangle(Game.Renderer.Resolution.Width - rightBar.Bounds.Width + border, 0, rightBar.Bounds.Width - border, Game.Renderer.Resolution.Height);
        }

        public override void Draw()
        {
            for (var y = 0; y < RenderBounds.Height; y += rightBar.Bounds.Height)
                WidgetUtils.DrawRGBA(rightBar, new float2(RenderBounds.X - border, y));
        }

        public void CreateBackground()
        {
            // Background Minimap
            var radar = new BamRadarWidget(this);

            AddChild(radar);

            // Background Health Frame
            AddChild(new SideBarBackgroundWidget(this, 0, 160, 4, 734, 152, 17));

            // Background IconFrame
            AddChild(new SideBarBackgroundWidget(this, 0, 180, 848, 0, 84, 74));

            // Background Icon
            AddChild(new SideBarBackgroundWidget(this, 4, 183, 0, 214, 76, 68));

            // Background Armor Numbers
            AddChild(new SideBarBackgroundWidget(this, 85, 183, 5, 754, 25, 47));

            // Background MainButton
            AddChild(new SideBarBackgroundWidget(this, 0, 264, 0, 316, 180, 34));

            // Background SecondButton
            AddChild(new SideBarBackgroundWidget(this, 0, 298, 0, 316, 180, 34));

            // Background Trinket
            AddChild(new SideBarBackgroundWidget(this, 0, 352, 0, 350, 76, 51));

            // Background Trinket Drop Button
            AddChild(new SideBarBackgroundWidget(this, 0, 403, 178, 774, 76, 17));

            // Background Convert Buttons
            for (int i = 0; i < 4; i++)
            {
                AddChild(new SideBarBackgroundWidget(this, 10, 450 + 68 * i, 0, 214, 75, 68));
                AddChild(new SideBarBackgroundWidget(this, 85, 450 + 68 * i, 0, 214, 75, 68));
            }
        }
    }
}