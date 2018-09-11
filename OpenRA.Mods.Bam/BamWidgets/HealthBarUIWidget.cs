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
    public class HealthBarUIWidget : Widget
    {
        private ActorActionsWidget actorActions;
        private int currentHP;
        private int maxHP;
        private int progress;

        public HealthBarUIWidget(ActorActionsWidget actorActions)
        {
            this.actorActions = actorActions;
        }

        public override void Tick()
        {
            if (actorActions.Actor == null)
                return;

            currentHP = actorActions.Actor.Trait<Health>().HP;
            maxHP = actorActions.Actor.Trait<Health>().MaxHP;

            progress = 144 * currentHP / maxHP;
            Bounds = new Rectangle(4, 163, 144, 10);
        }

        public override void Draw()
        {
            WidgetUtils.FillRectWithColor(new Rectangle(RenderBounds.X, RenderBounds.Y, progress, 10), Color.Firebrick);
            var text = "HP: " + currentHP + " / " + maxHP;
            actorActions.BamUi.Font.DrawTextWithShadow(text, new float2(RenderBounds.X + RenderBounds.Width / 2 - actorActions.BamUi.Font.Measure(text).X / 2, RenderBounds.Y - 3),
                Color.White, Color.Gray, 1);
        }
    }
}