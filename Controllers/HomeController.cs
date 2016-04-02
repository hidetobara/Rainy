using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using RainyLibrary;


namespace Rainy.Controllers
{
	public class HomeController : Controller
	{
		const int SCALE = 16;
		const double RAIN_BIAS = 0.0;

		// トップ画面、現在の雨
		public ActionResult Index()
		{
			ViewBag.Now = GetCurrentTimeAtJapan().ToString();
			return View();
		}

		// 5分後の雨
		public ActionResult Forecast5()
		{
			return View();
		}
		// 10分後の雨
		public ActionResult Forecast10()
		{
			return View();
		}

		// 定期的に画像をクロール＆学習
		public ActionResult Crawl()
		{
			string dirRaw = HttpContext.Server.MapPath("~/App_Data/raw/");
			DownloadClient client = new DownloadClient(dirRaw);
			DateTime now = GetCurrentTimeAtJapan();
			var list = client.Start(now - new TimeSpan(1, 15, 0), now);

			LearningManager m = GetLearningManager();
			string pathNeuro = GetNeuroPath();
			if (!m.IsInitialized(pathNeuro)) m.Initialize();	// ファイルが無い時
			List<LearningImage> images = new List<LearningImage>();
			foreach (var item in list) images.Add(RainImage.LoadGif(item.Path).Shrink(SCALE));
			m.Learn(images);
			m.Save(pathNeuro);

			string[] files = Directory.GetFiles(dirRaw, "*.gif", SearchOption.AllDirectories);
			ViewBag.Message = "Files=" + files.Length + Environment.NewLine;

			client.StartAsync(now - new TimeSpan(3, 0, 0), now - new TimeSpan(1, 0, 0));	// 念のため
			return View();
		}

		// 特定の時間を学習させる
		public ActionResult Learn(string id)
		{
			DateTime now = GetCurrentTimeAtJapan();
			TryPraseTime(id, ref now);

			string dirRaw = HttpContext.Server.MapPath("~/App_Data/raw/");
			DownloadClient client = new DownloadClient(dirRaw);
			var list = client.SearchLocal(now - new TimeSpan(0, 10, 0), now + new TimeSpan(0, 5, 0));

			LearningManager m = GetLearningManager();
			List<LearningImage> images = new List<LearningImage>();
			foreach (var item in list) images.Add(RainImage.LoadGif(item.Path).Shrink(SCALE));
			if (images.Count < 4) return View("Error");
			m.Learn(images);
			m.Save(GetNeuroPath());
			var forecasted = m.Forecast(images.Take(m.HistoryLimit).ToList());

			string path05 = GetForecast5Path(now, ".detail.png");
			if (forecasted is RainImage) (forecasted as RainImage).SavePngDetail(path05);

			DumpImage(path05);
			return View();
		}

		public ActionResult ForecastImage5(string id)
		{
			DateTime correctNow = GetCurrentTimeAtJapan();
			TryPraseTime(id, ref correctNow);

			string next05 = GetForecast5Path(correctNow);
			CheckDirectory(next05);

			if (!System.IO.File.Exists(next05)) Forecast(next05, correctNow - new TimeSpan(0, 10, 0), correctNow);
			DumpImage(next05);
			return View();
		}

		public ActionResult ForecastImage10(string id)
		{
			DateTime correctNow = GetCurrentTimeAtJapan();
			TryPraseTime(id, ref correctNow);

			string dirRaw = HttpContext.Server.MapPath("~/App_Data/raw/");
			DownloadClient client = new DownloadClient(dirRaw);
			var pre10 = client.Start(correctNow - new TimeSpan(0, 10, 0));
			var pre05 = client.Start(correctNow - new TimeSpan(0, 5, 0));
			var pre00 = client.Start(correctNow);
			string next05 = GetForecast5Path(correctNow);
			string next10 = GetForecast10Path(correctNow);
			CheckDirectory(next05);

			if (!System.IO.File.Exists(next05)) Forecast(next05, new List<string>() { pre10.Path, pre05.Path, pre00.Path });
			if (!System.IO.File.Exists(next10)) Forecast(next10, new List<string>() { pre05.Path, pre00.Path, next05 });
			DumpImage(next10);
			return View();
		}

		private RainImage Forecast(string forecasted, List<string> paths)
		{
			List<LearningImage> images = new List<LearningImage>();
			foreach(var path in paths)
			{
				LearningImage i = RainImage.LoadGif(path);
				if (i == null) continue;
				if (path.EndsWith(".gif")) i = i.Shrink(SCALE);
				images.Add(i);
			}
			LearningManager m = GetLearningManager();
			if (images.Count < m.HistoryLimit) return null;

			LearningImage image = m.Forecast(images);
			image.SavePng(forecasted, 1, RAIN_BIAS);
#if DEBUG
			if (image is RainImage) (image as RainImage).SavePngDetail(forecasted + ".detail.png");
#endif
			return image as RainImage;
		}
		private RainImage Forecast(string forecasted, DateTime start, DateTime end)
		{
			List<string> paths = new List<string>();
			string dirRaw = HttpContext.Server.MapPath("~/App_Data/raw/");
			DownloadClient client = new DownloadClient(dirRaw);
			var requests = client.Start(start, end);
			foreach (var r in requests) paths.Add(r.Path);

			return Forecast(forecasted, paths);
		}

		public ActionResult PassedImage(string id)
		{
			DateTime now = GetCurrentTimeAtJapan();
			TryPraseTime(id, ref now);

			string dirRaw = HttpContext.Server.MapPath("~/App_Data/raw/");
			DownloadClient client = new DownloadClient(dirRaw);
			var item = client.Start(now);

			DumpImage(item.Path, true);
			return View();
		}

		public ActionResult BaseImage(string id)
		{
			string dir = HttpContext.Server.MapPath("~/App_Data/Base/");
			string path = Path.Combine(dir, id + ".png");

			DumpImage(path, true);
			return View();
		}

		private void DumpImage(string path, bool cache = false)
		{
			if (System.IO.File.Exists(path))
			{
				Response.ContentType = "image/" + Path.GetExtension(path).Trim('.');
				if (cache) Response.CacheControl = "public";
				Response.Flush();
				Response.WriteFile(path);
				Response.End();
			}
			else
			{
				Response.ContentType = "text/plain";
				Response.StatusCode = 404;
				Response.Flush();
				Response.Write("NOT Found.");
				Response.End();
			}
		}

		private bool TryPraseTime(string id, ref DateTime t)
		{
			if (string.IsNullOrEmpty(id) || id.Length != 12) return false;
			t = DateTime.ParseExact(id, "yyyyMMddHHmm", null);
			return true;
		}

		private string GetForecast5Path(DateTime correct, string ext = ".png")
		{
			DateTime rough = new DateTime(correct.Year, correct.Month, correct.Day, correct.Hour, (correct.Minute / 5) * 5, 0);
			string dirCast = HttpContext.Server.MapPath("~/App_Data/forecast/");
			string future05name = rough.ToString("yyyyMM/dd/HHmm") + "-05" + ext;
			return Path.Combine(dirCast, future05name);
		}
		private string GetForecast10Path(DateTime correct, string ext = ".png")
		{
			DateTime rough = new DateTime(correct.Year, correct.Month, correct.Day, correct.Hour, (correct.Minute / 5) * 5, 0);
			string dirCast = HttpContext.Server.MapPath("~/App_Data/forecast/");
			string future05name = rough.ToString("yyyyMM/dd/HHmm") + "-10" + ext;
			return Path.Combine(dirCast, future05name);
		}

		private void CheckDirectory(string path)
		{
			string dir = Path.GetDirectoryName(path);
			if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
		}

		private string GetNeuroPath()
		{
			return HttpContext.Server.MapPath("~/App_Data/learn/");
		}

		private LearningManager _LearningManager = null;
		private LearningManager GetLearningManager()
		{
			if (_LearningManager != null) return _LearningManager;

			_LearningManager = new LearningDBN();
			_LearningManager.Load(GetNeuroPath());
			return _LearningManager;
		}

		private string GetTmpPath(string filename)
		{
			string dir = HttpContext.Server.MapPath("~/App_Data/tmp/");
			return Path.Combine(dir, filename);
		}

		private DateTime GetCurrentTimeAtJapan()
		{
#if DEBUG
			return new DateTime(2016, 3, 24, 9, 0, 0);
#else
			return DateTime.UtcNow + new TimeSpan(8, 59, 0);
#endif
		}
	}
}