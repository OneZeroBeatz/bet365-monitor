using Bet365Monitor.BusinessLogic;
using Bet365Monitor.Library.HtmlParsers;
using Bet365Monitor.Library.Loggers;
using Bet365Monitor.Library.Notifiers;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Bet365Monitor.Trigger
{
	public class PagePinger
	{
		private readonly AddedLeaguesProvider _addedLeaguesProvider;
		private readonly MailSender _mailSender;
		private readonly BasketballLeaguesParser _basketballLeaguesParser;
		private readonly string _pageUrl;

		public PagePinger(AddedLeaguesProvider addedLeaguesProvider, 
						  MailSender mailSender, 
						  BasketballLeaguesParser basketballLeaguesParser,
						  string pageUrl)
		{
			_addedLeaguesProvider = addedLeaguesProvider;
			_mailSender = mailSender;
			_basketballLeaguesParser = basketballLeaguesParser;
			_pageUrl = pageUrl;
		}

		public void Ping(ChromeDriver driver, List<string> targetLeagues)
		{
			driver.Navigate().Refresh();
			driver.Navigate().GoToUrl(driver.Url);

			var currentCheckLeagues = ParseWithRetry(driver);

			if (!currentCheckLeagues.Any())
				throw new Exception("List of leagues on page is empty.");

			var addedLeagues = _addedLeaguesProvider.GetAddedLeagues(currentCheckLeagues);

			if (!addedLeagues.Any())
			{
				Bet365Logger.Log($"There were no new leagues lately.");
				return;
			}

			Bet365Logger.Log($"Added leagues: \n{string.Join("\n", addedLeagues)}");

			var addedLeaguesThatAreTargetOnes = addedLeagues.Intersect(targetLeagues, StringComparer.OrdinalIgnoreCase).ToList();

			if (addedLeaguesThatAreTargetOnes.Any())
			{
				_mailSender.SendEmail(addedLeaguesThatAreTargetOnes);
				return;
			}

			Bet365Logger.Log($"None of added leagues are the target");

		}

		public void ClearLeagues()
		{
			_addedLeaguesProvider.Clear();
		}

		public List<string> ParseWithRetry(ChromeDriver driver)
		{
			var numberOfRetries = 8;
			for (var i = 0; i < numberOfRetries; i++)
			{
				var leagueNames = _basketballLeaguesParser.Parse(driver.PageSource);
				if (leagueNames.Any())
				{
					return leagueNames.ToList();
				}

				Thread.Sleep(TimeSpan.FromSeconds(1));
			}

			Bet365Logger.Log($"Searching for leagues finished after {numberOfRetries} retries.");

			return new List<string>();
		}
	}
}