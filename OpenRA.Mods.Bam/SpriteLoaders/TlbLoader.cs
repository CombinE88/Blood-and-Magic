using System.Drawing;
using System.IO;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.Bam.FileFormats;

namespace OpenRA.Mods.Bam.SpriteLoaders
{
    public class TlbLoader : ISpriteLoader
    {
        public class TlbSpriteFrame : ISpriteFrame
        {
            public Size Size { get; private set; }
            public Size FrameSize { get; private set; }
            public float2 Offset { get; set; }
            public byte[] Data { get; set; }

            public bool DisableExportPadding
            {
                get { return true; }
            }

            public TlbSpriteFrame(TlbTile tlbTile)
            {
                var width = tlbTile.Width;
                var height = tlbTile.Height;

                Size = new Size(width, height);
                FrameSize = new Size(width, height);
                Offset = float2.Zero;
                Data = tlbTile.Pixels;
            }
        }

        private bool IsTlb(Stream stream)
        {
            return stream is TlbMemoryStream;
        }

        public bool TryParseSprite(Stream stream, out ISpriteFrame[] frames)
        {
            if (!IsTlb(stream))
            {
                frames = null;
                return false;
            }

            var tlbTileLibrary = new TlbTileLibrary(stream);
            frames = tlbTileLibrary.Tiles.Select(frame => new TlbSpriteFrame(frame)).ToArray();
            return true;
        }
    }
}
