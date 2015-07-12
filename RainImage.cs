using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using System.IO;


namespace Rainy
{
	public class RainImage
	{
		public double[] Data;
		public int Width, Height;

		public RainImage(int height, int width)
		{
			this.Height = height;
			this.Width = width;
			Data = new double[height * width];
		}
		public RainImage(int height, int width, double[] bytes)
			: this(height, width)
		{
			Array.Copy(bytes, Data, Math.Min(bytes.Length, this.Width * this.Height));
		}

		public double GetAmount()
		{
			return Data.Sum();
		}

		private static List<ColorValue> _Table = new List<ColorValue>()
		{
			new ColorValue(0, 0, 0, 0),
			new ColorValue(204, 255, 255, 0.1),
			new ColorValue(102, 153, 255, 0.2),
			new ColorValue(51, 51, 255, 0.3),
			new ColorValue(0, 255, 0, 0.4),
			new ColorValue(255, 255, 0, 0.5),
			new ColorValue(255, 153, 0, 0.6),
			new ColorValue(255, 0, 255, 0.7),
			new ColorValue(255, 0, 0, 0.8)
		};
		unsafe private static void Resign(double v, byte* p)
		{
			foreach(ColorValue cv in _Table)
			{
				if(cv.Value + 0.05 > v)
				{
					p[0] = cv.B; p[1] = cv.G; p[2] = cv.R;
					if (v < 0.05) p[3] = 0; else p[3] = 255;
					return;
				}
			}
			p[0] = 0; p[1] = 0; p[2] = 0; p[3] = 0;
		}
		unsafe private static double Parse(byte* p)
		{
			foreach (var v in _Table)
			{
				if (v.Equals(p)) return v.Value;
			}
			return 0;
		}

		unsafe public static RainImage FromFile(string path)
		{
			if (!File.Exists(path)) return null;

			Bitmap src = new Bitmap(path);
			BitmapData srcData = src.LockBits(new Rectangle(Point.Empty, src.Size), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

			RainImage i = new RainImage(src.Height, src.Width);
			for(int h = 0; h < srcData.Height; h++)
			{
				byte* ps = (byte*)srcData.Scan0 + srcData.Stride * h;
				for (int w = 0; w < srcData.Width; w++, ps += 3)
				{
					i.Data[i.Width * h + w] = Parse(ps);
				}
			}
			src.UnlockBits(srcData);
			return i;
		}

		public RainImage Half()
		{
			RainImage i = new RainImage(this.Height / 2, this.Width / 2);
			for (int h = 0; h < i.Height; h++)
			{
				int h2 = h * 2;
				for(int w = 0; w < i.Width; w++)
				{
					int w2 = w * 2;
					i.Data[i.Width * h + w] = Average(this.Width * h2 + w2, this.Width * h2 + w2 + 1, this.Width * (h2 + 1) + w2, this.Width * (h2 + 1) + w2 + 1);
				}
			}
			return i;
		}
		public RainImage Quater()
		{
			RainImage i = new RainImage(this.Height / 4, this.Width / 4);
			for (int h = 0; h < i.Height; h++)
			{
				int h4 = h * 4;
				for (int w = 0; w < i.Width; w++)
				{
					int w4 = w * 4;
					List<int> list = new List<int>();
					for (int hh = h4; hh < h4 + 3; hh++)
						for (int ww = w4; ww < w4 + 3; ww++) list.Add(this.Width * hh + ww);
					i.Data[i.Width * h + w] = Average(list.ToArray());
				}
			}
			return i;
		}
		private double Average(params int[] list)
		{
			double amount = 0;
			foreach (int i in list) amount += this.Data[i];
			return amount / list.Length;
		}

		unsafe public void SavePng(string path)
		{
			Bitmap b = new Bitmap(this.Width, this.Height, PixelFormat.Format32bppArgb);
			BitmapData d = b.LockBits(new Rectangle(Point.Empty, b.Size), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
			for (int h = 0; h < d.Height; h++)
			{
				byte* p = (byte*)d.Scan0 + d.Stride * h;
				for(int w = 0; w < d.Width; w++, p += 4)
				{
					double v = this.Data[this.Width * h + w];
					if (v < 0) v = 0;
					if (v > 1) v = 1;
					Resign(v, p);
				}
			}
			b.UnlockBits(d);
			string dir = Path.GetDirectoryName(path);
			if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
			b.Save(path, ImageFormat.Png);
		}

		unsafe public void SavePngDetail(string path)
		{
			Bitmap b = new Bitmap(this.Width, this.Height, PixelFormat.Format32bppArgb);
			BitmapData d = b.LockBits(new Rectangle(Point.Empty, b.Size), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
			for (int h = 0; h < d.Height; h++)
			{
				byte* p = (byte*)d.Scan0 + d.Stride * h;
				for (int w = 0; w < d.Width; w++, p += 4)
				{
					double v = this.Data[this.Width * h + w];
					if (v < 0) v = 0;
					if (v > 1) v = 1;
					byte vb = (byte)(255.0 * v);
					p[0] = vb; p[1] = vb; p[2] = vb;
					p[3] = (byte)((v == 0) ? 0 : 255);
				}
			}
			b.UnlockBits(d);
			string dir = Path.GetDirectoryName(path);
			if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
			b.Save(path, ImageFormat.Png);
		}

		class ColorValue
		{
			public byte R, G, B;
			public double Value;
			public ColorValue(byte r, byte g, byte b, double v) { R = r; G = g; B = b; Value = v; }
			public unsafe bool Equals(byte* rgba)
			{
				return rgba[2] == R && rgba[1] == G && rgba[0] == B;
			}
		}
	}
}