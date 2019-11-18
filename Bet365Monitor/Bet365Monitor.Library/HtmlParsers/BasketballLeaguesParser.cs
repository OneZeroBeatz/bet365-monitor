using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bet365Monitor.Library.HtmlParsers
{
	public class BasketballLeaguesParser
	{
		private readonly HtmlDocument _document;
		private readonly string _leagueNameDivClass;

		public BasketballLeaguesParser(string leagueNameDivClass)
		{
			_leagueNameDivClass = leagueNameDivClass;
			_document = new HtmlDocument();
		}

		public List<string> Parse(string text)
		{
			_document.LoadHtml(text);

			if (_document.ParseErrors != null && _document.ParseErrors.Count() > 0)
				throw new Exception("Basketball leagues parser error.");
			
			if(_document.DocumentNode == null)
				throw new Exception("Basketball leagues document node is not parsed proparely.");

			var bodyElement = _document.DocumentNode.ChildNodes.First().ChildNodes[2];

			if (!bodyElement.InnerHtml.Contains(_leagueNameDivClass))
				return new List<string>();

			var leagueNames = ParseNodes(bodyElement.ChildNodes.ToList());
			
			var leaguesFromDocument = leagueNames.Select(x => x.InnerText.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
			return leaguesFromDocument;
		}

		private List<HtmlNode> ParseNodes(List<HtmlNode> parentNodes)
		{
			var childNodes = parentNodes.SelectMany(x => x.ChildNodes).Where(x => x.InnerHtml.Contains(_leagueNameDivClass)).ToList();
			if (!childNodes.Any())
				return parentNodes;

			return ParseNodes(childNodes);
		}
	}
}
