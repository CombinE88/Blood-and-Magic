#region Copyright & License Information

/*
 * Copyright 2007-2018 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */

#endregion

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenRA.Activities;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Activities;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits.Render
{
    [Desc("Renders a decorative animation on units and buildings.")]
    public class WithWalkOnTerrainOverlayInfo : ITraitInfo, Requires<MobileInfo>
    {
        public readonly string Image = "effects";

        [Desc("Sequence name to use")] [SequenceReference]
        public readonly string Sequence = "water";

        [Desc("Position relative to body")] public readonly WVec Offset = WVec.Zero;

        public readonly string[] TerrainTypes = { "Swamp" };

        [Desc("Custom palette name")] [PaletteReference("IsPlayerPalette")]
        public readonly string Palette = "bam11195";

        [Desc("Custom palette is a player palette BaseName")]
        public readonly bool IsPlayerPalette = false;

        public object Create(ActorInitializer init)
        {
            return new WithWalkOnTerrainOverlay(init.Self, this);
        }
    }

    public class WithWalkOnTerrainOverlay
    {
        private Animation overlay;
        private WithWalkOnTerrainOverlayInfo info;
        private IMove movement;

        public WithWalkOnTerrainOverlay(Actor self, WithWalkOnTerrainOverlayInfo info)
        {
            this.info = info;

            var rs = self.Trait<RenderSprites>();

            movement = self.Trait<IMove>();

            overlay = new Animation(self.World, this.info.Image);

            overlay.PlayRepeating(info.Sequence);

            var anim = new AnimationWithOffset(overlay,
                () => info.Offset,
                () => !info.TerrainTypes.Contains(self.World.Map.GetTerrainInfo(self.World.Map.CellContaining(self.CenterPosition)).Type) || self.IsDead || !movement.IsMoving);

            rs.Add(anim, info.Palette, info.IsPlayerPalette);
        }
    }
}