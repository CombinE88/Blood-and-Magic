using System.Drawing;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Widgets;

namespace OpenRA.Mods.Bam.BamWidgets
{
    public class SideBarRadarBackgroundWidget : Widget
    {
        private Sprite sheet;

        public int PosX = 0;
        public int PosY = 0;

        public SideBarRadarBackgroundWidget(Sprite sheet, int posX, int posy)
        {
            this.sheet = sheet;

            PosX = posX;
            PosY = posy;
        }

        public override void Tick()
        {
            Bounds = new Rectangle(PosX, PosY, sheet.Bounds.Width, sheet.Bounds.Height);
        }

        public override void Draw()
        {
            WidgetUtils.DrawRGBA(sheet, new float2(RenderBounds.X, RenderBounds.Y));
        }
    }
}