using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiNET.Net.Types
{
	public class MapPixels
	{
		public MapPixels(int size)
		{
			Data = new MapPixel[size];
		}
		public MapPixels(MapPixel[] data)
		{
			Data = data;
		}
		public MapPixel[] Data;
	}
	public class MapPixel
	{
		public int Color; // replace with rgba color type
		public ushort Index;
	}
}
