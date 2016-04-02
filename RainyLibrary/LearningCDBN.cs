using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Accord.Neuro;
using Accord.Neuro.Learning;
using AForge.Neuro.Learning;
using Accord.Neuro.Networks;


namespace RainyLibrary
{
	public class LearningCDBN : LearningManager
	{
		const double IgnoreRate = 0.01;
		const int AREA_SIZE = 16;
		const int AREA_STEP = 4;
		int AREA_LENGTH { get { return AREA_SIZE * AREA_SIZE; } }
		const int MiddleCount = 32;

		protected DeepBeliefNetwork _Network;
		protected BackPropagationLearning _Teacher;

		protected override string Filename { get { return "CDBN.bin"; } }

		public override bool IsInitialized(string folder)
		{
			return File.Exists(Path.Combine(folder, Filename));
		}

		public override void Initialize()
		{
			_Network = new DeepBeliefNetwork(AREA_LENGTH * HistoryLimit, new int[] { MiddleCount, AREA_LENGTH });
			new GaussianWeights(_Network).Randomize();
			_Network.UpdateVisibleWeights();
			_Teacher = new BackPropagationLearning(_Network);
		}
		public override bool Load(string folder)
		{
			string path = Path.Combine(folder, Filename);
			if (!File.Exists(path)) return false;
			_Network = DeepBeliefNetwork.Load(path);
			_Teacher = new BackPropagationLearning(_Network);
			return true;
		}
		public override void Save(string folder)
		{
			string path = Path.Combine(folder, Filename);
			string dir = System.IO.Path.GetDirectoryName(path);
			if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);
			_Network.Save(path);
		}

		public override void Learn(List<LearningImage> images, int iterate = 1)
		{
			List<double[]> inputs = new List<double[]>();
			List<double[]> outputs = new List<double[]>();
			for (int i = 0; i < images.Count - HistoryLimit; i++)
			{
				double amount = 0;
				for (int h = 0; h < HistoryLimit; h++) amount += images[i + h].GetAmount();
				if (amount < images[0].Area * IgnoreRate) continue;	// 何も降ってない時は無視

				for (int y = 0; y < Height; y += AREA_STEP)
				{
					for (int x = 0; x < Width; x += AREA_STEP)
					{
						double[] input = new double[AREA_LENGTH * HistoryLimit];
						for (int h = 0; h < HistoryLimit; h++)
						{
							LearningImage trimed = images[i + h].Trim(x, y, AREA_SIZE, AREA_SIZE);
							Array.Copy(trimed.Data, 0, input, AREA_LENGTH * h, AREA_LENGTH);
						}
						inputs.Add(input);
						outputs.Add(images[i + HistoryLimit].Trim(x, y, AREA_SIZE, AREA_SIZE).Data);
					}
				}
			}

			for (int i = 0; i < iterate; i++) _Teacher.RunEpoch(inputs.ToArray(), outputs.ToArray());
			_Network.UpdateVisibleWeights();
		}

		public override LearningImage Forecast(List<LearningImage> images)
		{
			if (images.Count < HistoryLimit) return null;

			RainImage output = new RainImage(Height, Width);
			for (int y = 0; y < Height; y += AREA_STEP)
			{
				for (int x = 0; x < Width; x += AREA_STEP)
				{
					double[] input = new double[AREA_LENGTH * HistoryLimit];
					for (int h = 0; h < HistoryLimit; h++)
					{
						LearningImage trimed = images[h].Trim(x, y, AREA_SIZE, AREA_SIZE);
						Array.Copy(trimed.Data, 0, input, AREA_LENGTH * h, AREA_LENGTH);
					}
					double[] data = _Network.Compute(input);
					RainImage part = new RainImage(AREA_SIZE, AREA_SIZE, data);
					output.Paste(x, y, part);
				}
			}

			return output;
		}

	}
}
