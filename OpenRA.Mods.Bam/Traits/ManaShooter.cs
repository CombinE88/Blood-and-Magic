using System.Collections.Generic;
using System.Drawing;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits
{
    public class ManaShooterInfo : ConditionalTraitInfo
    {
        public readonly int MaxStorage = 10;

        public readonly int Interval = 175;

        public readonly int OverWait = 100;

        public readonly Dictionary<string,int> Modifier = new Dictionary<string, int>();

        public bool OnlyStores = false;

        public override object Create(ActorInitializer init)
        {
            return new ManaShooter(init, this);
        }
    }

    public class ManaShooter : ConditionalTrait<ManaShooterInfo>, ITick, INotifyTransform, IResolveOrder
    {
        private int tick;
        private int overTick;
        public int CurrentStorage;
        private Actor self;
        private bool delivering = false;

        private ManaShooterInfo info;

        private PlayerResources pr;

        public ManaShooter(ActorInitializer init, ManaShooterInfo info) : base(info)
        {
            self = init.Self;
            this.info = info;
            pr = init.Self.Owner.PlayerActor.TraitOrDefault<PlayerResources>();
        }

        public void Tick(Actor self)
        {
            if (IsTraitDisabled || info.OnlyStores || delivering)
                return;

            if (CurrentStorage < info.MaxStorage)
            {
                var ground = self.World.Map.GetTerrainInfo(self.Location).Type;
                var modifier = Info.Modifier.ContainsKey(ground) ? Info.Modifier[ground] : 100;
                if (tick++ >= Info.Interval * modifier / 100)
                {
                    CurrentStorage++;
                    tick = 0;
                }
            }
            else if (CurrentStorage == info.MaxStorage)
            {
                if (overTick++ >= Info.OverWait)
                {
                    ShootMana();
                }
            }
        }

        public void ShootMana()
        {
            if (CurrentStorage > 0 && pr.CanGiveResources(CurrentStorage) && !info.OnlyStores)
            {
                delivering = true;

                if (self.Info.HasTraitInfo<WithManaAnimationInfo>())
                {
                    self.Trait<WithManaAnimation>().PlayManaAnimation(self, () =>
                    {
                        pr.GiveResources(CurrentStorage);
                        if (self != null && !self.IsDead && self.IsInWorld)
                        {
                            CurrentStorage = 0;
                            overTick = 0;
                            tick = 0;
                            delivering = false;
                        }
                    });
                }
                else
                {
                    pr.GiveResources(CurrentStorage);
                    CurrentStorage = 0;
                    overTick = 0;
                    tick = 0;
                    delivering = false;
                }
            }
        }

        public void BeforeTransform(Actor self)
        {
        }

        public void OnTransform(Actor self)
        {
        }

        public void AfterTransform(Actor toActor)
        {
            if (toActor != null && toActor.IsInWorld && !toActor.IsDead && toActor.Info.HasTraitInfo<ManaShooterInfo>() && !delivering)
                toActor.Trait<ManaShooter>().CurrentStorage = CurrentStorage;
        }

        public void ResolveOrder(Actor self, Order order)
        {
            if (order.OrderString != "ShootMana")
                return;

            ShootMana();
        }
    }
}