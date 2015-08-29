namespace RainyDesktop
{
	partial class FormMain
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.label1 = new System.Windows.Forms.Label();
			this.ButtonRainImages = new System.Windows.Forms.Button();
			this.ButtonNeuralNetwork = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.TextBoxNeuralNetwork = new System.Windows.Forms.TextBox();
			this.TextBoxRainImages = new System.Windows.Forms.TextBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.ButtonRunLearning = new System.Windows.Forms.Button();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.ButtonCurrentRain = new System.Windows.Forms.Button();
			this.TextBoxCurrentRain = new System.Windows.Forms.TextBox();
			this.ButtonRunPredict = new System.Windows.Forms.Button();
			this.PictureBoxPredicted = new System.Windows.Forms.PictureBox();
			this.TextBoxLog = new System.Windows.Forms.TextBox();
			this.FolderBrowserDialogRainy = new System.Windows.Forms.FolderBrowserDialog();
			this.OpenFileDialogRain = new System.Windows.Forms.OpenFileDialog();
			this.BackgroundWorkerRain = new System.ComponentModel.BackgroundWorker();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PictureBoxPredicted)).BeginInit();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 17);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(104, 12);
			this.label1.TabIndex = 0;
			this.label1.Text = "Rain Images Folder";
			// 
			// ButtonRainImages
			// 
			this.ButtonRainImages.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonRainImages.Location = new System.Drawing.Point(377, 12);
			this.ButtonRainImages.Name = "ButtonRainImages";
			this.ButtonRainImages.Size = new System.Drawing.Size(75, 23);
			this.ButtonRainImages.TabIndex = 2;
			this.ButtonRainImages.Text = "Folder";
			this.ButtonRainImages.UseVisualStyleBackColor = true;
			this.ButtonRainImages.Click += new System.EventHandler(this.ButtonFolder_Click);
			// 
			// ButtonNeuralNetwork
			// 
			this.ButtonNeuralNetwork.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonNeuralNetwork.Location = new System.Drawing.Point(377, 41);
			this.ButtonNeuralNetwork.Name = "ButtonNeuralNetwork";
			this.ButtonNeuralNetwork.Size = new System.Drawing.Size(75, 23);
			this.ButtonNeuralNetwork.TabIndex = 5;
			this.ButtonNeuralNetwork.Text = "Folder";
			this.ButtonNeuralNetwork.UseVisualStyleBackColor = true;
			this.ButtonNeuralNetwork.Click += new System.EventHandler(this.ButtonFolder_Click);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 46);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(57, 12);
			this.label2.TabIndex = 3;
			this.label2.Text = "NN Folder";
			// 
			// TextBoxNeuralNetwork
			// 
			this.TextBoxNeuralNetwork.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TextBoxNeuralNetwork.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::RainyDesktop.Properties.Settings.Default, "NeuralNetwork", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
			this.TextBoxNeuralNetwork.Location = new System.Drawing.Point(191, 43);
			this.TextBoxNeuralNetwork.Name = "TextBoxNeuralNetwork";
			this.TextBoxNeuralNetwork.Size = new System.Drawing.Size(180, 19);
			this.TextBoxNeuralNetwork.TabIndex = 4;
			this.TextBoxNeuralNetwork.Text = global::RainyDesktop.Properties.Settings.Default.NeuralNetwork;
			// 
			// TextBoxRainImages
			// 
			this.TextBoxRainImages.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TextBoxRainImages.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::RainyDesktop.Properties.Settings.Default, "RainImages", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
			this.TextBoxRainImages.Location = new System.Drawing.Point(191, 14);
			this.TextBoxRainImages.Name = "TextBoxRainImages";
			this.TextBoxRainImages.Size = new System.Drawing.Size(180, 19);
			this.TextBoxRainImages.TabIndex = 1;
			this.TextBoxRainImages.Text = global::RainyDesktop.Properties.Settings.Default.RainImages;
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.ButtonRunLearning);
			this.groupBox1.Location = new System.Drawing.Point(12, 70);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(440, 47);
			this.groupBox1.TabIndex = 6;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Learning";
			// 
			// ButtonRunLearning
			// 
			this.ButtonRunLearning.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonRunLearning.Location = new System.Drawing.Point(359, 18);
			this.ButtonRunLearning.Name = "ButtonRunLearning";
			this.ButtonRunLearning.Size = new System.Drawing.Size(75, 23);
			this.ButtonRunLearning.TabIndex = 0;
			this.ButtonRunLearning.Text = "Run";
			this.ButtonRunLearning.UseVisualStyleBackColor = true;
			this.ButtonRunLearning.Click += new System.EventHandler(this.ButtonRun_Click);
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.ButtonCurrentRain);
			this.groupBox2.Controls.Add(this.TextBoxCurrentRain);
			this.groupBox2.Controls.Add(this.ButtonRunPredict);
			this.groupBox2.Controls.Add(this.PictureBoxPredicted);
			this.groupBox2.Location = new System.Drawing.Point(14, 135);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(438, 118);
			this.groupBox2.TabIndex = 7;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Predict";
			// 
			// ButtonCurrentRain
			// 
			this.ButtonCurrentRain.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonCurrentRain.Location = new System.Drawing.Point(192, 16);
			this.ButtonCurrentRain.Name = "ButtonCurrentRain";
			this.ButtonCurrentRain.Size = new System.Drawing.Size(75, 23);
			this.ButtonCurrentRain.TabIndex = 4;
			this.ButtonCurrentRain.Text = "File";
			this.ButtonCurrentRain.UseVisualStyleBackColor = true;
			this.ButtonCurrentRain.Click += new System.EventHandler(this.ButtonFile_Click);
			// 
			// TextBoxCurrentRain
			// 
			this.TextBoxCurrentRain.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TextBoxCurrentRain.Location = new System.Drawing.Point(6, 18);
			this.TextBoxCurrentRain.Name = "TextBoxCurrentRain";
			this.TextBoxCurrentRain.Size = new System.Drawing.Size(180, 19);
			this.TextBoxCurrentRain.TabIndex = 3;
			// 
			// ButtonRunPredict
			// 
			this.ButtonRunPredict.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonRunPredict.Location = new System.Drawing.Point(357, 18);
			this.ButtonRunPredict.Name = "ButtonRunPredict";
			this.ButtonRunPredict.Size = new System.Drawing.Size(75, 23);
			this.ButtonRunPredict.TabIndex = 1;
			this.ButtonRunPredict.Text = "Run";
			this.ButtonRunPredict.UseVisualStyleBackColor = true;
			this.ButtonRunPredict.Click += new System.EventHandler(this.ButtonRun_Click);
			// 
			// PictureBoxPredicted
			// 
			this.PictureBoxPredicted.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PictureBoxPredicted.BackColor = System.Drawing.SystemColors.ControlDark;
			this.PictureBoxPredicted.Location = new System.Drawing.Point(336, 48);
			this.PictureBoxPredicted.Name = "PictureBoxPredicted";
			this.PictureBoxPredicted.Size = new System.Drawing.Size(96, 60);
			this.PictureBoxPredicted.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.PictureBoxPredicted.TabIndex = 0;
			this.PictureBoxPredicted.TabStop = false;
			// 
			// TextBoxLog
			// 
			this.TextBoxLog.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TextBoxLog.Location = new System.Drawing.Point(12, 316);
			this.TextBoxLog.Multiline = true;
			this.TextBoxLog.Name = "TextBoxLog";
			this.TextBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.TextBoxLog.Size = new System.Drawing.Size(440, 114);
			this.TextBoxLog.TabIndex = 8;
			// 
			// OpenFileDialogRain
			// 
			this.OpenFileDialogRain.Filter = "*.*|*.*";
			// 
			// BackgroundWorkerRain
			// 
			this.BackgroundWorkerRain.WorkerReportsProgress = true;
			this.BackgroundWorkerRain.DoWork += new System.ComponentModel.DoWorkEventHandler(this.BackgroundWorkerRain_DoWork);
			this.BackgroundWorkerRain.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.BackgroundWorkerRain_ProgressChanged);
			this.BackgroundWorkerRain.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.BackgroundWorkerRain_RunWorkerCompleted);
			// 
			// FormMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(464, 442);
			this.Controls.Add(this.TextBoxLog);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.ButtonNeuralNetwork);
			this.Controls.Add(this.TextBoxNeuralNetwork);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.ButtonRainImages);
			this.Controls.Add(this.TextBoxRainImages);
			this.Controls.Add(this.label1);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormMain";
			this.Text = "Rainy";
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PictureBoxPredicted)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox TextBoxRainImages;
		private System.Windows.Forms.Button ButtonRainImages;
		private System.Windows.Forms.Button ButtonNeuralNetwork;
		private System.Windows.Forms.TextBox TextBoxNeuralNetwork;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button ButtonRunLearning;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.PictureBox PictureBoxPredicted;
		private System.Windows.Forms.Button ButtonCurrentRain;
		private System.Windows.Forms.TextBox TextBoxCurrentRain;
		private System.Windows.Forms.Button ButtonRunPredict;
		private System.Windows.Forms.TextBox TextBoxLog;
		private System.Windows.Forms.FolderBrowserDialog FolderBrowserDialogRainy;
		private System.Windows.Forms.OpenFileDialog OpenFileDialogRain;
		private System.ComponentModel.BackgroundWorker BackgroundWorkerRain;
	}
}

