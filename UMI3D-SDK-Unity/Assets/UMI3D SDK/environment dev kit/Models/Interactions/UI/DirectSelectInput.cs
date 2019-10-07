/*
Copyright 2019 Gfi Informatique

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
using System.Collections;
using System.Collections.Generic;
using umi3d.common;
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.edk
{
    /// <summary>
    /// String options selection from UI.
    /// </summary>
    public class DirectSelectInput : DirectInput
    {
        /// <summary>
        /// Default value.
        /// </summary>
        public string m_default = null;

        /// <summary>
        /// Availables options.
        /// </summary>
        public List<string> m_options = new List<string>();
        public class OnChangeListener : UnityEvent<UMI3DUser, string> { }
        /// <summary>
        /// Event raised on value change.
        /// </summary>
        public OnChangeListener onChange = new OnChangeListener();

        /// <summary>
        /// Initialize interaction's properties.
        /// </summary>
        protected override void initDefinition()
        {
            input.ValuesEnum = new List<string>();
            base.initDefinition();
        }

        /// <summary>
        /// automatically check if the object has been updated in the editor
        /// </summary>
        protected override void checkForUpdates()
        {
            base.checkForUpdates();
            bool inputUpdated = input.DefaultValue == null ||
                input.ValuesEnum == null ||
                input.ValuesEnum.Count != m_options.Count ||
                m_default != input.DefaultValue;
            foreach (var s in input.ValuesEnum)
                inputUpdated = inputUpdated || (!m_options.Contains(s));
            if (inputUpdated)
                PropertiesHandler.NotifyUpdate();
        }

        /// <summary>
        /// Update input properties.
        /// </summary>
        protected override void syncProperties()
        {
            base.syncProperties();
            input.inputType = InputType.Select;
            input.DefaultValue = m_default;
            input.ValuesEnum.Clear();
            foreach (string s in m_options)
                input.ValuesEnum.Add(s);
        }

        /// <summary>
        /// Called by a user on interaction.
        /// </summary>
        /// <param name="user">User interacting</param>
        /// <param name="evt">Interaction data</param>
        public override void OnUserInteraction(UMI3DUser user, JSONObject evt)
        {
            if (evt.IsString)
            {
                var value = evt.str;
                m_default = value;
                onChange.Invoke(user, m_default);
            }
        }

    }
}