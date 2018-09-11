using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenRA.Mods.Bam.BamWidgets.Buttons;
using OpenRA.Mods.Bam.Traits;
using OpenRA.Mods.Bam.Traits.RPGTraits;
using OpenRA.Mods.Bam.Traits.TrinketLogics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;
using OpenRA.Widgets;

namespace OpenRA.Mods.Bam.BamWidgets
{
    public class ActorActionsWidget : Widget
    {
        public BamUIWidget BamUi;
        public Actor Actor;
        public Actor[] ActorGroup;

        private ManaSendButtonWidget manaSend;
        private SpawnGolemWidget SpawnGolem;
        private TransformToBuildingButtonWidget TransformToBuilding;

        private DrawActorStatisticsWidget DrawActorStatisticsW;
        private TrinketButtonsWidget TrinketButtons;
        private TrinketDropButtonWidget TrinketDropButton;
        private DrawTransformStatisticsWidget drawTransformStatistics;
        private DrawValueStatisticsWidget DrawValueStatistics;
        private HealthBarUIWidget HealthBarUI;
        private SelectionNameWidget SelectionName;

        private List<ConvertToButtonWidget> ConvertToButtons = new List<ConvertToButtonWidget>();

        public ActorActionsWidget(BamUIWidget bamUi)
        {
            BamUi = bamUi;

            AddChild(manaSend = new ManaSendButtonWidget(this) {Visible = false});
            AddChild(SpawnGolem = new SpawnGolemWidget(this) {Visible = false});

            AddChild(TransformToBuilding = new TransformToBuildingButtonWidget(this) {Visible = false});

            AddChild(DrawActorStatisticsW = new DrawActorStatisticsWidget(this) {Visible = false});
            AddChild(drawTransformStatistics = new DrawTransformStatisticsWidget(this) {Visible = false});
            AddChild(DrawValueStatistics = new DrawValueStatisticsWidget(this) {Visible = false});
            AddChild(HealthBarUI = new HealthBarUIWidget(this) {Visible = false});
            AddChild(SelectionName = new SelectionNameWidget(this) {Visible = false});



            AddChild(TrinketButtons = new TrinketButtonsWidget(this) {Visible = false});
            AddChild(TrinketDropButton = new TrinketDropButtonWidget(this) {Visible = false});
        }

        public override void Tick()
        {
            Bounds = new Rectangle(0, 0, Parent.Bounds.Width, Parent.Bounds.Height);
        }

        public override void Draw()
        {
            //WidgetUtils.FillRectWithColor(RenderBounds, Color.Pink);

            manaSend.Visible = false;
            SpawnGolem.Visible = false;
            TransformToBuilding.Visible = false;
            TrinketButtons.Visible = false;
            TrinketDropButton.Visible = false;

            DrawActorStatisticsW.Visible = false;
            drawTransformStatistics.Visible = false;
            DrawValueStatistics.Visible = false;
            HealthBarUI.Visible = false;
            SelectionName.Visible = false;

            // TODO hide all others here

            ActorGroup = BamUi.World.Selection.Actors.Where(a => !a.IsDead && a.IsInWorld && a.Owner == BamUi.World.LocalPlayer).ToArray();
            Actor = ActorGroup.FirstOrDefault(a => !a.IsDead && a.IsInWorld && a.Owner == BamUi.World.LocalPlayer);

            if (Actor == null)
                return;

            DrawActorStatistics();

            if (Actor.Info.HasTraitInfo<ManaShooterInfo>() && !Actor.Info.TraitInfo<ManaShooterInfo>().OnlyStores)
                manaSend.Visible = true;

            if (Actor.Info.HasTraitInfo<SpawnsAcolytesInfo>())
                SpawnGolem.Visible = true;

            var ca = Actor.TraitOrDefault<ConvertAdjetant>();
            if (Actor.Info.HasTraitInfo<ConvertAdjetantInfo>() && ca.AllowTransform && ca.TransformEnabler != null)
                if (!ConvertToButtons.Any())
                {
                    CreateSpawnMenu();
                }
                else
                {
                    foreach (var widget in ConvertToButtons)
                    {
                        widget.Visible = true;
                    }
                }
            else if (ConvertToButtons.Any())
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
                        && a.IsIdle
                    );

                if (selectedValidActors.Count() >= 4)
                {
                    TransformToBuilding.SelectedValidActors = selectedValidActors.ToHashSet();
                    TransformToBuilding.Visible = true;
                }
            }

            if (Actor.Info.HasTraitInfo<CanHoldTrinketInfo>() && Actor.Trait<CanHoldTrinket>().HoldsTrinket != null)
            {
                TrinketButtons.Visible = true;
                if (!Actor.Info.HasTraitInfo<TransformAfterTimeInfo>())
                    TrinketDropButton.Visible = true;
            }

            // TODO add new buttons here
        }

        void CreateSpawnMenu()
        {
            var transformable = Actor.Trait<ConvertAdjetant>().TransformEnabler.Trait<AllowConvert>().Transformable;
            for (int i = 0; i < transformable.Count; i++)
            {
                var con = new ConvertToButtonWidget(this, 0, 430 + 68 * i, transformable[i].Item2, transformable[i].Item1, transformable[i].Item1) {Visible = false};
                ConvertToButtons.Add(con);
            }

            foreach (var button in ConvertToButtons)
            {
                AddChild(button);
            }
        }

        void RemoveSpawnmenu()
        {
            if (ConvertToButtons.Any())
                foreach (var button in ConvertToButtons)
                {
                    RemoveChild(button);
                    button.Removed();
                }

            ConvertToButtons = new List<ConvertToButtonWidget>();
        }

        void DrawActorStatistics()
        {
            // Draw Picture
            DrawActorStatisticsW.Visible = true;

            if (Actor.Info.HasTraitInfo<TransformAfterTimeInfo>())
                drawTransformStatistics.Visible = true;

            if (Actor.Info.HasTraitInfo<DungeonsAndDragonsStatsInfo>())
                DrawValueStatistics.Visible = true;

            if(Actor.TraitOrDefault<IHealth>() != null)
                HealthBarUI.Visible = true;

            if(Actor.Info.HasTraitInfo<TooltipInfo>())
                SelectionName.Visible = true;

        }
    }
}