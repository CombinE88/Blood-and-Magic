using System.Collections.Generic;
using System.Drawing;
using System.IO;
using OpenRA.Graphics;
using OpenRA.Mods.Bam.FileFormats;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Traits.World
{
	class PaletteFromPalInfo : ITraitInfo
	{
		[FieldLoader.Require, PaletteDefinition]
		[Desc("internal palette name")]
		public readonly string Name = null;

		[Desc("If defined, load the palette only for this tileset.")]
		public readonly string Tileset = null;

		[FieldLoader.Require]
		[Desc("filename to load")]
		public readonly string Filename = null;

		public readonly bool AllowModifiers = true;

		public object Create(ActorInitializer init) { return new PaletteFromPal(init.World, this); }
	}

	class PaletteFromPalLoader : IPaletteLoader
	{
		public ImmutablePalette ReadPalette(Stream stream, int[] remap)
		{
			var palette = new PalPalette(stream).Colors;
			palette[254] = 0;

			return new ImmutablePalette(palette);
		}
	}

	class PaletteFromPal : ILoadsPalettes, IProvidesAssetBrowserPalettes
	{
		readonly OpenRA.World world;
		readonly PaletteFromPalInfo info;

		public PaletteFromPal(OpenRA.World world, PaletteFromPalInfo info)
		{
			this.world = world;
			this.info = info;
		}

		public void LoadPalettes(WorldRenderer wr)
		{
			if (info.Tileset != null && info.Tileset.ToLowerInvariant() != world.Map.Tileset.ToLowerInvariant())
				return;

			Log.Write("debug", "PAL " + info.Filename);
			wr.AddPalette(info.Name, new PaletteFromPalLoader().ReadPalette(world.Map.Open(info.Filename), new int[0]), info.AllowModifiers);
		}

		public IEnumerable<string> PaletteNames
		{
			get
			{
				if (info.Tileset == null || info.Tileset == world.Map.Rules.TileSet.Id)
					yield return info.Name;
			}
		}
	}
}
