using System;

namespace Rucker.Data
{
    public class UsernameUri: BaseUri
    {
        public static UsernameUri From(string username)
        {
            return new UsernameUri($"username://{username}");
        }

        public string Username { get; }

        public UsernameUri(string uriString) : base(uriString)
        {
            if (Scheme.ToLower() != "username")
            {
                throw new UriFormatException("A username URI must have a 'username://' scheme");
            }

            if (Parts.Length != 1)
            {
                throw new UriFormatException("The proper format for a username URI is username://<username>");
            }

            Username = Parts[0];
        }
    }
}