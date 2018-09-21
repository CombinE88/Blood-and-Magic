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
        public CPos Location = new CPos(10, 10);

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
        private ConvertToWallButtonWidget wallbutton;
        private KillSelfWidget terminate;
        private SecondAbilityButtonWidget secondabilityButton;
        private BamRadarWidget radar;

        public ActorActionsWidget(BamUIWidget bamUi)
        {
            BamUi = bamUi;

            CreateBackground();

            AddChild(manaSend = new ManaSendButtonWidget(this) { Visible = false });
            AddChild(spawnGolem = new SpawnGolemWidget(this) { Visible = false });

            AddChild(drawActorStatisticsW = new DrawActorStatisticsWidget(this) { Visible = false });
            AddChild(drawTransformStatistics = new DrawTransformStatisticsWidget(this) { Visible = false });
            AddChild(drawValueStatistics = new DrawValueStatisticsWidget(this) { Visible = false });
            AddChild(healthBarUI = new HealthBarUIWidget(this) { Visible = false });
            AddChild(selectionName = new SelectionNameWidget(this) { Visible = false });

            AddChild(abilityButton = new AbilityButtonWidget(this) { Visible = false });
            AddChild(secondabilityButton = new SecondAbilityButtonWidget(this) { Visible = false });

            AddChild(researchEnabler = new ShowResearchButtonWidget(this) { Visible = true });

            AddChild(trinketButtons = new TrinketButtonsWidget(this) { Visible = false });
            AddChild(trinketDropButton = new TrinketDropButtonWidget(this) { Visible = false });

            AddChild(wallbutton = new ConvertToWallButtonWidget(this, -30 - 76, 50) { Visible = false });
            AddChild(terminate = new KillSelfWidget(this) { Visible = false });
        }

        public override void Tick()
        {
            Bounds = new Rectangle(5, 0, Parent.Bounds.Width, Parent.Bounds.Height);
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
            secondabilityButton.Visible = false;

            wallbutton.Visible = false;

            //// TODO hide all others here

            ActorGroup = BamUi.World.Selection.Actors.Where(a => !a.IsDead && a.IsInWorld && a.Owner == BamUi.World.LocalPlayer).ToArray();
            Actor = ActorGroup.FirstOrDefault(a => !a.IsDead && a.IsInWorld && a.Owner == BamUi.World.LocalPlayer);
            AllActor = BamUi.World.Selection.Actors.FirstOrDefault(a => !a.IsDead && a.IsInWorld);

            if (AllActor != null)
            {
                DrawActorStatistics();
            }

            if (AllActor != null && AllActor.TraitOrDefault<CanHoldTrinket>() != null && AllActor.Trait<CanHoldTrinket>().HoldsTrinket != null)
                trinketButtons.Visible = true;

            if (Actor == null)
            {
                if (transformToButtons.Any())
                    RemoveTransformmenu();

                return;
            }

            if (Actor.TraitOrDefault<Terminate>() != null)
                terminate.Visible = true;

            if (Actor.TraitOrDefault<CanHoldTrinket>() != null && Actor.Trait<CanHoldTrinket>().HoldsTrinket != null)
            {
                trinketDropButton.Visible = true;
            }

            if (Actor.TraitOrDefault<AbortConvert>() != null)
                secondabilityButton.Visible = true;

            if (Actor.Info.HasTraitInfo<HealTargetAbilityInfo>() || Actor.Info.HasTraitInfo<StealEnemyAbilityInfo>())
                abilityButton.Visible = true;

            if (Actor.Info.HasTraitInfo<ManaShooterInfo>() && Actor.Trait<ManaShooter>().CanShoot)
                manaSend.Visible = true;

            if (Actor.Info.HasTraitInfo<SpawnsAcolytesInfo>())
                spawnGolem.Visible = true;

            var ca = Actor.TraitOrDefault<ConvertAdjetant>();

            if (Actor.Info.HasTraitInfo<ConvertAdjetantInfo>() && ca.AllowTransform && ca.TransformEnabler != null)
                if (!convertToButtons.Any())
                {
                    CreateSpawnMenu();
                    wallbutton.Visible = false;
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
                wallbutton.Visible = true;
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
                            a.TraitsImplementing<TransformToBuilding>().FirstOrDefault(t => t.Buildingbelow == trait.Buildingbelow) != null).ToHashSet();

                    if (selectedValidActors.Count >= 4)
                    {
                        var con = new TransformToBuildingButtonWidget(
                            this,
                            -30 - 76,
                            50 + 68 * i,
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
                    wallbutton.Visible = false;
                }
            }
            else if (transformToButtons.Any())
            {
                RemoveTransformmenu();
            }
            else if (ca != null && !transformToButtons.Any())
            {
                wallbutton.Visible = true;
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
                        -30 - 76,
                        50 + 68 + 68 * i,
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

        public void CreateBackground()
        {
            // Minimap
            AddChild(radar = new BamRadarWidget(BamUi));

            // Background Health Frame
            AddChild(new SideBarBackgroundWidget(BamUi, 0, 200, 4, 734, 152, 17));

            // Background IconFrame
            AddChild(new SideBarBackgroundWidget(BamUi, 0, 220, 848, 0, 84, 74));

            // Background Icon
            AddChild(new SideBarBackgroundWidget(BamUi, 4, 223, 0, 214, 76, 68));

            // Background Armor Numbers
            AddChild(new SideBarBackgroundWidget(BamUi, 85, 223, 5, 754, 25, 47));

            // Background MainButton
            AddChild(new SideBarBackgroundWidget(BamUi, 0, 304, 0, 316, 180, 34));

            // Background SecondButton
            AddChild(new SideBarBackgroundWidget(BamUi, 0, 338, 0, 316, 180, 34));

            // Background TrinketFrame
            AddChild(new SideBarBackgroundWidget(BamUi, 0, 357 + 17, 848, 0, 84, 74));

            // Background Trinket
            AddChild(new SideBarBackgroundWidget(BamUi, 4, 377, 0, 350, 76, 51));

            // Background Trinket Drop Button
            AddChild(new SideBarBackgroundWidget(BamUi, 4, 428, 178, 774, 76, 17));

            // Background SpawnAbilityFrame
            AddChild(new SideBarBackgroundWidget(BamUi, 96, 373, 848, 0, 84, 74));

            // Background SpawnAbility
            AddChild(new SideBarBackgroundWidget(BamUi, 100, 377, 0, 214, 76, 68));

            for (int i = 0; i < 5; i++)
            {
                AddChild(new SideBarBackgroundWidget(BamUi, -30 - 16, 50 + 68 * i, 161, 734, 16, 68));
            }
        }
    }
}