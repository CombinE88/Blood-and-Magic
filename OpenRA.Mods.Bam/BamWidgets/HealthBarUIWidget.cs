using System.Drawing;
using OpenRA.Mods.Common.Traits;
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
            if (actorActions.AllActor == null)
                return;

            currentHP = actorActions.AllActor.Trait<Health>().HP;
            maxHP = actorActions.AllActor.Trait<Health>().MaxHP;

            progress = 144 * currentHP / maxHP;
            Bounds = new Rectangle(3, 203, 144, 10);
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