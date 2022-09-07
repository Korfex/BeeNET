using System.Drawing;
using MiNET.Worlds;

namespace MiNET.Particles
{
	public class MobSpellParticle : LegacyParticle
	{
		public enum MobSpellType
		{
			Default = 0,
			Ambient,
			Instantaneous
		}

		public MobSpellParticle(Level level, Color color, MobSpellType type = MobSpellType.Default) : base(ParticleType.MobSpell + (int)type, level)
		{
			byte r = color.R;
			byte g = color.G;
			byte b = color.B;
			byte a = color.A;

			Data = ((a & 0xff) << 24) | ((r & 0xff) << 16) | ((g & 0xff) << 8) | (b & 0xff);
		}
	}
}