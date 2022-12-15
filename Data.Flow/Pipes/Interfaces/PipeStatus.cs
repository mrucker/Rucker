namespace Data.Flow
{
    public enum PipeStatus
    {
        Created = 0,
        Waiting,
        Working,
        Stopped,
        Errored,
        Finished
    }
}