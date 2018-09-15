using System.Drawing;
using OpenRA.Graphics;
using OpenRA.Mods.Bam.BamWidgets;
using OpenRA.Mods.Bam.Traits.UnitAbilities;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Widgets;

namespace OpenRA.Mods.Common.Warheads
{
    public class AbilityButtonWidget : Widget
    {
        private ActorActionsWidget actorActions;
        private string text;
        private Animation anim;

        public AbilityButtonWidget(ActorActionsWidget actorActions)
        {
            this.actorActions = actorActions;
            anim = new Animation(actorActions.BamUi.World, "basic_ui");
            Bounds = new Rectangle(1, 265, 180, 34);
        }

        public override void Tick()
        {
            text = actorActions.Actor.Info.TraitInfoOrDefault<HealTargetAbilityInfo>().AbilityString;
        }

        public override void Draw()
        {
            if (text == null)
                return;

            var disabled = !(actorActions.Actor.Trait<HealTargetAbility>().CurrentDelay >= actorActions.Actor.Info.TraitInfoOrDefault<HealTargetAbilityInfo>().Delay);

            anim.PlayFetchIndex(disabled ? "ui_Ability_button_disabled" : "ui_Ability_button", () => 0);
            WidgetUtils.DrawSHPCentered(anim.Image, new float2(RenderBounds.X, RenderBounds.Y), actorActions.BamUi.Palette);

            actorActions.BamUi.Font.DrawTextWithShadow(text, new float2(RenderBounds.X + RenderBounds.Width / 4 - actorActions.BamUi.Font.Measure(text).X / 2, RenderBounds.Y - 2),
                Color.White, Color.Gray, 1);
        }
    }
}