namespace Rucker.Sql
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