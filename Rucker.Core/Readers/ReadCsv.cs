using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualBasic.FileIO;

namespace Rucker.Core
{
    public class ReadCsv : Disposable, IRead<IRows>
    {
        #region Fields
        private readonly Stream _stream;
        private readonly string _path;
        private int? _size;
        private string[] _headers;
        #endregion

        #region Properties
        public Encoding Encoding { get; set; }
        #endregion

        #region Public Methods
        private ReadCsv()
        {
            Encoding = Encoding.Default;
        }

        public ReadCsv(string path): this()
        {
            if (path == null) throw new NullReferenceException("Path can't be null when initializing a ReadCsvFile");

            _path     = path;
        }

        public ReadCsv(Stream stream): this()
        {
            if (stream == null) throw new NullReferenceException("Stream can't be null when initializing a ReadCsvFile");

            _stream = stream;
        }

        public int Size()
        {
            DisposeCheck();

            _size = _size ?? ReadLines().Count() - 1;
            
            return _size.Value;
        }

        public IRows Read(int skip, int take)
        {
            DisposeCheck();

            _headers = _headers ?? GetLineValues(0, 1).Single();

            AccountForHeaderRow(ref skip);

            return GetRows(skip, take).ToRows();
        }
        #endregion

        #region Private Methods
        private IEnumerable<IRow> GetRows(int skip, int take)
        {
            foreach (var lineValues in GetLineValues(skip, take))
            {
                var row = new ObjectRow();

                for (var i = 0; i < _headers.Length; i++)
                {
                    var value = (lineValues[i].ToLower() == "null") ? null : lineValues[i];

                    if (_headers[i] != "") row.Add(_headers[i], value);

                    if (_headers[i] == "" && value != "") throw new UserMessageException("There is a column with values that doesn't have a header");
                }

                yield return row;
            }
        }

        private static void AccountForHeaderRow(ref int skip)
        {
            skip++;
        }

        private IEnumerable<string[]> GetLineValues(int skip, int take)
        {
            var lineBytes = GetBytes(ReadLines().Skip(skip).Take(take).ToArray());

            using (var lineStream    = new MemoryStream(lineBytes))
            using (var lineCsvReader = new TextFieldParser(lineStream, Encoding) { TextFieldType = FieldType.Delimited, Delimiters = new[] {","}, HasFieldsEnclosedInQuotes = true })
            {
                while (!lineCsvReader.EndOfData)
                {
                    yield return lineCsvReader.ReadFields();
                }
            }
        }

        private byte[] GetBytes(params string[] lines)
        {
            return Encoding.GetBytes(string.Join("\r\n", lines));
        }

        private IEnumerable<string> ReadLines()
        {
            return ReadPathLines() ?? ReadStreamLines();
        }

        private IEnumerable<string> ReadPathLines()
        {
            return (_path == null) ? null : File.ReadLines(_path, Encoding);
        }

        private IEnumerable<string> ReadStreamLines()
        {
            var localStream = GetLocalStream();

            using (var reader = new StreamReader(localStream, Encoding))
            {
                while (!reader.EndOfStream)
                {
                    yield return reader.ReadLine();
                }
            }
        }

        private MemoryStream GetLocalStream()
        {
            _stream.Position = 0;

            var localCopy = new MemoryStream();

            _stream.CopyTo(localCopy);

            localCopy.Position = 0;
            
            return localCopy;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _stream?.Close();
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}