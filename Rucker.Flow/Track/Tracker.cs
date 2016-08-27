using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Rucker.Core.Testing;
using Rucker.Core;

namespace Rucker.Flow
{
    public class Tracker
    {
        #region Classes
        private class FinishOnDispose: IDisposable
        {
            private readonly Action _finish;
            private readonly string _description;

            public FinishOnDispose(Action finish, string description)
            {
                _finish      = finish;
                _description = description;

                if (_description != null) Test.WriteLine($"Starting {_description}");
            }

            public void Dispose()
            {
                _finish();
                if(_description != null) Test.WriteLine($"Finished {_description}");
            }
        }
        #endregion

        #region Fields
        private Task _reporter;
        private BlockingCollection<Action> _reports;
        private readonly List<Tracker> _children;
        
        private int? _partCount;
        private int _partFinishedCount;
        private readonly bool _reportAsync;
        private Action<string> _parentsReportPieceStart;
        private Action<string> _parentsReportPieceFinish;
        #endregion

        #region Properties
        public List<IStateReporter> WholeReporters { get; }
        public List<IStateReporter> PieceReporters { get; }
        public List<IErrorReporter> ErrorReporters { get; }
        
        public string Message { get; set; }
        
        public decimal Percent => ChildPercent() ?? ThisPercent();

        public bool Errored => Marshal.GetExceptionCode() != 0;
        #endregion

        #region Constructor
        public Tracker(bool reportAsync = true)
        {
            WholeReporters = new List<IStateReporter>();
            PieceReporters = new List<IStateReporter>();
            ErrorReporters = new List<IErrorReporter>();

            _children    = new List<Tracker>();
            _reportAsync = reportAsync;            
        }
        #endregion

        #region Public Methods
        public IDisposable Whole(int partCount, string description = null)
        {
            if (_reportAsync)
            {
                _reports = new BlockingCollection<Action>();
                _reporter = Task.Factory.StartNew(Reporter);                
            }

            _partCount = partCount;

            ReportStart(WholeReporters, Message, Percent, Errored);

            return new FinishOnDispose(FinishWhole, description);
        }

        public IDisposable Whole(IEnumerable<Tracker> children, string description = null)
        {
            if (_reportAsync)
            {
                _reports = new BlockingCollection<Action>();
                _reporter = Task.Factory.StartNew(Reporter);
            }

            _partCount = null;
            
            AlsoTrack(children);
            ReportStart(WholeReporters, Message, Percent, Errored);

            return new FinishOnDispose(FinishWhole, description);
        }
        
        public IDisposable Piece(string description = null)
        {
            ReportPieceStart(Message);

            return new FinishOnDispose(FinishPiece, description);
        }

        public void Error(Exception exception, string description = null)
        {
            foreach (var reporter in ErrorReporters)
            {
                reporter.Report(exception);
            }            
            
            if(description != null) Test.WriteLine($"Failed {description}");
        }
        #endregion

        #region Private Methods
        private void AlsoTrack(IEnumerable<Tracker> children)
        {
            foreach (var child in children)
            {
                _children.Add(child);
                child._parentsReportPieceFinish = ReportPieceFinish;
                child._parentsReportPieceStart  = ReportPieceStart;
            }
        }
        #endregion

        #region Finish
        private void FinishPiece()
        {
            _partFinishedCount++;

            ReportPieceFinish();
        }

        private void FinishWhole()
        {
            ReportFinish(WholeReporters, Message, Percent, Errored);

            if (_reportAsync)
            {                
                _reports.CompleteAdding();
                _reporter.Wait();

                _reports.Dispose();
                _reporter.Dispose();
            }
        }        

        #endregion

        #region Percent
        private decimal? ChildPercent()
        {
            return _children.Any() ? _children.Average(c => c.Percent).To<decimal?>() : null;
        }

        private decimal ThisPercent()
        {
            //We don't know if it has any parts to track because it hasn't told us so just say nothing is finished
            if (_partCount == null) return 0;

            //We know it doesn't have any parts to track so it doesn't have a percent complete
            if (_partCount == 0) return 100;

            return 100M * _partFinishedCount / _partCount.Value;
        }

        #endregion

        #region Reporting
        private void ReportPieceStart(string message = null)
        {
            ReportStart(PieceReporters, message ?? Message, Percent, Errored);
            _parentsReportPieceStart?.Invoke(message ?? Message);
        }

        private void ReportPieceFinish(string message = null)
        {
            ReportFinish(PieceReporters, message ?? Message, Percent, Errored);
            _parentsReportPieceFinish?.Invoke(message ?? Message);
        }

        private void ReportFinish(IEnumerable<IStateReporter> reporters, string message, decimal percent, bool errored)
        {
            foreach (var reporter in reporters)
            {                
                Report(() => reporter.ReportFinish(message, percent, errored));
            }
        }

        private void ReportStart(IEnumerable<IStateReporter> reporters, string message, decimal percent, bool errored)
        {
            foreach (var reporter in reporters)
            {
                Report(() => reporter.ReportStart(message, percent, errored));
            }
        }

        private void Report(Action report)
        {
            if (_reportAsync) _reports.Add(report);
            if (!_reportAsync) report();
        }

        private void Reporter()
        {
            foreach (var report in _reports.GetConsumingEnumerable())
            {
                report();
            }
        }
        #endregion
    }
}