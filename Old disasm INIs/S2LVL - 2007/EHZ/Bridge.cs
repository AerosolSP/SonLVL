using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using SonicRetro.SonLVL;

namespace S2ObjectDefinitions.EHZ
{
	class Bridge : ObjectDefinition
	{
		private Sprite img;

		public override void Init(Dictionary<string, string> data)
		{
			byte[] artfile = ObjectHelper.OpenArtFile("../art/nemesis/EHZ bridge.bin", Compression.CompressionType.Nemesis);
			byte[] mapfile = System.IO.File.ReadAllBytes("../mappings/sprite/obj11_b.bin");
			img = ObjectHelper.MapToBmp(artfile, mapfile, 0, 2);
		}

		public override ReadOnlyCollection<byte> Subtypes()
		{
			return new ReadOnlyCollection<byte>(new byte[] { 8, 10, 12, 14, 16 });
		}

		public override string Name()
		{
			return "Bridge";
		}

		public override bool RememberState()
		{
			return false;
		}

		public override string SubtypeName(byte subtype)
		{
			return (subtype & 0x1F) + " logs";
		}

		public override BitmapBits Image()
		{
			return img.Image;
		}

		public override BitmapBits Image(byte subtype)
		{
			return img.Image;
		}

		public override Sprite GetSprite(ObjectEntry obj)
		{
			int st = -(((obj.SubType & 0x1F) * img.Width) / 2) + img.X;
			List<Sprite> sprs = new List<Sprite>();
			for (int i = 0; i < (obj.SubType & 0x1F); i++)
				sprs.Add(new Sprite(img.Image, new Point(st + (i * img.Width), img.Y)));
			Sprite spr = new Sprite(sprs.ToArray());
			spr.Offset = new Point(spr.X + obj.X, spr.Y + obj.Y);
			return spr;
		}

		public override Rectangle Bounds(ObjectEntry obj, Point camera)
		{
			int w = (obj.SubType & 0x1F) * img.Width;
			return new Rectangle((obj.X - (w / 2) + img.X) - camera.X, (obj.Y + img.Y) - camera.Y, w, img.Height);
		}
	}
}
