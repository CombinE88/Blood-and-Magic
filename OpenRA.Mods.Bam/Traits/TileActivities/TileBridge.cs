using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits.TileActivities
{
    public class TileBridgeInfo : ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new TileBridge(init, this);
        }
    }

    public class TileBridge : ITick
    {
        public List<Actor> Buttons = new List<Actor>();

        private ushort disabledBridge;
        private ushort enabledBridge;

        private bool active;


        public TileBridge(ActorInitializer init, TileBridgeInfo tileTrapInfo)
        {
            var tileUshot = init.Self.World.Map.Rules.TileSet.Templates[init.Self.World.Map.Tiles[init.Self.Location].Type];
            if (tileUshot.Id == 47)
            {
                enabledBridge = 47;
                disabledBridge = 48;
            }
            else if (tileUshot.Id == 48)
            {
                enabledBridge = 48;
                disabledBridge = 47;
            }
            else if (tileUshot.Id == 480)
            {
                enabledBridge = 480;
                disabledBridge = 470;
            }
            else if (tileUshot.Id == 470)
            {
                enabledBridge = 470;
                disabledBridge = 480;
            }
            else if (tileUshot.Id == 94)
            {
                enabledBridge = 49;
                disabledBridge = 50;
            }
            else if (tileUshot.Id == 50)
            {
                enabledBridge = 50;
                disabledBridge = 49;
            }
            else if (tileUshot.Id == 490)
            {
                enabledBridge = 490;
                disabledBridge = 500;
            }
            else if (tileUshot.Id == 500)
            {
                enabledBridge = 500;
                disabledBridge = 490;
            }
            else if (tileUshot.Id == 55)
            {
                enabledBridge = 56;
                disabledBridge = 55;
            }
            else if (tileUshot.Id == 56)
            {
                enabledBridge = 55;
                disabledBridge = 56;
            }
            else
                init.Self.Dispose();
        }

        void ITick.Tick(Actor self)
        {
            HandleActivation();
            if (active)
                Enable(self);
            else
                Dissable(self);
        }

        void HandleActivation()
        {
            if (Buttons.FirstOrDefault(b => b.Trait<TileButton>().Active) != null)
                active = true;
            else
                active = false;
        }

        void Enable(Actor self)
        {
            var tileUshot = self.World.Map.Rules.TileSet.Templates[self.World.Map.Tiles[self.Location].Type];

            if (tileUshot.Id != enabledBridge)
                self.World.Map.Tiles[self.Location] = new TerrainTile(enabledBridge, 0);
        }

        void Dissable(Actor self)
        {
            var tileUshot = self.World.Map.Rules.TileSet.Templates[self.World.Map.Tiles[self.Location].Type];

            if (tileUshot.Id != disabledBridge)
                self.World.Map.Tiles[self.Location] = new TerrainTile(disabledBridge, 0);
        }
    }
}