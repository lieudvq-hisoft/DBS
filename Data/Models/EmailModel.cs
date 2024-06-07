using System;
namespace Data.Models
{
    public class EmailInfoModel
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Text { get; set; }
    }
    public class EmailSettings
    {
        public string From { get; set; }
        public string SmtpServer { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}

