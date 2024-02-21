using Data.DataAccess;
using Data.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Services.Core
{
    public interface IMailService
    {
        Task<bool> SendEmail(EmailInfoModel email);
    }

    public class MailService : IMailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        public MailService(IOptions<EmailSettings> emailSettings, AppDbContext appDbContext, IConfiguration configuration)
        {
            _emailSettings = emailSettings.Value;
            _context = appDbContext;
            _configuration = configuration;
        }

        public async Task<bool> SendEmail(EmailInfoModel email)
        {

            var Mimeemail = new MimeMessage();
            Mimeemail.From.Add(MailboxAddress.Parse(_emailSettings.From));
            Mimeemail.To.Add(MailboxAddress.Parse(email.To));
            Mimeemail.Subject = email.Subject;
            var builder = new BodyBuilder();
            builder.HtmlBody = string.Format(email.Text);
            Mimeemail.Body = builder.ToMessageBody();
            bool result = false;

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                try
                {
                    await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port);
                    await client.AuthenticateAsync(_emailSettings.UserName, _emailSettings.Password);
                    await client.SendAsync(Mimeemail);
                    result = true;
                }
                catch
                {
                    result = false;
                }
                finally
                {
                    await client.DisconnectAsync(true);
                    client.Dispose();
                }
            };
            return result;
        }
    }
}

