namespace Data.Flow
{
    public interface IStateReporter
    {
        void ReportStart(string message, decimal percent, bool errored);
        void ReportFinish(string message, decimal percent, bool errored);
    }
}