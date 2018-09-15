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
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Warheads
{
    public class WizardSpecialWarhead : SpreadDamageWarhead, IRulesetLoaded<WeaponInfo>
    {
        void IRulesetLoaded<WeaponInfo>.RulesetLoaded(Ruleset rules, WeaponInfo info)
        {
            if (Range != null)
            {
                if (Range.Length != 1 && Range.Length != Falloff.Length)
                    throw new YamlException("Number of range values must be 1 or equal to the number of Falloff values.");

                for (var i = 0; i < Range.Length - 1; i++)
                    if (Range[i] > Range[i + 1])
                        throw new YamlException("Range values must be specified in an increasing order.");
            }
            else
                Range = Exts.MakeArray(Falloff.Length, i => i * Spread);
        }

        public override void DoImpact(WPos pos, Actor firedBy, IEnumerable<int> damageModifiers)
        {
            var world = firedBy.World;

            var debugVis = world.WorldActor.TraitOrDefault<DebugVisualizations>();
            if (debugVis != null && debugVis.CombatGeometry)
                world.WorldActor.Trait<WarheadDebugOverlay>().AddImpact(pos, Range, DebugOverlayColor);

            var hitActors = world.FindActorsOnCircle(pos, Range[Range.Length - 1]);

            foreach (var victim in hitActors)
            {
                // Cannot be damaged without a Health trait
                var healthInfo = victim.Info.TraitInfoOrDefault<HealthInfo>();
                if (healthInfo == null)
                    continue;

                // Cannot be damaged without an active HitShape
                var activeShapes = victim.TraitsImplementing<HitShape>().Where(Exts.IsTraitEnabled);
                if (!activeShapes.Any())
                    continue;

                var distance = activeShapes.Min(t => t.Info.Type.DistanceFromEdge(pos, victim));
                var damageFalloff = GetDamageFalloff(distance.Length);

                if (damageFalloff == 0)
                    continue;

                var localModifiers = damageModifiers.Append(damageFalloff);

                DoImpact(victim, firedBy, localModifiers);
            }
        }

        public override void DoImpact(Actor victim, Actor firedBy, IEnumerable<int> damageModifiers)
        {
            if (!IsValidAgainst(victim, firedBy))
                return;

            var extra = 0;
            if (firedBy != null && !firedBy.IsDead && firedBy.IsInWorld)
            {
                extra = firedBy.Info.HasTraitInfo<DungeonsAndDragonsStatsInfo>() ? firedBy.TraitOrDefault<DungeonsAndDragonsStats>().ModifiedDamage : 1;
            }

            victim.InflictDamage(firedBy, new Damage(extra, DamageTypes));
        }

        int GetDamageFalloff(int distance)
        {
            var inner = Range[0].Length;
            for (var i = 1; i < Range.Length; i++)
            {
                var outer = Range[i].Length;
                if (outer > distance)
                    return int2.Lerp(Falloff[i - 1], Falloff[i], distance - inner, outer - inner);

                inner = outer;
            }

            return 0;
        }
    }
}