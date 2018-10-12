using System.Drawing;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Widgets;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Widgets;

namespace OpenRA.Mods.Bam.BamWidgets
{
    public class SideBarRadarBackgroundWidget : Widget
    {
        public int PosX = 0;
        public int PosY = 0;
        private Sheet radarsheet;
        private Sprite radarBG;
        private BamUIWidget bamUi;
        private BamRadarWidget radar;

        public SideBarRadarBackgroundWidget(BamUIWidget bamUi)
        {
            this.bamUi = bamUi;

            radarsheet = new Sheet(SheetType.BGRA, Game.ModData.DefaultFileSystem.Open("uibits/radarbg.png"));
            radarBG = new Sprite(radarsheet, new Rectangle(0, 0, 172, 150), TextureChannel.RGBA);
            Bounds = new Rectangle(5, 30, radarBG.Bounds.Width, radarBG.Bounds.Height);

            AddChild(radar = new BamRadarWidget(bamUi, this));
        }

        public override void Tick()
        {
            Bounds = new Rectangle(5, 30, radarBG.Bounds.Width, radarBG.Bounds.Height);
        }

        public override void Draw()
        {
            WidgetUtils.DrawRGBA(radarBG,
                new float2(RenderBounds.X + RenderBounds.Width / 2 - radarBG.Bounds.Width / 2, RenderBounds.Y + RenderBounds.Height / 2 - radarBG.Bounds.Height / 2));
        }
    }
}