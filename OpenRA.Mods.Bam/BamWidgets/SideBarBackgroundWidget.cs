using System.Drawing;
using OpenRA.Graphics;
using OpenRA.Widgets;

namespace OpenRA.Mods.Bam.BamWidgets
{
    public class SideBarBackgroundWidget : Widget
    {
        private Sprite background;

        public int PosX = 0;
        public int PosY = 0;

        public int BoundsX = 0;
        public int BoundsY = 0;
        public int BoundsXMax = 0;
        public int BoundsYMax = 0;

        public SideBarBackgroundWidget(BamUIWidget bamUi, int posX, int posy, int boundsx, int boundsy, int boundsxmax, int boundsymax)
        {
            PosX = posX;
            PosY = posy;
            BoundsX = boundsx;
            BoundsY = boundsy;
            BoundsXMax = boundsxmax;
            BoundsYMax = boundsymax;

            background = new Sprite(bamUi.Sheet, new Rectangle(BoundsX, BoundsY, BoundsXMax, BoundsYMax), TextureChannel.RGBA);
        }

        public override void Tick()
        {
            Bounds = new Rectangle(PosX, PosY, background.Bounds.Width, background.Bounds.Height);
        }

        public override void Draw()
        {
            WidgetUtils.DrawRGBA(background, new float2(RenderBounds.X, RenderBounds.Y));
        }
    }
}