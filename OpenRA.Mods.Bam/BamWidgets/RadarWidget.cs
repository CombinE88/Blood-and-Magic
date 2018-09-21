using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.Bam.BamWidgets;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Widgets;
using OpenRA.Primitives;
using OpenRA.Traits;
using OpenRA.Widgets;

namespace OpenRA.Mods.Kknd.Widgets.Ingame
{
    public class BamRadarWidget : Widget
    {
        private readonly BamUIWidget ingameUi;

        private readonly Sheet radarSheet;
        private readonly byte[] radarData;
        private readonly Sprite terrainSprite;
        private readonly Sprite shroudSprite;

        private int Size = 3;

        public Stance ShowStances { get; set; }

        public BamRadarWidget(BamUIWidget ingameUi)
        {
            this.ingameUi = ingameUi;

            radarSheet = new Sheet(SheetType.BGRA, new Size(ingameUi.World.Map.MapSize.X, ingameUi.World.Map.MapSize.Y * 2).NextPowerOf2());
            radarSheet.CreateBuffer();
            radarData = radarSheet.GetData();

            terrainSprite = new Sprite(radarSheet, new Rectangle(0, 0, ingameUi.World.Map.MapSize.X, ingameUi.World.Map.MapSize.Y), TextureChannel.RGBA);
            shroudSprite = new Sprite(radarSheet, new Rectangle(0, ingameUi.World.Map.MapSize.Y, ingameUi.World.Map.MapSize.X, ingameUi.World.Map.MapSize.Y), TextureChannel.RGBA);

            DrawTerrain();

            Visible = true;
        }

        private void DrawTerrain()
        {
            // TODO instead of using this colors, try a correct thumbnail variant.
            for (var y = 0; y < ingameUi.World.Map.MapSize.Y; y++)
            {
                for (var x = 0; x < ingameUi.World.Map.MapSize.X; x++)
                {
                    var type = ingameUi.World.Map.Rules.TileSet.GetTileInfo(ingameUi.World.Map.Tiles[new MPos(x, y)]);
                    radarData[(y * radarSheet.Size.Width + x) * 4] = type.LeftColor.B;
                    radarData[(y * radarSheet.Size.Width + x) * 4 + 1] = type.LeftColor.G;
                    radarData[(y * radarSheet.Size.Width + x) * 4 + 2] = type.LeftColor.R;
                    radarData[(y * radarSheet.Size.Width + x) * 4 + 3] = 0xff;
                }
            }
        }

        private void UpdateShroud()
        {
            var rp = ingameUi.World.RenderPlayer;

            for (var y = 0; y < ingameUi.World.Map.MapSize.Y; y++)
            {
                for (var x = 0; x < ingameUi.World.Map.MapSize.X; x++)
                {
                    var color = Color.FromArgb(0, Color.Black);

                    if (rp != null)
                    {
                        var pos = new MPos(x, y);
                        if (!rp.Shroud.IsExplored(pos))
                            color = Color.FromArgb(255, Color.Black);
                        else if (!rp.Shroud.IsVisible(pos))
                            color = Color.FromArgb(128, Color.Black);
                    }

                    radarData[radarSheet.Size.Width * ingameUi.World.Map.MapSize.Y * 4 + (y * radarSheet.Size.Width + x) * 4] = color.B;
                    radarData[radarSheet.Size.Width * ingameUi.World.Map.MapSize.Y * 4 + (y * radarSheet.Size.Width + x) * 4 + 1] = color.G;
                    radarData[radarSheet.Size.Width * ingameUi.World.Map.MapSize.Y * 4 + (y * radarSheet.Size.Width + x) * 4 + 2] = color.R;
                    radarData[radarSheet.Size.Width * ingameUi.World.Map.MapSize.Y * 4 + (y * radarSheet.Size.Width + x) * 4 + 3] = color.A;
                }
            }
        }

        public override string GetCursor(int2 pos)
        {
            var cell = new MPos((pos.X - RenderBounds.X) / Size, (pos.Y - RenderBounds.Y) / Size).ToCPos(ingameUi.World.Map);
            var worldPixel = ingameUi.WorldRenderer.ScreenPxPosition(ingameUi.World.Map.CenterOfCell(cell));
            var location = ingameUi.WorldRenderer.Viewport.WorldToViewPx(worldPixel);

            var mi = new MouseInput
            {
                Location = location,
                Button = Game.Settings.Game.MouseButtonPreference.Action,
                Modifiers = Game.GetModifierKeys()
            };

            var cursor = ingameUi.World.OrderGenerator.GetCursor(ingameUi.World, cell, worldPixel, mi);
            return cursor ?? "default";
        }

        public override bool HandleMouseInput(MouseInput mi)
        {
            var cell = new MPos((mi.Location.X - RenderBounds.X) / Size, (mi.Location.Y - RenderBounds.Y) / Size).ToCPos(ingameUi.World.Map);
            var pos = ingameUi.World.Map.CenterOfCell(cell);

            if ((mi.Event == MouseInputEvent.Down || mi.Event == MouseInputEvent.Move) && mi.Button == Game.Settings.Game.MouseButtonPreference.Cancel)
                ingameUi.WorldRenderer.Viewport.Center(pos);

            if (mi.Event == MouseInputEvent.Down && mi.Button == Game.Settings.Game.MouseButtonPreference.Action)
            {
                var location = ingameUi.WorldRenderer.Viewport.WorldToViewPx(ingameUi.WorldRenderer.ScreenPxPosition(pos));
                var fakemi = new MouseInput
                {
                    Event = MouseInputEvent.Down,
                    Button = Game.Settings.Game.MouseButtonPreference.Action,
                    Modifiers = mi.Modifiers,
                    Location = location
                };

                var controller = Ui.Root.Get<WorldInteractionControllerWidget>("INTERACTION_CONTROLLER");
                controller.HandleMouseInput(fakemi);
                fakemi.Event = MouseInputEvent.Up;
                controller.HandleMouseInput(fakemi);
            }

            return true;
        }

        public override void Draw()
        {
            Bounds = new Rectangle(25, 35, ingameUi.World.Map.MapSize.X * Size, ingameUi.World.Map.MapSize.Y * Size);
            UpdateShroud();

            // RadarBG
            var radarsheet = new Sheet(SheetType.BGRA, Game.ModData.DefaultFileSystem.Open("uibits/radarbg.png"));
            var radarBG = new Sprite(radarsheet, new Rectangle(0, 0, 172, 150), TextureChannel.RGBA);
            WidgetUtils.DrawRGBA(radarBG, new float2(RenderBounds.X + RenderBounds.Width / 2 - radarBG.Bounds.Width / 2, RenderBounds.Y + RenderBounds.Height / 2 - radarBG.Bounds.Height / 2));

            // WidgetUtils.FillRectWithColor(new Rectangle(RenderBounds.X - Size, RenderBounds.Y - Size, RenderBounds.Width + Size * 2, RenderBounds.Height + Size * 2), Color.White);

            radarSheet.CommitBufferedData();
            Game.Renderer.RgbaSpriteRenderer.DrawSprite(terrainSprite, new int2(RenderBounds.X, RenderBounds.Y), new int2(RenderBounds.Width, RenderBounds.Height));
            Game.Renderer.RgbaSpriteRenderer.DrawSprite(shroudSprite, new int2(RenderBounds.X, RenderBounds.Y), new int2(RenderBounds.Width, RenderBounds.Height));

            var cells = new List<Pair<CPos, Color>>();

            foreach (var e in ingameUi.World.ActorsWithTrait<IRadarSignature>())
            {
                if (!e.Actor.IsInWorld || e.Actor.IsDead || ingameUi.World.FogObscures(e.Actor) || e.Actor.Owner == null)
                    continue;

                if (!ShowStances.HasStance(Stance.Ally) && e.Actor.Owner.Stances[ingameUi.World.LocalPlayer].HasStance(Stance.Ally))
                    continue;

                if (!ShowStances.HasStance(Stance.Enemy) && e.Actor.Owner.Stances[ingameUi.World.LocalPlayer].HasStance(Stance.Enemy))
                    continue;

                cells.Clear();
                e.Trait.PopulateRadarSignatureCells(e.Actor, cells);

                foreach (var cell in cells)
                {
                    if (!ingameUi.World.Map.Contains(cell.First))
                        continue;

                    var pos = cell.First.ToMPos(ingameUi.World.Map.Grid.Type);
                    WidgetUtils.FillRectWithColor(new Rectangle(RenderBounds.X + pos.U * Size, RenderBounds.Y + pos.V * Size, Size, Size), e.Actor.Owner.Color.RGB);
                }
            }

            Game.Renderer.EnableScissor(RenderBounds);

            Game.Renderer.RgbaColorRenderer.DrawRect(
                new int2(RenderBounds.X, RenderBounds.Y) + ingameUi.WorldRenderer.Viewport.TopLeft / 32 * Size,
                new int2(RenderBounds.X, RenderBounds.Y) + ingameUi.WorldRenderer.Viewport.BottomRight / 32 * Size, 1,
                Color.White
            );

            foreach (var ping in ingameUi.RadarPings.Pings)
            {
                if (!ping.IsVisible())
                    continue;

                var center = ingameUi.World.Map.CellContaining(ping.Position).ToMPos(ingameUi.World.Map.Grid.Type);
                var points = ping.Points(new int2(RenderBounds.X + center.U * Size, RenderBounds.Y + center.V * Size)).ToArray();
                Game.Renderer.RgbaColorRenderer.DrawPolygon(points, 2, ping.Color);
            }

            Game.Renderer.DisableScissor();
        }
    }
}