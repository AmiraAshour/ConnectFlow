using ContactsManager.Core.ServicesContracts;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ContactsManager.Core.DTO;
using Microsoft.Extensions.Options;

namespace ContactsManager.Core.Services
{
  public class EmailSender: IEmailSender
  {
    private readonly SmtpSettings _smtpSettings;

    public EmailSender(IOptions<SmtpSettings> smtpSettings)
    {
      _smtpSettings = smtpSettings.Value;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
      var smtpClient = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
      {
        EnableSsl = true,
        UseDefaultCredentials = false,
        Credentials = new NetworkCredential(_smtpSettings.Email, _smtpSettings.Password)
      };

      var message = new MailMessage(_smtpSettings.Email!, toEmail)
      {
        Subject = subject,
        Body = body,
        IsBodyHtml = true,
        BodyEncoding = System.Text.Encoding.UTF8,
        SubjectEncoding = System.Text.Encoding.Default,
      };

      message.ReplyToList.Add(_smtpSettings.Email!);

      await smtpClient.SendMailAsync(message);

    }
  }
}
