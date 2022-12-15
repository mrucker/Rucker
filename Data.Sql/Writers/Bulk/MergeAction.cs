namespace Data.Sql
{
    public enum MergeAction
    {
        InsertOnly,
        DeleteOnly,
        UpdateOnly,
        MatchOnly,
        UpdateOrInsert,
    }
}