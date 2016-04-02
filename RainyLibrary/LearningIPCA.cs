using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Accord.Statistics.Analysis;
using Accord.Math;
using System.IO;

using MiniJSON;


namespace RainyLibrary
{
	using Hash = Dictionary<string, object>;

	public class LearningIPCA : LearningManager
	{
		const int MainMax = 16;
		double DynamicAmnesic
		{
			get { return 2.0 * (1 - Math.Exp(-_FrameNow / 32.0)); }	// 2.0fくらいが良い
		}
		const string FRAME_KEY = "frame";
		const string STATE_FILENAME = "state.json";

		LearningImage[] _MainImages;	// 主成分
		LearningImage[] _TmpImages;		// 副成分

		protected override string Filename { get { return "IPCA/"; } }

		long _FrameNow;

		public override bool IsInitialized(string folder)
		{
			return File.Exists(Path.Combine(folder, Filename, STATE_FILENAME));
		}

		public override void Initialize()
		{
			_FrameNow = 0;
			_MainImages = new LearningImage[MainMax];
			_TmpImages = new LearningImage[MainMax];
			for (int m = 0; m < MainMax; m++)
			{
				_MainImages[m] = new LearningImage(Height, Width);
				_TmpImages[m] = new LearningImage(Height, Width);
			}
		}

		public override bool Load(string folder)
		{
			Initialize();

			string path = Path.Combine(folder, Filename);
			if (!Directory.Exists(path)) return false;
			for (int m = 0; m < MainMax; m++)
			{
				_MainImages[m] = LearningImage.LoadBin(Path.Combine(path, "main" + m + ".bin"));
				_TmpImages[m] = LearningImage.LoadBin(Path.Combine(path, "tmp" + m + ".bin"));

				if (_MainImages[m] == null || _TmpImages[m] == null) return false;
			}

			string context = File.ReadAllText(Path.Combine(path, STATE_FILENAME));
			Hash hash = Json.Deserialize(context) as Hash;
			if(hash != null)
			{
				_FrameNow = (long)hash[FRAME_KEY];
			}
			return true;
		}

		public override void Save(string folder)
		{
			string path = Path.Combine(folder, Filename);
			for(int m = 0; m < MainMax; m++)
			{
				_MainImages[m].SaveBin(Path.Combine(path, "main" + m + ".bin"));
				_TmpImages[m].SaveBin(Path.Combine(path, "tmp" + m + ".bin"));

				List<double> list = LearningImage.HighLow(_MainImages[m]);
				_MainImages[m].SavePng(Path.Combine(path, "main" + m + ".png"), list[1], list[0]);
			}

			Hash hash = new Hash();
			hash[FRAME_KEY] = _FrameNow;
			string context = Json.Serialize(hash);
			File.WriteAllText(Path.Combine(path, STATE_FILENAME), context);
		}

		public override void Learn(List<LearningImage> images, int iterate = 1)
		{
			for(int i = 0; i < iterate; i++)
			{
				foreach (var image in images) Update(image);
			}
		}

		private void Update(LearningImage imgIn)
		{
			Array.Copy(imgIn.Data, _TmpImages[0].Data, Length);

			long iterateMax = MainMax - 1;
			if (MainMax > _FrameNow) iterateMax = _FrameNow;

			LearningImage imgA = new LearningImage(Height, Width);
			LearningImage imgB = new LearningImage(Height, Width);
			LearningImage imgC = new LearningImage(Height, Width);
			double scalerA, scalerB, scalerC;
			double nrmV;
			double l = DynamicAmnesic; //!<忘却の値、２ぐらいがよい

			for (int i = 0; i <= iterateMax; i++)
			{
				if (i == _FrameNow)
				{
					Array.Copy(_TmpImages[_FrameNow].Data, _MainImages[_FrameNow].Data, Length);
					continue;
				}

				///// Vi(n) = [a= (n-1-l)/n * Vi(n-1)] + [b= (1+l)/n * Ui(n)T Vi(n-1)/|Vi(n-1)| * Ui(n) ]
				nrmV = Norm.Euclidean(_MainImages[i].Data);

				scalerA = (double)(_FrameNow - 1 - l) / (double)_FrameNow;
				LearningImage.Sacle(_MainImages[i], imgA, scalerA);

				double dotUV = Matrix.InnerProduct(_TmpImages[i].Data, _MainImages[i].Data);
				scalerB = ((double)(1 + l) * dotUV) / ((double)_FrameNow * nrmV);
				LearningImage.Sacle(_TmpImages[i], imgB, scalerB);

				LearningImage.Add(imgA, imgB, _MainImages[i]);

				///// Ui+1(n) = Ui(n) - [c= Ui(n)T Vi(n)/|Vi(n)| * Vi(n)/|Vi(n)| ]
				if (i == iterateMax || i >= MainMax - 1) continue;

				nrmV = Norm.Euclidean(_MainImages[i].Data);
				dotUV = Matrix.InnerProduct(_TmpImages[i].Data, _MainImages[i].Data);
				scalerC = dotUV / (nrmV * nrmV);
				LearningImage.Sacle(_MainImages[i], imgC, scalerC);

				LearningImage.Sub(_TmpImages[i], imgC, _TmpImages[i + 1]);
			}
			_FrameNow++;
		}

		public override LearningImage Forecast(List<LearningImage> images)
		{
			var results = Project(images[0]);
			Log.Instance.Info("project: " + string.Join(",", results));
			return BackProject(results);
		}

		public List<double> Project(LearningImage image)
		{
			List<double> results = new List<double>();
			LearningImage amt = new LearningImage(Height, Width, image.Data);
			LearningImage tmp = new LearningImage(Height, Width);
			for (int m = 0; m < MainMax; m++)
			{
				double length = LearningImage.EuclideanLength(_MainImages[m]);
				double result = LearningImage.DotProduct(amt, _MainImages[m]) / length;
				LearningImage.Sacle(_MainImages[m], tmp, result / length);
				LearningImage.Sub(amt, tmp, amt);
				results.Add(result);
			}
			return results;
		}

		public LearningImage BackProject(List<double> list)
		{
			LearningImage image = new LearningImage(Height, Width);
			LearningImage tmp = new LearningImage(Height, Width);
			for (int m = 0; m < MainMax; m++)
			{
				double length = LearningImage.EuclideanLength(_MainImages[m]);
				LearningImage.Sacle(_MainImages[m], tmp, list[m] / length);
				LearningImage.Add(image, tmp, image);
			}
			return image;
		}
	}
}
