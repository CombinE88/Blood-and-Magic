using System.Collections.Generic;
using System.Linq;
using OpenRA.Mods.Bam.Traits.Activities;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Effects;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits
{
    public class SpawnHostileInfo : ITraitInfo
    {
        public readonly int InitialDelay = 4000;

        public readonly int RandomExtraDelay = 300;

        public readonly int Delay = 600;

        public readonly string SpawnActor = "";

        public readonly string Image = "explosion";
        public readonly string EffectSequence = "smokecloud_effect";
        public readonly string EffectPalette = "bam11195";

        public object Create(ActorInitializer init)
        {
            return new SpawnHostile(init.Self, this);
        }
    }

    public class SpawnHostile : ITick, INotifyCreated
    {
        private int initDelay;
        private int delay;
        private int nextMaxCount;
        private SpawnHostileInfo info;
        private List<Player> players = new List<Player>();
        private Player currentPlayer;
        private int startingCash;

        private HashSet<Actor> idles = new HashSet<Actor>();

        public SpawnHostile(Actor self, SpawnHostileInfo info)
        {
            this.info = info;
            foreach (var player in self.World.Players)
            {
                if (player.PlayerReference.Playable && player.WinState == WinState.Undefined && !player.Spectating)
                    players.Add(player);
            }

            players.Sort();
            currentPlayer = players[self.World.SharedRandom.Next(0, players.Count)];
            nextMaxCount = self.World.SharedRandom.Next(info.Delay, info.Delay + info.RandomExtraDelay);
            startingCash = currentPlayer.PlayerActor.Trait<PlayerResources>().Cash;
        }

        void CyclePlayers()
        {
            currentPlayer = players.Count > players.IndexOf(currentPlayer) + 1 ? players[players.IndexOf(currentPlayer) + 1] : players.First();
        }

        void SpawnActor(Actor self)
        {
            var validCell = self.World.Map.FindTilesInCircle(self.Location, 3, true).ToArray();
            var cells = validCell.Where(c =>
                self.World.Map.Rules.Actors[info.SpawnActor].TraitInfo<MobileInfo>().CanEnterCell(self.World, self, c));

            if (cells.Any())
                self.World.AddFrameEndTask(w =>
                {
                    var init = new TypeDictionary
                    {
                        new LocationInit(self.ClosestCell(cells)),
                        new OwnerInit(self.Owner),
                        new FacingInit(250)
                    };
                    var a = w.CreateActor(true, info.SpawnActor, init);
                    if (a != null && !a.IsDead && a.IsInWorld)
                    {
                        a.QueueActivity(new HuntPlayer(a, currentPlayer));
                        CyclePlayers();
                        idles.Add(a);
                    }

                    w.Add(new SpriteEffect(
                        a.CenterPosition,
                        w,
                        info.Image,
                        info.EffectSequence,
                        info.EffectPalette));
                });
        }

        void ITick.Tick(Actor self)
        {
            if (initDelay < info.InitialDelay + info.InitialDelay  / (startingCash / 100))
            {
                initDelay++;
                return;
            }

            if (delay++ >= nextMaxCount)
            {
                foreach (var actor in idles.Where(a => a != null && !a.IsDead && a.IsInWorld && a.IsIdle))
                {
                    actor.QueueActivity(new HuntPlayer(actor, players[self.World.SharedRandom.Next(0, players.Count)]));
                }

                delay = 0;
                nextMaxCount = self.World.SharedRandom.Next(info.Delay, info.Delay + info.RandomExtraDelay);
                SpawnActor(self);
            }
        }

        void INotifyCreated.Created(Actor self)
        {
            if (!self.Owner.PlayerActor.TraitOrDefault<ITechTreePrerequisite>().ProvidesPrerequisites.Contains("CreepHostile"))
                self.Dispose();
        }
    }
}