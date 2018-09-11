using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits.Mana
{
	public class ManaStorageInfo : ITraitInfo
	{
		public readonly int Capacity = 10;

		public object Create(ActorInitializer init)
		{
			return new ManaStorage(init, this);
		}
	}

	public class ManaStorage
	{
		private ManaStorageInfo info;

		public int Current = 0;
		public int Capacity { get { return info.Capacity; } }

		public ManaStorage(ActorInitializer init, ManaStorageInfo info)
		{
			this.info = info;
		}
	}
}
