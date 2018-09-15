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

using System;
using System.Linq;
using OpenRA.Mods.Bam.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits.Render
{
    public class WithManaAnimationInfo : ConditionalTraitInfo, Requires<WithSpriteBodyInfo>, Requires<ManaShooterInfo>
    {
        [Desc("Displayed while attacking.")] [SequenceReference]
        public readonly string Sequence = "mana";

        [Desc("Which sprite body to modify.")]
        public readonly string Body = "body";

        public override object Create(ActorInitializer init)
        {
            return new WithManaAnimation(init, this);
        }

        public override void RulesetLoaded(Ruleset rules, ActorInfo ai)
        {
            var matches = ai.TraitInfos<WithSpriteBodyInfo>().Count(w => w.Name == Body);
            if (matches != 1)
                throw new YamlException("WithMoveAnimation needs exactly one sprite body with matching name.");

            base.RulesetLoaded(rules, ai);
        }
    }

    public class WithManaAnimation : ConditionalTrait<WithManaAnimationInfo>
    {
        readonly WithSpriteBody wsb;
        private Actor self;
        private WithManaAnimationInfo info;

        public WithManaAnimation(ActorInitializer init, WithManaAnimationInfo info)
            : base(info)
        {
            this.info = info;
            self = init.Self;
            wsb = init.Self.TraitsImplementing<WithSpriteBody>().Single(w => w.Info.Name == Info.Body);
        }

        public void Play(Action onComplete)
        {
            PlayManaAnimation(self, onComplete);
        }

        public void PlayManaAnimation(Actor self, Action onComplete)
        {
            if (IsTraitDisabled)
                return;

            wsb.PlayCustomAnimation(self, info.Sequence, () => { onComplete(); });
        }
    }
}