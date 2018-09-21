using System.Drawing;
using OpenRA.Graphics;
using OpenRA.Mods.Bam.Traits;
using OpenRA.Mods.Bam.Traits.RPGTraits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Traits;
using OpenRA.Widgets;

namespace OpenRA.Mods.Bam.BamWidgets
{
    public class DrawValueStatisticsWidget : Widget
    {
        private ActorActionsWidget actorActions;

        public DrawValueStatisticsWidget(ActorActionsWidget actorActions)
        {
            this.actorActions = actorActions;
        }

        public override void Tick()
        {
            Bounds = new Rectangle(85, 223, 100, 100);
        }

        public override void Draw()
        {
            var ddtrait = actorActions.AllActor.TraitOrDefault<DungeonsAndDragonsStats>();

            if (ddtrait != null)
            {
                DrawDamage(ddtrait);
                DrawArmor(ddtrait);
                DrawSpeed(ddtrait);
            }
        }

        void DrawDamage(DungeonsAndDragonsStats ddtrait)
        {
            var damage = new Animation(actorActions.BamUi.World, "basic_ui");
            if (ddtrait.Damage > 0)
            {
                var methode = ddtrait.ModifiedDamage > ddtrait.Damage ? true : false;
                for (int i = 0; i < (ddtrait.ModifiedDamage > ddtrait.Damage ? ddtrait.ModifiedDamage : ddtrait.Damage); i++)
                {
                    if (i + 1 > ddtrait.Damage && methode)
                    {
                        damage.PlayFetchIndex("damage_extra", () => 0);
                    }
                    else if (i + 1 > ddtrait.ModifiedDamage && !methode)
                    {
                        damage.PlayFetchIndex("damage_disabled", () => 0);
                    }
                    else
                    {
                        damage.PlayFetchIndex("damage", () => 0);
                    }

                    WidgetUtils.DrawSHPCentered(damage.Image, new float2(RenderBounds.X + 30 + i * 10, RenderBounds.Y + 2), actorActions.BamUi.Palette);
                }

                actorActions.BamUi.Font.DrawTextWithShadow(ddtrait.ModifiedDamage + "", new float2(RenderBounds.X + 8, RenderBounds.Y), Color.Azure, Color.DarkSlateGray, 1);
            }
            else
            {
                damage.PlayFetchIndex("damage_none", () => 0);
                WidgetUtils.DrawSHPCentered(damage.Image, new float2(RenderBounds.X + 2, RenderBounds.Y), actorActions.BamUi.Palette);
            }
        }

        void DrawArmor(DungeonsAndDragonsStats ddtrait)
        {
            var armor = new Animation(actorActions.BamUi.World, "basic_ui");
            if (ddtrait.Armor > 0)
            {
                var methode = ddtrait.ModifiedArmor > ddtrait.Armor ? true : false;
                for (int i = 0; i < (ddtrait.ModifiedArmor > ddtrait.Armor ? ddtrait.ModifiedArmor : ddtrait.Armor); i++)
                {
                    if (i + 1 > ddtrait.Armor && methode)
                    {
                        armor.PlayFetchIndex("armor_extra", () => 0);
                    }
                    else if (i + 1 > ddtrait.ModifiedArmor && !methode)
                    {
                        armor.PlayFetchIndex("armor_disabled", () => 0);
                    }
                    else
                    {
                        armor.PlayFetchIndex("armor", () => 0);
                    }

                    WidgetUtils.DrawSHPCentered(armor.Image, new float2(RenderBounds.X + 30 + i * 10, RenderBounds.Y + 2 + 15), actorActions.BamUi.Palette);
                }

                actorActions.BamUi.Font.DrawTextWithShadow(ddtrait.ModifiedArmor + "", new float2(RenderBounds.X + 8, RenderBounds.Y + 16), Color.Azure, Color.DarkSlateGray, 1);
            }
            else
            {
                armor.PlayFetchIndex("armor_none", () => 0);
                WidgetUtils.DrawSHPCentered(armor.Image, new float2(RenderBounds.X + 2, RenderBounds.Y + 16), actorActions.BamUi.Palette);
            }
        }

        void DrawSpeed(DungeonsAndDragonsStats ddtrait)
        {
            var speed = new Animation(actorActions.BamUi.World, "basic_ui");
            if (ddtrait.Speed > 0)
            {
                var methode = ddtrait.ModifiedSpeed > ddtrait.Speed ? true : false;
                for (int i = 0; i < (ddtrait.ModifiedSpeed > ddtrait.Speed ? ddtrait.ModifiedSpeed : ddtrait.Speed); i++)
                {
                    if (i + 1 > ddtrait.Speed && methode)
                    {
                        speed.PlayFetchIndex("speed_extra", () => 0);
                    }
                    else if (i + 1 > ddtrait.ModifiedSpeed && !methode)
                    {
                        speed.PlayFetchIndex("speed_disabled", () => 0);
                    }
                    else
                    {
                        speed.PlayFetchIndex("speed", () => 0);
                    }

                    WidgetUtils.DrawSHPCentered(speed.Image, new float2(RenderBounds.X + 30 + i * 10, RenderBounds.Y + 4 + 30), actorActions.BamUi.Palette);
                }

                actorActions.BamUi.Font.DrawTextWithShadow(ddtrait.ModifiedSpeed + "", new float2(RenderBounds.X + 8, RenderBounds.Y + 32), Color.Azure, Color.DarkSlateGray, 1);
            }
            else
            {
                speed.PlayFetchIndex("speed_none", () => 0);
                WidgetUtils.DrawSHPCentered(speed.Image, new float2(RenderBounds.X + 2, RenderBounds.Y + 32), actorActions.BamUi.Palette);
            }
        }
    }
}