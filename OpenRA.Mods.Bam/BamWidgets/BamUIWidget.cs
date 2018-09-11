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
        public SpriteFont FontLarge;
        public PaletteReference Palette;

        public Sheet Sheet;
        private Sprite rightBar;
        public Sheet RightbarSheet;

        [ObjectCreator.UseCtor]
        public BamUIWidget(World world, WorldRenderer worldRenderer)
        {
            World = world;
            WorldRenderer = worldRenderer;
            Game.Renderer.Fonts.TryGetValue("Small", out Font);
            Game.Renderer.Fonts.TryGetValue("MediumBold", out FontLarge);
            Palette = WorldRenderer.Palette("BamPlayer" + World.LocalPlayer.InternalName);

            Sheet = new Sheet(SheetType.BGRA, Game.ModData.DefaultFileSystem.Open("uibits/chromebam.png"));

            RightbarSheet = new Sheet(SheetType.BGRA, Game.ModData.DefaultFileSystem.Open("uibits/mainbar.png"));
            rightBar = new Sprite(RightbarSheet, new Rectangle(0, 89, 200 + Border, 249), TextureChannel.RGBA);

            CreateBackground();


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


        public void CreateBackground()
        {
            //Example
            //AddChild(new SideBarBackgroundWidget(this,Position X,Position Y,PicturePosition X,PicturePosition Y,PicturePosition X End,PicturePosition Y End));


            //Background Minimap
            AddChild(new SideBarBackgroundWidget(this, 10, 20, 594, 24, 128, 120));

            //Background Health Frame
            AddChild(new SideBarBackgroundWidget(this, 0, 160, 4, 734, 152, 17));

            //Background IconFrame
            AddChild(new SideBarBackgroundWidget(this, 0, 180, 848, 0, 84, 74));
            //Background Icon
            AddChild(new SideBarBackgroundWidget(this, 4, 183, 0, 214, 76, 68));

            //Background Armor Numbers
            AddChild(new SideBarBackgroundWidget(this, 85, 183, 5, 754, 25, 47));

            //Background MainButton
            AddChild(new SideBarBackgroundWidget(this, 0, 264, 0, 316, 180, 34));

            //Background SecondButton
            AddChild(new SideBarBackgroundWidget(this, 0, 298, 0, 316, 180, 34));

            //Background Trinket
            AddChild(new SideBarBackgroundWidget(this, 0, 352, 0, 350, 76, 51));

            //Background Trinket Drop Button
            AddChild(new SideBarBackgroundWidget(this, 0, 403, 178, 774, 76, 17));

            //Background Convert Buttons
            for (int i = 0; i < 4; i++)
            {
                AddChild(new SideBarBackgroundWidget(this, 10, 450 + 68*i, 0, 214, 75, 68));
                AddChild(new SideBarBackgroundWidget(this, 85, 450 + 68*i, 0, 214, 75, 68));
            }
        }
    }
}