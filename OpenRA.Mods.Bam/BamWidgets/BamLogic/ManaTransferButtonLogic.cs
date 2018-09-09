using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.Bam.BamWidgets;
using OpenRA.Mods.Bam.Traits;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Widgets;
using OpenRA.Widgets;

namespace OpenRA.Mods.Bam.Widgets.Logic
{
    public class ManaTransferButtonLogic : ChromeLogic
    {
        private List<Actor> unitselected;
        private List<Actor> mainbuildings;
        ExtendetButtonWidget manabutton;
        ButtonWidget secondbutton;
        LabelWidget manalabel;
        private World world;

        [ObjectCreator.UseCtor]
        public ManaTransferButtonLogic(Widget widget, World world, WorldRenderer worldRenderer)
        {
            this.world = world;

            manalabel = widget.Get<LabelWidget>("TEXTLABEL");
            manabutton = widget.Get<ExtendetButtonWidget>("MANA_BUTTON");
            secondbutton = widget.Get<ButtonWidget>("SPAWN_BUTTON");
            manabutton.OnClick = () =>
            {
                if (mainbuildings.Any())
                {
                    var self = mainbuildings.First();
                    if (self == null || self.IsDead || !self.IsInWorld)
                        return;

                    self.World.IssueOrder( new Order("SpawnAcolyte", self, false));
                }
                else if (unitselected.Any())
                    foreach (var actor in unitselected)
                    {
                        if (actor != null && actor.IsInWorld && !actor.IsDead)
                            actor.World.IssueOrder( new Order("ShootMana", actor, false));
                    }
            };

            manabutton.Visible = false;
            manalabel.Visible = false;
            secondbutton.Visible = false;
        }

        public override void Tick()
        {
            unitselected = world.Selection.Actors
                .Where(a => a.Info.HasTraitInfo<ManaShooterInfo>() && !a.IsDead && a.IsInWorld && a.Owner == world.LocalPlayer)
                .ToList();

            mainbuildings = world.Selection.Actors
                .Where(a => a.Info.HasTraitInfo<SpawnsAcolytesInfo>() && !a.IsDead && a.IsInWorld && a.Owner == world.LocalPlayer)
                .ToList();

            if (mainbuildings.Any())
            {
                manabutton.Background = "bam_button_handymbol";
                manalabel.Text = "Spawn Golem";
                manabutton.Visible = true;
                manalabel.Visible = true;
            }
            else if (unitselected.Any())
            {
                manabutton.Background = "bam_button_mana";
                manalabel.Text = "Transferer";
                manabutton.Actor = unitselected.FirstOrDefault();
                manabutton.Visible = true;
                manalabel.Visible = true;
            }
            else
            {
                manalabel.Text = "";
                manabutton.Visible = false;
                manalabel.Visible = false;
            }
        }
    }
}