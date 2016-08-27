using System;
using System.Collections.Generic;
using Rucker.Data;

namespace Rucker.Sql
{
    public interface IMerge
    {
        string SourceSchema { get; }
        string SourceTable { get; }

        string DestSchema { get; }
        string DestTable { get; }
        
        IEnumerable<string> MergeColumns { get; }
        MergeAction MergeAction { get; }
        Action<IRow> MatchAction { get; } 
    }
}