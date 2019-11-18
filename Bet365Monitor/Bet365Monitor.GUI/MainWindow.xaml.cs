using Bet365Monitor.BusinessLogic;
using Bet365Monitor.GUI.Triggers;
using Bet365Monitor.Library.HtmlParsers;
using Bet365Monitor.Library.Loggers;
using Bet365Monitor.Library.Navigation;
using Bet365Monitor.Library.Notifiers;
using Bet365Monitor.Trigger;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Bet365Monitor.GUI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private readonly MonitoringTrigger _trigger;
		private readonly MailSender _mailSender;
		private readonly DriverGenerator _driverGenerator;
		private readonly PagePinger _pagePinger;
		private readonly AddedLeaguesProvider _addedLeaguesProvider;
		private readonly BasketballLeaguesParser _basketballLeaguesParser;
		private readonly Regex _validEmailRegex;
		private readonly SolidColorBrush _stoppedBrush = new SolidColorBrush(Color.FromRgb(100, 20, 20));
		private readonly SolidColorBrush _runningBrush = new SolidColorBrush(Color.FromRgb(20, 100, 20));
		private readonly List<UIElement> _disableElement = new List<UIElement>();
		
		private const string WebPageUrlDk = "https://www.bet365.dk";
		//private const string WebPageUrl = "https://www.bet365.com";
		private const string LeagueNameDivClass = "slm-MarketGroup_GroupName";
		private const string SportDivClass = "wn-Classification";
		private const string ChoosenSport = "Basketball";
		private const string ChoosenLanguage = "English";
		private const int MonitoringTriggerRetryTimeout = 10000;

		private const int EmailSendingRetryTimeout = 1000;
		private const int EmailSendingRetryCount = 10;
		private const string ValidEmailPattern = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|"
            + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)"
            + @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";
		

		public MainWindow()
		{
			Bet365Logger.Log("Bet365Monitor starting...");
			InitializeComponent();

			_mailSender = new MailSender(EmailSendingRetryTimeout, EmailSendingRetryCount);
			_driverGenerator = new DriverGenerator(WebPageUrlDk, SportDivClass, ChoosenSport, ChoosenLanguage, LeagueNameDivClass);
			_addedLeaguesProvider = new AddedLeaguesProvider();
			_basketballLeaguesParser = new BasketballLeaguesParser(LeagueNameDivClass);
			_pagePinger = new PagePinger(_addedLeaguesProvider, _mailSender, _basketballLeaguesParser, WebPageUrlDk);
			_trigger = new MonitoringTrigger(_pagePinger,_driverGenerator, MonitoringTriggerRetryTimeout);
			_validEmailRegex = new Regex(ValidEmailPattern, RegexOptions.IgnoreCase);

			_disableElement.Add(EmailListBox);
			_disableElement.Add(EmailTextBox);
			_disableElement.Add(AddEmailButton);
			_disableElement.Add(RemoveEmailButton);
			_disableElement.Add(ClearEmailsButton);

			_disableElement.Add(LeaguesListBox);
			_disableElement.Add(LeagueTextBox);
			_disableElement.Add(AddLeagueButton);
			_disableElement.Add(RemoveLeagueButton);
			_disableElement.Add(ClearLeaguesButton);

			Bet365Logger.Log("Bet365Monitor successfully started.");
		}

		private void StartButton_Click(object sender, RoutedEventArgs e)
		{
			Bet365Logger.Log("Monitoring starting...");
			if (EmailListBox.Items.Count == 0)
			{
				if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
				{
					MessageBox.Show("No emails added.\nAdd at least one email and try again!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}

				if (!_validEmailRegex.IsMatch(EmailTextBox.Text))
				{
					MessageBox.Show("No emails added.\nInvalid email!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}

				EmailListBox.Items.Add(EmailTextBox.Text.Trim());
				EmailTextBox.Text = string.Empty;
			}

			if (LeaguesListBox.Items.Count == 0)
			{
				if (string.IsNullOrWhiteSpace(LeagueTextBox.Text))
				{
					LeagueTextBox.Text = string.Empty;
					MessageBox.Show("No target leagues added.\nAdd at least one league and try again!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}

				LeaguesListBox.Items.Add(LeagueTextBox.Text.Trim());
				LeagueTextBox.Text = string.Empty;
			}

			Mouse.OverrideCursor = Cursors.Wait;
			_mailSender.ToEmailAddresses = EmailListBox.Items.Cast<string>().ToList(); ;
			_trigger.TargetLeagues = LeaguesListBox.Items.Cast<string>().ToList();
			_trigger.TriggerProcess();
			ProgramStatusLabel.Content = "Running...";
			ProgramStatusLabel.Foreground = _runningBrush;
			StartButton.IsEnabled = false;
			StopButton.IsEnabled = true;
			DisableFormElements();

			Mouse.OverrideCursor = null;
			Bet365Logger.Log("Monitoring successfully started.");
		}

		private void StopButton_Click(object sender, RoutedEventArgs e)
		{
			Mouse.OverrideCursor = Cursors.Wait;
			Bet365Logger.Log("Monitoring stopping...");
			ProgramStatusLabel.Content = "Stopping";
			_trigger.Stop();
			EnableFormElements();
			StartButton.IsEnabled = true;
			StopButton.IsEnabled = false;
			ProgramStatusLabel.Foreground = _stoppedBrush;
			ProgramStatusLabel.Content = "Stopped";
			Bet365Logger.Log("Monitoring successfully stopped.");
			Mouse.OverrideCursor = null;
		}

		private void Bet365_Monitor_Closed(object sender, System.EventArgs e)
		{
			_trigger.Stop();
		}

		#region Leagues event handlers

		private void AddLeagueButton_Click(object sender, RoutedEventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(LeagueTextBox.Text))
			{
				LeaguesListBox.Items.Add(LeagueTextBox.Text.Trim());
			}

			LeagueTextBox.Text = string.Empty;
		}

		private void RemoveLeagueButton_Click(object sender, RoutedEventArgs e)
		{
			if (LeaguesListBox.SelectedIndex != -1)
			{
				LeaguesListBox.Items.RemoveAt(LeaguesListBox.SelectedIndex);
			}
		}

		private void ClearLeaguesButton_Click(object sender, RoutedEventArgs e)
		{
			LeagueTextBox.Text = string.Empty;
			LeaguesListBox.Items.Clear();
		}

		#endregion

		#region Email event handlers

		private void AddEmailButton_Click(object sender, RoutedEventArgs e)
		{
			if (!_validEmailRegex.IsMatch(EmailTextBox.Text))
			{
				MessageBox.Show("Invalid email address!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			EmailListBox.Items.Add(EmailTextBox.Text.Trim());
			EmailTextBox.Text = string.Empty;
		}

		private void RemoveEmailButton_Click(object sender, RoutedEventArgs e)
		{
			if (EmailListBox.SelectedIndex != -1)
			{
				EmailListBox.Items.RemoveAt(EmailListBox.SelectedIndex);
			}
		}

		private void ClearEmailsButton_Click(object sender, RoutedEventArgs e)
		{
			EmailTextBox.Text = string.Empty;
			EmailListBox.Items.Clear();
		}

		#endregion

		#region Private fields
		private void DisableFormElements()
		{
			foreach (var element in _disableElement)
			{
				element.IsEnabled = false;
			}
		}
		private void EnableFormElements()
		{
			foreach (var element in _disableElement)
			{
				element.IsEnabled = true;
			}
		}

		#endregion
	}
}
