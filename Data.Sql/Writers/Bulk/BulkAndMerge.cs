using System;
using System.Collections.Generic;
using System.Linq;
using Data.Core;

namespace Data.Sql
{
    public class BulkAndMerge: IMerge, IBulk
    {
        #region Public Properties
        public string StageSchema { get; }
        public string DestSchema { get; }

        public string StageTable { get; }
        public string DestTable { get; }

        public IEnumerable<ColumnMap> ColumnMaps { get; }
        public TableDataReader DataReader { get; set; }
        
        public IEnumerable<string> MergeColumns { get; }
        public MergeAction MergeAction { get; }
        public Action<IRow> MatchAction { get; }
        #endregion

        #region Constructors
        public BulkAndMerge(BulkAndMerge bm, MergeAction? mergeAction = null)
        {
            StageSchema = bm.StageSchema;
            DestSchema  = bm.DestSchema;

            StageTable = bm.StageTable;
            DestTable  = bm.DestTable;

            MergeAction  = mergeAction?? bm.MergeAction;
            MergeColumns = bm.MergeColumns;
            MatchAction  = bm.MatchAction;

            DataReader = bm.DataReader;
            ColumnMaps = bm.ColumnMaps;
        }

        public BulkAndMerge(ITable table) : this(table, null)
        { }

        public BulkAndMerge(ITable table, IEnumerable<string> mergeColumns, MergeAction mergeAction = MergeAction.InsertOnly, IEnumerable<ColumnMap> columnMaps = null, bool guaranteedDistinct = false) : this(table, mergeColumns, null, mergeAction, columnMaps, guaranteedDistinct)
        { }

        public BulkAndMerge(ITable table, IEnumerable<string> mergeColumns, Action<IRow> matchAction, MergeAction mergeAction = MergeAction.InsertOnly, IEnumerable<ColumnMap> columnMaps = null, bool guaranteedDistinct = false)
        {
            mergeColumns = mergeColumns?.ToArray();
            columnMaps   = columnMaps?.ToArray() ?? Enumerable.Empty<ColumnMap>().ToArray();

            StageSchema = "dbo";
            DestSchema  = table.Schema;

            //WARNING: if the StageTable is a global temp (i.e. ## instead of #) then deleting from it will break for multithreaded steps
            StageTable = "#" + "Stage" + "_" + table.Name.Replace("#", "").TrimStart('[').TrimEnd(']');
            DestTable  = table.Name;

            MergeAction = mergeAction;            
            MatchAction = matchAction;
           
            DataReader = new TableDataReader(guaranteedDistinct ? table : RemoveMergeColumnDuplicates(table, mergeColumns));
            
            ColumnMaps = columnMaps.Any() ? columnMaps : table.Columns.Select(c => new ColumnMap(c, c)).ToArray();

            MergeColumns = mergeColumns?.Select(c => ColumnMaps.SingleOrDefault(m => m.Source == c)?.Target ?? c).ToArray();
        }
        #endregion

        #region Private Methods
        private ITable RemoveMergeColumnDuplicates(ITable table, IEnumerable<string> mergeColumns)
        {
            mergeColumns = mergeColumns?.ToArray();

            if (mergeColumns == null || mergeColumns.None()) return table;
            
            return new Table(table.Name, table.Rows.WithDistinct(mergeColumns).ToArray());
        }
        #endregion

        #region Public Methods
        public override string ToString()
        {
            return DestTable;
        }

        public void Clear()
        {
            DataReader = null;
        }
        #endregion

        #region Explicit IMerge
        string IMerge.SourceSchema => StageSchema;
        string IMerge.SourceTable => StageTable;
        #endregion

        #region Explicit IBulk
        string IBulk.Schema => StageSchema;
        string IBulk.Table => StageTable;
        DataReader IBulk.DataReader => DataReader;
        #endregion
    }
}