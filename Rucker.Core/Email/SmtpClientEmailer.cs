using System.Net.Mail;

namespace Rucker.Email
{
    public class SmtpClientEmailer: IEmailer
    {
        public void Send(string from, string to, string subject, string body)
        {
            using (var smtpClient = new SmtpClient())
            {
                smtpClient.Send(from, to.Replace(';', ','), subject, body);
            }
        }
    }
}