using System;
using OpenRA.Activities;
using OpenRA.Mods.Bam.Effect;
using OpenRA.Mods.Bam.Traits.Mana;
using OpenRA.Mods.Bam.Traits.Trinkets;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Bam.Activities
{
	public enum AdvancedTransformEffect { TRANSFORM, FADE, NONE }

	public class AdvancedTransform : Activity
	{
		private string toActor;
		private AdvancedTransformEffect effect;
		private Action<Actor> complete;

		public AdvancedTransform(string toActor, AdvancedTransformEffect effect, Action<Actor> complete = null)
		{
			this.toActor = toActor;
			this.effect = effect;
			this.complete = complete;
		}

		protected override void OnFirstRun(Actor self)
		{
			switch (effect)
			{
				case AdvancedTransformEffect.TRANSFORM:
					self.Trait<WithSpriteBody>().PlayCustomAnimation(self, "transform", () => Transform(self));
					break;

				case AdvancedTransformEffect.FADE:
					var wsb = self.Trait<WithSpriteBody>();
					var rsi = self.Info.TraitInfo<RenderSpritesInfo>();

					if (wsb.DefaultAnimation.HasSequence("after"))
					{
						self.World.Add(new AdvancedSpriteEffect(self.CenterPosition, self.World, self.Info.Name, "after", rsi.PlayerPalette + self.Owner.InternalName));
						Transform(self);
					}
					else
					{
						self.World.AddFrameEndTask(w =>
						{
							w.Add(new AdvancedSpriteEffect(self.CenterPosition, w, toActor, "before", rsi.PlayerPalette + self.Owner.InternalName, () => Transform(self)));
						});
					}

					break;

				default:
					Transform(self);
					break;
			}
		}

		public void Transform(Actor self)
		{
			self.World.AddFrameEndTask(world =>
			{
				foreach (var notifyTransform in self.TraitsImplementing<INotifyTransform>())
					notifyTransform.OnTransform(self);

				var init = new TypeDictionary
				{
					new LocationInit(self.Location),
					new OwnerInit(self.Owner)
				};

				var health = self.TraitOrDefault<IHealth>();
				if (health != null)
					init.Add(new HealthInit(health.HP * 100 / health.MaxHP));

				foreach (var modifier in self.TraitsImplementing<ITransformActorInitModifier>())
					modifier.ModifyTransformActorInit(self, init);

				var newActor = world.CreateActor(toActor, init);
				foreach (var nt in self.TraitsImplementing<INotifyTransform>())
					nt.AfterTransform(newActor);

				var trinket = self.TraitOrDefault<CanHoldTrinket>();
				if (trinket != null)
				{
					var canHoldTrinket = newActor.TraitOrDefault<CanHoldTrinket>();
					if (canHoldTrinket != null)
					{
						canHoldTrinket.Current = trinket.Current;
						canHoldTrinket.IgnorePickup = trinket.IgnorePickup;
					}
					else
						trinket.Drop(self);
				}

				var manaStorage = self.TraitOrDefault<ManaStorage>();
				if (manaStorage != null)
				{
					if (newActor.Info.HasTraitInfo<ManaStorageInfo>())
						newActor.Trait<ManaStorage>().Current = manaStorage.Current;
					else if (self.Info.HasTraitInfo<ManaGeneratorInfo>())
						self.Trait<ManaGenerator>().ShootMana(self);
				}

				var selected = world.Selection.Contains(self);
				if (selected)
					world.Selection.Add(world, newActor);

				var controlgroup = world.Selection.GetControlGroupForActor(self);
				if (controlgroup.HasValue)
					world.Selection.AddToControlGroup(newActor, controlgroup.Value);

				if (complete != null)
					complete(newActor);

				self.Dispose();
			});
		}

		public override Activity Tick(Actor self)
		{
			if (IsCanceled)
				return NextActivity;

			return this;
		}

		public override bool Cancel(Actor self, bool keepQueue = false)
		{
			return false;
		}
	}
}
