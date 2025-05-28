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
using System.Collections.ObjectModel;

namespace umi3d.cdk.interaction
{
    public sealed class InputManager 
    {
        #region Initialize

        static Lazy<InputManager> _default = new(() => new());
        public static InputManager @default => _default.Value;

        InputManager()
        {

        }

        #endregion

        List<Input> _inputs = new List<Input>();
        public ReadOnlyCollection<Input> inputs => _inputs.AsReadOnly();

        /// <summary>
        /// Searches for an existing input associated with the specified input system. If it exists, returns the input; 
        /// otherwise, creates a new input, adds it to the internal list, and returns it.<br/>
        /// <br/>
        /// <example>
        /// Given an input system, when calling this method, it will either return an existing input or create a new one.<br/>
        /// <br/>
        /// <code>
        /// bool isNew = inputManager.InstantiateOrGet(out Input input, inputSystem);
        /// if (isNew)
        /// {
        ///     Console.WriteLine("A new input was created.");
        /// }
        /// else
        /// {
        ///     Console.WriteLine("An existing input was returned.");
        /// }
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="input">The output parameter that will hold the existing or newly created input.</param>
        /// <param name="inputSystem">The input system to search for or associate with a new input.</param>
        /// <returns>
        /// A boolean value indicating whether a new input was created (`true`) or an existing input was found (`false`).
        /// </returns>
        internal bool InstantiateOrGet(out Input input, IInputSystem inputSystem)
        {
            input = _inputs.Find(input => input.inputSystem == inputSystem);

            if (input != null)
            {
                UnityEngine.Debug.Log($"[InputManager] Notice: input for id: '{inputSystem.id}' already exist.");
                return false;
            }

            UnityEngine.Debug.Log($"[InputManager] Notice: input for id: '{inputSystem.id}' created.");
            input = new(inputSystem);
            _inputs.Add(input);
            return true;
        }
    }
}