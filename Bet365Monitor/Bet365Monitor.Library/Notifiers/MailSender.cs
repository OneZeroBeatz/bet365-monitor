using Bet365Monitor.Library.Loggers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;

namespace Bet365Monitor.Library.Notifiers
{
	public class MailSender
	{
		private int _retryTimeout;
		private int _retryCount;
		public List<string> ToEmailAddresses { get; set; }

		public MailSender(int retryTimeout, int retryCount)
		{
			_retryTimeout = retryTimeout;
			_retryCount = retryCount;
		}

		public void SendEmail(List<string> leagueNames)
		{
			SmtpClient smtpClient = GenerateClient();
			MailMessage mail = GenerateMail(leagueNames);

			Bet365Logger.Log($"Sending email about: '{string.Join("\n", leagueNames)}' league(s)...");

			var retryCount = _retryCount;
			while (true)
			{
				try
				{
					smtpClient.Send(mail);
					break;
				}
				catch (Exception ex)
				{
					Bet365Logger.Log($"Exception occurred on email sending. Reason: {ex.Message} Retry will be processed. {retryCount} retries remaining.");

					if (retryCount == 0)
						throw;

					Thread.Sleep(_retryTimeout);
					retryCount--;
				}
			}
			Bet365Logger.Log($"Email is successfully sent.");
		}

		private SmtpClient GenerateClient()
		{
			SmtpClient smtpClient = new SmtpClient("smtp.gmail.com");
			smtpClient.Port = 587;
			smtpClient.Credentials = new NetworkCredential("jovailickn@gmail.com", "pavlaka94");
			smtpClient.EnableSsl = true;
			smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
			return smtpClient;
		}

		private MailMessage GenerateMail(List<string> leagueNames)
		{
			MailMessage mail = new MailMessage();
			mail.Priority = MailPriority.High;
			mail.From = new MailAddress("jovailickn@gmail.com");
			ToEmailAddresses.ForEach(address => mail.To.Add(address));
			mail.Subject = "New leagues available at bet365";
			mail.Body = GetMailBody(leagueNames);
			return mail;
		}
		
		private string GetMailBody(List<string> leagueNames)
		{
			var mailBodyBuilder = new StringBuilder();
			mailBodyBuilder.AppendLine("Hello,");
			mailBodyBuilder.AppendLine();
			mailBodyBuilder.AppendLine($"Following basketball league(s) appeard on bet365 site: '{string.Join(",", leagueNames)}'.");
			mailBodyBuilder.AppendLine();
			mailBodyBuilder.AppendLine("Best regards!");
			mailBodyBuilder.AppendLine("JovaIlicKn");
			return mailBodyBuilder.ToString();
		}
	}
}
