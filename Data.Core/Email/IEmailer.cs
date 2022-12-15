namespace Data.Core
{
    public interface IEmailer
    {
        void Send(string from, string to, string subject, string body);
    }
}