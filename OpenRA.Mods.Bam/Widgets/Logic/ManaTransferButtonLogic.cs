using OpenRA.Graphics;
using OpenRA.Mods.Bam.Traits;
using OpenRA.Mods.Common.Widgets;
using OpenRA.Widgets;

namespace OpenRA.Mods.Bam.Widgets.Logic
{
    public class ManaTransferButtonLogic : ChromeLogic
    {
        private Actor unitselected;
        ButtonWidget manabutton;

        [ObjectCreator.UseCtor]
        public ManaTransferButtonLogic(Widget widget, World world, WorldRenderer worldRenderer)
        {
            manabutton = widget.Get<ButtonWidget>("MANA_BUTTON");
            manabutton.Background = "bam_zier_button_long";
            manabutton.Disabled = true;
            manabutton.TooltipText = "";
        }

        public virtual void Tick(Actor self)
        {
            unitselected = self.World.Selection.Actors.First();

            Game.AddChatLine(Color.Aqua, "asdads", unitselected.Info.Name);

            if (unitselected != null && !unitselected.IsDead && unitselected.IsInWorld && unitselected.Info.HasTraitInfo<ManaShooterInfo>())
            {
                if (manabutton.Background != "bam_button_mana")
                {
                    manabutton.Background = "bam_button_mana";
                    manabutton.Disabled = false;
                    manabutton.TooltipText = "Transfer Mana";
                }
            }
            else if (manabutton.Background != "bam_zier_button_long")
            {
                manabutton.Background = "bam_zier_button_long";
                manabutton.Disabled = true;
                manabutton.TooltipText = "";
            }
        }
    }
}