using System.Collections.Generic;
using System.IO;
using OpenRA.Mods.Bam.FileSystem;

namespace OpenRA.Mods.Bam.FileFormats
{
    public class StfContainer
    {
        public Dictionary<string, StfFileLoader.StfFile.Entry> Files = new Dictionary<string, StfFileLoader.StfFile.Entry>();

        public StfContainer(Stream stream)
        {
            while (stream.Position < stream.Length)
            {
                var fileId = stream.ReadUInt16();

                var _compressionType = stream.ReadUInt32();

                var compressedSize = stream.ReadUInt32();
                var _uncompressedSize = stream.ReadUInt32();

                var fileType = stream.ReadUInt16();

                var _headerEntries = stream.ReadUInt16();
                var headerSize = stream.ReadUInt32();

                var _unk1 = stream.ReadUInt32(); // 0
                var _unk3 = stream.ReadUInt16(); // 0xfefe

                var size = headerSize + compressedSize;

                var fileEndings = new string[]
                {
                    "cel", // UNUSED
                    "ani", // Sprites
                    "pic", // UNUSED
                    "hmp", // Music
                    "wav", // Sounds
                    "pal", // Palettes
                    "dat", // UNUSED
                    "fon", // Fonts
                    "sqb", // Texts
                    "cnv", // UNUSED
                    "dcl", // UNUSED
                    "cgf", // UNUSED
                    "bnk", // UNUSED
                    "syn", // UNUSED
                    "chu", // UNUSED
                    "rot", // UNUSED
                    "sav", // UNUSED
                    "flc", // UNUSED
                    "tlb", // TODO what is this "exactly"? Contains unit stats and more.
                    "mif", // TODO what is this "exactly"? Possibly the maps.
                    "stf", // UNUSED
                    "8tr", // UNUSED
                    "smk", // UNUSED
                    "scr", // UNUSED
                };

                var filePosition = stream.Position;
                var fileSize = size;

                // Ugly hack, but openra trims buffers, and we need data before this one!
                if (fileType == 1)
                {
                    filePosition -= 12;
                    fileSize += 12;
                }

                Files.Add(fileId + "." + fileEndings[fileType], new StfFileLoader.StfFile.Entry((uint)filePosition, fileSize));
                stream.Position += size;
            }
        }
    }
}