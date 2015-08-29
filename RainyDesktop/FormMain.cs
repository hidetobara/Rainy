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
		const string LearningFilename = "Learning.bin";
		private LearningManager _Learning;

		public FormMain()
		{
			InitializeComponent();
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
			}
			if(sender == ButtonRunPredict)
			{
				task.Type = Task.TaskType.Predict;
				string target = TextBoxCurrentRain.Text;
				List<string> files = GetRainFiles();
				int index = files.IndexOf(target);
				if (index < 0) return;
				for (int i = index - 2; i <= index; i++)
				{
					if (i < 0) return;
					task.RainFiles.Add(files[i]);
				}
 			}
			Properties.Settings.Default.Save();
			if (task.Type == Task.TaskType.None) return;

			EnableButtons(false);
			BackgroundWorkerRain.RunWorkerAsync(task);
		}

		private void EnableButtons(bool enable)
		{
			ButtonRunLearning.Enabled = enable;
			ButtonRunPredict.Enabled = enable;
		}

		class Task
		{
			public enum TaskType { None, Learning, Predict }
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
				string path = Path.Combine(task.NeuralNetwork, LearningFilename);
				if (_Learning == null)
				{
					_Learning = new LearningManager();
					if (!_Learning.Load(path)) _Learning.Initialize();
				}

				if (task.Type == Task.TaskType.Learning)
				{
					const int Cut = 120;
					for (int i = 0; i < task.RainFiles.Count; i += Cut)
					{
						_Learning.Learn(GetRainImages(task.RainFiles, i, Cut).ToArray());
						Log.Instance.Info("Progress: " + i + " / " + task.RainFiles.Count);
						BackgroundWorkerRain.ReportProgress(i * 100 / task.RainFiles.Count);
					}
					_Learning.Save(path);
				}
				if(task.Type == Task.TaskType.Predict)
				{
					RainImage predicted = _Learning.Forecast(GetRainImages(task.RainFiles, 0, 3).ToArray());
					string filename = Path.GetFileNameWithoutExtension(task.RainFiles.Last());
					string saved = Path.Combine(task.NeuralNetwork, "forecast/" + filename + ".png");
					predicted.SavePngDetail(saved);
					task.OutputFiles.Add(saved);
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
			if(task.Type == Task.TaskType.Predict)
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

		private List<RainImage> GetRainImages(List<string> files, int from, int count)
		{
			List<RainImage> images = new List<RainImage>();
			for (int i = from; i < from + count; i++)
			{
				if (i >= files.Count) break;
				images.Add(RainImage.FromFile(files[i]).Shrink());
			}
			return images;
		}
	}
}
