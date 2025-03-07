using inetum.unityUtils.systemIO;
using NUnit.Framework;
using System;

public class PathTests
{
    public class CombineTest
    {
        [Test]
        public void GivenNothingOrNull_WhenCombine_ThenNull()
        {
            // --- New Test ---
            Assert.IsNull(Path.Combine());

            // --- New Test ---
            Assert.IsNull(Path.Combine(null));
        }

        [Test]
        public void GivenEmpty_WhenCombine_ThenEmpty()
        {
            Assert.IsEmpty(Path.Combine(""));
        }

        [Test]
        public void GivenValueOrValueAndNullOrNullAndValue_WhenCombine_ThenValue()
        {
            // --- New Test ---
            Assert.AreEqual("Value", Path.Combine("Value"));

            // --- New Test ---
            Assert.AreEqual("Value", Path.Combine("Value", null));

            // --- New Test ---
            Assert.AreEqual("Value", Path.Combine(null, "Value"));
        }

        [Test]
        public void GivenValueAndEmptyOrEmptyAndValue_WhenCombine_ThenValue()
        {
            // --- New Test ---
            Assert.AreEqual("Value", Path.Combine("Value", ""));

            // --- New Test ---
            Assert.AreEqual("Value", Path.Combine("", "Value"));
        }

        [Test]
        public void GivenValue1AndValue2_WhenCombine_ThenValueSlashValueAndAllTheBackslashAreReplacedBySlash()
        {
            // --- New Test ---
            Assert.AreEqual(
                "Value1/Value2",
                Path.Combine("Value1", "Value2")
            );

            // --- New Test ---
            Assert.AreEqual(
                "/Value1/Value2",
                Path.Combine("/Value1", "Value2")
            );

            // --- New Test ---
            Assert.AreEqual(
                "/Value1/Value2",
                Path.Combine("\\Value1", "Value2")
            );

            // --- New Test ---
            Assert.AreEqual(
                "Value1/Value2",
                Path.Combine("Value1/", "Value2")
            );

            // --- New Test ---
            Assert.AreEqual(
                "Value1/Value2",
                Path.Combine("Value1\\", "Value2")
            );

            // --- New Test ---
            Assert.AreEqual(
                "Value1/Value2",
                Path.Combine("Value1", "/Value2")
            );

            // --- New Test ---
            Assert.AreEqual(
                "Value1/Value2",
                Path.Combine("Value1", "\\Value2")
            );

            // --- New Test ---
            Assert.AreEqual(
                "Value1/Value2",
                Path.Combine("Value1", "Value2/")
            );

            // --- New Test ---
            Assert.AreEqual(
                "Value1/Value2",
                Path.Combine("Value1", "Value2\\")
            );
        }

        [Test]
        public void GivenFirstStringWithSlashAndSecondStringWithBackslash_WhenCombine_ThenValueSlashValueAndTheBackslashReplaceBySlash()
        {
            // --- New Test ---
            Assert.AreEqual(
                "/Value1/Value2",
                Path.Combine("/Value1", "\\Value2")
            );

            // --- New Test ---
            Assert.AreEqual(
                "/Value1/Value2",
                Path.Combine("/Value1", "Value2\\")
            );

            // --- New Test ---
            Assert.AreEqual(
                "Value1/Value2",
                Path.Combine("Value1/", "\\Value2")
            );

            // --- New Test ---
            Assert.AreEqual(
                "Value1/Value2",
                Path.Combine("Value1/", "Value2\\")
            );

            // --- New Test ---
            Assert.AreEqual(
                "Value1/Value2/Value3/Value4", 
                Path.Combine("Value1/Value2", "\\Value3\\Value4")
            );

            // --- New Test ---
            Assert.AreEqual(
                "Value1/Value2/Value3/Value4",
                Path.Combine("Value1/Value2/", "\\Value3\\Value4")
            );
        }

        [Test]
        public void GivenFirstStringWithBackslashAndSecondStringWithSlash_WhenCombine_ThenValueSlashValueAndTheBackslashReplaceBySlash()
        {
            // --- New Test ---
            Assert.AreEqual(
                "/Value1/Value2",
                Path.Combine("\\Value1", "/Value2")
            );

            // --- New Test ---
            Assert.AreEqual(
                "/Value1/Value2",
                Path.Combine("\\Value1", "Value2/")
            );

            // --- New Test ---
            Assert.AreEqual(
                "Value1/Value2",
                Path.Combine("Value1\\", "/Value2")
            );

            // --- New Test ---
            Assert.AreEqual(
                "Value1/Value2",
                Path.Combine("Value1\\", "Value2/")
            );

            // --- New Test ---
            Assert.AreEqual(
                "Value1/Value2/Value3/Value4",
                Path.Combine("Value1\\Value2", "/Value3/Value4")
            );

            // --- New Test ---
            Assert.AreEqual(
                "Value1/Value2/Value3/Value4",
                Path.Combine("Value1\\Value2\\", "/Value3/Value4")
            );
        }
    }

    public class TrimDirectorySeparatorTest
    {
        [Test]
        public void GivenNull_WhenTrimDirectorySeparator_ThenNull()
        {
            // Nothing

            string result = Path.TrimDirectorySeparator(null);

            Assert.IsNull(result);
        }

        [Test]
        public void GivenEmpty_WhenTrimDirectorySeparator_ThenEmpty()
        {
            // Nothing

            string result = Path.TrimDirectorySeparator("");

            Assert.IsEmpty(result);
        }

        [Test]
        public void GivenValue_WhenTrimDirectorySeparator_ThenValue()
        {
            string value = "Value";

            string result = value.TrimDirectorySeparator();

            Assert.AreEqual(result, value);
        }

        [Test]
        public void GivenValueSlash_WhenTrimDirectorySeparator_ThenValue()
        {
            string value = "Value/";

            string result = value.TrimDirectorySeparator();

            Assert.AreEqual(result, "Value");
        }

        [Test]
        public void GivenValueBackslash_WhenTrimDirectorySeparator_ThenValue()
        {
            string value = "Value\\";

            string result = value.TrimDirectorySeparator();

            Assert.AreEqual(result, "Value");
        }

        [Test]
        public void GivenValueSlashSlash_WhenTrimDirectorySeparator_ThenValue()
        {
            string value = "Value//";

            string result = value.TrimDirectorySeparator();

            Assert.AreEqual(result, "Value");
        }

        [Test]
        public void GivenValueBackslashBackslash_WhenTrimDirectorySeparator_ThenValue()
        {
            string value = "Value\\\\";

            string result = value.TrimDirectorySeparator();

            Assert.AreEqual(result, "Value");
        }

        [Test]
        public void GivenSlashValue_WhenTrimDirectorySeparator_ThenValue()
        {
            string value = "/Value";

            string result = value.TrimDirectorySeparator();

            Assert.AreEqual(result, "Value");
        }

        [Test]
        public void GivenBackslashValue_WhenTrimDirectorySeparator_ThenValue()
        {
            string value = "\\Value";

            string result = value.TrimDirectorySeparator();

            Assert.AreEqual(result, "Value");
        }

        [Test]
        public void GivenSlashSlashValue_WhenTrimDirectorySeparator_ThenValue()
        {
            string value = "//Value";

            string result = value.TrimDirectorySeparator();

            Assert.AreEqual(result, "Value");
        }

        [Test]
        public void GivenBackSlashBackslashValue_WhenTrimDirectorySeparator_ThenValue()
        {
            string value = "\\\\Value";

            string result = value.TrimDirectorySeparator();

            Assert.AreEqual(result, "Value");
        }

        [Test]
        public void GivenValueSlashBackslash_WhenTrimDirectorySeparator_ThenValue()
        {
            string value = "Value/\\";

            string result = value.TrimDirectorySeparator();

            Assert.AreEqual(result, "Value");
        }

        [Test]
        public void GivenValueBackslashSlash_WhenTrimDirectorySeparator_ThenValue()
        {
            string value = "Value\\/";

            string result = value.TrimDirectorySeparator();

            Assert.AreEqual(result, "Value");
        }

        [Test]
        public void GivenSlashBackslashValue_WhenTrimDirectorySeparator_ThenValue()
        {
            string value = "/\\Value";

            string result = value.TrimDirectorySeparator();

            Assert.AreEqual(result, "Value");
        }

        [Test]
        public void GivenBackslashSlashValue_WhenTrimDirectorySeparator_ThenValue()
        {
            string value = "\\/Value";

            string result = value.TrimDirectorySeparator();

            Assert.AreEqual(result, "Value");
        }

        [Test]
        public void GivenSlashValueSlash_WhenTrimDirectorySeparator_ThenValue()
        {
            string value = "/Value/";

            string result = value.TrimDirectorySeparator();

            Assert.AreEqual(result, "Value");
        }
    }

    public class ReplaceBackslashsBySlashsTest
    {
        [Test]
        public void GivenNull_WhenReplacingBackslashsBySlashs_ThenNull()
        {
            // Nothing

            string result = Path.ReplaceBackslashsBySlashs(null);

            Assert.IsNull(result);
        }

        [Test]
        public void GivenEmpty_WhenReplacingBackslashsBySlashs_ThenEmpty()
        {
            // Nothing

            string result = Path.ReplaceBackslashsBySlashs("");

            Assert.IsEmpty(result);
        }

        [Test]
        public void GivenValueWithoutSeparator_WhenReplacingBackslashsBySlashs_ThenValue()
        {
            string value = "Value";

            string result = Path.ReplaceBackslashsBySlashs(value);

            Assert.AreEqual(result, "Value");
        }

        [Test]
        public void GivenBackslash_WhenReplacingBackslashsBySlashs_ThenSlash()
        {
            string value = "\\";

            string result = Path.ReplaceBackslashsBySlashs(value);

            Assert.AreEqual(result, "/");
        }

        [Test]
        public void GivenValuesAndSeparators_WhenReplacingBackslashsBySlashs_ThenSlash()
        {
            string value = "Value0\\Value1/Value2\\Value3/Value4";

            string result = Path.ReplaceBackslashsBySlashs(value);

            Assert.AreEqual(result, "Value0/Value1/Value2/Value3/Value4");
        }
    }

    public class InsertSlashAtTest
    {
        [Test]
        public void GivenNull_WhenInsertingSlashAt0_ThenSlash()
        {
            // Nothing

            string result = Path.InsertSlashAt(null, 0);

            Assert.AreEqual(result, "/");
        }

        [Test]
        public void GivenEmpty_WhenInsertingSlashAt0_ThenSlash()
        {
            // Nothing

            string result = "".InsertSlashAt(0);

            Assert.AreEqual(result, "/");
        }

        [Test]
        public void GivenValue_WhenInsertingSlashAt0_ThenSlashValue()
        {
            string value = "Value";

            string result = value.InsertSlashAt(0);

            Assert.AreEqual(result, "/Value");
        }

        [Test]
        public void GivenValue_WhenInsertingSlashAtLastIndex_ThenSlashValue()
        {
            string value = "Value";

            string result = value.InsertSlashAt(value.Length);

            Assert.AreEqual(result, "Value/");
        }

        [Test]
        public void GivenSlashValue_WhenInsertingSlashAt0_ThenSlashValue()
        {
            string value = "/Value";

            string result = value.InsertSlashAt(0);

            Assert.AreEqual(result, "/Value");
        }

        [Test]
        public void GivenValueSlashAt2_WhenInsertingSlashAt1_ThenValueSlashAt1And3()
        {
            string value = "Va/lue";

            string result = value.InsertSlashAt(1);

            Assert.AreEqual(result, "V/a/lue");
        }

        [Test]
        public void GivenValueSlashAt2_WhenInsertingSlashAt2_ThenValueSlashAt2()
        {
            string value = "Va/lue";

            string result = value.InsertSlashAt(2);

            Assert.AreEqual(result, "Va/lue");
        }

        [Test]
        public void GivenValueSlashAt2_WhenInsertingSlashAt3_ThenValueSlashAt2()
        {
            string value = "Va/lue";

            string result = value.InsertSlashAt(3);

            Assert.AreEqual(result, "Va/lue");
        }

        [Test]
        public void GivenValueSlashAt2_WhenInsertingSlashAt4_ThenValueSlashAt2And4()
        {
            string value = "Va/lue";

            string result = value.InsertSlashAt(4);

            Assert.AreEqual(result, "Va/l/ue");
        }

        [Test]
        public void GivenValueBackslashAt2_WhenInsertingSlashAt2_ThenValueSlashAt2AndBackslashAt3()
        {
            string value = "Va\\lue";

            string result = value.InsertSlashAt(2);

            Assert.AreEqual(result, "Va/\\lue");
        }

        [Test]
        public void GivenValue_WhenInsertingSlashAtOutOfBound_ThenException()
        {
            string value = "Value";

            Assert.Throws<IndexOutOfRangeException>(() =>
            {
                value.InsertSlashAt(-1);
            });

            Assert.Throws<IndexOutOfRangeException>(() =>
            {
                value.InsertSlashAt(-5);
            });

            Assert.Throws<IndexOutOfRangeException>(() =>
            {
                value.InsertSlashAt(value.Length + 1);
            });

            Assert.Throws<IndexOutOfRangeException>(() =>
            {
                value.InsertSlashAt(value.Length + 3);
            });
        }
    }

    public class IsValideFileNameTest
    {
        [Test]
        public void GivenNull_WhenIsValideFileName_ThenFalse()
        {
            // Nothing

            bool result = Path.IsValideFileName(null);

            Assert.False(result);
        }

        [Test]
        public void GivenEmpty_WhenIsValideFileName_ThenFalse()
        {
            // Nothing

            bool result = "".IsValideFileName();

            Assert.False(result);
        }

        [Test]
        public void GivenInvalid_WhenIsValideFileName_ThenFalse()
        {
            char[] invalidFileNameChar
            = System.IO.Path.GetInvalidFileNameChars();

            foreach (char c in invalidFileNameChar)
            {
                bool result = c.ToString().IsValideFileName();

                Assert.False(result);
            }
        }

        [Test]
        public void GivenValideValue_WhenIsValideFileName_ThenTrue()
        {
            string value = "Value";

            bool result = value.IsValideFileName();

            Assert.True(result);
        }
    }

    public class IsValidePathTest
    {
        [Test]
        public void GivenNull_WhenIsValidePath_ThenFalse()
        {
            // Nothing

            bool result = Path.IsValidePath(null);

            Assert.False(result);
        }

        [Test]
        public void GivenNull_WhenIsValidePathAllowNullAndEmpty_ThenTrue()
        {
            // Nothing

            bool result = Path.IsValidePath(null, true);

            Assert.True(result);
        }

        [Test]
        public void GivenEmpty_WhenIsValidePath_ThenFalse()
        {
            // Nothing

            bool result = "".IsValidePath();

            Assert.False(result);
        }

        [Test]
        public void GivenEmpty_WhenIsValidePathAllowNullAndEmpty_ThenTrue()
        {
            // Nothing

            bool result = "".IsValidePath(true);

            Assert.True(result);
        }

        [Test]
        public void GivenInvalid_WhenIsValidePath_ThenFalse()
        {
            char[] invalidChars
            = System.IO.Path.GetInvalidPathChars();

            foreach (char c in invalidChars)
            {
                bool result = c.ToString().IsValidePath();

                Assert.False(result);
            }
        }

        [Test]
        public void GivenValideValue_WhenIsValidePath_ThenTrue()
        {
            string value = "Value";

            bool result = value.IsValidePath();

            Assert.True(result);
        }
    }
}
