using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using OpenRA.Activities;
using OpenRA.Graphics;
using OpenRA.Mods.Bam.Activities;
using OpenRA.Mods.Bam.Traits.TrinketLogics;
using OpenRA.Mods.Bam.Traits.World;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Effects;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits
{
    public class ConvertAdjetantInfo : ITraitInfo
    {
        [Desc("Offset to spawn the transformed actor relative to the current cell.")]
        public readonly CVec Offset = CVec.Zero;

        [Desc("Facing that the actor must face before transforming.")]
        public readonly int Facing = 96;

        [Desc("Sounds to play when transforming.")]
        public readonly string Capsule = "capsule";

        [Desc("Sounds to play when transforming.")]
        public readonly string[] TransformSounds = { };

        [Desc("Notification to play when transforming.")]
        public readonly string TransformNotification = null;

        public readonly string WallActor = "playerwall";

        public readonly string Image = "explosion";
        public readonly string EffectSequence = "smokecloud_effect";
        public readonly string EffectPalette = "bam11195";

        public readonly bool SkipSelfAnimation = false;

        public object Create(ActorInitializer init)
        {
            return new ConvertAdjetant(init, this);
        }
    }

    public class ConvertAdjetant : ITick, IResolveOrder
    {
        private ConvertAdjetantInfo Info;
        public bool AllowTransform;
        public Actor TransformEnabler;

        private string orderCut;
        public bool Disabled;

        public ConvertAdjetant(ActorInitializer init, ConvertAdjetantInfo info)
        {
            Info = info;
        }

        void ITick.Tick(Actor self)
        {
            if (self == null || self.IsDead || !self.IsInWorld)
                return;

            Disabled = self.World.WorldActor.Trait<BuildingInfluence>().GetBuildingAt(self.Location) != null
                       || self.World.Map.GetTerrainInfo(self.Location).Type == "Manaspot";

            var cellstandingOn = self.Location;
            var sorrundingActors = self.World.FindActorsInCircle(self.World.Map.CenterOfCell(cellstandingOn), new WDist(2560))
                .Where(a =>
                    a != null
                    && a.Owner == self.Owner
                    && a.Info.HasTraitInfo<AllowConvertInfo>());

            AllowTransform = sorrundingActors.Any();
            TransformEnabler = AllowTransform ? sorrundingActors.FirstOrDefault() : null;
        }

        void IResolveOrder.ResolveOrder(Actor self, Order order)
        {
            if (!order.OrderString.Contains("Convert-") && order.OrderString != "QuickWall")
                return;


            if (order.OrderString == "QuickWall" && !self.IsDead && self.IsInWorld)
            {
                if (!Disabled && self.Owner.PlayerActor.Trait<PlayerResources>().TakeCash(self.World.Map.Rules.Actors[Info.WallActor].TraitInfo<ValuedInfo>().Cost))
                {
                    TurnIntoWall(self);
                }

                return;
            }

            orderCut = order.OrderString.Replace("Convert-", "");

            self.CancelActivity();
            foreach (var actorname in TransformEnabler.Info.TraitInfoOrDefault<AllowConvertInfo>().ConvertTo)
            {
                if (orderCut.Contains(actorname))
                {
                    DoTransform(self, Info.Capsule);
                    break;
                }
            }
        }

        void TurnIntoWall(Actor self)
        {
            var location = self.Location;
            var ownerSelf = self.Owner;
            var health = self.TraitOrDefault<Health>();

            self.Dispose();

            self.World.AddFrameEndTask(w =>
            {
                var init = new TypeDictionary
                {
                    new LocationInit(location),
                    new OwnerInit(ownerSelf)
                };

                if (health != null)
                {
                    // Cast to long to avoid overflow when multiplying by the health
                    var newHP = (int)(health.HP * 100L / health.MaxHP);
                    init.Add(new HealthInit(newHP));
                }

                var actor = w.CreateActor(Info.WallActor, init);

                foreach (var cell in actor.Info.TraitInfo<BuildingInfo>().Tiles(actor.Location))
                {
                    w.Add(new SpriteEffect(
                        actor.World.Map.CenterOfCell(cell),
                        w,
                        Info.Image,
                        Info.EffectSequence,
                        Info.EffectPalette));
                }
            });
        }

        void DoTransform(Actor self, string into)
        {
            self.CancelActivity();
            self.QueueActivity(new CallFunc(() =>
            {
                if (self.Owner.PlayerActor.Trait<PlayerResources>().TakeCash(self.World.Map.Rules.Actors[orderCut].TraitInfo<ValuedInfo>().Cost))
                {
                    self.World.AddFrameEndTask(w =>
                        w.Add(new SpriteEffect(
                            self.CenterPosition,
                            w,
                            self.World.Map.Rules.Actors[into].TraitInfo<RenderSpritesInfo>().Image,
                            "transform",
                            self.World.Map.Rules.Actors[into].TraitInfo<RenderSpritesInfo>().PlayerPalette + self.Owner.InternalName)));

                    self.QueueActivity(new Wait(5));
                    self.QueueActivity(new AdvancedTransform(self, into)
                    {
                        Time = self.World.Map.Rules.Actors[orderCut].TraitInfo<ResearchableInfo>().TransformTime * 25,
                        CapsuleInto = orderCut,

                        Offset = Info.Offset,
                        Facing = Info.Facing,
                        Sounds = Info.TransformSounds,
                        Notification = Info.TransformNotification,
                        Trinket = self.Info.HasTraitInfo<CanHoldTrinketInfo>() ? self.Trait<CanHoldTrinket>().HoldsTrinket : null,
                        SelfSkipMakeAnims = Info.SkipSelfAnimation,
                        RallyPointActor = TransformEnabler,
                        UseRallyPoint = true
                    });
                }
            }, false));
        }
    }
}