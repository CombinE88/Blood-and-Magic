using System;
using System.Drawing;
using OpenRA.Graphics;
using OpenRA.Mods.Bam.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Widgets;

namespace OpenRA.Mods.Bam.BamWidgets
{
    public class DrawTransformStatisticsWidget : Widget
    {
        private ActorActionsWidget actorActions;

        private Sprite emptyMoons;
        private Sprite background2;
        private Sprite background3;
        private Sprite emptyMen;

        private TransformAfterTime transformAfterTime;
        private TransformAfterTimeInfo transformAfterTimeInfo;

        private Sprite readyMen;
        private Sprite moonPhase2;
        private Sprite moonPhase3;
        private Sprite moonPhase4;
        private Sprite moonPhase5;
        private Sprite moonPhase6;

        public DrawTransformStatisticsWidget(ActorActionsWidget actorActions)
        {
            this.actorActions = actorActions;

            moonPhase2 = new Sprite(actorActions.BamUi.Sheet, new Rectangle(147, 940, 12, 14), TextureChannel.RGBA);
            moonPhase3 = new Sprite(actorActions.BamUi.Sheet, new Rectangle(172, 940, 12, 14), TextureChannel.RGBA);
            moonPhase4 = new Sprite(actorActions.BamUi.Sheet, new Rectangle(198, 940, 14, 14), TextureChannel.RGBA);
            moonPhase5 = new Sprite(actorActions.BamUi.Sheet, new Rectangle(224, 940, 16, 14), TextureChannel.RGBA);
            moonPhase6 = new Sprite(actorActions.BamUi.Sheet, new Rectangle(252, 940, 17, 14), TextureChannel.RGBA);

            readyMen = new Sprite(actorActions.BamUi.Sheet, new Rectangle(47, 964, 50, 60), TextureChannel.RGBA);

            emptyMoons = new Sprite(actorActions.BamUi.Sheet, new Rectangle(114, 958, 172, 29), TextureChannel.RGBA);
            background2 = new Sprite(actorActions.BamUi.Sheet, new Rectangle(452, 374, 90, 34), TextureChannel.RGBA);
            background3 = new Sprite(actorActions.BamUi.Sheet, new Rectangle(867, 376, 90, 34), TextureChannel.RGBA);

            emptyMen = new Sprite(actorActions.BamUi.Sheet, new Rectangle(282, 958, 56, 63), TextureChannel.RGBA);
        }

        public override void Tick()
        {
            Bounds = new Rectangle(0, 0, Parent.Bounds.Width, Parent.Bounds.Height);
        }

        public override void Draw()
        {
            if (actorActions.Actor == null)
                return;

            transformAfterTime = actorActions.Actor.TraitOrDefault<TransformAfterTime>();
            transformAfterTimeInfo = actorActions.Actor.Info.TraitInfoOrDefault<TransformAfterTimeInfo>();

            //hide
            WidgetUtils.DrawRGBA(background2, new float2(RenderBounds.Left + 0, RenderBounds.Top + 298));
            WidgetUtils.DrawRGBA(background3, new float2(RenderBounds.Left + 90, RenderBounds.Top + 298));

            // draw empty
            WidgetUtils.DrawRGBA(emptyMoons, new float2(RenderBounds.Left + 4, RenderBounds.Top + 266));
            WidgetUtils.DrawRGBA(emptyMen, new float2(RenderBounds.Left + 124, RenderBounds.Top + 298));

            var progress = 8 * transformAfterTime.Ticker / transformAfterTimeInfo.Time;

            if (progress > 1)
                WidgetUtils.DrawRGBA(moonPhase2, new float2(RenderBounds.Left + 38, RenderBounds.Top + 266 + 8));
            if (progress > 2)
                WidgetUtils.DrawRGBA(moonPhase3, new float2(RenderBounds.Left + 62, RenderBounds.Top + 266 + 8));
            if (progress > 3)
                WidgetUtils.DrawRGBA(moonPhase4, new float2(RenderBounds.Left + 88, RenderBounds.Top + 266 + 8));
            if (progress > 5)
                WidgetUtils.DrawRGBA(moonPhase5, new float2(RenderBounds.Left + 114, RenderBounds.Top + 266 + 8));
            if (progress > 6)
                WidgetUtils.DrawRGBA(moonPhase6, new float2(RenderBounds.Left + 142, RenderBounds.Top + 266 + 8));
            if (progress > 7)
                WidgetUtils.DrawRGBA(readyMen, new float2(RenderBounds.Left + 128, RenderBounds.Top + 298));
        }
    }
}