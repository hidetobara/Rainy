using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

using RainyLibrary;


namespace RainyDesktop
{
	public partial class FormMain : Form
	{
		const int SCALE = 16;

		private LearningManager _Learning;

		public FormMain()
		{
			InitializeComponent();
			LearningManager.Instance = new LearningDBN();	// DBM
			//LearningManager.Instance = new LearningIPCA();	// IPCA
		}

		private void ButtonFolder_Click(object sender, EventArgs e)
		{
			if (FolderBrowserDialogRainy.ShowDialog() != DialogResult.OK) return;

			string folder = FolderBrowserDialogRainy.SelectedPath;
			if (sender == ButtonRainImages) TextBoxRainImages.Text = folder;
			if (sender == ButtonNeuralNetwork) TextBoxNeuralNetwork.Text = folder;
		}

		private void ButtonFile_Click(object sender, EventArgs e)
		{
			if (OpenFileDialogRain.ShowDialog() != DialogResult.OK) return;

			if (sender == ButtonCurrentRain) TextBoxCurrentRain.Text = OpenFileDialogRain.FileName;
		}		

		private void ButtonRun_Click(object sender, EventArgs e)
		{
			Task task = new Task() { NeuralNetwork = TextBoxNeuralNetwork.Text };
			if (sender == ButtonRunLearning)
			{
				task.Type = Task.TaskType.Learning;
				task.RainFiles = GetRainFiles();
#if DEBUG
				task.OutputFiles.AddRange(GetTargets(@"C:\obara\WebProjects\Rainy\App_Data\raw\201507\03\201507031815.gif"));
				task.OutputFiles.AddRange(GetTargets(@"C:\obara\WebProjects\Rainy\App_Data\raw\201508\02\201508022035.gif"));
				task.OutputFiles.AddRange(GetTargets(TextBoxCurrentRain.Text));
#endif
			}
			if(sender == ButtonRunPredict)
			{
				task.Type = Task.TaskType.Forecast;
				task.RainFiles = GetTargets(TextBoxCurrentRain.Text);
 			}
			Properties.Settings.Default.Save();
			if (task.Type == Task.TaskType.None) return;

			EnableButtons(false);
			BackgroundWorkerRain.RunWorkerAsync(task);
		}

		private List<string> GetTargets(string path)
		{
			List<string> targets = new List<string>();
			List<string> files = GetRainFiles();
			int index = files.IndexOf(path);
			if (index < 0) return targets;
			for (int i = index - LearningManager.Instance.HistoryLimit + 1; i <= index; i++)
			{
				if (i < 0) return targets;
				targets.Add(files[i]);
			}
			return targets;
		}

		private void EnableButtons(bool enable)
		{
			ButtonRunLearning.Enabled = enable;
			ButtonRunPredict.Enabled = enable;
		}

		class Task
		{
			public enum TaskType { None, Learning, Forecast }
			public TaskType Type = TaskType.None;
			public string NeuralNetwork;
			public List<string> RainFiles = new List<string>();
			public List<string> OutputFiles = new List<string>();
		}

		private void BackgroundWorkerRain_DoWork(object sender, DoWorkEventArgs e)
		{
			Task task = e.Argument as Task;
			if (task == null) return;

			try
			{
				string path = Path.Combine(task.NeuralNetwork, LearningManager.Instance.Filename);
				if (_Learning == null)
				{
					_Learning = LearningManager.Instance;
					if (!_Learning.Load(path)) _Learning.Initialize();
				}

				if (task.Type == Task.TaskType.Learning)
				{
					const int Cut = 18;
					const int Step = 40;
					List<RainFiles> list = new List<RainFiles>();
					for (int i = 0; i < task.RainFiles.Count; i += Cut) list.Add(new RainFiles(task.RainFiles, i, Cut));
					list = list.OrderBy(i => Guid.NewGuid()).ToList();

					List<LearningImage> images = new List<LearningImage>();
					for (int l = 0; l < list.Count; l++)
					{
						images.AddRange(GetRainImages(list[l].Files, 0, list[l].Files.Count));
						if ((l + 1) % Step != 0) continue;

						_Learning.Learn(images);
						images.Clear();
						GC.Collect();

						Log.Instance.Info("Progress: " + l + " / " + list.Count);
						BackgroundWorkerRain.ReportProgress(l * 100 / list.Count);
#if DEBUG && false
						int limit = LearningManager.Instance.HistoryLimit;
						int pair = task.OutputFiles.Count / limit;
						if (pair > 0)
						{
							for (int p = 0; p < pair; p++)
							{
								RainImage rain = _Learning.Forecast(GetRainImages(task.OutputFiles, p * limit, limit)) as RainImage;
								rain.SavePngDetail("../i" + l + "-p" + p + ".png");
							}
						}
#endif
					}
					_Learning.Save(path);
				}
				if(task.Type == Task.TaskType.Forecast)
				{
					RainImage predicted = _Learning.Forecast(GetRainImages(task.RainFiles, 0, 3)) as RainImage;
					string filename = Path.GetFileNameWithoutExtension(task.RainFiles.Last());
					string normal = Path.Combine(task.NeuralNetwork, "../forecast/" + filename + ".png");
					string detail = Path.Combine(task.NeuralNetwork, "../forecast/" + filename + ".detail.png");
					predicted.SavePng(normal, 1, 0.025);
					predicted.SavePngDetail(detail);
					task.OutputFiles.Add(detail);
					e.Result = task;
				}
			}
			catch(Exception ex)
			{
				Log.Instance.Error(ex.Message + "@" + ex.StackTrace);
				BackgroundWorkerRain.ReportProgress(100);
			}
		}

		private void BackgroundWorkerRain_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			TextBoxLog.Text = Log.Instance.Get();
		}

		private void BackgroundWorkerRain_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			EnableButtons(true);

			Task task = e.Result as Task;
			if (task == null) return;
			if(task.Type == Task.TaskType.Forecast)
			{
				string saved = task.OutputFiles.First();
				byte[] bytes = File.ReadAllBytes(saved);
				PictureBoxPredicted.Image = Bitmap.FromStream(new MemoryStream(bytes));
			}
		}

		private List<string> GetRainFiles()
		{
			List<string> files = new List<string>();
			files.AddRange(Directory.GetFiles(TextBoxRainImages.Text, "*.gif", SearchOption.AllDirectories));
			files.Sort();
			return files;
		}

		private List<LearningImage> GetRainImages(List<string> files, int from, int count)
		{
			List<LearningImage> images = new List<LearningImage>();
			for (int i = from; i < from + count; i++)
			{
				if (i >= files.Count) break;
				images.Add(RainImage.LoadGif(files[i]).Shrink(SCALE));
			}
			return images;
		}

		private class RainFiles
		{
			public List<string> Files = new List<string>();
			public int Index;

			public RainFiles(List<string> list, int from, int count)
			{
				Index = from;
				for (int i = from; i < list.Count && i < from + count; i++) Files.Add(list[i]);
			}
		}
	}
}
