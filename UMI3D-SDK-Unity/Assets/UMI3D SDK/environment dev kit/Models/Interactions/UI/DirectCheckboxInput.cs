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
    /// Checkbox UI Input.
    /// </summary>
    public class DirectCheckboxInput : DirectInput
    {
        /// <summary>
        /// Default value.
        /// </summary>
        public bool m_default = true;
        
        public class CheckboxListener : UnityEvent<UMI3DUser, bool> { }

        /// <summary>
        /// Event raised on value change.
        /// </summary>
        public CheckboxListener onChange = new CheckboxListener();

        /// <summary>
        /// automatically check if the object has been updated in the editor
        /// </summary>
        protected override void checkForUpdates()
        {
            base.checkForUpdates();
            if (input.DefaultValue == null || m_default != bool.Parse(input.DefaultValue))
                PropertiesHandler.NotifyUpdate();
        }

        /// <summary>
        /// Update input properties.
        /// </summary>
        protected override void syncProperties()
        {
            base.syncProperties();
            input.inputType = InputType.Checkbox;
            input.DefaultValue = m_default.ToString();
        }

        /// <summary>
        /// Called by a user on interaction.
        /// </summary>
        /// <param name="user">User interacting</param>
        /// <param name="evt">Interaction data</param>
        public override void OnUserInteraction(UMI3DUser user, JSONObject evt)
        {
            if (evt.IsBool)
            {
                var value = evt.b;
                m_default = value;
                onChange.Invoke(user, m_default);
            }
        }

    }
}
