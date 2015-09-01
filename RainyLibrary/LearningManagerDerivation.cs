using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RainyLibrary
{
	public class LearningManagerDerivation : LearningManager
	{
		public override string Filename { get { return "LearningDerivation.bin"; } }

		public override void Learn(RainImage[] images, int iterate)
		{
			List<double[]> inputs = new List<double[]>();
			List<double[]> outputs = new List<double[]>();
			for (int i = 0; i < images.Length - HistoryLimit; i++)
			{
				double amount = 0;
				for (int h = 0; h < HistoryLimit; h++) amount += images[i + h].GetAmount();
				if (amount < 30) continue;	// 何も降ってない時は無視

				inputs.Add(PrepareInput(images, i));
				// 差分
				outputs.Add(RainImage.Subtract(images[i + HistoryLimit], images[i + HistoryLimit - 1]).Data);
			}
			for (int i = 0; i < iterate; i++) _Teacher.RunEpoch(inputs.ToArray(), outputs.ToArray());
			_Network.UpdateVisibleWeights();
		}

		public override RainImage Forecast(RainImage[] images)
		{
			double[] data = _Network.Compute(PrepareInput(images, 0));
			RainImage o = new RainImage(Height, Width, data);
			o.Add(images[HistoryLimit - 1]);	// 直前のデータを足す
			return o;
		}

		private double[] PrepareInput(RainImage[] images, int index)
		{
			double[] input = new double[Area * HistoryLimit];
			// 差分
			for (int j = 0; j < HistoryLimit - 1; j++)
			{
				RainImage diff = RainImage.Subtract(images[index + j + 1], images[index + j]);
				Array.Copy(diff.Data, 0, input, Area * j, Area);
			}
			// 直前
			Array.Copy(images[index + HistoryLimit - 1].Data, 0, input, Area * (HistoryLimit - 1), Area);
			return input;
		}
	}
}
