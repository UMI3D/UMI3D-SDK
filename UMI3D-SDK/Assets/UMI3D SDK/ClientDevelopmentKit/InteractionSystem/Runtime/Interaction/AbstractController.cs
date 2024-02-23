/*
Copyright 2019 - 2024 Inetum

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
using System.Linq;
using umi3d.common.interaction;
using UnityEngine;

namespace umi3d.cdk.interaction
{
    /// <summary>
    /// A controller is a set of inputs.<br/>
    /// <br/>
    /// 
    /// <example>
    /// For example: The following device can be seen as a controller.
    /// <list type="bullet">
    /// <item>A VR controller.</item>
    /// <item>A keyboard and a mouse.</item>
    /// </list>
    /// </example>
    /// 
    /// You have as many controller as you have of selector.<br/>
    /// <example>
    /// For example: A VR headset has 2 controllers with laser to select. A computer as 1 mouse to select (mouse and trackpad can be seen as the same input).
    /// </example>
    /// </summary>
    [System.Serializable]
    public abstract class AbstractController : MonoBehaviour
    {
        public UMI3DInputManager inputManager;
        public UMI3DToolManager toolManager;
        public ProjectionManager projectionManager;

        private void Awake()
        {
            projectionManager.Init(
                this, 
                inputManager, 
                toolManager
            );
        }


        #region properties

        /// <summary>
        /// Controller's inputs.
        /// </summary>
        public abstract List<AbstractUMI3DInput> inputs { get; }

        /// <summary>
        /// Inputs associated to a given tool (keys are tools' ids).
        /// </summary>
        protected Dictionary<ulong, AbstractUMI3DInput[]> associatedInputs = new Dictionary<ulong, AbstractUMI3DInput[]>();

        #endregion

        #region interface

        /// <summary>
        /// Clear all menus and the projected tools
        /// </summary>
        public abstract void Clear();

        #endregion
    }
}