using System.Collections.Generic;
using System.Linq;
using OpenRA.Mods.Common.Effects;
using OpenRA.Mods.Common.Orders;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits.UnitAbilities
{
    public class HealTargetAbilityInfo : ITraitInfo
    {
        public readonly int Range = 5;

        public readonly string Cursor = "ability";

        public readonly string AbilityString = "Heal target by 10";

        public readonly int Delay = 200;

        public readonly string Image = "trinkethealsalve";
        public readonly string EffectSequence = "effect";
        public readonly string EffectPalette = "bam11195";

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
            get { yield return new HealTargetAbilityOrderTargeter(info.Cursor, info.Range); }
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

            if (self.Owner.PlayerActor.Trait<PlayerResources>().TakeCash(10))
            {
                order.Target.Actor.InflictDamage(order.Target.Actor, new Damage(-10));
                CurrentDelay = 0;
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
            if (CurrentDelay < info.Delay)
                CurrentDelay++;
        }
    }

    class HealTargetAbilityOrderTargeter : UnitOrderTargeter
    {
        private int range;

        public HealTargetAbilityOrderTargeter(string cursor, int range)
            : base("HealTarget", 6, cursor, false, true)
        {
            this.range = range;
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
                || (target.Location - self.Location).LengthSquared > range
                || !(hp.HP < hp.MaxHP)
                || !target.Owner.IsAlliedWith(self.Owner)
                || pr.Cash + pr.Resources < 10)
                return false;

            return true;
        }

        public override bool CanTargetFrozenActor(Actor self, FrozenActor target, TargetModifiers modifiers, ref string cursor)
        {
            return false;
        }
    }
}