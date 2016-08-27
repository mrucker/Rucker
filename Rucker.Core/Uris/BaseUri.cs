using System;
using System.Linq;
using System.Collections.Generic;

namespace Rucker.Core
{
    public class BaseUri
    {
        #region Properties
        public string Scheme { get; }
        public string[] Parts { get; }
        public IDictionary<string,string> Query { get; }
        #endregion

        #region Constructor
        public BaseUri(string uriString)
        {
            Scheme = uriString.Split(new[] {"://"}, StringSplitOptions.RemoveEmptyEntries)[0].ToLower();
            Parts  = uriString.Split(new[] {"://"}, StringSplitOptions.RemoveEmptyEntries)[1].Split(new [] {"?"}, StringSplitOptions.RemoveEmptyEntries)[0].Split(new[]{"/"}, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
            Query  = uriString.Missing("?") ? new Dictionary<string, string>() : uriString.Split(new[] {"?"}, StringSplitOptions.RemoveEmptyEntries)[1].Split(new [] {"&"}, StringSplitOptions.RemoveEmptyEntries).Select(kv => new {Key = kv.Split('=')[0], Value = kv.Split('=')[1]}).ToDictionary(kv => kv.Key, kv => kv.Value);
        }
        #endregion

        #region Public Methods
        public sealed override string ToString()
        {
            return $"{Scheme}://{Parts.Cat("/")}";
        }

        public sealed override bool Equals(object value)
        {
            return Equals(value as BaseUri);
        }

        public bool Equals(BaseUri uri)
        {            
            return !ReferenceEquals(uri, null) && GetType() == uri.GetType() && Scheme == uri.Scheme && Parts.SequenceEqual(uri.Parts);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
        #endregion
    }
}