using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using log4net;
using MiNET.Net;
using MiNET.Utils.Vectors;
using MiNET.Worlds;

namespace MiNET.Entities
{
	public class ExperienceOrb : Entity
	{
		public int Count = 1;
		public int pickupTimer = 0;
		public Vector3 FollowVelocity;
		public ExperienceOrb(Level level, int count = 1) : base(EntityType.ExperienceOrb, level)
		{

			Width = Length = Height = 0.25;
			Drag = 0.1;
			Gravity = 0.02;
			Velocity = new Vector3(0, 0.15f, 0);

			this.Count = count;

			HealthManager.IsInvulnerable = true;
		}

		public override void OnTick(Entity[] entities)
		{
			if (Velocity != Vector3.Zero)
			{
//				LogManager.GetLogger(EntityTypeId).Info($"Velocity vector: {Velocity}");
			}

			Velocity *= (float) (1.0 - Drag);
			Velocity -= new Vector3(0, (float) Gravity, 0);
			
			if (Level.GetBlock(KnownPosition.ToVector3() - new Vector3(0, 1, 0)).IsSolid && Level.GetBlock(KnownPosition.ToVector3()).GetBoundingBox().Contains(KnownPosition))
			{
				if (Velocity.Y < 0)
					Velocity = new Vector3(Velocity.X, 0, Velocity.Z);
				KnownPosition.Y = Level.GetBlock(KnownPosition.ToVector3()).Coordinates.Y;
			}


			Ray2 ray = new Ray2 { x = KnownPosition, d = Vector3.Normalize(Velocity) };

			bool intersects = false;
			var minDistance = 1f;
			var maxDistance = 8;
			var multiplier = 40;

			Player player = Level.GetSpawnedPlayers().OrderBy(entity => Vector3.Distance(KnownPosition.ToVector3(), entity.KnownPosition.ToVector3())).First();

			float vMax = ((float) Math.Clamp((maxDistance - 1) / 1.0f, 1f, maxDistance) - 1f) / multiplier;
			float v = Math.Min(((float) Math.Clamp((maxDistance - 1) / player.KnownPosition.DistanceTo(KnownPosition), minDistance, maxDistance) - minDistance) / multiplier, vMax);

			// move towards player with velocity and make it orbit around him

			Vector3 direction = Vector3.Normalize(player.KnownPosition.ToVector3() + new Vector3(-0.25f, 0.3f, -0.075f) - KnownPosition.ToVector3());
			direction /= direction.Length();
			Velocity += direction * v;

			LogManager.GetLogger(EntityTypeId).Info($"\nVelocity: {Velocity.X.ToString("0.00")}, {Velocity.Y.ToString("0.00")}, {Velocity.Z.ToString("0.00")}, \nv: {v}\ndistance: {player.KnownPosition.DistanceTo(KnownPosition)}\ndirection: {direction}");

			if (GetBoundingBox().Intersects(player.GetBoundingBox().OffsetBy(new Vector3(-0.25f, 0f, -0.075f))))
			{
				intersects = true;
				if (pickupTimer < 7)
				{
//						pickupTimer++;
				}

				player.ExperienceManager.AddExperience(Count);
				Level.BroadcastSound(KnownPosition.ToVector3(), LevelSoundEventType.Note, -40);
				DespawnEntity();
			}
			KnownPosition.X += Velocity.X;
			KnownPosition.Y += Velocity.Y;
			KnownPosition.Z += Velocity.Z;

			BroadcastMove();
		}
	}
}
