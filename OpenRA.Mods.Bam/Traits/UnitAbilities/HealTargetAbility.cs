using System.Collections.Generic;
using System.Linq;
using OpenRA.Mods.Bam.Traits.RPGTraits;
using OpenRA.Mods.Common.Effects;
using OpenRA.Mods.Common.Orders;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits.UnitAbilities
{
    public class HealTargetAbilityInfo : ITraitInfo
    {
        public readonly int Range = 5;

        public readonly string Cursor = "ability";

        public readonly string AbilityString = "Heal target by 10";

        public readonly int Ammount = 10;

        public readonly int Delay = 275;

        public readonly string Image = "trinkethealsalve";
        public readonly string EffectSequence = "effect";
        public readonly string EffectPalette = "bam11195";

        public readonly bool AutoTarget = false;

        public readonly string Sound = "7206.wav";

        public object Create(ActorInitializer init)
        {
            return new HealTargetAbility(this);
        }
    }

    public class HealTargetAbility : IIssueOrder, IResolveOrder, ITick
    {
        readonly HealTargetAbilityInfo info;
        public int CurrentDelay;
        private int autoRetry;

        public HealTargetAbility(HealTargetAbilityInfo info)
        {
            this.info = info;
        }

        public IEnumerable<IOrderTargeter> Orders
        {
            get { yield return new HealTargetAbilityOrderTargeter(info.Cursor, info.Range, info.Ammount); }
        }

        Order IIssueOrder.IssueOrder(Actor self, IOrderTargeter order, Target target, bool queued)
        {
            if (order.OrderID != "HealTarget")
                return null;

            return new Order(order.OrderID, self, target, queued);
        }

        void IResolveOrder.ResolveOrder(Actor self, Order order)
        {
            if (order.OrderString != "HealTarget")
                return;

            {
                HealTarget(self, order.Target.Actor);
            }
        }

        void HealTarget(Actor self, Actor target)
        {
            var actor = target;
            if (actor == null || actor.IsDead || !actor.IsInWorld || CurrentDelay < info.Delay)
                return;

            if (!self.Owner.PlayerActor.Trait<PlayerResources>().TakeCash(info.Ammount) && self.Owner.PlayerName != "Creeps")
                return;

            var pos = actor.CenterPosition;

            CurrentDelay = 0;

            target.InflictDamage(target, new Damage(-info.Ammount, new BitSet<DamageType>("Healing")));

            foreach (var trait in self.TraitsImplementing<WithAbilityAnimation>())
            {
                if (!trait.IsTraitDisabled)
                    trait.PlayManaAnimation(self);
            }

            Game.Sound.Play(SoundType.World, info.Sound, self.CenterPosition);

            self.World.AddFrameEndTask(w =>
                w.Add(new SpriteEffect(
                    pos,
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
                return;
            }

            if ((!self.IsIdle || !info.AutoTarget) && self.Owner.PlayerName != "Creeps")
                return;

            if (autoRetry++ < 10)
                return;

            var pr = self.Owner.PlayerActor.Trait<PlayerResources>();

            var targets = self.World.FindActorsInCircle(self.CenterPosition, WDist.FromCells(info.Range)).ToArray();

            var allowed = targets.FirstOrDefault(a =>
                a.IsInWorld
                && !a.IsDead
                && a.TraitOrDefault<Building>() == null
                && a.TraitOrDefault<Health>() != null
                && (a.Location - self.Location).Length < info.Range
                && a.TraitOrDefault<Health>().HP < a.TraitOrDefault<Health>().MaxHP
                && a.Owner.IsAlliedWith(self.Owner)
                && pr.Cash + pr.Resources >= info.Ammount
                && a.TraitOrDefault<DungeonsAndDragonsStats>() != null
                && a.Info.TraitInfo<DungeonsAndDragonsStatsInfo>().Attributes.Contains("alive")
                && CurrentDelay >= info.Delay);

            if (allowed != null)
            {
                HealTarget(self, allowed);
            }

            autoRetry = 0;
        }
    }

    class HealTargetAbilityOrderTargeter : UnitOrderTargeter
    {
        private int range;
        private int ammount;

        public HealTargetAbilityOrderTargeter(string cursor, int range, int ammount)
            : base("HealTarget", 6, cursor, false, true)
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
                || !(hp.HP < hp.MaxHP)
                || !target.Owner.IsAlliedWith(self.Owner)
                || pr.Cash + pr.Resources < ammount
                || target.TraitOrDefault<DungeonsAndDragonsStats>() == null
                || !target.Info.TraitInfo<DungeonsAndDragonsStatsInfo>().Attributes.Contains("Alive"))
                return false;

            return true;
        }

        public override bool CanTargetFrozenActor(Actor self, FrozenActor target, TargetModifiers modifiers, ref string cursor)
        {
            return false;
        }
    }
}