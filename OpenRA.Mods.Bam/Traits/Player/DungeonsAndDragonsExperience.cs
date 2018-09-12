using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits.Player
{
    public class DungeonsAndDragonsExperienceInfo : ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new DungeonsAndDragonsExperience();
        }
    }

    public class DungeonsAndDragonsExperience : ISync
    {
        [Sync] public int Experience;

        public bool TakeCash(int num)
        {
            if (Experience < num)
                return false;

            Experience -= num;

            return true;
        }

        public void AddCash(int num)
        {
            Experience = Experience + num;
        }
    }
}