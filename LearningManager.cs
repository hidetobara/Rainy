using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Accord.Neuro;
using Accord.Neuro.Learning;
using AForge.Neuro.Learning;
using Accord.Neuro.Networks;


namespace Rainy
{
	public class LearningManager
	{
		const int Width = 192;
		const int Height = 120;
		public int Area { get { return Width * Height; } }
		public const int HistoryLimit = 3;
		const int MiddleCount = 64;

		DeepBeliefNetwork _Network;
		BackPropagationLearning _Teacher;

		public void Initialize()
		{
			_Network = new DeepBeliefNetwork(Width * Height * HistoryLimit, new int[] { MiddleCount, Width * Height });
			new GaussianWeights(_Network).Randomize();
			_Network.UpdateVisibleWeights();
			_Teacher = new BackPropagationLearning(_Network);
		}
		public void Load(string path)
		{
			_Network = DeepBeliefNetwork.Load(path);
			_Teacher = new BackPropagationLearning(_Network);
		}
		public void Save(string path)
		{
			string dir = System.IO.Path.GetDirectoryName(path);
			if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);
			_Network.Save(path);
		}

		public void Learn(RainImage[] images)
		{
			List<double[]> inputs = new List<double[]>();
			List<double[]> outputs = new List<double[]>();
			for(int i = 0; i < images.Length - HistoryLimit; i++)
			{
				double amount = 0;
				for (int h = 0; h < HistoryLimit; h++) amount += images[i + h].GetAmount();
				if (amount < 30) continue;	// 何も降ってない時は無視

				double[] input = new double[Area * HistoryLimit];
				for (int h = 0; h < HistoryLimit; h++) Array.Copy(images[i + h].Data, 0, input, Area * h, Area);
				inputs.Add(input);
				outputs.Add(images[i + HistoryLimit].Data);
			}
			_Teacher.RunEpoch(inputs.ToArray(), outputs.ToArray());
			_Network.UpdateVisibleWeights();
		}

		public RainImage Forecast(RainImage[] images)
		{
			double[] input = new double[Area * HistoryLimit];
			for (int h = 0; h < HistoryLimit; h++) Array.Copy(images[h].Data, 0, input, Area * h, Area);

			double[] data = _Network.Compute(input);
			return new RainImage(Height, Width, data);
		}
	}
}