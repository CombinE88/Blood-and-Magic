using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits.Mana
{
    public class ManaGeneratorInfo : ConditionalTraitInfo, Requires<ManaStorageInfo>
    {
        public readonly int Interval = 250;

        public readonly int OverWait = 100;

        public override object Create(ActorInitializer init)
        {
            return new ManaGenerator(init, this);
        }
    }

    public class ManaGenerator : ConditionalTrait<ManaGeneratorInfo>, ITick, IResolveOrder
    {
        private ManaGeneratorInfo info;
        private ManaStorage manaStorage;
        private PlayerResources playerResources;
        private int ticker;

        public ManaGenerator(ActorInitializer init, ManaGeneratorInfo info) : base(info)
        {
            this.info = info;
            manaStorage = init.Self.Trait<ManaStorage>();
            playerResources = init.Self.Owner.PlayerActor.TraitOrDefault<PlayerResources>();
        }

        public void Tick(Actor self)
        {
            ticker++;

            if (manaStorage.Current < manaStorage.Capacity && ticker >= info.Interval)
            {
                ticker = 0;
                manaStorage.Current++;
            }
            else if (manaStorage.Current == manaStorage.Capacity && ticker >= info.OverWait)
                ShootMana(self);
        }

        public void ShootMana(Actor self)
        {
            if (manaStorage.Current > 0)
            {
                self.Trait<WithSpriteBody>().PlayCustomAnimation(self, "mana");
                playerResources.GiveCash(manaStorage.Current);
                manaStorage.Current = 0;
            }
        }

        public void ResolveOrder(Actor self, Order order)
        {
            if (order.OrderString != "ShootMana")
                return;

            ShootMana(self);
        }
    }
}
