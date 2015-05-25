using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using SonicRetro.SonLVL.API;

namespace GetArt.NET
{
	public static class Program
	{
		static string[] filetypes = { "bmp", "png", "gif", "jpg" };

		static void Main(string[] args)
		{
			LevelData.littleendian = false;
			string bmpfile = null;
			foreach (string extension in filetypes)
				if (File.Exists("Bitmap." + extension))
				{
					bmpfile = "Bitmap." + extension;
					break;
				}
			if (bmpfile == null)
			{
				Console.WriteLine("Bitmap file could not be found! Valid extensions are " + string.Join(", ", filetypes));
				return;
			}
			Bitmap img = new Bitmap(bmpfile);
			if (img.Width % 8 > 0 | img.Height % 8 > 0)
			{
				Bitmap dest = new Bitmap(img.Width % 8 == 0 ? img.Width : img.Width + (8 - (img.Width % 8)), img.Height % 8 == 0 ? img.Height : img.Height + (8 - (img.Height % 8)), img.PixelFormat);
				if ((img.PixelFormat & PixelFormat.Indexed) == PixelFormat.Indexed)
					dest.Palette = img.Palette;
				using (Graphics gfx = Graphics.FromImage(dest))
					gfx.DrawImage(img, 0, 0, img.Width, img.Height);
				img.Dispose();
				img = dest;
			}
			SonLVLColor[] palette = new SonLVLColor[64];
			if ((img.PixelFormat & PixelFormat.Indexed) == PixelFormat.Indexed)
			{
				for (int i = 0; i < Math.Min(64, img.Palette.Entries.Length); i++)
					palette[i] = new SonLVLColor(img.Palette.Entries[i]);
			}
			else
			{
				bmpfile = null;
				foreach (string extension in filetypes)
					if (File.Exists("Palette." + extension))
					{
						bmpfile = "Palette." + extension;
						break;
					}
				if (bmpfile == null)
				{
					Console.WriteLine("Palette file could not be found! Valid extensions are " + string.Join(", ", filetypes));
					return;
				}
				using (Bitmap palbmp = new Bitmap(bmpfile))
				{
					int i = 0;
					for (int y = 0; y < palbmp.Height; y += 8)
					{
						for (int x = 0; x < palbmp.Width; x += 8)
						{
							palette[i++] = new SonLVLColor(palbmp.GetPixel(x, y));
							if (i == 64)
								break;
						}
						if (i == 64)
							break;
					}
				}
			}
			using (Bitmap palbmp = new Bitmap(1, 1, PixelFormat.Format8bppIndexed))
				LevelData.BmpPal = palbmp.Palette;
			using (FileStream palfile = File.Create("Palette.bin"))
				for (int i = 0; i < 64; i++)
				{
					palfile.Write(ByteConverter.GetBytes(palette[i].MDColor), 0, 2);
					LevelData.BmpPal.Entries[i] = palette[i].RGBColor;
				}
			for (int i = 64; i < 256; i++)
				LevelData.BmpPal.Entries[i] = Color.Transparent;
			int w = img.Width;
			int h = img.Height;
			bool[,] priority = new bool[w / 8, h / 8];
			bmpfile = null;
			foreach (string extension in filetypes)
				if (File.Exists("Priority." + extension))
				{
					bmpfile = "Priority." + extension;
					break;
				}
			if (bmpfile != null)
				using (Bitmap pribmp = new Bitmap(bmpfile))
					LevelData.GetPriMap(pribmp, priority);
			int pal = 0;
			bool match = false;
			List<BitmapBits> tiles = new List<BitmapBits>();
			using (FileStream art = File.Create("Art.bin"))
			using (FileStream mapstr = File.Create("Map.bin"))
			{
				byte[] tile;
				PatternIndex map;
				for (int y = 0; y < h / 8; y++)
					for (int x = 0; x < w / 8; x++)
					{
						map = new PatternIndex() { Priority = priority[x, y] };
						tile = LevelData.BmpToTile(img.Clone(new Rectangle(x * 8, y * 8, 8, 8), img.PixelFormat), out pal);
						map.Palette = (byte)pal;
						BitmapBits bits = BitmapBits.FromTile(tile, 0);
						match = false;
						for (int i = 0; i < tiles.Count; i++)
						{
							if (tiles[i].Equals(bits))
							{
								match = true;
								map.Tile = (ushort)i;
								break;
							}
							BitmapBits flip = new BitmapBits(bits);
							flip.Flip(true, false);
							if (tiles[i].Equals(flip))
							{
								match = true;
								map.Tile = (ushort)i;
								map.XFlip = true;
								break;
							}
							flip = new BitmapBits(bits);
							flip.Flip(false, true);
							if (tiles[i].Equals(flip))
							{
								match = true;
								map.Tile = (ushort)i;
								map.YFlip = true;
								break;
							}
							flip = new BitmapBits(bits);
							flip.Flip(true, true);
							if (tiles[i].Equals(flip))
							{
								match = true;
								map.Tile = (ushort)i;
								map.XFlip = true;
								map.YFlip = true;
								break;
							}
						}
						if (!match)
						{
							tiles.Add(bits);
							art.Write(tile, 0, tile.Length);
							map.Tile = (ushort)(tiles.Count - 1);
						}
						mapstr.Write(map.GetBytes(), 0, PatternIndex.Size);
					}
			}
			img.Dispose();
		}
	}
}
