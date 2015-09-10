using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Accord.Math;

namespace RainyLibrary
{
    public class LearningImage
    {
		public int Height;
		public int Width;
		public virtual int Plane { get { return 1; } }
		public int Area { get { return Width * Height; } }
		public int Length { get { return Width * Height * Plane; } }
		public double[] Data;

		public LearningImage(int height, int width, double[] data = null)
		{
			Height = height;
			Width = width;
			Data = new double[Length];
			if (data != null) Array.Copy(data, Data, Math.Min(data.Length, Length));
		}
		public LearningImage(LearningImage image) : this(image.Height, image.Width, image.Data) { }

		public unsafe static LearningImage LoadPng(string path)
		{
			if (!File.Exists(path)) return null;
			try
			{
				Bitmap src = new Bitmap(path);
				BitmapData srcData = src.LockBits(new Rectangle(Point.Empty, src.Size), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

				LearningImage i = new LearningImage(src.Height, src.Width);
				for (int h = 0; h < srcData.Height; h++)
				{
					byte* ps = (byte*)srcData.Scan0 + srcData.Stride * h;
					for (int w = 0; w < srcData.Width; w++, ps += i.Plane)
					{
						int postion = (i.Width * h + w) * i.Plane;
						for (int p = 0; p < i.Plane; p++) i.Data[postion + p] = (double)ps[p] / 255;
					}
				}
				src.UnlockBits(srcData);
				return i;
			}
			catch(Exception ex)
			{
				Log.Instance.Error(ex.Message + "@" + ex.StackTrace);
				return null;
			}
		}

		public LearningImage Shrink(int scale = 2)
		{
			if (scale <= 1) return this;

			LearningImage i = new LearningImage(this.Height / scale, this.Width / scale);
			for (int h = 0; h < i.Height; h++)
			{
				int hs = h * scale;
				for (int w = 0; w < i.Width; w++)
				{
					int ws = w * scale;
					List<int> list = new List<int>();
					for (int hh = hs; hh < hs + scale; hh++)
						for (int ww = ws; ww < ws + scale; ww++) list.Add(this.Width * hh + ww);
					int postion = (i.Width * h + w) * i.Plane;
					for (int p = 0; p < i.Plane; p++)
						i.Data[postion + p] = Average(list, i.Plane, p);
				}
			}
			return i;
		}
		protected double Average(List<int> list, int scale = 1, int bias = 0)
		{
			double amount = 0;
			foreach (int i in list) amount += Data[i * scale + bias];
			return amount / list.Count;
		}
		protected double Max(List<int> list, int scale = 1, int bias = 0)
		{
			double max = 0;
			foreach (int i in list) if (Data[i] > max) max = Data[i * scale + bias];
			return max;
		}

		unsafe public virtual void SavePng(string path, double low = 0, double high = 1)
		{
			PixelFormat format = PixelFormat.Format8bppIndexed;
			if (Plane == 3) format = PixelFormat.Format24bppRgb;

			Bitmap b = new Bitmap(this.Width, this.Height, format);
			BitmapData d = b.LockBits(new Rectangle(Point.Empty, b.Size), ImageLockMode.WriteOnly, format);
			for (int h = 0; h < d.Height; h++)
			{
				byte* p = (byte*)d.Scan0 + d.Stride * h;
				for (int w = 0; w < d.Width; w++, p += Plane)
				{
					int position = (this.Width * h + w) * Plane;
					for (int pp = 0; pp < Plane; pp++) p[pp] = Step(Data[position + pp], low, high);
				}
			}
			b.UnlockBits(d);
			string dir = Path.GetDirectoryName(path);
			if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
			b.Save(path, ImageFormat.Png);
		}
		protected byte Step(double v, double low = 0, double high = 1)
		{
			if (v < low) return 0;
			if (v > high) return 255;
			return (byte)((v - low) / (high - low) * 255.0);
		}

		public static void Sacle(LearningImage i, LearningImage o, double scale = 1, double bias = 0)
		{
			for (int l = 0; l < o.Length; l++) o.Data[l] = i.Data[l] * scale + bias;
		}
		public static void Add(LearningImage a, LearningImage b, LearningImage o)
		{
			for (int l = 0; l < o.Length; l++) o.Data[l] = a.Data[l] + b.Data[l];
		}
		public static void Sub(LearningImage a, LearningImage b, LearningImage o)
		{
			for (int l = 0; l < o.Length; l++) o.Data[l] = a.Data[l] - b.Data[l];
		}
		public static double DotProduct(LearningImage a, LearningImage b)
		{
			return Matrix.InnerProduct(a.Data, b.Data);
		}
		public static double EuclideanLength(LearningImage a)
		{
			return Norm.Euclidean(a.Data);
		}
		public static List<double> HighLow(LearningImage a)
		{
			return new List<double>() { a.Data.Max(), a.Data.Min() };
		}

		public void Add(LearningImage image)
		{
			Add(this, image, this);
		}
		public void Sub(LearningImage image)
		{
			Sub(this, image, this);
		}

		public double GetAmount()
		{
			var sum = Data.Sum();
			return sum;
		}

		public virtual void SaveBin(string path)
		{
			BinaryWriter writer = null;
			try
			{
				string dir = Path.GetDirectoryName(path);
				if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

				List<byte> buffer = new List<byte>();
				buffer.AddRange(BitConverter.GetBytes(Height));
				buffer.AddRange(BitConverter.GetBytes(Width));
				for (int l = 0; l < Length; l++) buffer.AddRange(BitConverter.GetBytes(Data[l]));
				writer = new BinaryWriter(File.OpenWrite(path));
				writer.Write(buffer.ToArray());
			}
			catch(Exception ex)
			{
				Log.Instance.Error(ex.Message + "@" + ex.StackTrace);
			}
			finally
			{
				if (writer != null) writer.Close();
				writer = null;
			}
		}

		public static LearningImage LoadBin(string path)
		{
			BinaryReader reader = null;
			try
			{
				if (!File.Exists(path)) return null;

				byte[] bytes = File.ReadAllBytes(path);
				int shift = 0;
				int height = BitConverter.ToInt32(bytes, shift); shift += sizeof(int);
				int width = BitConverter.ToInt32(bytes, shift); shift += sizeof(int);
				LearningImage image = new LearningImage(height, width);
				for (int l = 0; l < image.Length; l++) image.Data[l] = BitConverter.ToDouble(bytes, l * sizeof(double) + shift);
				return image;
			}
			catch(Exception ex)
			{
				Log.Instance.Error(ex.Message + "@" + ex.StackTrace);
				return null;
			}
			finally
			{
				if (reader != null) reader.Close();
				reader = null;
			}
		}
	}
}
