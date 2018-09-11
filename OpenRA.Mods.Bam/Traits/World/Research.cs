using System;
using System.Collections.Generic;
using System.Linq;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits.World
{
    public class ResearchInfo : ITraitInfo
    {
        public readonly string Faction = "";

        public readonly Dictionary<string, int> Researchable;

        public readonly Dictionary<string, bool> PreResearched = new Dictionary<string, bool>();

        public object Create(ActorInitializer init)
        {
            return new Research(init, this);
        }
    }

    public class Research
    {
        public List<Tuple<string, bool>> Researchable = new List<Tuple<string, bool>>();

        public ResearchInfo Info;

        public Research(ActorInitializer init, ResearchInfo info)
        {
            Info = info;
            foreach (var key in info.Researchable.Keys)
            {
                var boolean = Info.PreResearched.Any() && Info.PreResearched.ContainsKey(key) && Info.PreResearched[key];
                Researchable.Add(new Tuple<string, bool>(key, boolean));
            }
        }
    }
}