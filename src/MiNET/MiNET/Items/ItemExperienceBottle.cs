using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using MiNET.Entities;
using MiNET.Entities.Projectiles;
using MiNET.Utils.Vectors;
using MiNET.Worlds;

namespace MiNET.Items
{
	public class ItemExperienceBottle : Item 
	{ 
		public ItemExperienceBottle() : base("minecraft:experience_bottle", 384) { }
		public override void UseItem(Level world, Player player, BlockCoordinates blockCoordinates)
		{
			// play thrown sound
			player.SendSound(player.KnownPosition.ToVector3(), LevelSoundEventType.Throw, 1);

			float force = 1.0f;

			var xpbottle = new ExperienceBottle(player, world);
			xpbottle.KnownPosition = (PlayerLocation) player.KnownPosition.Clone();
			xpbottle.KnownPosition.Y += 1.62f;
			xpbottle.Velocity = (xpbottle.KnownPosition.GetDirection().Normalize() + new Vector3(0, 0.7f, 0)) * (force);
			xpbottle.SpawnEntity();
		}
	}
}
