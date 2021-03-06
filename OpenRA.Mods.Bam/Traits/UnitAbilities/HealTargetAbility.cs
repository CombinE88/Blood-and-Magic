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

        public readonly int Delay = 200;

        public readonly string Image = "trinkethealsalve";
        public readonly string EffectSequence = "effect";
        public readonly string EffectPalette = "bam11195";

        public readonly string HealSound = "Heal";

        public readonly bool AutoTarget = false;

        public readonly string Sound = "7206.wav";

        public object Create(ActorInitializer init)
        {
            return new HealTargetAbility(init, this);
        }
    }

    public class HealTargetAbility : IIssueOrder, IResolveOrder, ITick
    {
        readonly HealTargetAbilityInfo info;
        public int CurrentDelay;

        public HealTargetAbility(ActorInitializer init, HealTargetAbilityInfo info)
        {
            this.info = info;
        }

        public IEnumerable<IOrderTargeter> Orders
        {
            get { yield return new HealTargetAbilityOrderTargeter(info.Cursor, info.Range, info.Ammount); }
        }

        public Order IssueOrder(Actor self, IOrderTargeter order, Target target, bool queued)
        {
            if (order.OrderID != "HealTarget")
                return null;

            return new Order(order.OrderID, self, target, queued);
        }

        public void ResolveOrder(Actor self, Order order)
        {
            if (order.OrderString != "HealTarget" || !(CurrentDelay >= info.Delay))
                return;

            var actor = order.Target.Actor;
            if (actor == null || actor.IsDead || !actor.IsInWorld)
                return;

            if (self.Owner.PlayerActor.Trait<PlayerResources>().TakeCash(info.Ammount))
            {
                order.Target.Actor.InflictDamage(order.Target.Actor, new Damage(-info.Ammount, new BitSet<DamageType>("Healing")));
                CurrentDelay = 0;

                foreach (var trait in self.TraitsImplementing<WithAbilityAnimation>())
                {
                    if (!trait.IsTraitDisabled)
                        trait.PlayManaAnimation(self);
                }

                Game.Sound.Play(SoundType.World, info.Sound, self.CenterPosition);

                self.World.AddFrameEndTask(w =>
                    w.Add(new SpriteEffect(
                        order.Target.Actor.CenterPosition,
                        w,
                        info.Image,
                        info.EffectSequence,
                        info.EffectPalette)));
            }
        }

        public void Tick(Actor self)
        {
            if (CurrentDelay++ < info.Delay || !info.AutoTarget || !self.IsIdle)
                return;

            var pr = self.Owner.PlayerActor.Trait<PlayerResources>();

            var targets = self.World.FindActorsInCircle(self.CenterPosition, WDist.FromCells(info.Range)).ToArray();
            var allowed = targets.Where(a =>
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
            );
            if (allowed.Any())
            {
                self.World.IssueOrder(new Order("HealTarget", self, Target.FromActor(allowed.ClosestTo(self)), false));
            }
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
                || !target.Info.TraitInfo<DungeonsAndDragonsStatsInfo>().Attributes.Contains("alive"))
                return false;

            return true;
        }

        public override bool CanTargetFrozenActor(Actor self, FrozenActor target, TargetModifiers modifiers, ref string cursor)
        {
            return false;
        }
    }
}