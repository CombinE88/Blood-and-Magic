using System.Linq;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits.Render
{
    public class ChangeImageOnTheatreInfo : ITraitInfo
    {
        [Desc("Which sprite body to modify.")] public readonly string Body = "body";

        public object Create(ActorInitializer init)
        {
            return new ChangeImageOnTheatre(init, this);
        }
    }

    public class ChangeImageOnTheatre : INotifyCreated
    {
        private ChangeImageOnTheatreInfo info;

        public ChangeImageOnTheatre(ActorInitializer init, ChangeImageOnTheatreInfo info)
        {
            this.info = info;
        }

        void INotifyCreated.Created(Actor self)
        {
            var wsb = self.TraitsImplementing<WithSpriteBody>().Single(w => w.Info.Name == info.Body);

            wsb.DefaultAnimation.ReplaceAnim(self.World.Map.Rules.TileSet.Name.ToLower() + "-idle");
        }
    }
}