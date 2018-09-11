using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using OpenRA.Mods.Bam.Activities;
using OpenRA.Mods.Bam.Traits.TrinketLogics;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits
{
    public class ConvertAdjetantInfo : ITraitInfo
    {
        public readonly string[] AdjetantActors = {"building1"};

        [Desc("Offset to spawn the transformed actor relative to the current cell.")]
        public readonly CVec Offset = CVec.Zero;

        [Desc("Facing that the actor must face before transforming.")]
        public readonly int Facing = 96;

        [Desc("Sounds to play when transforming.")]
        public readonly string[] TransformSounds = { };

        [Desc("Notification to play when transforming.")]
        public readonly string TransformNotification = null;

        public readonly bool SkipSelfAnimation = false;


        public object Create(ActorInitializer init)
        {
            return new ConvertAdjetant(init, this);
        }
    }

    public class ConvertAdjetant : ITick, IResolveOrder
    {
        private ConvertAdjetantInfo info;
        public bool AllowTransform;
        public Actor TransformEnabler;

        public ConvertAdjetant(ActorInitializer init, ConvertAdjetantInfo info)
        {
            this.info = info;
        }

        void ITick.Tick(Actor self)
        {
            if (self == null || self.IsDead || !self.IsInWorld)
                return;

            var cellstandingOn = self.Location;
            var sorrundingActors = self.World.FindActorsInCircle(self.World.Map.CenterOfCell(cellstandingOn), new WDist(2560))
                .Where(a =>
                    a != null
                    && a.Owner == self.Owner
                    && a.Info.HasTraitInfo<AllowConvertInfo>()
                );
            AllowTransform = sorrundingActors.Any();
            TransformEnabler = AllowTransform ? sorrundingActors.FirstOrDefault() : null;
        }

        void IResolveOrder.ResolveOrder(Actor self, Order order)
        {
            if (!order.OrderString.Contains("Convert-"))
                return;

            var orderCut = order.OrderString.Replace("Convert-", "");
            foreach (var actorname in TransformEnabler.Info.TraitInfo<AllowConvertInfo>().ConvertTo.Keys)
            {
                if (orderCut.Contains(actorname) && TransformEnabler.Info.TraitInfo<AllowConvertInfo>().ConvertTo[actorname])
                {
                    DoTransform(self, orderCut);
                    break;
                }
            }
        }

        void DoTransform(Actor self, string into)
        {
            self.QueueActivity(new AdvancedTransform(self, into)
            {
                Offset = info.Offset,
                Facing = info.Facing,
                Sounds = info.TransformSounds,
                Notification = info.TransformNotification,
                Trinket = self.Info.HasTraitInfo<CanHoldTrinketInfo>() ? self.Trait<CanHoldTrinket>().HoldsTrinket : null,
                SelfSkipMakeAnims = info.SkipSelfAnimation
            });
        }
    }
}