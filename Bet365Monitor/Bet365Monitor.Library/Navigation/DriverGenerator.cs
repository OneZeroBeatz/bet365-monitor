using Bet365Monitor.Library.Loggers;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Bet365Monitor.Library.Navigation
{
	public class DriverGenerator
	{
		private readonly string _pageUrl;
		private readonly string _sportDivClassName;
		private readonly string _leagueNamesDivClassName;
		private readonly string _choosenSport;
		private readonly string _choosenLanguage;

		public DriverGenerator(string pageUrl, 
							   string sportDivClassName, 
							   string choosenSport, 
							   string choosenLanguage,
							   string leagueNamesDivClassName)
		{
			_pageUrl = pageUrl;
			_sportDivClassName = sportDivClassName;
			_choosenSport = choosenSport;
			_choosenLanguage = choosenLanguage;
			_leagueNamesDivClassName = leagueNamesDivClassName;
		}

		public ChromeDriver GetNavigatedDriver()
		{
			var chromeDriverService = ChromeDriverService.CreateDefaultService(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
			chromeDriverService.HideCommandPromptWindow = true;
			ChromeOptions options = new ChromeOptions();
			var driver = new ChromeDriver(chromeDriverService, options);
			driver.Navigate().GoToUrl(_pageUrl);
			driver.Navigate().GoToUrl(_pageUrl);


			//var languageList = driver.FindElementByClassName("lpnm");
			//languageList.FindElements(By.TagName("li")).First(language => language.Text.Contains(_choosenLanguage)).Click();
			//driver.FindElementById("dv1").Click();

			var sportsElements = WaitUntilElementsShowsUp(driver, _sportDivClassName);
			var choosenSportElement = sportsElements.FirstOrDefault(sport => sport.Text.Equals(_choosenSport, StringComparison.OrdinalIgnoreCase));

			if (choosenSportElement == null)
				throw new Exception($"Sport name {_choosenSport} does not exists on Bet365 page.");

			choosenSportElement.Click();
			WaitUntilElementsShowsUp(driver, _leagueNamesDivClassName);
			
			return driver;
		}
		
		private List<IWebElement> WaitUntilElementsShowsUp(ChromeDriver driver, string elementClassName)
		{
			var numberOfRetries = 20;
			Bet365Logger.Log($"Searching for element with name '{elementClassName}'.");

			for (var i = 0; i < numberOfRetries; i++)
			{
				var elements = driver.FindElementsByClassName(elementClassName);
				if (elements.Any())
				{
					Bet365Logger.Log($"Searching for element(s) with name '{elementClassName}' finished after {i} retries.");
					return elements.ToList();
				}

				Thread.Sleep(TimeSpan.FromSeconds(1));
			}

			throw new NoSuchElementException($"Cannot find element with class name {elementClassName} after {numberOfRetries} retries.");
		}

	}
}
