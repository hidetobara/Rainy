using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;
using System.Threading;


namespace Rainy
{
	public class DownloadClient
	{
		const string BASE_URL = "http://tokyo-ame.jwa.or.jp/mesh/000/";
		string _Dir;

		public DownloadClient(string dir)
		{
			_Dir = dir;
		}

		private string Filename(DateTime t)
		{
			return t.ToString("yyyyMMddHHmm") + ".gif";
		}
		private string LocalPath(DateTime t)
		{
			string filename = t.ToString("yyyyMMddHHmm") + ".gif";
			string mon = t.ToString("yyyyMM");
			string day = t.ToString("dd");
			return Path.Combine(_Dir, Path.Combine(Path.Combine(mon, day), filename));
		}

		public RequestItem Start(DateTime time)
		{
			int min = (time.Minute / 5) * 5;
			time = new DateTime(time.Year, time.Month, time.Day, time.Hour, min, 0);
			RequestItem i = new RequestItem() { Url = BASE_URL + Filename(time), Path = LocalPath(time) };
			Request(i);
			return i;
		}

		public List<RequestItem> Start(DateTime start, DateTime end)
		{
			int min = (start.Minute / 5) * 5;
			start = new DateTime(start.Year, start.Month, start.Day, start.Hour, min, 0);
			List<RequestItem> items = new List<RequestItem>();
			for (DateTime t = start; t <= end; t += new TimeSpan(0, 5, 0))
			{
				items.Add(new RequestItem() { Url = BASE_URL + Filename(t), Path = LocalPath(t) });
			}
			Request(items);
			return items;
		}

		public void StartAsync(DateTime start, DateTime end)
		{
			int min = (start.Minute / 5) * 5;
			start = new DateTime(start.Year, start.Month, start.Day, start.Hour, min, 0);
			List<RequestItem> items = new List<RequestItem>();
			for (DateTime t = start; t <= end; t += new TimeSpan(0, 5, 0))
			{
				items.Add(new RequestItem() { Url = BASE_URL + Filename(t), Path = LocalPath(t) });
			}
			Thread thread = new Thread(Request);
			thread.Start(items);
		}

		private void Request(Object o)
		{
			List<RequestItem> items = null;
			if (o is RequestItem)
			{
				items = new List<RequestItem>();
				items.Add(o as RequestItem);
			}
			if (o is List<RequestItem>)
			{
				items = o as List<RequestItem>;
			}
			if (items == null) return;

			foreach (var item in items)
			{
				try
				{
					if (File.Exists(item.Path)) continue;

					HttpWebRequest req = WebRequest.CreateHttp(item.Url);
					req.Timeout = 5000;
					WebResponse res = req.GetResponse();
					BinaryReader reader = new BinaryReader(res.GetResponseStream());
					List<byte> bytes = new List<byte>();
					while(true)
					{
						byte[] buffer = reader.ReadBytes(1024);
						if (buffer == null || buffer.Length == 0) break;
						bytes.AddRange(buffer);
					}
					string dir = Path.GetDirectoryName(item.Path);
					if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
					if (bytes.Count == 0) return;
					File.WriteAllBytes(item.Path, bytes.ToArray());
				}
				catch (Exception ex)
				{
					item.Error = ex.Message + "@" + ex.StackTrace;
				}
				if (item.OnComplete != null) item.OnComplete(item);
			}
		}

		public class RequestItem
		{
			public string Url;
			public string Path;
			public string Error;
			public Action<RequestItem> OnComplete;
			public string Filename { get { return System.IO.Path.GetFileName(Path); } }
		}
	}
}