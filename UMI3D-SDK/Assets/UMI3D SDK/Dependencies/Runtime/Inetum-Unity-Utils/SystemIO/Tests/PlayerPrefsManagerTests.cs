using inetum.unityUtils.systemIO;
using NUnit.Framework;
using UnityEngine.TestTools;

public class PlayerPrefsManagerTests
{
    public class WriteToFileTest
    {
        [Test]
        public void GivenNullKey_WhenWriteToFile_ThenFalse()
        {
            // Nothing

            LogAssert.Expect(UnityEngine.LogType.Error, $"[PlayerPrefsManager.WriteToFile] Try to write to an empty or null file.");
            bool result = PlayerPrefsManager.WriteToFile(null, null);

            Assert.IsFalse(result);
        }

        [Test]
        public void GivenEmptyKey_WhenWriteToFile_ThenFalse()
        {
            // Nothing

            LogAssert.Expect(UnityEngine.LogType.Error, $"[PlayerPrefsManager.WriteToFile] Try to write to an empty or null file.");
            bool result = PlayerPrefsManager.WriteToFile("", null);

            Assert.IsFalse(result);
        }

        [Test]
        public void GivenKeyAndNullContent_WhenWriteToFile_ThenTrue()
        {
            string key = "TestKey";

            bool result = PlayerPrefsManager.WriteToFile(key, null);

            Assert.IsTrue(result);
            Assert.IsTrue(PlayerPrefsManager.Exists(key));

            // Teardown
            PlayerPrefsManager.Delete(key);
        }

        [Test]
        public void GivenKeyAndContent_WhenWriteToFile_ThenTrue()
        {
            string key = "TestKey";

            bool result = PlayerPrefsManager.WriteToFile(key, "This is a content");

            Assert.IsTrue(result);
            Assert.IsTrue(PlayerPrefsManager.Exists(key));
            Assert.True(PlayerPrefsManager.LoadFromFile(key, out string content));
            Assert.AreEqual(content, "This is a content");

            // Teardown
            PlayerPrefsManager.Delete(key);
        }
    }

    public class LoadFromFileTest
    {
        [Test]
        public void GivenNullKey_WhenLoadFromFile_ThenFalse()
        {
            // Nothing

            LogAssert.Expect(UnityEngine.LogType.Error, $"[PlayerPrefsManager.LoadFromFile] Try to load from an empty or null file.");
            bool result = PlayerPrefsManager.LoadFromFile(null, out string content);

            Assert.False(result);
            Assert.Null(content);
        }

        [Test]
        public void GivenEmptyKey_WhenLoadFromFile_ThenFalse()
        {
            // Nothing

            LogAssert.Expect(UnityEngine.LogType.Error, $"[PlayerPrefsManager.LoadFromFile] Try to load from an empty or null file.");
            bool result = PlayerPrefsManager.LoadFromFile("", out string content);

            Assert.False(result);
            Assert.Null(content);
        }

        [Test]
        public void GivenKeyThatDoesNotExist_WhenLoadFromFile_ThenFalse()
        {
            string key = "Key that does not exist";
            Assert.False(PlayerPrefsManager.Exists(key));

            LogAssert.Expect(UnityEngine.LogType.Error, $"[PlayerPrefsManager.LoadFromFile] Try to load from a key '{key}' that does not exist.");
            bool result = PlayerPrefsManager.LoadFromFile(key, out string content);

            Assert.False(result);
            Assert.Null(content);
        }

        [Test]
        public void GivenKeyAndNullContent_WhenLoadFromFile_ThenTrueAndEmptyContent()
        {
            string key = "TestKey";
            PlayerPrefsManager.WriteToFile(key, null);
            Assert.True(PlayerPrefsManager.Exists(key));

            bool result = PlayerPrefsManager.LoadFromFile(key, out string content);

            Assert.True(result);
            Assert.IsEmpty(content);

            // Teardown
            PlayerPrefsManager.Delete(key);
        }

        [Test]
        public void GivenKeyAndContent_WhenLoadFromFile_ThenTrueAndContent()
        {
            string key = "TestKey";
            PlayerPrefsManager.WriteToFile(key, "This is a content");
            Assert.True(PlayerPrefsManager.Exists(key));

            bool result = PlayerPrefsManager.LoadFromFile(key, out string content);

            Assert.True(result);
            Assert.AreEqual(content, "This is a content");

            // Teardown
            PlayerPrefsManager.Delete(key);
        }
    }

    public class MoveTest
    {
        [Test]
        public void GivenNullNull_WhenMove_ThenFalse()
        {
            // Nothing

            LogAssert.Expect(UnityEngine.LogType.Error, $"[PlayerPrefsManager.Move] Try to move from an empty or null file.");
            bool result = PlayerPrefsManager.Move(null, null);

            Assert.False(result);
        }

        [Test]
        public void GivenEmptyNull_WhenMove_ThenFalse()
        {
            // Nothing

            LogAssert.Expect(UnityEngine.LogType.Error, $"[PlayerPrefsManager.Move] Try to move from an empty or null file.");
            bool result = PlayerPrefsManager.Move("", null);

            Assert.False(result);
        }

        [Test]
        public void GivenKeyThatDoesNotExistNull_WhenMove_ThenFalse()
        {
            string key = "Key that does not exist";
            Assert.False(PlayerPrefsManager.Exists(key));

            LogAssert.Expect(UnityEngine.LogType.Error, $"[PlayerPrefsManager.Move] Try to move from a file '{key}' that does not exist.");
            bool result = PlayerPrefsManager.Move(key, null);

            Assert.False(result);
        }

        [Test]
        public void GivenKeyAndNull_WhenMove_ThenFalse()
        {
            string key = "TestKey";
            PlayerPrefsManager.WriteToFile(key, null);
            Assert.True(PlayerPrefsManager.Exists(key));

            LogAssert.Expect(UnityEngine.LogType.Error, $"[PlayerPrefsManager.Move] Try to move from file to an empty or null new file.");
            bool result = PlayerPrefsManager.Move(key, null);

            Assert.False(result);
        }

        [Test]
        public void GivenKeyAndEmpty_WhenMove_ThenFalse()
        {
            string key = "TestKey";
            PlayerPrefsManager.WriteToFile(key, null);
            Assert.True(PlayerPrefsManager.Exists(key));

            LogAssert.Expect(UnityEngine.LogType.Error, $"[PlayerPrefsManager.Move] Try to move from file to an empty or null new file.");
            bool result = PlayerPrefsManager.Move(key, "");

            Assert.False(result);
        }

        [Test]
        public void GivenKeyAndNewKey_WhenMove_ThenTrue()
        {
            string key = "TestKey";
            PlayerPrefsManager.WriteToFile(key, "This is a content");
            Assert.True(PlayerPrefsManager.Exists(key));
            string newKey = "NewTestKey";

            bool result = PlayerPrefsManager.Move(key, newKey);

            Assert.True(result);
            Assert.False(PlayerPrefsManager.Exists(key));
            Assert.True(PlayerPrefsManager.Exists(newKey));
            Assert.True(PlayerPrefsManager.LoadFromFile(newKey, out string content));
            Assert.AreEqual(content, "This is a content");

            // Teardown
            PlayerPrefsManager.Delete(newKey);
        }
    }

    public class DeleteTest
    {
        [Test]
        public void GivenNullKey_WhenDelete_ThenFalse()
        {
            // Nothing

            LogAssert.Expect(UnityEngine.LogType.Error, $"[PlayerPrefsManager.Delete] Try to delete a key '' that does not exist.");
            bool result = PlayerPrefsManager.Delete(null);

            Assert.False(result);
        }

        [Test]
        public void GivenEmptyKey_WhenDelete_ThenFalse()
        {
            // Nothing

            LogAssert.Expect(UnityEngine.LogType.Error, $"[PlayerPrefsManager.Delete] Try to delete a key '' that does not exist.");
            bool result = PlayerPrefsManager.Delete("");

            Assert.False(result);
        }

        [Test]
        public void GivenKeyThatDoesNotExist_WhenDelete_ThenFalse()
        {
            string key = "That key does not exist";

            LogAssert.Expect(UnityEngine.LogType.Error, $"[PlayerPrefsManager.Delete] Try to delete a key 'That key does not exist' that does not exist.");
            bool result = PlayerPrefsManager.Delete(key);

            Assert.False(result);
        }

        [Test]
        public void GivenKey_WhenDelete_ThenTrue()
        {
            string key = "TestKey";
            UnityEngine.PlayerPrefs.SetString(key, "This is a content");
            Assert.True(UnityEngine.PlayerPrefs.HasKey(key));

            bool result = PlayerPrefsManager.Delete(key);

            Assert.True(result);
            Assert.False(UnityEngine.PlayerPrefs.HasKey(key));
        }
    }

    public class ExistsTest
    {
        [Test]
        public void GivenNullKey_WhenExists_ThenFalse()
        {
            // Nothing

            bool result = PlayerPrefsManager.Exists(null);

            Assert.False(result);
        }

        [Test]
        public void GivenEmptyKey_WhenExists_ThenFalse()
        {
            // Nothing

            bool result = PlayerPrefsManager.Exists("");

            Assert.False(result);
        }

        [Test]
        public void GivenKeyThatDoesNotExist_WhenExists_ThenFalse()
        {
            string key = "Key that does not exist";

            bool result = PlayerPrefsManager.Exists(key);

            Assert.False(result);
        }

        [Test]
        public void GivenKey_WhenExists_ThenTrue()
        {
            string key = "TestKey";
            PlayerPrefsManager.WriteToFile(key, null);

            bool result = PlayerPrefsManager.Exists(key);

            Assert.True(result);

            // Teardown
            PlayerPrefsManager.Delete(key);
        }
    }
}
