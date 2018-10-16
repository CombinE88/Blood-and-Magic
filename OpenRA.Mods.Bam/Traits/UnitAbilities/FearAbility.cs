using System.Collections.Generic;
using System.Linq;
using OpenRA.Effects;
using OpenRA.Mods.Bam.Traits.Render;
using OpenRA.Mods.Bam.Traits.RPGTraits;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Effects;
using OpenRA.Mods.Common.Orders;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits.UnitAbilities
{
    public class FearAbilityInfo : ITraitInfo, Requires<ConditionManagerInfo>
    {
        public readonly int Range = 4;

        public readonly string Cursor = "ability";

        public readonly string AbilityString = "Spread Fear";

        public readonly int Ammount = 10;

        public readonly int Delay = 450;

        public readonly string ConditionToGrant = "NotLured";

        public readonly string Image = "explosion";
        public readonly string EffectSequence = "teleport_effect";
        public readonly string EffectPalette = "bam11195";

        public readonly string Sound = "7300.wav";

        public object Create(ActorInitializer init)
        {
            return new FearAbility(this);
        }
    }

    public class FearAbility : IIssueOrder, IResolveOrder, ITick
    {
        readonly FearAbilityInfo info;
        public int CurrentDelay;
        private List<Actor> fearer = new List<Actor>();

        public FearAbility(FearAbilityInfo info)
        {
            this.info = info;
        }

        public IEnumerable<IOrderTargeter> Orders
        {
            get { yield return new FearAbilityOrderTargeter(info.Cursor, info.Range, info.Ammount); }
        }

        public Order IssueOrder(Actor self, IOrderTargeter order, Target target, bool queued)
        {
            if (order.OrderID != "FearAllTarget")
                return null;

            return new Order(order.OrderID, self, target, queued);
        }

        public void ResolveOrder(Actor self, Order order)
        {
            if (order.OrderString != "FearAllTarget")
                return;

            if (CurrentDelay < info.Delay)
                return;

            var pr = self.Owner.PlayerActor.Trait<PlayerResources>();

            if (!pr.TakeCash(info.Ammount))
                return;

            var actors = self.World.FindActorsInCircle(self.CenterPosition, WDist.FromCells(5)).Where(a => a.TraitOrDefault<DungeonsAndDragonsStats>() != null);
            foreach (var actor in actors.Where(a => !a.Info.TraitInfo<DungeonsAndDragonsStatsInfo>().IgnoresAbilites.Contains("Fear")
                                                    && a.Info.TraitInfo<DungeonsAndDragonsStatsInfo>().Attributes.Contains("Civilized")))
            {
                var external = actor.TraitsImplementing<ExternalCondition>()
                    .FirstOrDefault(t => t.Info.Condition == info.ConditionToGrant && t.CanGrantCondition(actor, self));

                if (external != null)
                {
                    actor.CancelActivity();

                    external.GrantCondition(actor, info.ConditionToGrant, 125);

                    self.World.Add(new DelayedAction(125, () =>
                    {
                        actor.CancelActivity();
                        fearer = new List<Actor>();
                    }));

                    Game.Sound.Play(SoundType.World, info.Sound, actor.CenterPosition);
                    self.World.AddFrameEndTask(w =>
                        w.Add(new SpriteEffect(
                            actor.CenterPosition,
                            w,
                            info.Image,
                            info.EffectSequence,
                            info.EffectPalette)));

                    actor.QueueActivity(new Move(actor, actor.Location - new CVec(self.Location.X - actor.Location.X, self.Location.Y - self.Location.Y)));
                    fearer.Add(actor);
                }
            }

            foreach (var trait in self.TraitsImplementing<WithAbilityAnimation>())
            {
                if (!trait.IsTraitDisabled)
                    trait.PlayManaAnimation(self);
            }

            CurrentDelay = 0;
        }

        void ITick.Tick(Actor self)
        {
            if (CurrentDelay < info.Delay)
            {
                CurrentDelay++;
            }

            if (fearer.Any())
                foreach (var actor in fearer)
                {
                    if (actor.IsIdle)
                        actor.QueueActivity(new Move(actor, actor.Location - new CVec(self.Location.X - actor.Location.X, self.Location.Y - self.Location.Y)));
                }
        }
    }

    class FearAbilityOrderTargeter : UnitOrderTargeter
    {
        private int range;
        private int ammount;

        public FearAbilityOrderTargeter(string cursor, int range, int ammount)
            : base("FearAllTarget", 6, cursor, true, true)
        {
            this.range = range;
            this.ammount = ammount;
        }

        public override bool CanTargetActor(Actor self, Actor target, TargetModifiers modifiers, ref string cursor)
        {
            var pr = self.Owner.PlayerActor.Trait<PlayerResources>();

            // Obey force moving onto bridges
            if (!target.IsInWorld
                || target.IsDead
                || target.Info.HasTraitInfo<BuildingInfo>()
                || (target.Location - self.Location).Length > range
                || target.Info.TraitInfoOrDefault<DungeonsAndDragonsStatsInfo>() == null
                || pr.Cash + pr.Resources < ammount)
                return false;

            return true;
        }

        public override bool CanTargetFrozenActor(Actor self, FrozenActor target, TargetModifiers modifiers, ref string cursor)
        {
            return false;
        }
    }
}