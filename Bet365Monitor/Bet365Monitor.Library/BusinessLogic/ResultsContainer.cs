using System.Collections.Generic;
using System.Linq;

namespace Bet365Monitor.BusinessLogic
{
	public class AddedLeaguesProvider
	{
		private List<string> _leagueNamesInLastCheck;
		private object _lockObject;

		public AddedLeaguesProvider()
		{
			_leagueNamesInLastCheck = new List<string>();
			_lockObject = new object();
		}

		public List<string> GetAddedLeagues(List<string> newLeagueNames)
		{
			lock(_lockObject)
			{
				var addedLeagueNames = newLeagueNames.Where(newLeague => !_leagueNamesInLastCheck.Any(y => y.Equals(newLeague))).ToList();
				_leagueNamesInLastCheck = newLeagueNames;
				return addedLeagueNames;
			}
		}

		public void Clear()
		{
			lock (_lockObject)
			{
				_leagueNamesInLastCheck.Clear();
			}
		}
	}
}
