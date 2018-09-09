using System.Drawing;
using OpenRA.Graphics;
using OpenRA.Widgets;

namespace OpenRA.Mods.Bam.Widgets.Logic
{
    public class BamUIWidget : Widget
    {
        private World world;
        private WorldRenderer worldRenderer;

        [ObjectCreator.UseCtor]
        public BamUIWidget(World world, WorldRenderer worldRenderer)
        {
            this.world = world;
            this.worldRenderer = worldRenderer;

            Bounds = new Rectangle(0, 0, Game.Renderer.Resolution.Width, Game.Renderer.Resolution.Height);
        }
    }
}