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
    public sealed class ControllerManager 
    {
        #region Initialize

        static Lazy<ControllerManager> _default = new(() => new());
        public static ControllerManager @default => _default.Value;

        ControllerManager()
        {

        }

        #endregion

        List<Controller> _controllers = new();
        public ReadOnlyCollection<Controller> controllers => _controllers.AsReadOnly();

        /// <summary>
        /// Searches for an existing controller by its unique identifier. If a controller with the specified ID exists, 
        /// it is returned. Otherwise, a new controller is created, added to the internal list, and returned.<br/>
        /// <br/>
        /// <example>
        /// Given a controller ID, when calling this method, it will either return an existing controller or create a new one.<br/>
        /// <br/>
        /// <code>
        /// bool isNew = controllerManager.InstantiateOrGet(out Controller controller, "controller_1");
        /// if (isNew)
        /// {
        ///     Console.WriteLine("A new controller was created.");
        /// }
        /// else
        /// {
        ///     Console.WriteLine("An existing controller was returned.");
        /// }
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="controller">The output parameter that will hold the existing or newly created controller.</param>
        /// <param name="id">The unique identifier of the controller to search for or associate with a new controller.</param>
        /// <returns>
        /// A boolean value indicating whether a new controller was created (`true`) or an existing controller was found (`false`).
        /// </returns>
        public bool InstantiateOrGet(out Controller controller, string id)
        {
            controller = _controllers.Find(x => x.id == id);
            if (controller != null)
            {
                UnityEngine.Debug.Log($"[ControllerManager] Notice: controller for id: '{id}' already exist.");
                return false;
            }

            UnityEngine.Debug.Log($"[ControllerManager] Notice: controller for id: '{id}' created.");
            controller = new(id);
            _controllers.Add(controller);
            return true;
        }
    }
}