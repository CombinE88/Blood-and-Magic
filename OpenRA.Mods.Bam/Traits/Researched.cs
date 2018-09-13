using System;
using System.Collections.Generic;
using System.Linq;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits.World
{
    public class ResearchableInfo : ITraitInfo
    {
        public readonly string Class = "";

        public readonly int TransformTime = 10;

        public object Create(ActorInitializer init)
        {
            return new Researchable(init);
        }
    }

    public class Researchable
    {
        public Researchable(ActorInitializer init)
        {
        }
    }
}