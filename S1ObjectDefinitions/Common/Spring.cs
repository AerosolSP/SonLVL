﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using SonicRetro.SonLVL;

namespace S1ObjectDefinitions.Common
{
    class Spring : ObjectDefinition
    {
        private Point offset;
        private BitmapBits img;
        private int imgw, imgh;
        private List<Point> offsets = new List<Point>();
        private List<BitmapBits> imgs = new List<BitmapBits>();
        private List<int> imgws = new List<int>();
        private List<int> imghs = new List<int>();

        public override void Init(Dictionary<string, string> data)
        {
            byte[] artfile1 = ObjectHelper.OpenArtFile("../artnem/Spring Horizontal.bin", Compression.CompressionType.Nemesis);
            byte[] artfile2 = ObjectHelper.OpenArtFile("../artnem/Spring Vertical.bin", Compression.CompressionType.Nemesis);
            string map = "../_maps/Springs.asm";
            img = ObjectHelper.MapASMToBmp(artfile1, map, 0, 0, out offset);
            imgw = img.Width;
            imgh = img.Height;
            Point off = new Point();
            BitmapBits im;
            imgs.Add(img); // 0x00
            offsets.Add(offset);
            imgws.Add(imgw);
            imghs.Add(imgh);
            im = ObjectHelper.MapASMToBmp(artfile1, map, 0, 1, out off); // 0x02
            imgs.Add(im);
            offsets.Add(off);
            imgws.Add(im.Width);
            imghs.Add(im.Height);
            im = ObjectHelper.MapASMToBmp(artfile2, map, 3, 0, out off); // 0x10
            imgs.Add(im);
            offsets.Add(off);
            imgws.Add(im.Width);
            imghs.Add(im.Height);
            im = ObjectHelper.MapASMToBmp(artfile2, map, 3, 1, out off); // 0x12
            imgs.Add(im);
            offsets.Add(off);
            imgws.Add(im.Width);
            imghs.Add(im.Height);
            imgs.Add(imgs[0]); // 0x20
            offsets.Add(offsets[0]);
            imgws.Add(imgws[0]);
            imghs.Add(imghs[0]);
            imgs.Add(imgs[1]); // 0x22
            offsets.Add(offsets[1]);
            imgws.Add(imgws[1]);
            imghs.Add(imghs[1]);
            imgs.Add(imgs[2]); // 0x30
            offsets.Add(offsets[2]);
            imgws.Add(imgws[2]);
            imghs.Add(imghs[2]);
            imgs.Add(imgs[3]); // 0x32
            offsets.Add(offsets[3]);
            imgws.Add(imgws[3]);
            imghs.Add(imghs[3]);
        }

        public override ReadOnlyCollection<byte> Subtypes()
        {
            return new ReadOnlyCollection<byte>(new byte[] { 0x00, 0x02, 0x10, 0x12, 0x20, 0x22, 0x30, 0x32 });
        }

        public override string Name()
        {
            return "Spring";
        }

        public override bool RememberState()
        {
            return false;
        }

        public override string SubtypeName(byte subtype)
        {
            string result = ((SpringColor)((subtype & 2) >> 1)).ToString();
            return result;
        }

        public override string FullName(byte subtype)
        {
            return SubtypeName(subtype) + " " + Name();
        }

        public override BitmapBits Image()
        {
            return img;
        }

        private int imgindex(byte subtype)
        {
            int result = (subtype & 2) >> 1;
            result |= (subtype & 0x30) >> 3;
            return result;
        }

        public override BitmapBits Image(byte subtype)
        {
            return imgs[imgindex(subtype)];
        }

        public override Rectangle Bounds(Point loc, byte subtype)
        {
            return new Rectangle(loc.X + offsets[imgindex(subtype)].X, loc.Y + offsets[imgindex(subtype)].Y, imgws[imgindex(subtype)], imghs[imgindex(subtype)]);
        }

        public override void Draw(BitmapBits bmp, Point loc, byte subtype, bool XFlip, bool YFlip, bool includeDebug)
        {
            BitmapBits bits = new BitmapBits(imgs[imgindex(subtype)]);
            bits.Flip(XFlip, YFlip);
            bmp.DrawBitmapComposited(bits, new Point(loc.X + offsets[imgindex(subtype)].X, loc.Y + offsets[imgindex(subtype)].Y));
        }

        public override Type ObjectType
        {
            get
            {
                return typeof(SpringS1ObjectEntry);
            }
        }
    }

    public class SpringS1ObjectEntry : S1ObjectEntry
    {
        public SpringS1ObjectEntry() : base() { }
        public SpringS1ObjectEntry(byte[] file, int address) : base(file, address) { }

        public SpringDirection Direction
        {
            get
            {
                return (SpringDirection)((SubType & 0x30) >> 4);
            }
            set
            {
                SubType = (byte)((SubType & ~0x30) | ((int)value << 4));
            }
        }

        public SpringColor Color
        {
            get
            {
                return (SpringColor)((SubType & 2) >> 1);
            }
            set
            {
                SubType = (byte)((SubType & ~2) | ((int)value << 1));
            }
        }
    }

    public enum SpringDirection
    {
        Up,
        Horizontal,
        Down,
        Horizontal2
    }

    public enum SpringColor
    {
        Red,
        Yellow
    }
}