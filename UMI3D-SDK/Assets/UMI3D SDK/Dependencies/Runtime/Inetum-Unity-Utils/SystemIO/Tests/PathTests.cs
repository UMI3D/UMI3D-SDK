using inetum.unityUtils.systemIO;
using NUnit.Framework;
using System;

public class PathTests
{
    public class CombineTest
    {
        [Test]
        public void GivenNothing_WhenCombine_ThenNull()
        {
            // Nothing

            string result = Path.Combine();

            Assert.IsNull(result);
        }

        [Test]
        public void GivenNull_WhenCombine_ThenNull()
        {
            string value = null;

            string result = Path.Combine(value);

            Assert.IsNull(result);
        }

        [Test]
        public void GivenEmpty_WhenCombine_ThenEmpty()
        {
            string value = "";

            string result = Path.Combine(value);

            Assert.IsEmpty(result);
        }

        [Test]
        public void GivenNotNullAndNotEmpty_WhenCombine_ThenValue()
        {
            string value = "Value";

            string result = Path.Combine(value);

            Assert.AreEqual(result, "Value");
        }

        [Test]
        public void GivenValueAndNull_WhenCombine_ThenValue()
        {
            string value = "Value";

            string result = Path.Combine(value, null);

            Assert.AreEqual(result, "Value");
        }

        [Test]
        public void GivenNullAndValue_WhenCombine_ThenValue()
        {
            string value = "Value";

            string result = Path.Combine(null, value);

            Assert.AreEqual(result, "Value");
        }

        [Test]
        public void GivenValueAndEmpty_WhenCombine_ThenValue()
        {
            string value = "Value";

            string result = Path.Combine(value, "");

            Assert.AreEqual(result, "Value");
        }

        [Test]
        public void GivenEmptyAndValue_WhenCombine_ThenValue()
        {
            string value = "Value";

            string result = Path.Combine("", value);

            Assert.AreEqual(result, "Value");
        }

        [Test]
        public void GivenValue1AndValue2_WhenCombine_ThenValueSlashValue()
        {
            string value1 = "Value1";
            string value2 = "Value2";

            string result = Path.Combine(value1, value2);

            Assert.AreEqual(result, "Value1/Value2");
        }

        [Test]
        public void GivenSlashValue1AndValue2_WhenCombine_ThenValueSlashValue()
        {
            string value1 = "/Value1";
            string value2 = "Value2";

            string result = Path.Combine(value1, value2);

            Assert.AreEqual(result, "/Value1/Value2");
        }

        [Test]
        public void GivenValue1SlashAndValue2_WhenCombine_ThenValueSlashValue()
        {
            string value1 = "Value1/";
            string value2 = "Value2";

            string result = Path.Combine(value1, value2);

            Assert.AreEqual(result, "Value1/Value2");
        }

        [Test]
        public void GivenValue1BackslashAndValue2_WhenCombine_ThenValueSlashValue()
        {
            string value1 = "Value1\\";
            string value2 = "Value2";

            string result = Path.Combine(value1, value2);

            Assert.AreEqual(result, "Value1/Value2");
        }

        [Test]
        public void GivenBackslashInTheMiddleOfValue1AndValue2_WhenCombine_ThenValueSlashValueAndTheBackslashReplaceBySlash()
        {
            string value1 = "Val\\ue1";
            string value2 = "Value2";

            string result = Path.Combine(value1, value2);

            Assert.AreEqual(result, "Val/ue1/Value2");
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

    public class ReplaceSeparatorByAltDirectorySeparatorCharTest
    {
        [Test]
        public void GivenNull_WhenReplaceSeparatorByAltDirectorySeparatorChar_ThenNull()
        {
            // Nothing

            string result = Path.ReplaceSeparatorByAltDirectorySeparatorChar(null);

            Assert.IsNull(result);
        }

        [Test]
        public void GivenEmpty_WhenReplaceSeparatorByAltDirectorySeparatorChar_ThenEmpty()
        {
            // Nothing

            string result = Path.ReplaceSeparatorByAltDirectorySeparatorChar("");

            Assert.IsEmpty(result);
        }

        [Test]
        public void GivenValueWithoutSeparator_WhenReplaceSeparatorByAltDirectorySeparatorChar_ThenValue()
        {
            string value = "Value";

            string result = Path.ReplaceSeparatorByAltDirectorySeparatorChar(value);

            Assert.AreEqual(result, "Value");
        }

        [Test]
        public void GivenBackslash_WhenReplaceSeparatorByAltDirectorySeparatorChar_ThenSlash()
        {
            string value = "\\";

            string result = Path.ReplaceSeparatorByAltDirectorySeparatorChar(value);

            Assert.AreEqual(result, "/");
        }

        [Test]
        public void GivenValuesAndSeparators_WhenReplaceSeparatorByAltDirectorySeparatorChar_ThenSlash()
        {
            string value = "Value0\\Value1/Value2\\Value3/Value4";

            string result = Path.ReplaceSeparatorByAltDirectorySeparatorChar(value);

            Assert.AreEqual(result, "Value0/Value1/Value2/Value3/Value4");
        }
    }

    public class InsertAltDirectorySeparatorCharTest
    {
        [Test]
        public void GivenNull_WhenInsertAltDirectorySeparatorCharAt0_ThenSlash()
        {
            // Nothing

            string result = Path.InsertAltDirectorySeparatorChar(null, 0);

            Assert.AreEqual(result, "/");
        }

        [Test]
        public void GivenEmpty_WhenInsertAltDirectorySeparatorCharAt0_ThenSlash()
        {
            // Nothing

            string result = "".InsertAltDirectorySeparatorChar(0);

            Assert.AreEqual(result, "/");
        }

        [Test]
        public void GivenValue_WhenInsertAltDirectorySeparatorCharAt0_ThenSlashValue()
        {
            string value = "Value";

            string result = value.InsertAltDirectorySeparatorChar(0);

            Assert.AreEqual(result, "/Value");
        }

        [Test]
        public void GivenValue_WhenInsertAltDirectorySeparatorCharAtLastIndex_ThenSlashValue()
        {
            string value = "Value";

            string result = value.InsertAltDirectorySeparatorChar(value.Length);

            Assert.AreEqual(result, "Value/");
        }

        [Test]
        public void GivenSlashValue_WhenInsertAltDirectorySeparatorCharAt0_ThenSlashValue()
        {
            string value = "/Value";

            string result = value.InsertAltDirectorySeparatorChar(0);

            Assert.AreEqual(result, "/Value");
        }

        [Test]
        public void GivenValueSlashAt2_WhenInsertAltDirectorySeparatorCharAt1_ThenValueSlashAt1And3()
        {
            string value = "Va/lue";

            string result = value.InsertAltDirectorySeparatorChar(1);

            Assert.AreEqual(result, "V/a/lue");
        }

        [Test]
        public void GivenValueSlashAt2_WhenInsertAltDirectorySeparatorCharAt2_ThenValueSlashAt2()
        {
            string value = "Va/lue";

            string result = value.InsertAltDirectorySeparatorChar(2);

            Assert.AreEqual(result, "Va/lue");
        }

        [Test]
        public void GivenValueSlashAt2_WhenInsertAltDirectorySeparatorCharAt3_ThenValueSlashAt2()
        {
            string value = "Va/lue";

            string result = value.InsertAltDirectorySeparatorChar(3);

            Assert.AreEqual(result, "Va/lue");
        }

        [Test]
        public void GivenValueSlashAt2_WhenInsertAltDirectorySeparatorCharAt4_ThenValueSlashAt2And4()
        {
            string value = "Va/lue";

            string result = value.InsertAltDirectorySeparatorChar(4);

            Assert.AreEqual(result, "Va/l/ue");
        }

        [Test]
        public void GivenValueBackslashAt2_WhenInsertAltDirectorySeparatorCharAt2_ThenValueSlashAt2AndBackslashAt3()
        {
            string value = "Va\\lue";

            string result = value.InsertAltDirectorySeparatorChar(2);

            Assert.AreEqual(result, "Va/\\lue");
        }

        [Test]
        public void GivenValue_WhenInsertAltDirectorySeparatorCharAtOutOfBound_Then()
        {
            string value = "Value";

            Assert.Throws<IndexOutOfRangeException>(() =>
            {
                value.InsertAltDirectorySeparatorChar(-1);
            });

            Assert.Throws<IndexOutOfRangeException>(() =>
            {
                value.InsertAltDirectorySeparatorChar(-5);
            });

            Assert.Throws<IndexOutOfRangeException>(() =>
            {
                value.InsertAltDirectorySeparatorChar(value.Length + 1);
            });

            Assert.Throws<IndexOutOfRangeException>(() =>
            {
                value.InsertAltDirectorySeparatorChar(value.Length + 3);
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
