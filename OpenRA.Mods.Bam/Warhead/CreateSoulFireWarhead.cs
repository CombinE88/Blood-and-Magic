using System.Collections.Generic;
using System.Linq;
using OpenRA.GameRules;
using OpenRA.Mods.Bam.Traits;
using OpenRA.Mods.Bam.Traits.RPGTraits;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Warheads;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Warhead
{
    public class CreateSoulFireWarhead : SpreadDamageWarhead, IRulesetLoaded<WeaponInfo>
    {
        public readonly string Actor = null;

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
            if (Actor == null)
                return;

            var actors = firedBy.World.FindActorsInCircle(pos, new WDist(1024))
                .Where(a => a.TraitOrDefault<SoulFire>() != null)
                .ToList();

            if (actors.Any())
            {
                actors.First().Trait<SoulFire>().Ticker = 0;
                return;
            }

            var init = new TypeDictionary
            {
                new LocationInit(firedBy.World.Map.CellContaining(pos)),
                new CenterPositionInit(pos),
                new OwnerInit(firedBy.Owner)
            };

            firedBy.World.AddFrameEndTask(w => w.CreateActor(Actor, init));
        }
    }
}