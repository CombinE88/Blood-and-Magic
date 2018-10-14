using System;
using System.Collections.Generic;
using OpenRA.Effects;
using OpenRA.Graphics;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits.UnitAbilities
{
    public class ChangeToRandomBuildingInfo : ITraitInfo
    {
        public readonly string[] Actors = { "platformcrypt", "platformbarracks", "platformtemple", "platformarbor", "platformrune" };

        public object Create(ActorInitializer init)
        {
            return new ChangeToRandomBuilding(this);
        }
    }

    public class ChangeToRandomBuilding : INotifyCreated
    {
        private ChangeToRandomBuildingInfo info;

        public ChangeToRandomBuilding(ChangeToRandomBuildingInfo info)
        {
            this.info = info;
        }

        void INotifyCreated.Created(Actor self)
        {
            var td = new TypeDictionary
            {
                new OwnerInit("Neutral"),
                new LocationInit(self.Location)
            };

            self.Dispose();

            Action act = () =>
            {
                self.World.AddFrameEndTask(w => { w.CreateActor(true, info.Actors[self.World.SharedRandom.Next(0, info.Actors.Length)], td); });
            };

            self.World.AddFrameEndTask(w => w.Add(new DelayedAction(2, act)));
        }
    }
}