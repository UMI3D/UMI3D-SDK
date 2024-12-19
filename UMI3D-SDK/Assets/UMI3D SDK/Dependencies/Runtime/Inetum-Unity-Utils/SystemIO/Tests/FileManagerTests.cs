using inetum.unityUtils.systemIO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class FileManagerTests
{
    public class FullPathFromPersistentDataPathTest
    {
        string companyName;
        string productName;

        [SetUp]
        public void SetUp()
        {
            companyName = Application.companyName;
            productName = Application.productName;
        }

        [Test]
        public void GivenNull_WhenFullPathFromPersistentDataPath_ThenFullPath()
        {
            // Null;

            string fullPath = FileManager.FullPathFromPersistentDataPath(null);
            
            Assert.True(fullPath.EndsWith($"/AppData/LocalLow/{companyName}/{productName}"));
            Assert.True(System.IO.Directory.Exists(fullPath));
        }

        [Test]
        public void GivenEmpty_WhenFullPathFromPersistentDataPath_ThenFullPath()
        {
            // Null;

            string fullPath = FileManager.FullPathFromPersistentDataPath("");

            Assert.True(fullPath.EndsWith($"/AppData/LocalLow/{companyName}/{productName}"));
            Assert.True(System.IO.Directory.Exists(fullPath));
        }

        [Test]
        public void GivenPartialFilePath_WhenFullPathFromPersistentDataPath_ThenFullPath()
        {
            string fileNameWithExtension = "Test.txt";

            string fullPath = FileManager.FullPathFromPersistentDataPath(fileNameWithExtension);

            Assert.True(fullPath.EndsWith($"/AppData/LocalLow/{companyName}/{productName}/Test.txt"));
            Assert.True(System.IO.Directory.Exists(fullPath.Substring(0, fullPath.Length - "/Test.txt".Length)));
        }

        [Test]
        public void GivenFullFilePath_WhenFullPathFromPersistentDataPath_ThenFullPath()
        {
            string fileNameWithExtension = "Test.txt";
            string path = Path.Combine(Application.persistentDataPath, fileNameWithExtension);
            string wrongPath = Application.persistentDataPath + "\\" + fileNameWithExtension;

            string fullPath = FileManager.FullPathFromPersistentDataPath(path);
            string fullPath2 = FileManager.FullPathFromPersistentDataPath(wrongPath);

            Assert.True(fullPath.EndsWith($"/AppData/LocalLow/{companyName}/{productName}/Test.txt"));
            Assert.True(System.IO.Directory.Exists(fullPath.Substring(0, fullPath.Length - "/Test.txt".Length)));
            Assert.True(fullPath2.EndsWith($"/AppData/LocalLow/{companyName}/{productName}/Test.txt"));
            Assert.True(System.IO.Directory.Exists(fullPath2.Substring(0, fullPath2.Length - "/Test.txt".Length)));
        }
    }

    public class ExistsTest
    {
        [Test]
        public void GivenNull_WhenExists_ThenFalse()
        {
            // Null;

            bool result = FileManager.Exists(null);

            Assert.False(result);
        }

        [Test]
        public void GivenEmpty_WhenExists_ThenFalse()
        {
            // Null;

            bool result = FileManager.Exists("");

            Assert.False(result);
        }

        [Test]
        public void GivenWrongValue_WhenExists_ThenFalse()
        {
            string value = "NotAFile";

            bool result = FileManager.Exists(value);

            Assert.False(result);
        }

        [Test]
        public void GivenFile_WhenExistsFromPartialPath_ThenTrue()
        {
            string directory = "TestDirectory";
            string file = "TestFile.txt";
            string content = "This is a test file";
            FileManager.WriteToFile(content, directory, file, out string path);

            bool result = FileManager.Exists("TestDirectory/TestFile.txt");

            Assert.True(result);

            // Teardown
            System.IO.Directory.Delete(FileManager.FullPathFromPersistentDataPath(directory), true);
        }

        [Test]
        public void GivenFile_WhenExistsFromFullPath_ThenTrue()
        {
            string directory = "TestDirectory";
            string file = "TestFile.txt";
            string content = "This is a test file";
            FileManager.WriteToFile(content, directory, file, out string path);

            bool result = FileManager.Exists(path);

            Assert.True(result);

            // Teardown
            System.IO.Directory.Delete(FileManager.FullPathFromPersistentDataPath(directory), true);
        }
    }

    public class WriteToFileTest
    {
        [Test]
        public void GivenNullNullNull_WhenWriteToFile_ThenFalseAndLogError()
        {
            // Null;

            LogAssert.Expect(LogType.Error, $"[FileManager.WriteToFile] Invalide file name ''.");

            bool result = FileManager.WriteToFile(
                null, 
                null, 
                null, 
                out string path
            );

            Assert.False(result);
            Assert.IsNull(path);
        }

        [Test]
        public void GivenNullNullEmpty_WhenWriteToFile_ThenFalseAndLogError()
        {
            // Null;

            LogAssert.Expect(LogType.Error, $"[FileManager.WriteToFile] Invalide file name ''.");

            bool result = FileManager.WriteToFile(
                null,
                null,
                "",
                out string path
            );

            Assert.False(result);
            Assert.IsNull(path);
        }

        [Test]
        public void GivenNullNullValid_WhenWriteToFile_ThenTrue()
        {
            string companyName = Application.companyName;
            string productName = Application.productName;
            string fileName = "FileName.txt";

            bool result = FileManager.WriteToFile(
                null,
                null,
                fileName,
                out string path
            );

            Assert.True(result);
            Assert.True(path.EndsWith($"/AppData/LocalLow/{companyName}/{productName}/FileName.txt"));

            // Teardown
            FileManager.Delete(fileName);
        }

        [Test]
        public void GivenNullDirectoryFile_WhenWriteToFile_ThenFalseAndLogError()
        {
            string companyName = Application.companyName;
            string productName = Application.productName;
            string directory = "TestDirectory";

            bool result = FileManager.WriteToFile(
                null,
                directory,
                "FileName.txt",
                out string path
            );

            Assert.True(result);
            Assert.True(path.EndsWith($"/AppData/LocalLow/{companyName}/{productName}/TestDirectory/FileName.txt"));

            // Teardown
            System.IO.Directory.Delete(FileManager.FullPathFromPersistentDataPath(directory), true);
        }
    }

    public class LoadFromFileTest
    {
        [Test]
        public void GivenNullNull_WhenLoadFromFile_ThenFalseAndLogError()
        {
            // Null;

            LogAssert.Expect(LogType.Error, $"[FileManager.LoadFromFile] Invalide file name ''.");
            bool result = FileManager.LoadFromFile(
                null,
                null,
                out string path,
                out string content
            );

            Assert.False(result);
            Assert.IsNull(path);
            Assert.IsNull(content);
        }

        [Test]
        public void GivenNullFileThatDoesNotExist_WhenLoadFromFile_ThenFalseAndLogError()
        {
            string fileName = "File That Does not exist";

            LogAssert.Expect(LogType.Error, $"[FileManager.LoadFromFile] File at {fileName} doesn't exist.");
            bool result = FileManager.LoadFromFile(
                null,
                fileName,
                out string path,
                out string content
            );

            Assert.False(result);
            Assert.IsNull(path);
            Assert.IsNull(content);
        }

        [Test]
        public void GivenDirectoryFile_WhenLoadFromFile_ThenTrue()
        {
            string companyName = Application.companyName;
            string productName = Application.productName;
            string directory = "TestDirectory";
            string fileName = "TestFile.txt";
            FileManager.WriteToFile("This is a test.", directory, fileName, out string _);

            bool result = FileManager.LoadFromFile(
                directory,
                fileName,
                out string path,
                out string content
            );

            Assert.True(result);
            Assert.True(path.EndsWith($"/AppData/LocalLow/{companyName}/{productName}/TestDirectory/TestFile.txt"));
            Assert.AreEqual(content, "This is a test.");

            // Teardown
            System.IO.Directory.Delete(FileManager.FullPathFromPersistentDataPath(directory), true);
        }
    }

    public class MoveTest
    {
        [Test]
        public void GivenNullNUllNull_WhenMove_ThenFalseAndLogError()
        {
            string fullPath = FileManager.FullPathFromPersistentDataPath(null);

            LogAssert.Expect(LogType.Error, $"[FileManager.MoveFile] Failed to move file because file does not exist {fullPath}.");
            bool result = FileManager.Move(null, null, null, out string newPath);

            Assert.False(result);
            Assert.IsNull(newPath);
        }

        [Test]
        public void GivenFileDirectoryNewFileName_WhenMoveInSameDirectoryWithPartialPath_ThenTrue()
        {
            string companyName = Application.companyName;
            string productName = Application.productName;
            string directory = "TestDirectory";
            string file = "TestFile";
            FileManager.WriteToFile(null, directory, file, out string path);
            string partialPath = Path.Combine(directory, file);

            bool result = FileManager.Move(partialPath, directory, "newTestFile", out string newPath);
            // TODO
            Assert.True(result);
            Assert.True(newPath.EndsWith($"/AppData/LocalLow/{companyName}/{productName}/TestDirectory/newTestFile"));
            Assert.False(FileManager.Exists(path));

            // Teardown
            System.IO.Directory.Delete(FileManager.FullPathFromPersistentDataPath(directory), true);
        }

        [Test]
        public void GivenFileDirectoryNewFileName_WhenMoveInSameDirectoryWithFullPath_ThenTrue()
        {
            string companyName = Application.companyName;
            string productName = Application.productName;
            string directory = "TestDirectory";
            string file = "TestFile";
            FileManager.WriteToFile(null, directory, file, out string path);

            bool result = FileManager.Move(path, directory, "newTestFile", out string newPath);
            
            Assert.True(result);
            Assert.True(newPath.EndsWith($"/AppData/LocalLow/{companyName}/{productName}/TestDirectory/newTestFile"));
            Assert.True(FileManager.Exists(newPath));
            Assert.False(FileManager.Exists(path));

            // Teardown
            System.IO.Directory.Delete(FileManager.FullPathFromPersistentDataPath(directory), true);
        }

        [Test]
        public void GivenFileDirectoryNewFileName_WhenMoveInOtherDirectory_ThenTrue()
        {
            string companyName = Application.companyName;
            string productName = Application.productName;
            string startDirectory = "TestDirectory";
            string startFile = "TestFile";
            string endDirectory = "NewTestDirectory";
            string endFile = "newTestFile";
            FileManager.WriteToFile(null, startDirectory, startFile, out string path);

            bool result = FileManager.Move(path, endDirectory, endFile, out string newPath);
            
            Assert.True(result);
            Assert.True(newPath.EndsWith($"/AppData/LocalLow/{companyName}/{productName}/NewTestDirectory/newTestFile"));
            Assert.True(FileManager.Exists(newPath));
            Assert.False(FileManager.Exists(path));
            Assert.True(System.IO.Directory.Exists(Application.persistentDataPath + "/" + startDirectory));

            // Teardown
            System.IO.Directory.Delete(FileManager.FullPathFromPersistentDataPath(startDirectory), true);
            System.IO.Directory.Delete(FileManager.FullPathFromPersistentDataPath(endDirectory), true);
        }
    }

    public class DeleteTest
    {
        [Test]
        public void GivenNull_WhenDelete_ThenFalseAndLogError()
        {
            string fullPath = FileManager.FullPathFromPersistentDataPath(null);

            LogAssert.Expect(LogType.Error, $"[FileManager.Delete] Try to delete a file (at: {fullPath}) that doesn't exist.");
            bool result = FileManager.Delete(null);

            Assert.False(result);
        }

        [Test]
        public void GivenFileThatDoesNotExist_WhenDelete_ThenFalse()
        {
            string fileName = "TestFile";
            string fullPath = FileManager.FullPathFromPersistentDataPath(fileName);
            Assert.False(FileManager.Exists(fullPath));

            LogAssert.Expect(LogType.Error, $"[FileManager.Delete] Try to delete a file (at: {fullPath}) that doesn't exist.");
            bool result = FileManager.Delete(fileName);

            Assert.False(result);
        }

        [Test]
        public void GivenFileInDirectory_WhenDeleteDirectory_ThenFalse()
        {
            string directory = "TestDirectory";
            string fileName = "TestFile";
            FileManager.WriteToFile(null, directory, fileName, out string path);

            string fullPath = FileManager.FullPathFromPersistentDataPath(directory);
            LogAssert.Expect(LogType.Error, $"[FileManager.Delete] Try to delete a file but path point to a directory '{fullPath}'.");
            bool result = FileManager.Delete(directory);

            Assert.False(result);
            Assert.True(FileManager.Exists("TestDirectory/TestFile"));

            // Teardown
            System.IO.Directory.Delete(fullPath, true);
        }

        [Test]
        public void GivenFile_WhenDeletePartialPath_ThenTrue()
        {
            string fileName = "TestFile";
            FileManager.WriteToFile(null, null, fileName, out string path);

            bool result = FileManager.Delete(fileName);

            Assert.True(result);
        }

        [Test]
        public void GivenFile_WhenDeleteFullPath_ThenTrue()
        {
            string fileName = "TestFile";
            FileManager.WriteToFile(null, null, fileName, out string path);

            bool result = FileManager.Delete(path);

            Assert.True(result);
        }
    }
}
