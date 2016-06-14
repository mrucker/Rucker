using System.Security;
using Rucker.Email;


namespace Rucker.Security
{
    public static class Confirm
    {
        public class Confirmation
        {
            public bool Success { get; set; }
        }

        public static Confirmation That(bool isTrue)
        {
            return new Confirmation {Success = isTrue};
        }

        public static Confirmation Or(this Confirmation confirmation, bool isTrue)
        {
            return new Confirmation { Success = confirmation.Success || isTrue };
        }

        public static void ElseThrow(this Confirmation confirmation, string message)
        {
            if(!confirmation.Success) throw new SecurityException(message);
        }

        public static Confirmation ElseEmail(this Confirmation confirmation, string application, string environment, string from, string to, string body, IEmailer emailer)
        {
            if (!confirmation.Success)
            {
                emailer.Send(from, to, $"{application} Error *{environment}*", body);
            }

            return confirmation;
        }

        public static Confirmation ElseLogToDatabase(this Confirmation confirmation, string message)
        {
            return confirmation;
        }
    }
}