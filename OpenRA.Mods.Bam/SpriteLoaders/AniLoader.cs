using System.Drawing;
using System.IO;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.Bam.FileFormats;

namespace OpenRA.Mods.Bam.SpriteLoaders
{
	public class AniLoader : ISpriteLoader
	{
		public class AniSpriteFrame : ISpriteFrame
		{
			public Size Size { get; private set; }
			public Size FrameSize { get; private set; }
			public float2 Offset { get; set; }
			public byte[] Data { get; set; }
			public bool DisableExportPadding { get { return true; } }

			public AniSpriteFrame(AniFrame aniFrame)
			{
				var width = aniFrame.Width;
				var height = aniFrame.Height;
				var x = aniFrame.OriginX;
				var y = aniFrame.OriginY;

				Size = new Size(width, height);
				FrameSize = new Size(width, height);
				Offset = new int2(0, 0);
				Data = aniFrame.Pixels;
			}
		}

		private bool IsAni(Stream stream)
		{
			var position = stream.Position;
			stream.Position += 10;
			var magic = stream.ReadUInt16();
			stream.Position = position;
			return magic == 0xfefe;
		}

		public bool TryParseSprite(Stream stream, out ISpriteFrame[] frames)
		{
			if (!IsAni(stream))
			{
				frames = null;
				return false;
			}

			var aniAnimation = new AniAnimation(stream);
			frames = aniAnimation.Frames.Select(frame => new AniSpriteFrame(frame)).ToArray();
			return true;
		}
	}
}
