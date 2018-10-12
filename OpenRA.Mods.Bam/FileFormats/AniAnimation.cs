using System.Collections.Generic;
using System.IO;

namespace OpenRA.Mods.Bam.FileFormats
{
    public class AniAnimation
    {
        public List<AniFrame> Frames = new List<AniFrame>();

        public AniAnimation(Stream stream)
        {
            // We need to re-read a part of the container file for this...
            var headerEntries = stream.ReadUInt16();
            var headerSize = stream.ReadUInt32();
            stream.Position += 6;

            var pixelsStart = stream.Position + headerSize;

            for (var i = 0; i < headerEntries; i++)
            {
                Frames.Add(new AniFrame(stream, pixelsStart));
            }
        }
    }

    public class AniFrame
    {
        public int OriginX;
        public int OriginY;
        public ushort Width;
        public ushort Height;
        public byte[] Pixels;

        public AniFrame(Stream stream, long pixelsStart)
        {
            OriginX = stream.ReadInt32();
            OriginY = stream.ReadInt32();

            Width = (ushort)(stream.ReadUInt16() * 2);
            Height = stream.ReadUInt16();
            var _unk1 = stream.ReadUInt16(); // priority
            var frameOffset = stream.ReadUInt32();

            var position = stream.Position;
            stream.Position = pixelsStart + frameOffset;
            Pixels = new byte[Width * Height];

            for (var i = 0; i < Width * Height; i += 2)
                Pixels[i] = Pixels[i + 1] = stream.ReadUInt8();

            stream.Position = position;
        }
    }
}