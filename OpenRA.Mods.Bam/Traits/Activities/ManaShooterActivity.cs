using OpenRA.Activities;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;

namespace OpenRA.Mods.Bam.Traits.Activities
{
    public class Obelisk : Activity
    {
        readonly WithSpriteBody wsb;

        private int tick;
        private int overTick;
        public int CurrentStorage;
        private Actor self;
        private bool delivering = false;
        private bool complete = false;

        private ManaShooterInfo shooterInfo;

        private PlayerResources pr;
        private string normalSequence;
        private ManaShooter shooter;

        public Obelisk(Actor self, ManaShooterInfo shooterInfo, WithSpriteBody wsb, string normalSequence)
        {
            this.normalSequence = normalSequence;
            this.shooterInfo = shooterInfo;
            this.self = self;
            pr = self.Owner.PlayerActor.TraitOrDefault<PlayerResources>();
            this.wsb = wsb;

            shooter = self.Trait<ManaShooter>();
            shooter.CanShoot = true;
        }

        public override Activity Tick(Actor self)
        {
            if (delivering)
                return this;

            if (shooter.SendMana)
                ShootMana();

            if (CurrentStorage < shooterInfo.MaxStorage)
            {
                var ground = self.World.Map.GetTerrainInfo(self.Location).Type;
                var modifier = shooterInfo.Modifier.ContainsKey(ground) ? shooterInfo.Modifier[ground] : 100;
                var modifier2 = self.Trait<ManaShooter>().ExtraModifier / 100;
                var max = shooterInfo.Interval * modifier * modifier2;

                if (tick++ >= max / 100)
                {
                    CurrentStorage++;
                    shooter.CurrentStorage = CurrentStorage;
                    tick = 0;
                }
            }
            else if (CurrentStorage == shooterInfo.MaxStorage)
            {
                if (overTick++ >= shooterInfo.OverWait)
                {
                    ShootMana();
                }
            }

            if (!IsCanceled)
                return this;

            shooter.CanShoot = false;

            if (complete)
                return NextActivity;

            if (wsb.DefaultAnimation.CurrentSequence.Name != shooterInfo.EndObeliskSequence)
            {
                wsb.PlayCustomAnimationBackwards(self, shooterInfo.EndObeliskSequence, () =>
                {
                    complete = true;
                    wsb.DefaultAnimation.ReplaceAnim(normalSequence);
                });
            }

            return this;
        }

        public void ShootMana()
        {
            if (CurrentStorage > 0 && pr.CanGiveResources(CurrentStorage))
            {
                delivering = true;
                shooter.SendMana = false;

                wsb.PlayCustomAnimation(self, shooterInfo.ProduceManaSequence, () =>
                {
                    pr.GiveResources(CurrentStorage);
                    if (self != null && !self.IsDead && self.IsInWorld)
                    {
                        wsb.DefaultAnimation.ReplaceAnim(shooterInfo.ObeliskSequence);

                        CurrentStorage = 0;
                        shooter.CurrentStorage = CurrentStorage;
                        overTick = 0;
                        tick = 0;
                        delivering = false;
                    }
                });
            }
        }
    }

    public class ToObelisk : Activity
    {
        private bool complete = false;
        private bool completeAbort = false;
        private Actor self;
        private ManaShooterInfo shooterInfo;
        private WithSpriteBody wsb;
        private string normalSequence;
        private ManaShooter shooter;

        public ToObelisk(Actor self, ManaShooterInfo shooterInfo, ManaShooter shooter, WithSpriteBody wsb)
        {
            this.self = self;
            this.shooterInfo = shooterInfo;
            this.wsb = wsb;
            this.shooter = shooter;

            normalSequence = wsb.Info.Sequence;
            wsb.PlayCustomAnimation(self, shooterInfo.StartObeliskSequence, () =>
            {
                complete = true;
                wsb.DefaultAnimation.ReplaceAnim(shooterInfo.ObeliskSequence);
            });
        }

        public override Activity Tick(Actor self)
        {
            if (complete)
            {
                return new Obelisk(this.self, shooterInfo, wsb, normalSequence);
            }

            if (!IsCanceled)
                return this;

            if (wsb.DefaultAnimation.CurrentSequence.Name != shooterInfo.EndObeliskSequence)
            {
                wsb.PlayCustomAnimationBackwards(self, shooterInfo.EndObeliskSequence, () =>
                {
                    completeAbort = true;
                    wsb.DefaultAnimation.ReplaceAnim(normalSequence);
                });
            }

            if (!completeAbort)
                return this;

            return NextActivity;
        }
    }
}