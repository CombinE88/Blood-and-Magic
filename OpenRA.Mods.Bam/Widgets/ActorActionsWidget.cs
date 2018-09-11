using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenRA.Mods.Bam.BamWidgets.Buttons;
using OpenRA.Mods.Bam.Traits;
using OpenRA.Mods.Bam.Traits.Mana;
using OpenRA.Mods.Bam.Traits.Transform;
using OpenRA.Mods.Bam.Traits.Trinkets;
using OpenRA.Mods.Bam.Widgets;
using OpenRA.Mods.Bam.Widgets.Buttons;
using OpenRA.Mods.Common.Traits;
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
            AllActor = BamUi.World.Selection.Actors.FirstOrDefault(a => !a.IsDead && a.IsInWorld);

            if (AllActor != null)
                DrawActorStatistics();

            if (Actor == null)
                return;

            if (Actor.Info.HasTraitInfo<ManaGeneratorInfo>())
                manaSend.Visible = true;

            if (Actor.Info.HasTraitInfo<SpawnActorInfo>())
                SpawnGolem.Visible = true;

            var ca = Actor.TraitOrDefault<TransformToUnit>();
            if (ca != null)
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

            if (Actor.Info.HasTraitInfo<TransformToBuildingInfo>() && Actor.Trait<TransformToBuilding>().CanTransform(Actor))
            {
                TransformToBuilding.Visible = true;
            }

            if (Actor.Info.HasTraitInfo<CanHoldTrinketInfo>() && Actor.Trait<CanHoldTrinket>().Current != null)
            {
                TrinketButtons.Visible = true;
                if(!Actor.Info.HasTraitInfo<TransformOnIdleInfo>())
                    TrinketDropButton.Visible = true;
            }

            // TODO add new buttons here
        }

        void CreateSpawnMenu()
        {
            var transformable = Game.ModData.DefaultRules.Actors.Where(a =>
            {
                var buildableInfo = a.Value.TraitInfoOrDefault<BuildableInfo>();
                return buildableInfo != null && buildableInfo.Queue.Contains(Actor.Info.Name);
            }).Select(a => a.Key).ToArray();

            for (int i = 0; i < transformable.Length; i++)
            {
                var con = new ConvertToButtonWidget(
                    this,
                    10 + i % 2 * 75,
                    450 + 68 * (i / 2),
                    transformable[i],
                    transformable[i]);
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

            if (AllActor.Info.HasTraitInfo<TransformOnIdleInfo>())
                drawTransformStatistics.Visible = true;

            if (AllActor.TraitOrDefault<IHealth>() != null)
                HealthBarUI.Visible = true;

            if (AllActor.Info.HasTraitInfo<TooltipInfo>())
                SelectionName.Visible = true;

        }
    }
}
