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

using System;
using System.Collections.Generic;
using System.Linq;

namespace inetum.unityUtils.conditionalCompilation
{
    public class ScriptingSymbolHelper 
    {
        #region Initialization

        public static ScriptingSymbolHelper @default => _default.Value;
        static readonly Lazy<ScriptingSymbolHelper> _default = new(() => new());
        ScriptingSymbolHelper() { }

        #endregion

        public IScriptingSymbolDelegate @delegate
        {
            get => _delegate;
            set
            {
                if (value == null)
                {
                    _delegate = new UnityScriptingSymbolDelegate();
                    return;
                }

                _delegate = value;
            }
        }
        IScriptingSymbolDelegate _delegate = new UnityScriptingSymbolDelegate();

        /// <summary>
        /// Updates the scripting define symbols based on the provided new symbols and all possible cases.<br/>
        /// <br/>
        /// This method takes a list of all possible cases of symbols and a list of symbols to be added.<br/>
        /// If a symbol is part of the first list but not part of the second list, it will not be present in the final list.<br/>
        /// If a symbol is not part of the first list but part of the current scripting symbols then it will remain in the final list.<br/>
        /// <br/>
        /// <example>
        /// Given no current symbols when updating symbols with new symbols then symbols are updated.<br/>
        /// <code>
        /// ScriptingSymbolHelper.@default.UpdateSymbols(
        ///     new[] { "SYMBOL_A", "SYMBOL_B", "SYMBOL_C" }, 
        ///     new[] { "SYMBOL_A", "SYMBOL_B" }
        /// );
        /// // Scripting symbols will be { "SYMBOL_A", "SYMBOL_B" }
        /// </code>
        /// <br/>
        /// Given current symbols when updating symbols with new symbols then symbols are updated.<br/>
        /// <code>
        /// // Current scripting symbols are { "SYMBOL_A", "SYMBOL_D" }
        /// ScriptingSymbolHelper.@default.UpdateSymbols(
        ///     new[] { "SYMBOL_A", "SYMBOL_B", "SYMBOL_C" }, 
        ///     new[] { "SYMBOL_B", "SYMBOL_C" }
        /// );
        /// // Scripting symbols will be { "SYMBOL_D", "SYMBOL_B", "SYMBOL_C" }
        /// </code>
        /// <br/>
        /// Given current symbols when updating symbols without cases then symbols are updated.<br/>
        /// <code>
        /// // Current scripting symbols are { "SYMBOL_A", "SYMBOL_B" }
        /// ScriptingSymbolHelper.@default.UpdateSymbols(
        ///     null, 
        ///     new[] { "SYMBOL_C" }
        /// );
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="allCases">An array of all possible symbol cases.</param>
        /// <param name="symbols">An array of new symbols to be added.</param>
        public void UpdateSymbols(string[] allCases, params string[] symbols)
        {
            // Get the current scripting define symbols
            string[] currentSymbols = @delegate.GetScriptingDefineSymbols();

            // Create a new set of symbols from the provided symbols
            HashSet<string> newSymbols = new(symbols);
            for (int i = 0; i < (currentSymbols?.Length ?? 0); i++)
            {
                string symbol = currentSymbols[i];
                // If the symbol is not in allCases, add it to the new symbols set
                if (!allCases?.Contains(symbol) ?? true)
                {
                    newSymbols.Add(symbol);
                }
            }

            // If the new symbols set is different from the current symbols, update the scripting define symbols
            if (!newSymbols.SetEquals(currentSymbols))
            {
                @delegate.SetScriptingDefineSymbols(newSymbols.ToArray());
            }
        }
    }
}