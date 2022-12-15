using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace Data.Core.Testing
{
    public class TestCsvFile : IDisposable
    {
        private static readonly string AssemblyDirectoryName = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath).Replace("\\", "/");

        public FileUri Uri { get; }

        public TestCsvFile(params object[] rows)
        {
            Uri = new FileUri("file://" + AssemblyDirectoryName + "/" + "UnitTest" + ".csv");
            
            File.Delete(Uri.FilePath);

            Write(rows.Select(r => new ObjectRow(r)));
        }

        private void Write(IEnumerable<ObjectRow> rows)
        {
            using (var streamWriter = new StreamWriter(Uri.FilePath))
            {
                streamWriter.WriteLine(string.Join(",", rows.First().Columns));

                foreach (var row in rows)
                {
                    streamWriter.WriteLine(string.Join(",", row.Values.Select(Valify)));
                }
            }
        }

        public IRows Read()
        {
            using (var reader = new ReadCsv(Uri.FilePath))
            {
                return reader.ReadAll();
            }
        }

        public void Dispose()
        {
            File.Delete(Uri.FilePath);
        }

        private static string Valify(object value)
        {
            if (value == null) return "NULL";
            if (value is int ) return value.ToString();

            return "\"" + value + "\"";
        }
    }
}