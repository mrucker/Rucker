using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Rucker.Data;
using Rucker.Testing;
using Rucker.Extensions;


namespace Rucker.Flow
{
    /// <summary>
    /// Core of the ETL Framework. Represents a single ETL job to be processed by a Processor.
    /// </summary>
    /// <typeparam name="TSource">Incoming, Source Type</typeparam>
    /// <typeparam name="TDest">Outgoing, Dest Type</typeparam>
    public class EtlJob<TSource, TDest>: Job
    {
        #region Fields
        private IRead<TSource> _reader;
        private IWrite<TDest> _writer;
        private IEnumerable<IMap<TSource, TDest>> _mappers;
        private ISetting _setting;
        private int? _jobSize;
        private int _totalTimeoutCount;
        #endregion

        #region Properties
        /// <summary>
        /// Object that provides the data we want to Extract
        /// </summary>
        public IRead<TSource> Reader
        {
            get
            {
                DisposeCheck();
                return _reader;
            }
            set
            {
                DisposeCheck();
                _reader = value;
            }
        }

        /// <summary>
        /// Object that Loads the data after it has been Transformed
        /// </summary>
        public IWrite<TDest> Writer
        {
            get
            {
                DisposeCheck();
                return _writer;
            }
            set
            {
                DisposeCheck();
                _writer = value;
            }
        }

        /// <summary>
        /// Maps the Source Data into its Destination format
        /// </summary>
        public IEnumerable<IMap<TSource, TDest>> Mappers
        {
            get
            {
                DisposeCheck();
                return _mappers;
            }
            set
            {
                DisposeCheck();
                _mappers = value;
            }
        }

        /// <summary>
        /// Settings used during the job process
        /// </summary>
        public ISetting Setting
        {
            get
            {
                DisposeCheck();
                return _setting;
            }
            set
            {
                DisposeCheck();
                _setting = value;
            }
        }
        #endregion

        #region Constructors
        public EtlJob()
        {
            Setting = new Setting();
        }
        #endregion

        #region Overrides
        protected override void Initializing()
        { }

        protected sealed override void Processing()
        {
            Setting = Setting ?? new Setting();
            Mappers = Mappers ?? Enumerable.Empty<IMap<TSource, TDest>>();

            using (Tracker.Whole(PieceCount(), ToString()))
            {
                //It is actually faster to do a Parallel.For than Parallel.ForEach because otherwise .NET will materialize every Page in order to count them
                Parallel.For(0, PageCount(), new ParallelOptions { MaxDegreeOfParallelism = Setting.MaxDegreeOfParallelism }, i => Load(Transform(Extract(i))));
            }
        }

        public override string ToString()
        {
            return base.ToString().Replace("EtlJob`2", "EtlJob");
        }
        #endregion

        #region Dispose
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Reader?.Dispose();
                Writer?.Dispose();
            }

            base.Dispose(disposing);
        }
        #endregion

        #region Private Extract/Transform/Load
        private TSource Extract(int pageIndex)
        {
            var take = Setting.MaxPageSize == -1 ? Reader.Size() : Setting.MaxPageSize;
            var skip = Setting.MaxPageSize == -1 ? 0 : pageIndex * take;

            using (Tracker.Piece($"Extracting {Test.ClassNameOnly(Reader)}({pageIndex})"))
            {
                return TimeoutSafe(() => Reader.Read(skip, take));
            }
        }

        private IEnumerable<TDest> Transform(TSource page)
        {
            if (Mappers.None() && typeof(TSource) != typeof(TDest))
            {
                throw new ArgumentException("At least one Mapper has to be defined when the Source type doesn't equal the Dest type");
            }

            if (Mappers.None() && typeof(TSource) == typeof(TDest))
            {
                yield return (TDest)(object)page;
            }

            foreach (var mapper in Mappers)
            {
                TDest map;

                using (Tracker.Piece($"Transforming {Test.ClassNameOnly(mapper)}"))
                {
                    map = TimeoutSafe(() => mapper.Map(page));
                }

                yield return map;
            }
        }

        private void Load(IEnumerable<TDest> pages)
        {
            foreach (var page in pages)
            {
                using (Tracker.Piece($"Loading {Test.ClassNameOnly(Writer)}"))
                {
                    TimeoutSafe(() => Writer.Write(page));
                }
            }
        }
        #endregion

        #region Private Timeout Retry
        private void TimeoutSafe(Action action, int retryCount = 0)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                if (IsTimeoutException(ex) && ShouldRetryAfterTimeout(retryCount))
                {
                    TimeoutSafe(action, retryCount + 1);
                }

                throw;
            }
        }

        private T TimeoutSafe<T>(Func<T> function, int retryCount = 0)
        {
            try
            {
                return function();
            }
            catch (Exception ex)
            {
                if (IsTimeoutException(ex) && ShouldRetryAfterTimeout(retryCount))
                {
                    return TimeoutSafe(function, retryCount + 1);
                }

                throw;
            }
        }

        private bool IsTimeoutException(Exception exception)
        {
            return exception != null && (exception.Message.ToLower().Contains("timeout") || IsTimeoutException(exception.InnerException));
        }

        private bool ShouldRetryAfterTimeout(int retryCount)
        {
            const bool doRetry    = true;
            const bool doNotRetry = false;

            _totalTimeoutCount++;

            if (_totalTimeoutCount == Setting.MaximumTimeoutsAllowed)
            {
                return doNotRetry;
            }

            if (retryCount == Setting.MaximumTimeoutsAllowedPerQuery)
            {
                return doNotRetry;
            }

            return doRetry;
        }
        #endregion

        #region Private Counts
        private int PieceCount()
        {
            var pageCount   = PageCount();
            var readerCount = 1;
            var mapperCount = Mappers.Count();
            var writerCount = Mappers.Count(); //1 write per mapper

            return pageCount * (readerCount + mapperCount + writerCount);
        }

        /// <summary>
        /// Reader.Size() / JobSettings.PageSize
        /// </summary>
        /// <returns>Given the Reader and Settings returns the number of pages there are to process</returns>
        public int PageCount()
        {
            if (Setting.MaxPageSize ==  0) throw new InvalidOperationException("Setting.MaxPageSize must be greater than 0");
            if (Setting.MaxPageSize == -1) return 1;

            _jobSize = _jobSize ?? Reader.Size();

            return (int)Math.Ceiling((decimal)_jobSize / Setting.MaxPageSize);
        }
        #endregion
    }
}
