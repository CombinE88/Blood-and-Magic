using System.Collections.Generic;
using System.Linq;
using OpenRA.Mods.Common.Effects;
using OpenRA.Mods.Common.Traits;
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
        private int tick;
        private int random;

        private bool isBridge;
        private TileBridgeInfo info;

        public TileBridge(ActorInitializer init, TileBridgeInfo info)
        {
            this.info = info;

            var tileUshot = init.Self.World.Map.Rules.TileSet.Templates[init.Self.World.Map.Tiles[init.Self.Location].Type];
            if (tileUshot.Id == 47)
            {
                enabledBridge = 48;
                disabledBridge = 47;
                isBridge = true;
            }
            else if (tileUshot.Id == 48)
            {
                enabledBridge = 47;
                disabledBridge = 48;
                isBridge = true;
            }
            else if (tileUshot.Id == 480)
            {
                enabledBridge = 470;
                disabledBridge = 480;
                isBridge = true;
            }
            else if (tileUshot.Id == 470)
            {
                enabledBridge = 480;
                disabledBridge = 470;
                isBridge = true;
            }
            else if (tileUshot.Id == 49)
            {
                enabledBridge = 50;
                disabledBridge = 49;
                isBridge = true;
            }
            else if (tileUshot.Id == 50)
            {
                enabledBridge = 49;
                disabledBridge = 50;
                isBridge = true;
            }
            else if (tileUshot.Id == 490)
            {
                enabledBridge = 500;
                disabledBridge = 490;
                isBridge = true;
            }
            else if (tileUshot.Id == 500)
            {
                enabledBridge = 490;
                disabledBridge = 500;
                isBridge = true;
            }
            else if (tileUshot.Id == 55)
            {
                enabledBridge = 56;
                disabledBridge = 55;
                isBridge = false;
            }
            else if (tileUshot.Id == 56)
            {
                enabledBridge = 55;
                disabledBridge = 56;
                isBridge = false;
            }
            else
                init.Self.Dispose();

            random = init.Self.World.SharedRandom.Next(25, 100);
        }

        void ITick.Tick(Actor self)
        {
            HandleActivation();
            if (active)
                Enable(self);
            else
                Dissable(self);

            if (isBridge)
                return;

            var tileUshot = self.World.Map.Rules.TileSet.Templates[self.World.Map.Tiles[self.Location].Type];
            if (tileUshot.Id == 56 && tick++ >= random)
            {
                tick = 0;
                random = self.World.SharedRandom.Next(25, 100);
                self.World.AddFrameEndTask(w =>
                    w.Add(new SpriteEffect(
                        self.CenterPosition,
                        w,
                        "explosion",
                        "explode_fireball_effect",
                        "bam11195")));
            }
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
            {
                self.World.Map.Tiles[self.Location] = new TerrainTile(enabledBridge, 0);

                var domainIndex = self.World.WorldActor.TraitOrDefault<DomainIndex>();
                if (domainIndex != null)
                    domainIndex.UpdateCells(self.World, new List<CPos> { self.Location });
            }
        }

        void Dissable(Actor self)
        {
            var tileUshot = self.World.Map.Rules.TileSet.Templates[self.World.Map.Tiles[self.Location].Type];

            if (tileUshot.Id != disabledBridge)
            {
                self.World.Map.Tiles[self.Location] = new TerrainTile(disabledBridge, 0);

                var domainIndex = self.World.WorldActor.TraitOrDefault<DomainIndex>();
                if (domainIndex != null)
                    domainIndex.UpdateCells(self.World, new List<CPos> { self.Location });
            }
        }
    }
}