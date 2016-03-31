using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

using Accord.Neuro;
using Accord.Neuro.Learning;
using AForge.Neuro.Learning;
using Accord.Neuro.Networks;


namespace RainyLibrary
{
	public class LearningDBN : LearningManager
	{
		public LearningDBN() { }

		const int MiddleCount = 32;
		const double IgnoreRate = 0.01;

		protected DeepBeliefNetwork _Network;
		protected BackPropagationLearning _Teacher;

		public override string Filename { get { return "DBN.bin"; } }

		public override void Initialize()
		{
			_Network = new DeepBeliefNetwork(Width * Height * HistoryLimit, new int[] { MiddleCount, Width * Height });
			new GaussianWeights(_Network).Randomize();
			_Network.UpdateVisibleWeights();
			_Teacher = new BackPropagationLearning(_Network);
		}
		public override bool Load(string path)
		{
			if (!File.Exists(path)) return false;
			_Network = DeepBeliefNetwork.Load(path);
			_Teacher = new BackPropagationLearning(_Network);
			return true;
		}
		public override void Save(string path)
		{
			string dir = System.IO.Path.GetDirectoryName(path);
			if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);
			_Network.Save(path);
		}

		public override void Learn(List<LearningImage> images, int iterate = 1)
		{
			List<double[]> inputs = new List<double[]>();
			List<double[]> outputs = new List<double[]>();
			for(int i = 0; i < images.Count - HistoryLimit; i++)
			{
				double amount = 0;
				for (int h = 0; h < HistoryLimit; h++) amount += images[i + h].GetAmount();
				if (amount < images[0].Area * IgnoreRate) continue;	// 何も降ってない時は無視

				double[] input = new double[Area * HistoryLimit];
				for (int h = 0; h < HistoryLimit; h++) Array.Copy(images[i + h].Data, 0, input, Area * h, Area);
				inputs.Add(input);
				outputs.Add(images[i + HistoryLimit].Data);
			}
			for (int i = 0; i < iterate; i++) _Teacher.RunEpoch(inputs.ToArray(), outputs.ToArray());
			_Network.UpdateVisibleWeights();
		}

		public override LearningImage Forecast(List<LearningImage> images)
		{
			double[] input = new double[Area * HistoryLimit];
			for (int h = 0; h < HistoryLimit; h++) Array.Copy(images[h].Data, 0, input, Area * h, Area);

			double[] data = _Network.Compute(input);
			return new RainImage(Height, Width, data);
		}
	}
}