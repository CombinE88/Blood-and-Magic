using System;
using System.Collections.Generic;
using System.Linq;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits.World
{
    public class ResearchedInfo : ITraitInfo
    {
        public readonly string Class = "";

        public object Create(ActorInitializer init)
        {
            return new Researched(init);
        }
    }

    public class Researched
    {
        public Researched(ActorInitializer init)
        {
        }
    }
}