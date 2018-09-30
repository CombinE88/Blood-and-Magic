using System.Collections.Generic;
using System.Linq;
using OpenRA.Mods.Bam.Traits.Render;
using OpenRA.Mods.Bam.Traits.RPGTraits;
using OpenRA.Mods.Common.Effects;
using OpenRA.Mods.Common.Orders;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits.UnitAbilities
{
    public class StealEnemyAbilityInfo : ITraitInfo
    {
        public readonly int Range = 2;

        public readonly string Cursor = "ability";

        public readonly string AbilityString = "Steal Target Unit";

        public readonly int Ammount = 60;

        public readonly int Delay = 300;

        public readonly string Image = "explosion";
        public readonly string EffectSequence = "electric_sphere_effect";
        public readonly string EffectPalette = "bam11195";

        public readonly string Sound = "7200.wav";

        public object Create(ActorInitializer init)
        {
            return new StealEnemyAbility(this);
        }
    }

    public class StealEnemyAbility : IIssueOrder, IResolveOrder, ITick
    {
        readonly StealEnemyAbilityInfo info;
        public int CurrentDelay;

        public StealEnemyAbility(StealEnemyAbilityInfo info)
        {
            this.info = info;
        }

        public IEnumerable<IOrderTargeter> Orders
        {
            get { yield return new StealEnemyAbilityOrderTargeter(info.Cursor, info.Range, info.Ammount); }
        }

        public Order IssueOrder(Actor self, IOrderTargeter order, Target target, bool queued)
        {
            if (order.OrderID != "StealTarget")
                return null;

            return new Order(order.OrderID, self, target, queued);
        }

        public void ResolveOrder(Actor self, Order order)
        {
            if (order.OrderString != "StealTarget")
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

            order.Target.Actor.InflictDamage(order.Target.Actor, new Damage(actor.Trait<Health>().HP / 2, new BitSet<DamageType>("Death")));

            actor.ChangeOwner(self.Owner);

            Game.Sound.Play(SoundType.World, info.Sound, self.CenterPosition);

            self.World.AddFrameEndTask(w =>
                w.Add(new SpriteEffect(
                    order.Target.Actor.CenterPosition,
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
        }
    }

    class StealEnemyAbilityOrderTargeter : UnitOrderTargeter
    {
        private int range;
        private int ammount;

        public StealEnemyAbilityOrderTargeter(string cursor, int range, int ammount)
            : base("StealTarget", 6, cursor, true, false)
        {
            this.range = range;
            this.ammount = ammount;
        }

        public override bool CanTargetActor(Actor self, Actor target, TargetModifiers modifiers, ref string cursor)
        {
            var pr = self.Owner.PlayerActor.Trait<PlayerResources>();
            var hp = target.TraitOrDefault<Health>();

            // Obey force moving onto bridges
            if (target == null
                || !target.IsInWorld
                || target.IsDead
                || target.Info.HasTraitInfo<BuildingInfo>()
                || hp == null
                || (target.Location - self.Location).Length > range
                || target.Owner.IsAlliedWith(self.Owner)
                || pr.Cash + pr.Resources < ammount
                || (target.Info.TraitInfo<DungeonsAndDragonsStatsInfo>() != null && target.Info.TraitInfo<DungeonsAndDragonsStatsInfo>().Attributes.Contains("Immune")))
                return false;

            return true;
        }

        public override bool CanTargetFrozenActor(Actor self, FrozenActor target, TargetModifiers modifiers, ref string cursor)
        {
            return false;
        }
    }
}