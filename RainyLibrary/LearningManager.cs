using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RainyLibrary
{
	public class LearningManager
	{
		private static LearningManager _Instance;
		public static LearningManager Instance
		{
			get { if (_Instance == null) _Instance = new LearningManager(); return _Instance; }
			set { if (_Instance == null) _Instance = value; }
		}

		public const int Width = 48;	// 770/16
		public const int Height = 30;	// 480/16
		public int Plane = 1;
		public int Area { get { return Width * Height; } }
		public int Length { get { return Width * Height * Plane; } }
		public int HistoryLimit = 3;

		protected virtual string Filename { get { return "Learning.bin"; } }
		public virtual bool IsInitialized(string folder) { return true; }
		public virtual void Initialize() { }
		public virtual bool Load(string folder) { return false; }
		public virtual void Save(string folder) { }
		public virtual void Learn(List<LearningImage> images, int iterate = 1) { }
		public virtual LearningImage Forecast(List<LearningImage> images) { return null; }
	}
}
