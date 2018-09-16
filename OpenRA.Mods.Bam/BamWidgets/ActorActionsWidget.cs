using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenRA.Mods.Bam.BamWidgets.Buttons;
using OpenRA.Mods.Bam.Traits;
using OpenRA.Mods.Bam.Traits.RPGTraits;
using OpenRA.Mods.Bam.Traits.TrinketLogics;
using OpenRA.Mods.Bam.Traits.UnitAbilities;
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

        private DrawActorStatisticsWidget drawActorStatisticsW;
        private TrinketButtonsWidget trinketButtons;
        private TrinketDropButtonWidget trinketDropButton;
        private DrawTransformStatisticsWidget drawTransformStatistics;
        private DrawValueStatisticsWidget drawValueStatistics;
        private HealthBarUIWidget healthBarUI;
        private SelectionNameWidget selectionName;
        private AbilityButtonWidget abilityButton;

        private List<ConvertToButtonWidget> convertToButtons = new List<ConvertToButtonWidget>();
        private List<TransformToBuildingButtonWidget> transformToButtons = new List<TransformToBuildingButtonWidget>();


        private ShowResearchButtonWidget researchEnabler;

        public ActorActionsWidget(BamUIWidget bamUi)
        {
            BamUi = bamUi;

            AddChild(manaSend = new ManaSendButtonWidget(this) { Visible = false });
            AddChild(spawnGolem = new SpawnGolemWidget(this) { Visible = false });

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

            if (AllActor != null && AllActor.TraitOrDefault<CanHoldTrinket>() != null && AllActor.Trait<CanHoldTrinket>().HoldsTrinket != null)
                trinketButtons.Visible = true;

            if (Actor == null)
                return;

            if (Actor.TraitOrDefault<CanHoldTrinket>() != null && Actor.Trait<CanHoldTrinket>().HoldsTrinket != null)
            {
                trinketDropButton.Visible = true;
            }

            if (Actor.Info.HasTraitInfo<HealTargetAbilityInfo>() || Actor.Info.HasTraitInfo<StealEnemyAbilityInfo>())
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

            var traits = Actor.TraitsImplementing<TransformToBuilding>().Where(t => t.StandsOnBuilding && t.Info.Factions.Contains(Actor.Owner.Faction.InternalName)).ToList();

            if (traits.Any() && !transformToButtons.Any())
            {
                var i = 0;

                foreach (var trait in traits)
                {
                    if (!trait.Info.Factions.Contains(Actor.Owner.Faction.InternalName))
                        continue;

                    var selectedValidActors = ActorGroup
                        .Where(a =>
                            a.TraitsImplementing<TransformToBuilding>().FirstOrDefault(t => t.Buildingbelow == trait.Buildingbelow) != null
                            && a.IsIdle).ToHashSet();

                    if (selectedValidActors.Count >= 4)
                    {
                        var con = new TransformToBuildingButtonWidget(
                            this,
                            10 + i % 2 * 75,
                            450 + 68 * (i / 2),
                            trait.Info.IntoBuilding,
                            trait,
                            selectedValidActors) { Visible = true };
                        transformToButtons.Add(con);

                        AddChild(con);
                        i += 1;
                    }
                }
            }
            else if (traits.Any() && transformToButtons.Any())
            {
                foreach (var widget in transformToButtons)
                {
                    widget.Visible = true;
                }
            }
            else if (transformToButtons.Any())
            {
                RemoveTransformmenu();
            }
        }

//// TODO add new buttons here

        void CreateSpawnMenu()
        {
            var transformable = Actor.Trait<ConvertAdjetant>().TransformEnabler.Info.TraitInfo<AllowConvertInfo>().ConvertTo.ToList();
            for (int i = 0;
                i < transformable.Count;
                i++)
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

        public void RemoveTransformmenu()
        {
            if (transformToButtons.Any())
                foreach (var button in transformToButtons)
                {
                    RemoveChild(button);
                    button.Removed();
                }

            transformToButtons = new List<TransformToBuildingButtonWidget>();
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