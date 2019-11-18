using System;
using System.IO;
using System.Reflection;

namespace Bet365Monitor.Library.Loggers
{
	public static class Bet365Logger
	{
		private static string LogFileName = $"Bet365MonitorLog_{DateTime.Now.Ticks}.log";
		private static string LogFilePath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/Logs/";

		public static void Log(string message)
		{
			Directory.CreateDirectory(LogFilePath);
			using (StreamWriter streamWriter = new StreamWriter($"{LogFilePath}{LogFileName}", true))
			{
				string fullLog = $"{DateTime.Now.ToString("dddd, dd-MMMM-yyyy HH:mm:ss.")} - {message}";
				Console.WriteLine(fullLog);
				streamWriter.WriteLine(fullLog);
				streamWriter.Close();
			}
		}
	}
}
