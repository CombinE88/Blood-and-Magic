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
            Bounds = new Rectangle(0, 306, 180, 34);
        }

        public override void Tick()
        {
            if (actorActions.Actor.TraitOrDefault<HealTargetAbility>() != null)
                text = actorActions.Actor.Info.TraitInfoOrDefault<HealTargetAbilityInfo>().AbilityString;
            else if (actorActions.Actor.TraitOrDefault<StealEnemyAbility>() != null)
                text = actorActions.Actor.Info.TraitInfoOrDefault<StealEnemyAbilityInfo>().AbilityString;
        }

        public override void Draw()
        {
            if (text == null)
                return;

            var disabled = false;


            var fill = 0;
            var cost = 0;

            var heal = actorActions.Actor.TraitOrDefault<HealTargetAbility>();
            var steal = actorActions.Actor.TraitOrDefault<StealEnemyAbility>();

            if (heal != null)
            {
                var healInfo = actorActions.Actor.Info.TraitInfoOrDefault<HealTargetAbilityInfo>();

                disabled = !(heal.CurrentDelay >= healInfo.Delay);

                fill = 170 - 170 * heal.CurrentDelay / healInfo.Delay;

                cost = healInfo.Ammount;
            }

            else if (steal != null)
            {
                var stealInfo = actorActions.Actor.Info.TraitInfoOrDefault<StealEnemyAbilityInfo>();

                disabled = !(steal.CurrentDelay >= stealInfo.Delay);

                fill = 170 - 170 * steal.CurrentDelay / stealInfo.Delay;

                cost = stealInfo.Ammount;
            }


            anim.PlayFetchIndex(disabled ? "ui_Ability_button_disabled" : "ui_Ability_button", () => 0);
            WidgetUtils.DrawSHPCentered(anim.Image, new float2(RenderBounds.X, RenderBounds.Y), actorActions.BamUi.Palette);

            var rect = new Rectangle(RenderBounds.X, RenderBounds.Y, fill, 32);
            var color = Color.FromArgb(125, 0, 0, 0);

            WidgetUtils.FillRectWithColor(rect, color);

            actorActions.BamUi.Font.DrawTextWithShadow(text, new float2(RenderBounds.X + RenderBounds.Width / 4 - actorActions.BamUi.Font.Measure(text).X / 2, RenderBounds.Y - 2),
                Color.White, Color.Gray, 1);

            actorActions.BamUi.FontLarge.DrawTextWithShadow(cost.ToString(), new float2(RenderBounds.X + 5, RenderBounds.Y + 10),
                Color.CornflowerBlue, Color.DarkBlue, 2);
        }
    }
}