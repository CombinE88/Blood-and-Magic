using System.IO;

namespace OpenRA.Mods.Bam.FileFormats
{
	public class PalPalette
	{
		public uint[] Colors { get; set; }

		public PalPalette(Stream stream)
		{
			Colors = new uint[256];
	        for (var i = 0; i < 256; i++)
            	Colors[i] = (uint) ((0xff << 24) | (stream.ReadUInt8() << 16) | (stream.ReadUInt8() << 8) | stream.ReadUInt8());
        }
	}
}
