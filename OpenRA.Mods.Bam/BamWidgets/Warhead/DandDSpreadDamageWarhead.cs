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
using OpenRA.GameRules;
using OpenRA.Mods.Bam.Traits.RPGTraits;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Warheads
{
    public class DandDSpreadDamageWarhead : DamageWarhead, IRulesetLoaded<WeaponInfo>
    {
        [Desc("Range between falloff steps.")] public readonly WDist Range = new WDist(512);

        void IRulesetLoaded<WeaponInfo>.RulesetLoaded(Ruleset rules, WeaponInfo info)
        {
        }

        // TODO: Remove Fallof and Spreadbullshit

        public override void DoImpact(Target target, Actor firedBy, IEnumerable<int> damageModifiers)
        {
            var build = firedBy.World.WorldActor.Trait<BuildingInfluence>().GetBuildingAt(firedBy.World.Map.CellContaining(target.CenterPosition));
            var hitActors = firedBy.World.FindActorsInCircle(target.CenterPosition, Range).ToHashSet();

            if (build != null)
                hitActors.Add(build);

            foreach (var victim in hitActors)
            {
                if (victim == null || victim.IsDead || !victim.IsInWorld)
                    continue;
                // Cannot be damaged without a Health trait
                var healthInfo = victim.Info.TraitInfoOrDefault<HealthInfo>();
                if (healthInfo == null)
                    continue;

                // Cannot be damaged without an active HitShape
                var activeShapes = victim.TraitsImplementing<HitShape>().Where(Exts.IsTraitEnabled);
                if (!activeShapes.Any())
                    continue;

                DoImpact(victim, firedBy);
            }
        }

        public override void DoImpact(WPos pos, Actor firedBy, IEnumerable<int> damageModifiers)
        {
            return;
        }

        protected virtual void DoImpact(Actor victim, Actor firedBy)
        {
            if (victim == null || victim.IsDead || !victim.IsInWorld)
                return;

            if (!IsValidAgainst(victim, firedBy))
                return;

            var damage = firedBy.Info.HasTraitInfo<DungeonsAndDragonsStatsInfo>() ? Damage * firedBy.Trait<DungeonsAndDragonsStats>().ModifiedDamage : 0;
            var victimArmor = victim.Info.HasTraitInfo<DungeonsAndDragonsStatsInfo>() ? victim.Trait<DungeonsAndDragonsStats>().ModifiedArmor : 0;
            damage = damage - victimArmor > 0 ? damage - victimArmor : 1;

            victim.InflictDamage(firedBy, new Damage(damage, DamageTypes));
        }
    }
}