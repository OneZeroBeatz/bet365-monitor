using Bet365Monitor.Library.Loggers;
using Bet365Monitor.Library.Navigation;
using Bet365Monitor.Trigger;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Bet365Monitor.GUI.Triggers
{
	public class MonitoringTrigger
	{
		private readonly Timer _timer;
		private readonly int _retryTimeout;
		private bool _isStarted;
		private object _lockObject;
		private readonly PagePinger _pagePinger;
		private ChromeDriver _chromeDriver;
		private DriverGenerator _driverGenerator;

		public List<string> TargetLeagues { get; set; }

		public MonitoringTrigger(PagePinger pagePinger, DriverGenerator driverGenerator, int retryTimeout)
		{
			_retryTimeout = retryTimeout;
			_pagePinger = pagePinger;
			_isStarted = false;
			_lockObject = new object();
			_driverGenerator = driverGenerator;
			_timer = new Timer(OnTimerPingCallback, _timer, Timeout.Infinite, Timeout.Infinite);
		}

		private void OnTimerPingCallback(object state)
		{
			try
			{
				_pagePinger.Ping(_chromeDriver, TargetLeagues);
			}
			catch (Exception e)
			{
				var errorMessageBuilder = new StringBuilder();
				errorMessageBuilder.AppendLine($"{e.GetType().Name} occured during page pinging.");
				errorMessageBuilder.AppendLine($"Reason: {e.Message}");
				errorMessageBuilder.AppendLine($"StackTrace: {e.StackTrace}");

				if(e.InnerException!= null)
					errorMessageBuilder.AppendLine($"Additional reason: {(e.InnerException.Message)}");

				Bet365Logger.Log(errorMessageBuilder.ToString());
			}
		}

		public void TriggerProcess()
		{
			if (_isStarted)
				return;

			lock (_lockObject)
			{
				if (_isStarted)
					return;

				try
				{
					_isStarted = true;
					_chromeDriver = _driverGenerator.GetNavigatedDriver();
					_timer.Change(0, _retryTimeout);
				}
				catch (Exception e)
				{
					Bet365Logger.Log($"Exception occured during monitoring starting. Reason: {e.Message}, StackTrace: {e.StackTrace}");
				}
			}
		}
		public void Stop()
		{
			if (!_isStarted)
				return;

			lock (_lockObject)
			{
				if (!_isStarted)
					return;

				try
				{
					_isStarted = false;
					_timer.Change(Timeout.Infinite, Timeout.Infinite);
					_chromeDriver.Close();
				}
				catch (Exception e)
				{
					Bet365Logger.Log($"Exception occured during monitoring stopping. Reason: {e.Message}, StackTrace: {e.StackTrace}");
				}
				finally
				{
					if (_chromeDriver != null)
						_chromeDriver.Dispose();
					TargetLeagues = new List<string>();
					_pagePinger.ClearLeagues();
				}
			}
		}
	}
}
