﻿#region LICENSE

// The contents of this file are subject to the Common Public Attribution
// License Version 1.0. (the "License"); you may not use this file except in
// compliance with the License. You may obtain a copy of the License at
// https://github.com/NiclasOlofsson/MiNET/blob/master/LICENSE. 
// The License is based on the Mozilla Public License Version 1.1, but Sections 14 
// and 15 have been added to cover use of software over a computer network and 
// provide for limited attribution for the Original Developer. In addition, Exhibit A has 
// been modified to be consistent with Exhibit B.
// 
// Software distributed under the License is distributed on an "AS IS" basis,
// WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
// the specific language governing rights and limitations under the License.
// 
// The Original Code is MiNET.
// 
// The Original Developer is the Initial Developer.  The Initial Developer of
// the Original Code is Niclas Olofsson.
// 
// All portions of the code written by Niclas Olofsson are Copyright (c) 2014-2017 Niclas Olofsson. 
// All Rights Reserved.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using log4net;
using MiNET.Blocks;
using MiNET.Entities.Behaviors;
using MiNET.Utils;
using MiNET.Worlds;

namespace MiNET.Entities
{
	public class Mob : Entity
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof (Mob));

		public bool DespawnIfNotSeenPlayer { get; set; }
		public DateTime LastSeenPlayerTimer { get; set; }

		public List<IBehavior> Behaviors { get; } = new List<IBehavior>();
		private IBehavior _currentBehavior = null;
		public MobController Controller { get; private set; }

		public double Direction { get; set; }
		public virtual double Speed { get; set; } = 0.25f;

		public Entity Target { get; private set; }

		public Mob(int entityTypeId, Level level) : base(entityTypeId, level)
		{
			Width = Length = 0.6;
			Height = 1.80;
			Controller = new MobController(this);
		}

		public Mob(EntityType mobTypes, Level level) : this((int) mobTypes, level)
		{
		}

		public virtual void SetTarget(Entity target)
		{
			if (Target == target) return;

			Target = target;

			if (target != null && !IsTamed && !target.HealthManager.IsDead)
				IsAngry = true;
			else
				IsAngry = false;

			BroadcastSetEntityData();
		}

		public static double ClampDegrees(double degrees)
		{
			return Math.Floor((degrees%360 + 360)%360);
		}

		public Vector3 GetHorizDir()
		{
			Direction = ClampDegrees(Direction);
			Vector3 vector = new Vector3();

			double pitch = 0;
			double yaw = Direction.ToRadians();
			vector.X = (float) (-Math.Sin(yaw)*Math.Cos(pitch));
			vector.Y = (float) -Math.Sin(pitch);
			vector.Z = (float) (Math.Cos(yaw)*Math.Cos(pitch));

			return Vector3.Normalize(vector);
		}

		public override void SpawnEntity()
		{
			LastSeenPlayerTimer = DateTime.UtcNow;
			base.SpawnEntity();
		}


		public Vector3 _lastSentRotation = Vector3.Zero;
		public Vector3 _lastSentPos = new Vector3();

		public override void OnTick(Entity[] entities)
		{
			base.OnTick(entities);

			if (HealthManager.IsDead) return;

			bool noPlayersWithin32 = false;
			if (Level.EnableChunkTicking && DespawnIfNotSeenPlayer)
			{
				if (Level.Players.Count(player => player.Value.IsSpawned && Vector3.Distance(KnownPosition, player.Value.KnownPosition) < 128) == 0)
				{
					if (Log.IsDebugEnabled)
						Log.Debug($"Despawn because didn't see any players within 128 blocks.");

					DespawnEntity();
					return;
				}
				if (DateTime.UtcNow - LastSeenPlayerTimer > TimeSpan.FromSeconds(30))
				{
					if (Level.Players.Count(player => player.Value.IsSpawned && Vector3.Distance(KnownPosition, player.Value.KnownPosition) < 32) == 0)
					{
						if (Level.Random.Next(800) == 0)
						{
							if (Log.IsDebugEnabled)
								Log.Debug($"Despawn because didn't see any players within 32 blocks for 30s or longer. Last seen {LastSeenPlayerTimer}");

							DespawnEntity();
							return;
						}

						noPlayersWithin32 = true;
					}
					else
					{
						LastSeenPlayerTimer = DateTime.UtcNow;
					}
				}
				else
				{
				}
			}

			_currentBehavior = GetBehavior();

			// Execute move
			bool onGroundBefore = IsMobOnGround(KnownPosition);

			KnownPosition.X += (float) Velocity.X;
			KnownPosition.Y += (float) Velocity.Y;
			KnownPosition.Z += (float) Velocity.Z;

			// Fix potential fall through ground because of speed
			bool inWater = IsMobInFluid(KnownPosition);
			IsOnGround = !inWater && IsMobOnGround(KnownPosition);
			if (!onGroundBefore && IsOnGround)
			{
				while (Level.GetBlock(KnownPosition).IsSolid)
				{
					KnownPosition.Y = (float) Math.Floor(KnownPosition.Y + 1);
				}

				KnownPosition.Y = (float) Math.Floor(KnownPosition.Y);
				Velocity *= new Vector3(0, 1, 0);
			}

			//if (Math.Abs(_lastSentDir - Direction) < 1.1) Direction = _lastSentDir;
			//if (Math.Abs(_lastSentHeadYaw - KnownPosition.HeadYaw) < 1.1) KnownPosition.HeadYaw = (float) _lastSentHeadYaw;

			if ((_lastSentPos - KnownPosition).Length() > 0.01 || KnownPosition.GetDirection() != _lastSentRotation)
			{
				_lastSentPos = KnownPosition;
				_lastSentRotation = KnownPosition.GetDirection();

				BroadcastMove();
				BroadcastMotion();
			}

			var oldVelocity = Velocity;
			// Calculate velocity for next move
			_currentBehavior?.OnTick(entities);

			if (noPlayersWithin32)
			{
				Velocity = oldVelocity;
			}

			if (inWater && Level.Random.NextDouble() < 0.8)
			{
				Velocity += new Vector3(0, 0.039f, 0);
				Velocity *= new Vector3(0.2f, 1.0f, 0.2f);
			}
			else if (IsOnGround)
			{
				if (Velocity.Y < 0) Velocity *= new Vector3(1, 0, 1);
			}
			else
			{
				Velocity -= new Vector3(0, (float) Gravity, 0);
			}

			float drag = (float) (1 - Drag);
			if (inWater)
			{
				drag = 0.8F;
			}

			Velocity *= drag;
		}

		private IBehavior GetBehavior()
		{
			foreach (var behavior in Behaviors)
			{
				if (behavior == _currentBehavior)
				{
					if (behavior.CanContinue())
					{
						return behavior;
					}

					behavior.OnEnd();
					_currentBehavior = null;
				}

				if (behavior.ShouldStart())
				{
					if (_currentBehavior == null || Behaviors.IndexOf(_currentBehavior) > Behaviors.IndexOf(behavior))
					{
						_currentBehavior?.OnEnd();
						return behavior;
					}
				}
			}

			return null;
		}

		protected void CheckBlockAhead()
		{
			var length = Length/2;
			var direction = Vector3.Normalize(Velocity*1.00000101f);
			Vector3 position = KnownPosition;
			int count = (int) (Math.Ceiling(Velocity.Length()/length) + 2);
			for (int i = 0; i < count; i++)
			{
				var distVec = direction*(float) length*i;
				BlockCoordinates blockPos = position + distVec;
				Block block = Level.GetBlock(blockPos);
				if (block.IsSolid)
				{
					var yaw = (Math.Atan2(direction.X, direction.Z)*180.0D/Math.PI) + 180;
					//Log.Warn($"Will hit block {block} at angle of {yaw}");

					Ray ray = new Ray(position, direction);
					if (ray.Intersects(block.GetBoundingBox()).HasValue)
					{
						int face = IntersectSides(block.GetBoundingBox(), ray);

						//Log.Warn($"Hit block {block} at angle of {yaw} on face {face}");
						if (face == -1) continue;
						switch (face)
						{
							case 0:
								Velocity *= new Vector3(1, 1, 0);
								break;
							case 1:
								Velocity *= new Vector3(0, 1, 1);
								break;
							case 2:
								Velocity *= new Vector3(1, 1, 0);
								break;
							case 3:
								Velocity *= new Vector3(0, 1, 1);
								break;
							case 4: // Under
								Velocity *= new Vector3(1, 0, 1);
								break;
							//case 5:
							//	float ff = 0.6f * 0.98f;
							//	Velocity *= new Vector3(ff, 0.0f, ff);
							//	break;
						}
						return;
					}
					else
					{
						//Log.Warn($"Hit block {block} at angle of {yaw} had no intersection (strange)");
						Velocity *= new Vector3(0, 0, 0);
					}
				}
			}
		}

		public static int IntersectSides(BoundingBox box, Ray ray)
		{
			BoundingBox[] sides = new[]
			{
				// -Z 
				new BoundingBox(new Vector3(box.Min.X, box.Min.Y, box.Min.Z), new Vector3(box.Max.X, box.Max.Y, box.Min.Z)),

				// -X 
				new BoundingBox(new Vector3(box.Min.X, box.Min.Y, box.Min.Z), new Vector3(box.Min.X, box.Max.Y, box.Max.Z)),

				// +Z 
				new BoundingBox(new Vector3(box.Min.X, box.Min.Y, box.Max.Z), new Vector3(box.Max.X, box.Max.Y, box.Max.Z)),

				// +X 
				new BoundingBox(new Vector3(box.Max.X, box.Min.Y, box.Min.Z), new Vector3(box.Max.X, box.Max.Y, box.Max.Z)),


				// -Y
				new BoundingBox(new Vector3(box.Min.X, box.Min.Y, box.Min.Z), new Vector3(box.Max.X, box.Min.Y, box.Max.Z)),

				// +Y
				new BoundingBox(new Vector3(box.Min.X, box.Max.Y, box.Min.Z), new Vector3(box.Max.X, box.Max.Y, box.Max.Z)),
			};

			double? dist = null;
			int side = -1;
			for (int i = 0; i < sides.Length; i++)
			{
				var s = sides[i];
				var d = ray.Intersects(s);
				if (d.HasValue && (!dist.HasValue || d.Value < dist.Value))
				{
					dist = d;
					side = i;
				}
			}

			return side;
		}

		private static readonly int[] Layers = {-1, 0, +1};
		private static readonly int[] Arounds = {0, 1, -1};

		protected Block Intersects(Vector3 position)
		{
			BlockCoordinates pos = position;
			foreach (int layer in Layers)
			{
				foreach (int x in Arounds)
				{
					foreach (int z in Arounds)
					{
						var offset = new BlockCoordinates(x, layer, z);
						Block block = Level.GetBlock(pos + offset);
						if (block.IsSolid)
						{
							return block;
						}
					}
				}
			}

			return null;
		}

		private bool IsMobInFluid(Vector3 position)
		{
			float y = (float) (position.Y + Height*0.7);

			BlockCoordinates waterPos = new BlockCoordinates
			{
				X = (int) Math.Floor(position.X),
				Y = (int) Math.Floor(y),
				Z = (int) Math.Floor(position.Z)
			};

			var block = Level.GetBlock(waterPos);

			if (block == null || (block.Id != 8 && block.Id != 9)) return false;

			return y < Math.Floor(y) + 1 - ((1f/9f) - 0.1111111);
		}

		private bool IsMobStandingInFluid(Vector3 position)
		{
			Block block = Level.GetBlock(position);

			return block is FlowingWater || block is StationaryWater || block is FlowingLava || block is StationaryLava;
		}

		protected bool IsMobOnGround(Vector3 pos)
		{
			if (pos.Y - Math.Truncate(pos.Y) > 0.1)
				return IsMobInGround(pos);

			BlockCoordinates coord = pos;
			Block block = Level.GetBlock(coord + BlockCoordinates.Down);

			return block.IsSolid;
			//return block.IsSolid && block.GetBoundingBox().Contains(GetBoundingBox().OffsetBy(new Vector3(0, -0.1f, 0))) == ContainmentType.Intersects;
		}

		protected bool IsMobInGround(Vector3 position)
		{
			Block block = Level.GetBlock(position);

			return block.IsSolid;
		}
	}
}