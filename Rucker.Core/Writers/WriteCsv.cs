using System.IO;

namespace Rucker.Core
{
    public class WriteCsv: Disposable, IWrite<IRows>
    {
        #region Fields
        private readonly FileUri _dest;
        private StreamWriter _stream;
        #endregion

        #region Constructors
        public WriteCsv(FileUri dest)
        {
            _dest = dest;   
        }
        #endregion

        #region Public Methods
        public void Write(IRows rows)
        {
            DisposeCheck();

            if (_stream == null)
            {
                _stream = new StreamWriter(_dest.FilePath);

                _stream.WriteLine(rows.Columns().Cat(","));
                _stream.Flush();
            }

            foreach (var row in rows)
            {
                _stream.WriteLine(row.Values.Cat(","));
                _stream.Flush();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _stream.Close();
                _stream.Dispose();
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}
