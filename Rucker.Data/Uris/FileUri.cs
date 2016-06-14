using System;
using System.Linq;
using Rucker.Extensions;

namespace Rucker.Data
{
    public class FileUri: BaseUri
    {
        public DirectoryUri DirectoryUri { get; } 

        /// <summary>
        /// Just the name of the file. Inclues the file extension.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// The full path to the file. Includes the FileName
        /// </summary>
        public string FilePath { get; }

        public FileUri(string uriString): base(uriString)
        {
            if (Scheme.ToLower() != "file")
            {
                throw new UriFormatException("A file URI must have a 'file://' scheme.");
            }

            DirectoryUri = new DirectoryUri($"directory://{Parts.Take(Parts.Length - 1).Cat("/")}");

            FileName = Parts.Last();
            FilePath = $@"{DirectoryUri.DirectoryPath}\{FileName}";
        }
    }
}