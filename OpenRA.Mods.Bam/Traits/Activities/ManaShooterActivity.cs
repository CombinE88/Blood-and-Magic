using System;
using OpenRA.Activities;
using OpenRA.Effects;
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
        private bool delivering;
        private bool complete;
        private bool transformStarted;

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
                var modifier2 = self.Trait<ManaShooter>().ExtraModifier;
                var max = shooterInfo.Interval * modifier * modifier2;

                if (tick++ >= max / 10000)
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

            if (!transformStarted)
            {
                //// TODO: Hack, we want to end the activity even when the animation gets interrupted.

                Action timer = () => { wsb.DefaultAnimation.ReplaceAnim(normalSequence); };
                self.World.AddFrameEndTask(w => w.Add(new DelayedAction(25, timer)));

                wsb.PlayCustomAnimationBackwards(self, shooterInfo.EndObeliskSequence, () =>
                {
                    complete = true;
                    wsb.DefaultAnimation.ReplaceAnim(normalSequence);
                });

                transformStarted = true;
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
        private bool complete;
        private bool completeAbort;
        private bool transformStarted;
        private Actor self;
        private ManaShooterInfo shooterInfo;
        private WithSpriteBody wsb;
        private string normalSequence;

        public ToObelisk(Actor self, ManaShooterInfo shooterInfo, WithSpriteBody wsb)
        {
            this.self = self;
            this.shooterInfo = shooterInfo;
            this.wsb = wsb;

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

            if (!transformStarted)
            {
                //// TODO: Hack, we want to end the activity even when the animation gets interrupted.

                Action timer = () => { completeAbort = true; };
                self.World.AddFrameEndTask(w => w.Add(new DelayedAction(25, timer)));

                wsb.PlayCustomAnimationBackwards(self, shooterInfo.EndObeliskSequence, () =>
                {
                    completeAbort = true;
                    wsb.DefaultAnimation.ReplaceAnim(normalSequence);
                });

                transformStarted = true;
            }

            if (!completeAbort)
                return this;

            return NextActivity;
        }
    }
}