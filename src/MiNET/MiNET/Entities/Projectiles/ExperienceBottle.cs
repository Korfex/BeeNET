using System;
using System.Drawing;
using System.Numerics;
using log4net;
using MiNET.Items;
using MiNET.Particles;
using MiNET.Worlds;

namespace MiNET.Entities.Projectiles
{
	public class ExperienceBottle : Projectile
	{
		public ExperienceBottle(Player shooter, Level level) : base(shooter, EntityType.ThrownBottleoEnchanting, level, 0)
		{
			Width = 0.25;
			Length = 0.25;
			Height = 0.25;

			Gravity = 0.2;
			Drag = 0.07;

			HealthManager.IsInvulnerable = true;
			DespawnOnImpact = true;
			BroadcastMovement = true;
		}

		public override void DespawnEntity()
		{
			Color color = Color.FromArgb(40, 67, 143);

			for (int i = 0; i < Level.Random.Next(80, 100); i++)
			{
				float r = (float) (Level.Random.NextDouble() * 2 * Math.PI);
				float rd = (float) (Level.Random.NextDouble() * 0.2 + 0.1) * 4.5f;
				float x = (float) (Math.Cos(r) * rd);
				float z = (float) (Math.Sin(r) * rd);
				new MobSpellParticle(Level, color) { Position = new Vector3(KnownPosition.X + x, KnownPosition.Y + (float) Level.Random.NextDouble() * 0.15f, KnownPosition.Z + z) }.Spawn();
			}

			//for (int i = 0; i < Level.Random.Next(60, 80); i++)
			//{
			//	float r = Level.Random.Next(360);
			//	float x = (float) (Math.Cos(r) * 1.5f);
			//	float y = Level.Random.Next(10) / 10f;
			//	float z = (float) (Math.Sin(r) * 1.5f);

			//	new MobSpellParticle(Level, color) { Position = new Vector3(KnownPosition.X + x, KnownPosition.Y + y, KnownPosition.Z + z) }.Spawn();
			//}
			//for (int i = 0; i < Level.Random.Next(60, 80); i++)
			//{
			//	new MobSpellParticle(Level, color) { Position = KnownPosition + new Vector3(((float)Level.Random.NextDouble() * 2) - 1, 0, ((float) Level.Random.NextDouble() * 2) - 1) }.Spawn();
			//	//particle = new ItemBreakParticle(Level, ItemFactory.GetItem(384)) { Position = KnownPosition };
			//	//particle.Spawn();
			//}
			for (int i = 0; i < 20; i++)
			{
				LegacyParticle particle = new MobSpellParticle(Level, color) { Position = KnownPosition + new Vector3((float) Level.Random.NextDouble() * 0.2f, (float) Level.Random.NextDouble() * 0.5f, (float) Level.Random.NextDouble() * 0.2f) };
				particle.Spawn();
			}
			float velocity = 0.3f;
			LogManager.GetLogger(this.EntityTypeId).Info($"-----------------");
			for (int i = 0; i < Level.Random.Next(2, 7); i++)
			{
				var xpOrb = new ExperienceOrb(Level) { KnownPosition = KnownPosition };
				xpOrb.Velocity = new Vector3(Level.Random.NextSingle() * 0.2f - 0.1f, Level.Random.NextSingle() * 0.1f, Level.Random.NextSingle() * 0.2f - 0.1f) * velocity;
//				LogManager.GetLogger(this.EntityTypeId).Info($"Random XYZ: {xpOrb.Velocity.X}, {xpOrb.Velocity.Y}, {xpOrb.Velocity.Z}");
				xpOrb.SpawnEntity();
				// log Level.Random.NextSingle()
			}
			Level.BroadcastSound(KnownPosition.ToVector3(), LevelSoundEventType.Glass);

			base.DespawnEntity();
		}
	}
}