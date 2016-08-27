using System;

namespace Rucker.Core
{
    public class EmailUri: BaseUri
    {
        public static EmailUri From(string email)
        {
            return new EmailUri($"email://{email}");
        }

        public string Email { get; }

        public string Domain { get; set; }

        public EmailUri(string uriString) : base(uriString)
        {
            if (Scheme.ToLower() != "email")
            {
                throw new UriFormatException("An email URI must have an 'email://' scheme");
            }

            if (Parts.Length != 1)
            {
                throw new UriFormatException("The proper format for an email URI is email://[EmailAddress]");
            }

            Email  = Parts[0];
            Domain = Parts[0].Split('@')[1];
        }
    }
}