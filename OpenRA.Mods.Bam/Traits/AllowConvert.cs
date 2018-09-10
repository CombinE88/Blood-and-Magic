using System;
using System.Collections.Generic;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits
{
    public class AllowConvertInfo : ITraitInfo
    {
        public readonly Dictionary<string, bool> ConvertTo = new Dictionary<string, bool>();

        public object Create(ActorInitializer init)
        {
            return new AllowConvert(init, this);
        }
    }

    public class AllowConvert
    {
        public List<Tuple<string, bool>> Transformable = new List<Tuple<string, bool>>();

        public AllowConvert(ActorInitializer init, AllowConvertInfo info)
        {
            foreach (var keypair in info.ConvertTo)
            {
                Transformable.Add(new Tuple<string, bool>(keypair.Key, keypair.Value));
            }
        }
    }
}