using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenRA.Mods.Bam.BamWidgets.Buttons;
using OpenRA.Mods.Bam.Traits;
using OpenRA.Mods.Bam.Traits.RPGTraits;
using OpenRA.Mods.Bam.Traits.TrinketLogics;
using OpenRA.Mods.Bam.Traits.UnitAbilities;
using OpenRA.Mods.Bam.Traits.World;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Warheads;
using OpenRA.Traits;
using OpenRA.Widgets;

namespace OpenRA.Mods.Bam.BamWidgets
{
    public class ActorActionsWidget : Widget
    {
        public BamUIWidget BamUi;
        public Actor Actor;
        public Actor AllActor;
        public Actor[] ActorGroup;

        private ManaSendButtonWidget manaSend;
        private SpawnGolemWidget spawnGolem;
        private TransformToBuildingButtonWidget transformToBuilding;

        private DrawActorStatisticsWidget drawActorStatisticsW;
        private TrinketButtonsWidget trinketButtons;
        private TrinketDropButtonWidget trinketDropButton;
        private DrawTransformStatisticsWidget drawTransformStatistics;
        private DrawValueStatisticsWidget drawValueStatistics;
        private HealthBarUIWidget healthBarUI;
        private SelectionNameWidget selectionName;
        private AbilityButtonWidget abilityButton;

        private List<ConvertToButtonWidget> convertToButtons = new List<ConvertToButtonWidget>();

        private ShowResearchButtonWidget researchEnabler;

        public ActorActionsWidget(BamUIWidget bamUi)
        {
            BamUi = bamUi;

            AddChild(manaSend = new ManaSendButtonWidget(this) { Visible = false });
            AddChild(spawnGolem = new SpawnGolemWidget(this) { Visible = false });

            AddChild(transformToBuilding = new TransformToBuildingButtonWidget(this) { Visible = false });

            AddChild(drawActorStatisticsW = new DrawActorStatisticsWidget(this) { Visible = false });
            AddChild(drawTransformStatistics = new DrawTransformStatisticsWidget(this) { Visible = false });
            AddChild(drawValueStatistics = new DrawValueStatisticsWidget(this) { Visible = false });
            AddChild(healthBarUI = new HealthBarUIWidget(this) { Visible = false });
            AddChild(selectionName = new SelectionNameWidget(this) { Visible = false });
            AddChild(abilityButton = new AbilityButtonWidget(this) { Visible = false });

            AddChild(researchEnabler = new ShowResearchButtonWidget(this) { Visible = true });

            AddChild(trinketButtons = new TrinketButtonsWidget(this) { Visible = false });
            AddChild(trinketDropButton = new TrinketDropButtonWidget(this) { Visible = false });
        }

        public override void Tick()
        {
            Bounds = new Rectangle(0, 0, Parent.Bounds.Width, Parent.Bounds.Height);
        }

        public override void Draw()
        {
            manaSend.Visible = false;
            spawnGolem.Visible = false;
            transformToBuilding.Visible = false;
            trinketButtons.Visible = false;
            trinketDropButton.Visible = false;

            drawActorStatisticsW.Visible = false;
            drawTransformStatistics.Visible = false;
            drawValueStatistics.Visible = false;
            healthBarUI.Visible = false;
            selectionName.Visible = false;
            abilityButton.Visible = false;

            //// TODO hide all others here

            ActorGroup = BamUi.World.Selection.Actors.Where(a => !a.IsDead && a.IsInWorld && a.Owner == BamUi.World.LocalPlayer).ToArray();
            Actor = ActorGroup.FirstOrDefault(a => !a.IsDead && a.IsInWorld && a.Owner == BamUi.World.LocalPlayer);
            AllActor = BamUi.World.Selection.Actors.FirstOrDefault(a => !a.IsDead && a.IsInWorld);

            if (AllActor != null)
                DrawActorStatistics();

            if (Actor == null)
                return;

            if (Actor.Info.HasTraitInfo<HealTargetAbilityInfo>())
                abilityButton.Visible = true;

            if (Actor.Info.HasTraitInfo<ManaShooterInfo>() && !Actor.Info.TraitInfo<ManaShooterInfo>().OnlyStores)
                manaSend.Visible = true;

            if (Actor.Info.HasTraitInfo<SpawnsAcolytesInfo>())
                spawnGolem.Visible = true;

            var ca = Actor.TraitOrDefault<ConvertAdjetant>();

            if (Actor.Info.HasTraitInfo<ConvertAdjetantInfo>() && ca.AllowTransform && ca.TransformEnabler != null)
                if (!convertToButtons.Any())
                {
                    CreateSpawnMenu();
                }
                else
                {
                    foreach (var widget in convertToButtons)
                    {
                        widget.Visible = true;
                    }
                }
            else if (convertToButtons.Any())
            {
                RemoveSpawnmenu();
            }

            if (Actor.Info.HasTraitInfo<TransformToBuildingInfo>() && Actor.Trait<TransformToBuilding>().StandsOnBuilding)
            {
                var selectedValidActors = ActorGroup
                    .Where(a =>
                        a.Info.HasTraitInfo<TransformToBuildingInfo>()
                        && a.Trait<TransformToBuilding>().StandsOnBuilding
                        && a.Trait<TransformToBuilding>().Buildingbelow == Actor.Trait<TransformToBuilding>().Buildingbelow
                        && a.IsIdle);

                if (selectedValidActors.Count() >= 4)
                {
                    transformToBuilding.SelectedValidActors = selectedValidActors.ToHashSet();
                    transformToBuilding.Visible = true;
                }
            }

            if (Actor.Info.HasTraitInfo<CanHoldTrinketInfo>() && Actor.Trait<CanHoldTrinket>().HoldsTrinket != null)
            {
                trinketButtons.Visible = true;
                if (!Actor.Info.HasTraitInfo<TransformAfterTimeInfo>())
                    trinketDropButton.Visible = true;
            }

            //// TODO add new buttons here
        }

        void CreateSpawnMenu()
        {
            var transformable = Actor.Trait<ConvertAdjetant>().TransformEnabler.Info.TraitInfo<AllowConvertInfo>().ConvertTo.ToList();
            for (int i = 0; i < transformable.Count; i++)
            {
                var con = new ConvertToButtonWidget(
                        this,
                        10 + i % 2 * 75,
                        450 + 68 * (i / 2),
                        transformable[i],
                        transformable[i])
                    { Visible = false };
                convertToButtons.Add(con);
            }

            foreach (var button in convertToButtons)
            {
                AddChild(button);
            }
        }

        void RemoveSpawnmenu()
        {
            if (convertToButtons.Any())
                foreach (var button in convertToButtons)
                {
                    RemoveChild(button);
                    button.Removed();
                }

            convertToButtons = new List<ConvertToButtonWidget>();
        }

        void DrawActorStatistics()
        {
            drawActorStatisticsW.Visible = true;
            if (AllActor.Info.HasTraitInfo<TransformAfterTimeInfo>())
                drawTransformStatistics.Visible = true;
            if (AllActor.Info.HasTraitInfo<DungeonsAndDragonsStatsInfo>())
                drawValueStatistics.Visible = true;
            if (AllActor.TraitOrDefault<IHealth>() != null)
                healthBarUI.Visible = true;
            if (AllActor.Info.HasTraitInfo<TooltipInfo>())
                selectionName.Visible = true;
        }
    }
}