using System;
using System.Collections.Generic;
using System.IO;
using OpenRA.FileSystem;
using OpenRA.Graphics;
using OpenRA.Mods.Bam.FileFormats;
using OpenRA.Mods.Bam.SpriteLoaders;

namespace OpenRA.Mods.Bam.FileSystem
{
    public class StfFileLoader : IPackageLoader
    {
        public sealed class StfFile : IReadOnlyPackage
        {
            public class Entry
            {
                public readonly uint Offset;
                public readonly uint Length;

                public Entry(uint offset, uint length)
                {
                    Offset = offset;
                    Length = length;
                }
            }

            public string Name { get; private set; }

            private List<string> contents = new List<string>();

            public IEnumerable<string> Contents
            {
                get { return contents; }
            }

            readonly Stream stream;
            readonly Dictionary<string, Entry> index;
            readonly Dictionary<string, byte[]> virtualAnis = new Dictionary<string, byte[]>();

            public StfFile(Stream stream, string filename, OpenRA.FileSystem.FileSystem filesystem)
            {
                this.stream = stream;
                Name = filename;
                var container = new StfContainer(stream);
                index = container.Files;
                contents.AddRange(index.Keys);

                Stream remapStream;
                if (!filesystem.TryOpen(filename + ".yaml", out remapStream))
                    return;

                var remapYaml = MiniYaml.FromStream(remapStream);
                var aniLoader = new AniLoader();

                foreach (var entry in remapYaml)
                {
                    var targetAni = entry.Key;
                    var targetFrames = new List<AniLoader.AniSpriteFrame>();

                    foreach (var set in entry.Value.Value.Split(' '))
                    {
                        var flipX = set.Contains("x");
                        var flipY = set.Contains("y");
                        var sourceParts = set.Replace("x", "").Replace("y", "").Split(':');
                        ISpriteFrame[] sourceFrames;
                        aniLoader.TryParseSprite(GetStream(sourceParts[0] + ".ani"), out sourceFrames);

                        var targetFramesList = new List<int>();

                        if (sourceParts[1].Contains("-"))
                        {
                            var rangeParts = sourceParts[1].Split('-');
                            var from = int.Parse(rangeParts[0]);
                            var to = rangeParts.Length > 1 ? int.Parse(rangeParts[1]) : from;

                            if (from < to)
                                for (var i = from; i <= to; i++)
                                    targetFramesList.Add(i);
                            else
                                for (var i = from; i >= to; i--)
                                    targetFramesList.Add(i);
                        }
                        else
                            targetFramesList.Add(int.Parse(sourceParts[1]));

                        foreach (var frameId in targetFramesList)
                        {
                            var sourceFrame = sourceFrames[frameId] as AniLoader.AniSpriteFrame;

                            if (flipX)
                            {
                                var newData = new byte[sourceFrame.Data.Length];

                                for (var y = 0; y < sourceFrame.Size.Height; y++)
                                for (var x = 0; x < sourceFrame.Size.Width; x++)
                                    newData[y * sourceFrame.Size.Width + sourceFrame.Size.Width - x - 1] = sourceFrame.Data[y * sourceFrame.Size.Width + x];

                                sourceFrame.Data = newData;
                                sourceFrame.OffsetOrigin = new int2((sourceFrame.Size.Width - sourceFrame.OffsetOrigin.X * 2 - 1) / 2, sourceFrame.OffsetOrigin.Y);
                            }

                            if (flipY)
                            {
                                var newData = new byte[sourceFrame.Data.Length];

                                for (var y = 0; y < sourceFrame.Size.Height; y++)
                                for (var x = 0; x < sourceFrame.Size.Width; x++)
                                    newData[(sourceFrame.Size.Height - y - 1) * sourceFrame.Size.Width + x] = sourceFrame.Data[y * sourceFrame.Size.Width + x];

                                sourceFrame.Data = newData;
                                sourceFrame.OffsetOrigin = new int2(sourceFrame.OffsetOrigin.X, sourceFrame.Size.Height - sourceFrame.OffsetOrigin.Y - 1);
                            }

                            targetFrames.Add(sourceFrame);
                        }
                    }

                    var currentFrameOffset = 0;
                    var virtualStream = new MemoryStream();
                    virtualStream.Write(BitConverter.GetBytes((ushort)targetFrames.Count), 0, 2);
                    virtualStream.Write(targetFrames.Count * 18);
                    virtualStream.Write(0);
                    virtualStream.Write(BitConverter.GetBytes((ushort)0xfefe), 0, 2);

                    foreach (var targetFrame in targetFrames)
                    {
                        virtualStream.Write(targetFrame.OffsetOrigin.X);
                        virtualStream.Write(targetFrame.OffsetOrigin.Y);
                        virtualStream.Write(BitConverter.GetBytes((ushort)targetFrame.Size.Width / 2), 0, 2);
                        virtualStream.Write(BitConverter.GetBytes((ushort)targetFrame.Size.Height), 0, 2);
                        virtualStream.Write(BitConverter.GetBytes((ushort)0), 0, 2); // priority
                        virtualStream.Write(currentFrameOffset);
                        currentFrameOffset += targetFrame.Data.Length / 2;
                    }

                    foreach (var targetFrame in targetFrames)
                        for (var y = 0; y < targetFrame.Size.Height; y++)
                        for (var x = 0; x < targetFrame.Size.Width; x += 2)
                            virtualStream.WriteByte(targetFrame.Data[y * targetFrame.Size.Width + x]);

                    virtualAnis.Add(targetAni + ".ani", virtualStream.ToArray());
                }

                contents.AddRange(virtualAnis.Keys);
            }

            public Stream GetStream(string filename)
            {
                Entry entry;

                if (!index.TryGetValue(filename, out entry))
                {
                    byte[] virtualData;

                    if (!virtualAnis.TryGetValue(filename, out virtualData))
                        return null;

                    return new MemoryStream(virtualData);
                }

                stream.Seek(entry.Offset, SeekOrigin.Begin);
                var bytes = stream.ReadBytes((int) entry.Length);

                return filename.EndsWith(".tlb") ? new TlbMemoryStream(bytes) : new MemoryStream(bytes);
            }

            public bool Contains(string filename)
            {
                return index.ContainsKey(filename) || virtualAnis.ContainsKey(filename);
            }

            public IReadOnlyPackage OpenPackage(string filename, OpenRA.FileSystem.FileSystem context)
            {
                // Not implemented
                return null;
            }

            public void Dispose()
            {
                stream.Dispose();
            }
        }

        public bool TryParsePackage(Stream stream, string filename, OpenRA.FileSystem.FileSystem filesystem, out IReadOnlyPackage package)
        {
            if (!filename.EndsWith(".stf"))
            {
                package = null;
                return false;
            }

            package = new StfFile(stream, filename, filesystem);
            return true;
        }
    }
}
