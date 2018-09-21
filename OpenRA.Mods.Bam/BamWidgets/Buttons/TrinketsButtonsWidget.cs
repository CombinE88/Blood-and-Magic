using System.Drawing;
using OpenRA.Graphics;
using OpenRA.Mods.Bam.Traits.TrinketLogics;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Widgets;

namespace OpenRA.Mods.Bam.BamWidgets.Buttons
{
    public class TrinketButtonsWidget : Widget
    {
        private ActorActionsWidget actorActions;
        private bool pressed;
        private Animation animation;
        private ActorInfo actorInfo;
        private string text;
        private bool show;

        public TrinketButtonsWidget(ActorActionsWidget actorActions)
        {
            this.actorActions = actorActions;
        }

        public override void Tick()
        {
            if (actorActions.AllActor == null || actorActions.AllActor.TraitOrDefault<CanHoldTrinket>() == null ||
                actorActions.AllActor.Trait<CanHoldTrinket>().HoldsTrinket == null)
                return;

            string actorString = null;
            if (actorActions.AllActor.Trait<CanHoldTrinket>().HoldsTrinket != null)
                actorString = actorActions.AllActor.Trait<CanHoldTrinket>().HoldsTrinket.Info.Name;

            actorInfo = actorActions.BamUi.World.Map.Rules.Actors[actorString];

            var seq = actorActions.BamUi.World.Map.Rules.Sequences;
            var name = actorActions.AllActor.Owner.Faction.Name;

            if (actorInfo != null && actorInfo.HasTraitInfo<RenderSpritesInfo>())
                animation = new Animation(actorActions.BamUi.World, actorInfo.TraitInfo<RenderSpritesInfo>().GetImage(actorInfo, seq, name));

            var x = pressed ? 1 + 4 : 4;
            var y = pressed ? 361 + 1 : 361;
            Bounds = new Rectangle(x, y, 76, 51);
        }

        public override bool HandleMouseInput(MouseInput mi)
        {
            if (mi.Button != MouseButton.Left || actorActions.AllActor.Owner != actorActions.BamUi.World.LocalPlayer)
                return true;

            if (mi.Event == MouseInputEvent.Down)
            {
                actorActions.AllActor.World.IssueOrder(new Order("UseTrinket", actorActions.AllActor, false));
                pressed = true;
            }

            if (mi.Event == MouseInputEvent.Up)
            {
                pressed = false;
            }

            return true;
        }

        public override void MouseExited()
        {
            pressed = false;
            show = false;
        }

        public override void MouseEntered()
        {
            show = true;
        }

        public override void Draw()
        {
            if (actorActions.AllActor == null || actorActions.AllActor.TraitOrDefault<CanHoldTrinket>() == null ||
                actorActions.AllActor.Trait<CanHoldTrinket>().HoldsTrinket == null)
                return;

            var trinket = actorActions.AllActor.TraitOrDefault<CanHoldTrinket>().HoldsTrinket;

            if (animation != null)
            {
                animation.PlayFetchIndex("icon", () => 0);
                WidgetUtils.DrawSHPCentered(animation.Image, new float2(RenderBounds.X, RenderBounds.Y + 17), actorActions.BamUi.Palette);
            }

            if (!show)
                return;

            var traitInfo = trinket.Info.TraitInfoOrDefault<IsTrinketInfo>();
            if (traitInfo != null)
            {
                text = traitInfo.Description.Replace("\\n", "\n");
                var textSize = actorActions.BamUi.Font.Measure(text);

                actorActions.BamUi.Font.DrawTextWithShadow(text,
                    new float2(RenderBounds.X + -textSize.X - 35, RenderBounds.Y + RenderBounds.Height - textSize.Y - 2),
                    Color.White, Color.Black, 2);
            }
        }
    }
}