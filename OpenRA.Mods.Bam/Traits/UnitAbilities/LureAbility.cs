using System.Collections.Generic;
using System.Linq;
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
    public class LureAbilityInfo : ITraitInfo, Requires<ConditionManagerInfo>
    {
        public readonly int Range = 3;

        public readonly string Cursor = "ability";

        public readonly string AbilityString = "Lure Target Unit";

        public readonly int Ammount = 10;

        public readonly int Delay = 300;

        public readonly string ConditionToGrant = "NotLured";

        public readonly string Image = "explosion";
        public readonly string EffectSequence = "electric_sphere_effect";
        public readonly string EffectPalette = "bam11195";

        public readonly string Sound = "7200.wav";

        public object Create(ActorInitializer init)
        {
            return new LureAbility(this);
        }
    }

    public class LureAbility : IIssueOrder, IResolveOrder, ITick, INotifyKilled
    {
        readonly LureAbilityInfo info;
        public int CurrentDelay;

        private Actor lureTarget;
        private int condition = ConditionManager.InvalidConditionToken;

        public LureAbility(LureAbilityInfo info)
        {
            this.info = info;
        }

        public IEnumerable<IOrderTargeter> Orders
        {
            get { yield return new LureAbilityOrderTargeter(info.Cursor, info.Range, info.Ammount); }
        }

        public Order IssueOrder(Actor self, IOrderTargeter order, Target target, bool queued)
        {
            if (order.OrderID != "LureEnemyTarget")
                return null;

            return new Order(order.OrderID, self, target, queued);
        }

        public void ResolveOrder(Actor self, Order order)
        {
            if (order.OrderString != "LureEnemyTarget")
                return;

            if (CurrentDelay < info.Delay)
                return;

            var pr = self.Owner.PlayerActor.Trait<PlayerResources>();

            if (!pr.TakeCash(info.Ammount))
                return;

            var actor = order.Target.Actor;
            if (actor == null || actor.IsDead || !actor.IsInWorld)
                return;

            CurrentDelay = 0;

            foreach (var trait in self.TraitsImplementing<WithAbilityAnimation>())
            {
                if (!trait.IsTraitDisabled)
                    trait.PlayManaAnimation(self);
            }

            if (lureTarget != null)
            {
                condition = lureTarget.Trait<ConditionManager>().RevokeCondition(self, condition);
            }

            lureTarget = actor;
            condition = lureTarget.Trait<ConditionManager>().GrantCondition(self, info.ConditionToGrant);

            Game.Sound.Play(SoundType.World, info.Sound, self.CenterPosition);

            self.World.AddFrameEndTask(w =>
                w.Add(new SpriteEffect(
                    actor.CenterPosition,
                    w,
                    info.Image,
                    info.EffectSequence,
                    info.EffectPalette)));
        }

        void ITick.Tick(Actor self)
        {
            if (CurrentDelay < info.Delay)
            {
                CurrentDelay++;
            }

            if (lureTarget != null && (lureTarget.IsDead || !lureTarget.IsInWorld))
                lureTarget = null;

            if (lureTarget != null)
            {
                if (lureTarget.IsIdle || (lureTarget.Location - self.Location).Length > 1)
                    lureTarget.QueueActivity(new Follow(lureTarget, Target.FromActor(self), new WDist(1024), new WDist(1512)));
            }
        }

        void INotifyKilled.Killed(Actor self, AttackInfo e)
        {
            if (lureTarget != null)
            {
                lureTarget.CancelActivity();
                condition = lureTarget.Trait<ConditionManager>().RevokeCondition(self, condition);
            }
        }
    }

    class LureAbilityOrderTargeter : UnitOrderTargeter
    {
        private int range;
        private int ammount;

        public LureAbilityOrderTargeter(string cursor, int range, int ammount)
            : base("LureEnemyTarget", 7, cursor, true, false)
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
                || target.Owner.IsAlliedWith(self.Owner)
                || pr.Cash + pr.Resources < ammount
                || target.Info.TraitInfo<DungeonsAndDragonsStatsInfo>().IgnoresAbilites.Contains("Lure")
                || !target.Info.TraitInfo<DungeonsAndDragonsStatsInfo>().Attributes.Contains("Civilized"))
                return false;

            return true;
        }

        public override bool CanTargetFrozenActor(Actor self, FrozenActor target, TargetModifiers modifiers, ref string cursor)
        {
            return false;
        }
    }
}