#region LICENSE

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
// All portions of the code written by Niclas Olofsson are Copyright (c) 2014-2018 Niclas Olofsson. 
// All Rights Reserved.

#endregion

using System.Numerics;
using log4net;
using MiNET.Net;
using MiNET.Worlds;

namespace MiNET.Particles
{
	public enum ParticleType
	{
		Bubble = 1,
		Critical = 3,
		BlockForceField = 4,
		Smoke = 6,
		Explode = 7,
		WhiteSmoke,
		Flame = 8,
		Lava = 10,
		LargeSmoke,
		Redstone = 12,
		RisingRedDust = 13,
		ItemBreak = 14,
		SnowballPoof = 15,
		LargeExplode = 16,
		HugeExplode = 17,
		MobFlame = 18,
		Heart = 19,
		Terrain = 20,
		TownAura = 21,
		Portal = 22,
		WaterSplash = 24,
		WaterWake = 26,
		DripHoney = 29,
		DripWater = 30,
		DripLava = 31,
		Dust = 32,
		MobSpell = 33,
		MobSpellAmbient = 34,
		MobSpellInstantaneous = 35,
		Ink,
		Slime = 37,
		RainSplash = 38,
		VillagerAngry = 39,
		VillagerHappy = 40,
		EnchantmentTable = 41,
		TrackingEmitter = 42,
		Note = 43,
		WitchSpell = 44,
		Carrot = 45,
		Unknown39,
		EndRod = 47,
		DragonsBreath,
		Spit,
		Totem = 50,
		Food,
		FireworksStarter = 52,
		FireworksSpark = 53,
		FireworksOverlay = 54,
		BalloonGas,
		ColoredFlame = 56,
		Sparkler,
		Conduit,
		BubbleColumnUp,
		BubbleColumnDown,
		Sneeze
	}

	public class LegacyParticle : Particle
	{
		public int Id { get; private set; }
		protected int Data { get; set; }

		public LegacyParticle(ParticleType particle, Level level): this((int) particle, level)
		{
//			LogManager.GetLogger(GetType()).Info($"LegacyParticle: {particle}({(int) particle})");
		}

		public LegacyParticle(int id, Level level) : base(level)
		{
			Id = id;
			Level = level;
		}

		public override void Spawn(Player[] players)
		{
			var particleEvent = McpeLevelEvent.CreateObject();
			particleEvent.eventId = (short) (0x4000 | Id);
			particleEvent.position = Position;
			particleEvent.data = Data;
			Level.RelayBroadcast(players, particleEvent);
		}
	}
}