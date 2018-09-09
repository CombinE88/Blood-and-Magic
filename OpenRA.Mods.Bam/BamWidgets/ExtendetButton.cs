using System;
using System.Drawing;
using OpenRA.Mods.Bam.Traits;
using OpenRA.Mods.Common.Widgets;
using OpenRA.Widgets;

namespace OpenRA.Mods.Bam.BamWidgets
{
    public class ExtendetButtonWidget : ButtonWidget
    {
        [ObjectCreator.UseCtor]
        public ExtendetButtonWidget(ModData modData) : base(modData)
        {
        }

        public Actor Actor { get; set; }

        public override void Draw()
        {
            base.Draw();

            if (Actor != null && !Actor.IsDead && Actor.IsInWorld)
            {
                var currentMana = Actor.Trait<ManaShooter>().CurrentStorage;
                var maxMana = Actor.Info.TraitInfo<ManaShooterInfo>().MaxStorage;
                var progress = 128 * currentMana / maxMana;

                WidgetUtils.FillRectWithColor(new Rectangle(RenderBounds.X + 26, RenderBounds.Y + 18, progress, 10), Color.RoyalBlue);
            }
        }
    }
}