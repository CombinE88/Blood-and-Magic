using System.Collections.Generic;
using System.Linq;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits.TileActivities
{
    public class TileButtonInfo : ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new TileButton(init, this);
        }
    }

    public class TileButton : ITick
    {
        public bool Active;

        private ushort disabledButton;
        private ushort enabledButton;

        private Actor standsOn;

        private int testTick;
        private bool set = false;

        public TileButton(ActorInitializer init, TileButtonInfo info)
        {
            var tileUshot = init.Self.World.Map.Rules.TileSet.Templates[init.Self.World.Map.Tiles[init.Self.Location].Type];
            if (tileUshot.Id == 51)
            {
                enabledButton = 52;
                disabledButton = 51;
            }
            else if (tileUshot.Id == 52)
            {
                enabledButton = 51;
                disabledButton = 52;
            }
            else if (tileUshot.Id == 53)
            {
                enabledButton = 53;
                disabledButton = 53;
            }
            else if (tileUshot.Id == 54)
            {
                enabledButton = 53;
                disabledButton = 54;
            }
        }

        void RunOnce(Actor self)
        {
            var listActor = self.World.Actors.ToList();

            var buttonSelfIndex = listActor.IndexOf(self);

            List<Actor> trapsAndBridges = new List<Actor>();

            Log.Write("debug", "Button: " + self.Info.Name + buttonSelfIndex);

            for (int i = buttonSelfIndex; i < listActor.Count; i++)
            {
                Log.Write("debug", "Checking on: " + listActor[i].Info.Name);

                if (listActor[i].TraitOrDefault<TileButton>() != null && trapsAndBridges.Any())
                {
                    Log.Write("debug", "Button: " + self.Info.Name + buttonSelfIndex + " detected: " + listActor[i].Info.Name + " and aborts");
                    break;
                }

                if (listActor[i].TraitOrDefault<TileBridge>() != null)
                {
                    trapsAndBridges.Add(listActor[i]);
                    Log.Write("debug", "Button: " + self.Info.Name + buttonSelfIndex + " added: " + listActor[i].Info.Name);
                }
            }

            if (trapsAndBridges.Any())
            {
                foreach (var actor in trapsAndBridges)
                {
                    Log.Write("debug", "Button: " + self.Info.Name + buttonSelfIndex + " registers in: " + actor.Info.Name);

                    var bridge = actor.TraitOrDefault<TileBridge>();

                    if (bridge != null)
                    {
                        bridge.Buttons.Add(self);
                        Log.Write("debug", "Succeed");
                    }
                }
            }
        }

        void ITick.Tick(Actor self)
        {
            if (!set && testTick++ < 10)
            {
                RunOnce(self);
                set = true;
            }

            standsOn = self.World.FindActorsInCircle(self.CenterPosition, new WDist(125)).FirstOrDefault(a => a.TraitOrDefault<Mobile>() != null && !a.IsDead && a.IsInWorld);
            Active = standsOn != null;
            if (Active)
                Enable(self);
            else
                Dissable(self);
        }

        void Enable(Actor self)
        {
            var tileUshot = self.World.Map.Rules.TileSet.Templates[self.World.Map.Tiles[self.Location].Type];

            if (tileUshot.Id != enabledButton)
                self.World.Map.Tiles[self.Location] = new TerrainTile(enabledButton, 0);
        }

        void Dissable(Actor self)
        {
            var tileUshot = self.World.Map.Rules.TileSet.Templates[self.World.Map.Tiles[self.Location].Type];

            if (tileUshot.Id != disabledButton)
                self.World.Map.Tiles[self.Location] = new TerrainTile(disabledButton, 0);
        }
    }
}