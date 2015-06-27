using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;


namespace Rainy.Controllers
{
	public class HomeController : Controller
	{
		public ActionResult Index()
		{
			ViewBag.Now = GetCurrentTimeAtJapan().ToString();
			return View();
		}

		public ActionResult Crawl()
		{
			string dirRaw = HttpContext.Server.MapPath("~/App_Data/raw/");
			DownloadClient client = new DownloadClient(dirRaw);
			var list = client.Start(GetCurrentTimeAtJapan() - new TimeSpan(1, 15, 0), GetCurrentTimeAtJapan());
			DownloadClient.RequestItem i = list[list.Count - 1];

			string dir = HttpContext.Server.MapPath("~/App_Data/tmp/");
			if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
			System.IO.File.Copy(i.Path, Path.Combine(dir, "last.gif"), true);

			string pathNeuro = GetNeuroPath();
			LearningManager m = new LearningManager();
			if (System.IO.File.Exists(pathNeuro)) m.Load(pathNeuro);
			else m.Initialize();
			List<RainImage> images = new List<RainImage>();
			foreach (var item in list) images.Add(RainImage.FromFile(item.Path).Quater());
			m.Learn(images.ToArray());
			m.Save(pathNeuro);

			string[] files = Directory.GetFiles(dirRaw, "*.gif", SearchOption.AllDirectories);
			ViewBag.Message = "Files=" + files.Length + Environment.NewLine;

			return View();
		}

		public ActionResult ForecastImage5(string id)
		{
			DateTime correctNow = GetCurrentTimeAtJapan();
			TryPraseTime(id, ref correctNow);
			DateTime roughNow = new DateTime(correctNow.Year, correctNow.Month, correctNow.Day, correctNow.Hour, (correctNow.Minute / 5) * 5, 0);
			DateTime future5 = roughNow + new TimeSpan(0, 5, 0);
			string dirCast = HttpContext.Server.MapPath("~/App_Data/forecast/");
			string future05name = roughNow.ToString("yyyyMM/dd/HHmm") + "-05.png";
			string path05 = Path.Combine(dirCast, future05name);
			string dir = Path.GetDirectoryName(path05);
			if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

			if (!System.IO.File.Exists(path05))
			{
				string dirRaw = HttpContext.Server.MapPath("~/App_Data/raw/");
				DownloadClient client = new DownloadClient(dirRaw);
				var requests = client.Start(correctNow - new TimeSpan(0, 10, 0), correctNow);
				List<RainImage> images = new List<RainImage>();
				foreach (var request in requests) images.Add(RainImage.FromFile(request.Path).Quater());

				string pathNeuro = GetNeuroPath();
				LearningManager m = new LearningManager();
				if (!System.IO.File.Exists(pathNeuro)) return View();
				m.Load(pathNeuro);

				RainImage forecasted5 = m.Forecast(images.ToArray());
				forecasted5.SavePng(path05);

//				string dirTmp = HttpContext.Server.MapPath("~/App_Data/tmp/");
//				string pathDetail = Path.Combine(dirTmp, "detail5.png");
//				forecasted5.SavePngDetail(pathDetail);
			}
			DumpImage(path05);
			return View();
		}

		public ActionResult PassedImage(string id)
		{
			DateTime now = GetCurrentTimeAtJapan();
			TryPraseTime(id, ref now);

			string dirRaw = HttpContext.Server.MapPath("~/App_Data/raw/");
			DownloadClient client = new DownloadClient(dirRaw);
			var item = client.Start(now);

			DumpImage(item.Path);
			return View();
		}

		public ActionResult BaseImage(string id)
		{
			string dir = HttpContext.Server.MapPath("~/App_Data/Base/");
			string path = Path.Combine(dir, id + ".png");

			DumpImage(path);
			return View();
		}

		private void DumpImage(string path)
		{
			if (System.IO.File.Exists(path))
			{
				Response.ContentType = "image/" + Path.GetExtension(path).Trim('.');
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

		private string GetNeuroPath()
		{
			string dirLearn = HttpContext.Server.MapPath("~/App_Data/learn/");
			return Path.Combine(dirLearn, "neuro.bin");
		}

		private DateTime GetCurrentTimeAtJapan(int min = 0)
		{
			return DateTime.UtcNow + new TimeSpan(9, min, 0);
		}
	}
}