using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using OpenRA.Graphics;
using OpenRA.Mods.Bam.BamWidgets;
using OpenRA.Mods.Bam.Traits;
using OpenRA.Mods.Bam.Traits.TrinketLogics;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Widgets;
using OpenRA.Support;
using OpenRA.Widgets;

namespace OpenRA.Mods.Bam.Widgets.Logic
{
    public class BamButtonLogic : ChromeLogic
    {
        private IEnumerable<Actor> selectedUnits = new List<Actor>();

        private List<Actor> unitselected;
        private List<Actor> mainbuildings;
        private List<Actor> selectedConvert;

        private List<Actor> validSelection;

        private Actor activeActor;

        ExtendetButtonWidget manabutton;
        ButtonWidget secondbutton;
        LabelWidget manalabel;
        LabelWidget secondlabel;

        private bool queueVisible = false;

        private BackgroundWidget transformbackground;
        private ButtonWidget transformButton;

        private ButtonWidget dropItemButton;

        private World world;

        [ObjectCreator.UseCtor]
        public BamButtonLogic(Widget widget, World world, WorldRenderer worldRenderer)
        {
            this.world = world;

            selectedUnits = world.Selection.Actors
                .Where(a => a != null && !a.IsDead && a.IsInWorld && a.Owner == world.LocalPlayer);

            manalabel = widget.Get<LabelWidget>("TEXTLABEL");
            manabutton = widget.Get<ExtendetButtonWidget>("MANA_BUTTON");
            manabutton.OnClick = () => { HandleFirstInput(); };

            secondlabel = widget.Get<LabelWidget>("SECONDTEXTLABEL");
            secondbutton = widget.Get<ButtonWidget>("SPAWN_BUTTON");
            secondbutton.OnClick = () => { HandleSecondInput(); };

            manabutton.Visible = false;
            manalabel.Visible = false;
            secondbutton.Visible = false;
            secondlabel.Visible = false;

            //// handle TransformButtons

            transformbackground = widget.Get<BackgroundWidget>("TRANSFORM_CONTAINER");
            transformButton = widget.Get<ButtonWidget>("TRANSFORM_ICON_BUTTON");

            transformbackground.Visible = false;
            transformButton.Visible = false;

            transformButton.OnClick = () => { HandleTransformButtonInput(); };

            dropItemButton = widget.Get<ButtonWidget>("ITEM_DROP_BUTTON");
            dropItemButton.OnClick = () =>
            {
                if (activeActor!= null && activeActor.Info.HasTraitInfo<CanHoldTrinketInfo>() && activeActor.Trait<CanHoldTrinket>().HoldsTrinket != null)
                {
                    activeActor.World.IssueOrder(new Order("dropItem", activeActor, false));
                }
            };
        }


        public override void Tick()
        {
            selectedUnits = world.Selection.Actors
                .Where(a => a != null && !a.IsDead && a.IsInWorld && a.Owner == world.LocalPlayer);

            if (selectedUnits.Any())
                activeActor = selectedUnits.First();

            unitselected = selectedUnits
                .Where(a => a.Info.HasTraitInfo<ManaShooterInfo>() && !a.IsDead && a.IsInWorld)
                .ToList();

            mainbuildings = selectedUnits
                .Where(a => a.Info.HasTraitInfo<SpawnsAcolytesInfo>() && !a.IsDead && a.IsInWorld)
                .ToList();

            selectedConvert = selectedUnits
                .Where(a => a.Info.HasTraitInfo<ConvertAdjetantInfo>() && a.Trait<ConvertAdjetant>().AllowTransform)
                .ToList();

            HandleTransformButton();
            HandleFirstButton();
            HandleSecondButton();
            HandleDropButton();
        }

        void HandleDropButton()
        {
            if (selectedUnits.Any() && activeActor != null)
            {
                dropItemButton.Visible = true;
            }
            else
            {
                dropItemButton.Visible = false;
            }
        }

        void HandleTransformButtonInput()
        {
            if (selectedConvert.Any())
            {
                foreach (var actor in selectedConvert)
                {
                    actor.World.IssueOrder(new Order("Convert-" + "capsule.warrior", actor, false));
                    ;
                }
            }
        }

        void HandleFirstInput()
        {
            if (mainbuildings.Any())
            {
                foreach (var actor in mainbuildings)
                {
                    if (actor != null && actor.IsInWorld && !actor.IsDead)
                        actor.World.IssueOrder(new Order("SpawnAcolyte", actor, false));
                }
            }
            else if (unitselected.Any())
                foreach (var actor in unitselected)
                {
                    if (actor != null && actor.IsInWorld && !actor.IsDead)
                        actor.World.IssueOrder(new Order("ShootMana", actor, false));
                }
        }

        void HandleSecondInput()
        {
            var standingUnitselected = unitselected
                .Where
                (a =>
                    a.Info.HasTraitInfo<TransformToBuildingInfo>()
                    && a.Trait<TransformToBuilding>().StandsOnBuilding
                ).ToList();

            if (standingUnitselected.Any() && standingUnitselected.Count >= 4)
            {
                var converterBuilding = unitselected.First().Trait<TransformToBuilding>().Buildingbelow;

                var unitselectedTransform = unitselected
                    .Where
                    (a =>
                        a.Info.HasTraitInfo<TransformToBuildingInfo>()
                        && a.Trait<TransformToBuilding>().StandsOnBuilding
                        && a.Trait<TransformToBuilding>().Buildingbelow == converterBuilding
                    )
                    .ToList();

                if (unitselectedTransform.Count >= 4)
                {
                    foreach (var actor in standingUnitselected)
                    {
                        if (actor != standingUnitselected.Last())
                            actor.World.IssueOrder(new Order("RemoveSelf", actor, false));
                        else
                            actor.World.IssueOrder(new Order("TransformTo", actor, false));
                    }
                }
            }
            else if (selectedConvert.Any())
            {
                secondbutton.Visible = false;
                secondlabel.Visible = false;
                queueVisible = true;

                var aviableTransforms = selectedConvert.FirstOrDefault().Trait<ConvertAdjetant>().TransformEnabler;
                transformbackground.RemoveChildren();
            }
        }

        void HandleFirstButton()
        {
            var AllowedShooter = unitselected
                .Where(a => !a.Info.TraitInfo<ManaShooterInfo>().OnlyStores);

            if (mainbuildings.Any())
            {
                manabutton.Background = "bam_button_handymbol";
                manalabel.Text = "Spawn Golem";

                manabutton.Visible = true;
                manalabel.Visible = true;
            }
            else if (AllowedShooter.Any())
            {
                manabutton.Background = "bam_button_mana";
                manalabel.Text = "Transferer";
                manabutton.Actor = unitselected.FirstOrDefault();

                manabutton.Visible = true;
                manalabel.Visible = true;
            }
            else
            {
                manabutton.Visible = false;
                manalabel.Visible = false;
            }
        }

        void HandleSecondButton()
        {
            if (validSelection.Count >= 4 && !queueVisible)
            {
                secondbutton.Background = "bam_button_handymbol";
                secondlabel.Text = "Transform";

                secondbutton.Visible = true;
                secondlabel.Visible = true;
            }
            else if (selectedConvert.Any() && queueVisible)
            {
                secondbutton.Visible = false;
                secondlabel.Visible = false;

                transformbackground.Visible = true;
                transformButton.Visible = true;
            }
            else if (selectedConvert.Any() && !queueVisible)
            {
                secondbutton.Background = "bam_button_handymbol";
                secondlabel.Text = "Convert";

                secondbutton.Visible = true;
                secondlabel.Visible = true;

                transformbackground.Visible = false;
                transformButton.Visible = false;
            }
            else
            {
                if (!selectedConvert.Any())
                {
                    transformbackground.Visible = false;
                    transformButton.Visible = false;
                    queueVisible = false;
                }

                secondbutton.Visible = false;
                secondlabel.Visible = false;
            }
        }

        void HandleTransformButton()
        {
            var newSelection = world.Selection.Actors
                .Where(a =>
                    a != null
                    && !a.IsDead
                    && a.IsInWorld
                    && a.Info.HasTraitInfo<TransformToBuildingInfo>()
                    && a.Owner == world.LocalPlayer
                    && a.Trait<TransformToBuilding>().StandsOnBuilding
                )
                .ToList();

            if (newSelection.Any())
            {
                var building = newSelection.FirstOrDefault().Trait<TransformToBuilding>().Buildingbelow;

                if (building != null && building.IsInWorld)
                {
                    validSelection = newSelection.Where(a => a.Trait<TransformToBuilding>().Buildingbelow == building).ToList();
                }
            }
            else
            {
                validSelection = new List<Actor>();
            }
        }
    }
}