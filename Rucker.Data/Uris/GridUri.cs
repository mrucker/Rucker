using System;
using Rucker.Convert;

namespace Rucker.Data
{
    public class GridUri: BaseUri
    {
        public static GridUri From(string uriString)
        {
            return new GridUri(uriString);
        }

        public static GridUri From(string gridPreferenceId, int auditId)
        {
            return new GridUri($"grid://{gridPreferenceId}/{auditId}");
        }

        public string GridPreferenceId { get; }

        public int AuditId { get; }

        public GridUri(string uriString) : base(uriString)
        {
            if (Scheme.ToLower() != "grid")
            {
                throw new UriFormatException("A grid URI must have a 'grid' scheme");
            }

            if (Parts.Length != 2)
            {
                throw new UriFormatException("The proper format for a grid URI is grid://<GridPreferenceId>//<AuditId>");
            }

            GridPreferenceId = Parts[0];
            AuditId          = Parts[1].To<int>();
        }        
    }
}