using NUnit.Framework;
using Rucker.Data;

namespace Rucker.Tests
{
    [TestFixture]
    public class FileUriTests
    {
        [Test]
        public void FileUriWorks()
        {
            var dirName  = "201602";
            var fileName = "TW-20160226111347.xlsx";

            var dirPath  = $@"\\Server1\ABC\Files\ExcelFileUploads\{dirName}";
            var filePath = $@"{dirPath}\{fileName}";

            var uriString = $"file:{filePath.Replace(@"\", "/")}";
            var uriClass  = new FileUri(uriString);

            Assert.AreEqual(dirName, uriClass.DirectoryUri.DirectoryName);
            Assert.AreEqual(fileName, uriClass.FileName);

            Assert.AreEqual(dirPath, uriClass.DirectoryUri.DirectoryPath);
            Assert.AreEqual(filePath, uriClass.FilePath);

            Assert.AreEqual(uriString, uriClass.ToString());
        }
    }
}