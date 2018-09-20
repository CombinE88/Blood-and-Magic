using System.Drawing;
using System.Linq;
using OpenRA.Mods.Bam.Traits.Player;
using OpenRA.Mods.Bam.Traits.RPGTraits;
using OpenRA.Mods.Bam.Traits.TrinketLogics;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Effects;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits
{
    public class SpawnsAcolytesInfo : ITraitInfo
    {
        public readonly string Actor = "acolyte";
        public readonly string Image = "explosion";
        public readonly string EffectSequence = "smokecloud_effect";
        public readonly string EffectPalette = "bam11195";

        public object Create(ActorInitializer init)
        {
            return new SpawnsAcolytes(init, this);
        }
    }

    public class SpawnsAcolytes : IResolveOrder, INotifyCreated, ITick
    {
        private SpawnsAcolytesInfo info;
        private PlayerResources pr;
        private int tick;

        public SpawnsAcolytes(ActorInitializer init, SpawnsAcolytesInfo info)
        {
            this.info = info;
        }

        public void ResolveOrder(Actor self, Order order)
        {
            if (order.OrderString != "SpawnAcolyte")
                return;

            if (tick < 25)
                return;

            tick = 0;

            var findEmptyActor = self.World.FindActorsInCircle(self.World.Map.CenterOfCell(self.Location + self.Info.TraitInfo<ExitInfo>().ExitCell), new WDist(265)).ToArray();
            var sortetActors = findEmptyActor.Where(a => a.TraitOrDefault<DungeonsAndDragonsStats>() != null && a.Trait<Mobile>() != null).ToArray();

            if (pr.TakeCash(self.World.Map.Rules.Actors[info.Actor].TraitInfo<ValuedInfo>().Cost))
            {
                self.World.AddFrameEndTask(w =>
                {
                    var init = new TypeDictionary
                    {
                        new LocationInit(self.World.Map.CellContaining(self.CenterPosition + self.Info.TraitInfo<ExitInfo>().SpawnOffset)),
                        new CenterPositionInit(self.CenterPosition + self.Info.TraitInfo<ExitInfo>().SpawnOffset),
                        new OwnerInit(self.Owner),
                        new FacingInit(250)
                    };
                    var a = w.CreateActor(info.Actor, init);
                    if (a != null && !a.IsDead && a.IsInWorld)
                    {
                        w.Add(new SpriteEffect(
                            a.CenterPosition,
                            w,
                            info.Image,
                            info.EffectSequence,
                            info.EffectPalette));

                        Game.Sound.Play(SoundType.World, "7226.wav", self.CenterPosition, 0.3f);

                        var move = a.TraitOrDefault<IMove>();
                        a.QueueActivity(move.MoveIntoWorld(a, self.Location + self.Info.TraitInfo<ExitInfo>().ExitCell));

                        a.QueueActivity(move.MoveTo(self.Trait<RallyPoint>().Location, 2));

                        var exp = a.Owner.PlayerActor.TraitOrDefault<DungeonsAndDragonsExperience>();
                        if (exp != null)
                        {
                            if (a.Info.HasTraitInfo<ValuedInfo>())
                                exp.AddCash(a.Info.TraitInfo<ValuedInfo>().Cost / 2);
                        }

                        if (sortetActors.Any())
                        {
                            foreach (var actor in sortetActors)
                            {
                                actor.Trait<Mobile>().Nudge(actor, a, true);
                            }
                        }
                    }
                });
            }
            else if (pr.Cash + pr.Resources < self.World.Map.Rules.Actors[info.Actor].TraitInfo<ValuedInfo>().Cost)
            {
                Game.Sound.PlayNotification(
                    self.World.Map.Rules,
                    self.Owner,
                    "Speech",
                    "LowMana",
                    self.World.LocalPlayer.Faction.InternalName);
            }
        }

        void INotifyCreated.Created(Actor self)
        {
            pr = self.Owner.PlayerActor.Trait<PlayerResources>();
        }

        public void Tick(Actor self)
        {
            if (tick++ < 25)
                return;
        }
    }
}