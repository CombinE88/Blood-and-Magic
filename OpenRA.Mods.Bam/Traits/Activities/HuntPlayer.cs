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

using System.Collections.Generic;
using System.Linq;
using OpenRA.Activities;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits.Activities
{
    public class HuntPlayer : Activity
    {
        readonly IEnumerable<Actor> targets;
        readonly IMove move;
        readonly Player player;

        public HuntPlayer(Actor self, Player player)
        {
            this.player = player;

            move = self.Trait<IMove>();
            var attack = self.Trait<AttackBase>();
            targets = self.World.Actors.Where(
                a => self != a && !a.IsDead && a.IsInWorld && a.AppearsHostileTo(self)
                     && a.IsTargetableBy(self) && attack.HasAnyValidWeapons(Target.FromActor(a))
                     && a.Owner == player);
        }

        public override Activity Tick(Actor self)
        {
            if (IsCanceled)
                return this;

            var target = targets.Where(a => a != null && !a.IsDead && a.IsInWorld).ClosestTo(self);
            if (target == null)
                return this;

            return ActivityUtils.SequenceActivities(
                new AttackMoveActivity(self, move.MoveTo(target.Location, 2)),
                new Wait(25),
                this);
        }
    }
}