#region Copyright & License Information

/*
 * Copyright 2007-2018 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */

#endregion

using OpenRA.Activities;
using OpenRA.Mods.Bam.Traits.PlayerTraits;
using OpenRA.Mods.Bam.Traits.TrinketLogics;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits.Activities
{
    public class AdvancedTransform : Activity
    {
        public readonly string ToActor;
        public CVec Offset = CVec.Zero;
        public int Facing = 96;
        public string[] Sounds = { };
        public string Notification = null;
        public int ForceHealthPercentage = 0;
        public bool SkipMakeAnims = false;
        public bool SelfSkipMakeAnims = false;
        public bool NotifyBuildComplete = false;
        public string Faction = null;
        public Actor Trinket = null;
        public Actor IgnoreTrinket = null;
        public Actor RallyPointActor = null;
        public bool UseRallyPoint = false;

        public int Time = 250;
        public string CapsuleInto = null;

        public AdvancedTransform(Actor self, string toActor)
        {
            ToActor = toActor;
        }

        protected override void OnFirstRun(Actor self)
        {
            if (self.Info.HasTraitInfo<IFacingInfo>())
                QueueChild(new Turn(self, Facing));

            if (self.Info.HasTraitInfo<AircraftInfo>())
                QueueChild(new HeliLand(self, true));
        }

        public override Activity Tick(Actor self)
        {
            if (IsCanceled)
                return NextActivity;

            if (ChildActivity != null)
            {
                ActivityUtils.RunActivity(self, ChildActivity);
                return this;
            }

            // Prevent deployment in bogus locations
            var transforms = self.TraitOrDefault<Transforms>();
            var building = self.TraitOrDefault<Building>();
            if ((transforms != null && !transforms.CanDeploy()) || (building != null && !building.Lock()))
            {
                Cancel(self, true);
                return NextActivity;
            }

            foreach (var nt in self.TraitsImplementing<INotifyTransform>())
                nt.BeforeTransform(self);

            var makeAnimation = self.TraitOrDefault<WithMakeAnimation>();
            if (!SelfSkipMakeAnims && makeAnimation != null)
            {
                // Once the make animation starts the activity must not be stopped anymore.
                IsInterruptible = false;

                // Wait forever
                QueueChild(new WaitFor(() => false));
                makeAnimation.Reverse(self, () => DoTransform(self));
                return this;
            }

            return NextActivity;
        }

        protected override void OnLastRun(Actor self)
        {
            if (!IsCanceled)
                DoTransform(self);
        }

        void DoTransform(Actor self)
        {
            self.World.AddFrameEndTask(w =>
            {
                if (self.IsDead)
                    return;

                foreach (var nt in self.TraitsImplementing<INotifyTransform>())
                    nt.OnTransform(self);

                var selected = w.Selection.Contains(self);
                var controlgroup = w.Selection.GetControlGroupForActor(self);

                if (!self.Info.HasTraitInfo<CanHoldTrinketInfo>())
                    self.Trait<CanHoldTrinket>().DropTrinket(self);

                self.Dispose();
                foreach (var s in Sounds)
                    Game.Sound.PlayToPlayer(SoundType.World, self.Owner, s, self.CenterPosition);

                Game.Sound.PlayNotification(self.World.Map.Rules, self.Owner, "Speech", Notification, self.Owner.Faction.InternalName);

                var init = new TypeDictionary
                {
                    new LocationInit(self.Location + Offset),
                    new OwnerInit(self.Owner),
                    new FacingInit(Facing),
                };

                if (SkipMakeAnims)
                    init.Add(new SkipMakeAnimsInit());

                if (Faction != null)
                    init.Add(new FactionInit(Faction));

                var health = self.TraitOrDefault<Health>();
                if (health != null)
                {
                    // Cast to long to avoid overflow when multiplying by the health
                    var newHP = ForceHealthPercentage > 0 ? ForceHealthPercentage : (int)(health.HP * 100L / health.MaxHP);
                    init.Add(new HealthInit(newHP));
                }

                foreach (var modifier in self.TraitsImplementing<ITransformActorInitModifier>())
                    modifier.ModifyTransformActorInit(self, init);

                var a = w.CreateActor(ToActor, init);
                foreach (var nt in self.TraitsImplementing<INotifyTransform>())
                    nt.AfterTransform(a);

                if (NotifyBuildComplete)
                {
                    var exp = a.Owner.PlayerActor.TraitOrDefault<DungeonsAndDragonsExperience>();
                    if (exp != null)
                    {
                        if (a.Info.HasTraitInfo<ValuedInfo>())
                            exp.AddCash(a.Info.TraitInfo<ValuedInfo>().Cost);
                    }
                }

                if (a.Info.HasTraitInfo<TransformAfterTimeInfo>() && CapsuleInto != null)
                {
                    var trait = a.Trait<TransformAfterTime>();
                    trait.Time = Time;
                    trait.IntoActor = CapsuleInto;
                    trait.Transforming = true;
                }

                if (Trinket != null && a.Info.HasTraitInfo<CanHoldTrinketInfo>())
                    a.Trait<CanHoldTrinket>().HoldsTrinket = Trinket;

                if (IgnoreTrinket != null && a.Info.HasTraitInfo<CanHoldTrinketInfo>())
                    a.Trait<CanHoldTrinket>().IgnoreTrinket = IgnoreTrinket;

                if (selected)
                    w.Selection.Add(w, a);

                if (a.TraitOrDefault<WithMakeAnimation>() != null)
                    a.Trait<WithMakeAnimation>().Forward(self, () => { });

                if (controlgroup.HasValue)
                    w.Selection.AddToControlGroup(a, controlgroup.Value);

                if (!RallyPointActor.IsDead && RallyPointActor.IsInWorld && UseRallyPoint && RallyPointActor.TraitOrDefault<RallyPoint>() != null)
                {
                    a.QueueActivity(a.Trait<Mobile>().MoveTo(RallyPointActor.Trait<RallyPoint>().Location, 2));
                    var transOnTime = a.TraitOrDefault<TransformAfterTime>();
                    if (transOnTime != null)
                    {
                        transOnTime.RallyPointActor = RallyPointActor;
                        transOnTime.UseRallyPoint = UseRallyPoint;
                    }
                }
            });
        }
    }
}