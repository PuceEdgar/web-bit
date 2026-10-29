using System;
using System.Collections.Generic;
using System.Text;

namespace BitDisplayDaemo.SSNS_ENV
{
	public class TraceManager
	{
		private static string saveFolder = Directory.GetCurrentDirectory();

		public static string StartupFolder
		{
			get => saveFolder;
			set => saveFolder = value;
		}

		public static void AddLog(string log)
		{
			try
			{
				string str = Path.Combine(saveFolder, "Trace");
				if (!Directory.Exists(str))
					Directory.CreateDirectory(str);
				string path = Path.Combine(str, string.Format("{0}_except.log", (object)DateTime.Now.ToString("yyyyMMdd")));
				DateTime now = DateTime.Now;
				log = string.Format("{0:0000}-{1:00}-{2:00} {3:00}:{4:00}:{5:00} [{6:000}] ", (object)now.Year, (object)now.Month, (object)now.Day, (object)now.Hour, (object)now.Minute, (object)now.Second, (object)now.Millisecond) + log;
				StreamWriter streamWriter = File.AppendText(path);
				streamWriter.WriteLine(log, (object)Encoding.UTF8);
				streamWriter.Close();
			}
			catch (Exception ex)
			{
			}
		}

		public static void AddInfoLog(string log, string HEAD_NM = "bit", bool UseDate = true)
		{
			try
			{
				string str1 = Path.Combine(saveFolder, "LOG", "INFO");
				if (!Directory.Exists(str1))
					Directory.CreateDirectory(str1);
				string str2 = Path.Combine(str1, DateTime.Now.ToString("yyyyMMdd"));
				if (!Directory.Exists(str2))
					Directory.CreateDirectory(str2);
				string path = Path.Combine(str2, string.Format("{0}_{1:d2}.log", (object)HEAD_NM, (object)DateTime.Now.Hour));
				if (UseDate)
				{
					DateTime now = DateTime.Now;
					log = string.Format("{0:0000}-{1:00}-{2:00} {3:00}:{4:00}:{5:00}:{6:000} ", (object)now.Year, (object)now.Month, (object)now.Day, (object)now.Hour, (object)now.Minute, (object)now.Second, (object)now.Millisecond) + log;
				}
				StreamWriter streamWriter = File.AppendText(path);
				streamWriter.WriteLine(log, (object)Encoding.UTF8);
				streamWriter.Close();
			}
			catch (Exception ex)
			{
			}
		}
	}
}
