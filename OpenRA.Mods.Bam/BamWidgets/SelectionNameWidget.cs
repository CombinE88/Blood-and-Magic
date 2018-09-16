using System.Drawing;
using OpenRA.Graphics;
using OpenRA.Mods.Bam.SpriteLoaders;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Mods.Common.Widgets;
using OpenRA.Traits;
using OpenRA.Widgets;

namespace OpenRA.Mods.Bam.BamWidgets
{
    public class SelectionNameWidget : Widget
    {
        private ActorActionsWidget actorActions;
        private string text = "";

        public SelectionNameWidget(ActorActionsWidget actorActions)
        {
            this.actorActions = actorActions;
        }

        public override void Tick()
        {
            if (actorActions.AllActor == null)
                return;

            text = actorActions.AllActor.Info.TraitInfo<TooltipInfo>().Name;
            Bounds = new Rectangle(4, 138, 100, 20);
        }

        public override void Draw()
        {
            actorActions.BamUi.FontLarge.DrawTextWithShadow(text, new float2(RenderBounds.X, RenderBounds.Y), Color.White, Color.Gray, 1);
        }
    }
}