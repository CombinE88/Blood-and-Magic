using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenRA.Activities;
using OpenRA.Mods.Bam.Activities;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits
{
    public class ManaShooterInfo : ITraitInfo
    {
        public readonly int MaxStorage = 10;

        public readonly int Interval = 250;

        public readonly int OverWait = 100;

        public readonly Dictionary<string, int> Modifier = new Dictionary<string, int>();

        [SequenceReference] public readonly string StartObeliskSequence = "transform";

        [SequenceReference] public readonly string EndObeliskSequence = "transform";

        [SequenceReference] public readonly string ProduceManaSequence = "mana";

        [SequenceReference] public readonly string ObeliskSequence = "transform_idle";

        [Desc("Which sprite body to modify.")] public readonly string Body = "body";


        public object Create(ActorInitializer init)
        {
            return new ManaShooter(init, this);
        }
    }

    public class ManaShooter : ITick, INotifyCreated, IResolveOrder, INotifyDamage
    {
        private Actor self;
        private ManaShooterInfo info;
        public int CurrentStorage;
        private int tick;

        private PlayerResources pr;
        WithSpriteBody wsb;

        public bool CanShoot = false;
        public bool SendMana = false;

        public ManaShooter(ActorInitializer init, ManaShooterInfo info)
        {
            self = init.Self;
            this.info = info;
            pr = init.Self.Owner.PlayerActor.TraitOrDefault<PlayerResources>();
        }

        void ITick.Tick(Actor self)
        {
            if (!self.IsIdle)
                return;

            if (tick++ >= 25)
            {
                self.QueueActivity(new ToObelisk(self, info, this, wsb));
                tick = 0;
            }

        }

        void INotifyCreated.Created(Actor self)
        {
            wsb = self.TraitsImplementing<WithSpriteBody>().Single(w => w.Info.Name == info.Body);
        }


        void IResolveOrder.ResolveOrder(Actor self, Order order)
        {
            if (order.OrderString != "ShootMana")
                return;

            SendMana = true;
        }

        void INotifyDamage.Damaged(Actor self, AttackInfo e)
        {
            var activeAttackBases = self.TraitsImplementing<AttackBase>().ToArray().Where(Exts.IsTraitEnabled);
            var auto = self.TraitOrDefault<AutoTarget>();
            foreach (var ab in activeAttackBases)
            {
                if (ab.IsAiming)
                    return;
            }

            if( auto != null)
                auto.ScanAndAttack(this.self, false);
        }
    }
}