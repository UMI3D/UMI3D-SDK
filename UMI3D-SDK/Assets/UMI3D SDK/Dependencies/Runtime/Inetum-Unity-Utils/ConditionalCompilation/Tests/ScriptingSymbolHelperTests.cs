/*
Copyright 2019 - 2025 Inetum

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
using inetum.unityUtils.conditionalCompilation;
using NUnit.Framework;

public class ScriptingSymbolHelperTests
{
    public class UpdateSymbolsTest
    {
        class ScriptingSymbolTestDelegate : IScriptingSymbolDelegate
        {
            public string[] _symbols = new string[0];
            public bool hasBeenUpdated = false;

            public string[] GetScriptingDefineSymbols()
            {
                return _symbols;
            }

            public void SetScriptingDefineSymbols(string[] defines)
            {
                _symbols = defines;
                hasBeenUpdated = true;
            }
        }

        ScriptingSymbolTestDelegate _testDelegate;

        [SetUp]
        public void SetUp()
        {
            _testDelegate = new ScriptingSymbolTestDelegate();
            ScriptingSymbolHelper.@default.@delegate = _testDelegate;
        }

        [Test]
        public void GivenNoCurrentSymbols_WhenUpdateSymbolsWithNewSymbols_ThenSymbolsAreUpdated()
        {
            // Given
            string[] allCases = { "SYMBOL_A", "SYMBOL_B", "SYMBOL_C" };
            string[] newSymbols = { "SYMBOL_A", "SYMBOL_B" };

            // When
            ScriptingSymbolHelper.@default.UpdateSymbols(allCases, newSymbols);

            // Then
            string[] expectedSymbols = { "SYMBOL_A", "SYMBOL_B" };
            CollectionAssert.AreEquivalent(expectedSymbols, _testDelegate.GetScriptingDefineSymbols());
            Assert.True(_testDelegate.hasBeenUpdated);
        }

        [Test]
        public void GivenCurrentSymbols_WhenUpdateSymbolsWithNewSymbols_ThenSymbolsAreUpdated()
        {
            // Given
            _testDelegate._symbols = new[] { "SYMBOL_A", "SYMBOL_D" };
            string[] allCases = { "SYMBOL_A", "SYMBOL_B", "SYMBOL_C" };
            string[] newSymbols = { "SYMBOL_B", "SYMBOL_C" };

            // When
            ScriptingSymbolHelper.@default.UpdateSymbols(allCases, newSymbols);

            // Then
            string[] expectedSymbols = { "SYMBOL_D", "SYMBOL_B", "SYMBOL_C" };
            CollectionAssert.AreEquivalent(expectedSymbols, _testDelegate.GetScriptingDefineSymbols());
            Assert.True(_testDelegate.hasBeenUpdated);
        }

        [Test]
        public void GivenCurrentSymbols_WhenUpdateSymbolsWithNoNewSymbols_ThenSymbolsAreUpdated()
        {
            // Given
            _testDelegate._symbols = new[] { "SYMBOL_A", "SYMBOL_B" };
            string[] allCases = { "SYMBOL_A", "SYMBOL_B", "SYMBOL_C" };
            string[] newSymbols = { };

            // When
            ScriptingSymbolHelper.@default.UpdateSymbols(allCases, newSymbols);

            // Then
            string[] expectedSymbols = { };
            CollectionAssert.AreEquivalent(expectedSymbols, _testDelegate.GetScriptingDefineSymbols());
            Assert.True(_testDelegate.hasBeenUpdated);
        }

        [Test]
        public void GivenNullDelegate_WhenSetDelegate_ThenDefaultDelegateIsUsed()
        {
            // Given
            ScriptingSymbolHelper.@default.@delegate = null;

            // When
            var result = ScriptingSymbolHelper.@default.@delegate;

            // Then
            Assert.IsInstanceOf<UnityScriptingSymbolDelegate>(result);
        }

        [Test]
        public void GivenCurrentSymbols_WhenUpdateSymbolsWithoutCases_ThenSymbolsAreUpdated()
        {
            // Given
            _testDelegate._symbols = new[] { "SYMBOL_A", "SYMBOL_B" };
            string[] allCases = null;
            string[] newSymbols = { "SYMBOL_C" };

            // When
            ScriptingSymbolHelper.@default.UpdateSymbols(allCases, newSymbols);

            // Then
            string[] expectedSymbols = { "SYMBOL_A", "SYMBOL_B", "SYMBOL_C" };
            CollectionAssert.AreEquivalent(expectedSymbols, _testDelegate.GetScriptingDefineSymbols());
            Assert.True(_testDelegate.hasBeenUpdated);
        }

        [Test]
        public void GivenCurrentSymbols_WhenUpdateSymbolsWithoutChanging_ThenSymbolsAreNotUpdated()
        {
            // Given
            _testDelegate._symbols = new[] { "SYMBOL_A", "SYMBOL_B" };
            string[] allCases = null;
            string[] newSymbols = { "SYMBOL_A", "SYMBOL_B" };

            // When
            ScriptingSymbolHelper.@default.UpdateSymbols(allCases, newSymbols);

            // Then
            string[] expectedSymbols = { "SYMBOL_A", "SYMBOL_B" };
            CollectionAssert.AreEquivalent(expectedSymbols, _testDelegate.GetScriptingDefineSymbols());
            Assert.False(_testDelegate.hasBeenUpdated);
        }
    }
}