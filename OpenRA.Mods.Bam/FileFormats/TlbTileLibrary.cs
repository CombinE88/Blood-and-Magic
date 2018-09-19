using System.Collections.Generic;
using System.IO;

namespace OpenRA.Mods.Bam.FileFormats
{
    public class TlbMemoryStream : MemoryStream {
        public TlbMemoryStream(byte[] bytes) : base(bytes) { }
    }

    public class TlbTileLibrary
    {
        public List<TlbTile> Tiles = new List<TlbTile>();

        public TlbTileLibrary(Stream stream)
        {
            stream.ReadUInt32(); // id
            var numTiles = stream.ReadUInt32();
            var numObjects = stream.ReadUInt32();

            stream.Position += 64; // 0x00 filled
            stream.Position += 52 * numObjects;
            stream.Position += (128 - numObjects) * 52; // 0x00 filled

            for (var i = 0; i < numTiles; i++) {
                Tiles.Add(new TlbTile(stream));
            }
        }
    }

    public class TlbTile
    {
        public int Width = 40;
        public int Height = 38;
        public byte[] Pixels;

        public TlbTile(Stream stream)
        {
            stream.ReadUInt8(); // id
            stream.ReadUInt32(); // swapTile
            stream.ReadUInt32(); // aniRes
            stream.ReadUInt32(); // aniDelay

            Pixels = new byte[Width * Height];
            for (var i = 0; i < Width * Height; i += 2)
                Pixels[i] = Pixels[i + 1] = stream.ReadUInt8();
        }
    }
}
