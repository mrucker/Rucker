using System;
using System.Linq;

namespace Data.Core
{
    public class DirectoryUri: BaseUri
    {
        public string DirectoryName { get; }
        public string DirectoryPath { get; }

        public DirectoryUri(string uriString) : base(uriString)
        {            
            if (Scheme != "directory")
            {
                throw new UriFormatException("A directory URI must have a 'directory://' scheme.");
            }

            var isLocal       = Parts[0].Contains(":");
            var directoryName = Parts.Skip(1).LastOrDefault() ?? "";
            var directoryPath = Parts.Cat(@"\");

            DirectoryName = directoryName;
            DirectoryPath = isLocal ? directoryPath : $@"\\{directoryPath}";
        }
    }
}