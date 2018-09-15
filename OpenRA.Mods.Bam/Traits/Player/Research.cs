using System;
using System.Collections.Generic;
using System.Linq;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits.World
{
    public class ResearchInfo : ITraitInfo
    {
        public readonly string Faction = "";

        public readonly int TimePerCost = 5;

        public readonly Dictionary<string, int> Researchable;

        public readonly string[] PreResearched = { };

        public object Create(ActorInitializer init)
        {
            return new Research(init, this);
        }
    }

    public class Research
    {
        public List<string> Researchable = new List<string>();

        public ResearchInfo Info;

        public Research(ActorInitializer init, ResearchInfo info)
        {
            Info = info;
            foreach (var key in info.PreResearched.ToArray())
            {
                Researchable.Add(key);
            }
        }
    }
}