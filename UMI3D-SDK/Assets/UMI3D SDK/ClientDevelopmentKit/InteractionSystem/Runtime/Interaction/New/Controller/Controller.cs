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

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace umi3d.cdk.interaction
{
    public sealed class Controller 
    {
        internal Controller(string id)
        {
            this.id = id;
        }

        /// <summary>
        /// The unique identifier of this controller.<br/>
        /// <br/>
        /// You can set the name of the controller here but it has to be unique.
        /// </summary>
        public readonly string id;

        /// <summary>
        /// Whether a tool can be projected on this controller.
        /// </summary>
        public bool isActive { get; private set; } = true;
        /// <summary>
        /// Set the active status.<br/>
        /// <br/>
        /// If true then a tool can be projected on this controller.
        /// </summary>
        /// <param name="active"></param>
        public void SetActive(bool active)
        {
            this.isActive = active;
        }

        List<Input> _inputs = new List<Input>();
        public ReadOnlyCollection<Input> inputs => _inputs.AsReadOnly();
        public bool Add(Input input)
        {
            if (input.controller != null) { return false; }

            input.Associate(this);
            _inputs.Add(input);
            return true;
        }
        public bool Remove(Input input)
        {
            if (input.controller != this) { return false; }

            input.DissociateFromController();
            _inputs.Remove(input);
            return true;
        }

        public bool TryToInstantiateOrGetInput(out Input input, IInputSystem inputSystem, out bool hasBeenInstantiated)
        {
            hasBeenInstantiated = InputManager.@default.InstantiateOrGet(
                out input,
                inputSystem
            );

            return _inputs.Contains(input) || Add(input);
        }

        /// <summary>
        /// Try to add the input corresponding to <paramref name="control"/> to the list of <paramref name="inputs"/>.<br/>
        /// <br/>
        /// Which actionType to choose:
        /// <list type="bullet">
        /// <item>
        /// Value:<br/>
        /// Action used to read a continuous or single value (e.g., joystick position, trigger pressure).<br/>
        /// Calls the following phases:<br/>
        /// - started: When the input starts changing.<br/>
        /// - performed: On every value update.<br/>
        /// - canceled: When the input is canceled (e.g., returns to a neutral value).
        /// </item>
        /// <item>
        /// Button:<br/>
        /// Action triggered by a button press or release (e.g., a key or gamepad button).<br/>
        /// Calls the following phases:<br/>
        /// - started: When a button is pressed.<br/>
        /// - performed: When the button reaches its activation threshold (default: full press).<br/>
        /// - canceled: When the button is released.
        /// </item>
        /// <item>
        /// PassThrough:<br/>
        /// Action that directly passes input without state or context management (useful for continuous input or multiple simultaneous inputs, e.g., multiple joystick movements).<br/>
        /// Only calls the performed phase on every input update.<br/>
        /// Does not handle started or canceled phases, as there is no state tracking.
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="inputs"></param>
        /// <param name="control"></param>
        /// <param name="actionType"></param>
        /// <returns></returns>
        public bool TryToAddInput(List<Input> inputs, IInputSystem inputSystem)
        {
            if (!isActive) { return false; }

            bool succeeded = TryToInstantiateOrGetInput(out Input input, inputSystem, out _);
            if (!succeeded) { return false; }

            if (!inputs.Contains(input))
            {
                inputs.Add(input);
            }

            return true;
        }
    }
}