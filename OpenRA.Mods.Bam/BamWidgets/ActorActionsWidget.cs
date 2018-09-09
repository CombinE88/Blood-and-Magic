using System.Drawing;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.Bam.BamWidgets.Buttons;
using OpenRA.Mods.Bam.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Widgets;

namespace OpenRA.Mods.Bam.BamWidgets
{
    public class ActorActionsWidget : Widget
    {
        public BamUIWidget BamUi;
        public Actor Actor;
        public Actor[] ActorGroup;

        private ManaSendButtonWidget manaSend;

        public ActorActionsWidget(BamUIWidget bamUi)
        {
            BamUi = bamUi;

            AddChild(manaSend = new ManaSendButtonWidget(this) {Visible = false});
        }

        public override void Tick()
        {
            Bounds = new Rectangle(0, 60, 160, 100);
        }

        public override void Draw()
        {
            WidgetUtils.FillRectWithColor(RenderBounds, Color.Pink);

            manaSend.Visible = false;
            // TODO hide all others here

            ActorGroup = BamUi.World.Selection.Actors.Where(a => !a.IsDead && a.IsInWorld && a.Owner == BamUi.World.LocalPlayer).ToArray();
            Actor = ActorGroup.FirstOrDefault(a => !a.IsDead && a.IsInWorld && a.Owner == BamUi.World.LocalPlayer);

            if (Actor == null)
                return;

            var animation = new Animation(BamUi.World, Actor.Trait<RenderSprites>().GetImage(Actor));
            animation.PlayFetchIndex("icon", () => 0);
            WidgetUtils.DrawSHPCentered(animation.Image, new float2(RenderBounds.X, RenderBounds.Y), BamUi.Palette);

            if (Actor.Info.HasTraitInfo<ManaShooterInfo>() && !Actor.Info.TraitInfo<ManaShooterInfo>().OnlyStores)
                manaSend.Visible = true;
            // TODO add new buttons here
        }
    }
}