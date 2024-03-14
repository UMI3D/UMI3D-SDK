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
using System;
using System.Collections.Generic;
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
    public class UMI3DController
    {
        debug.UMI3DLogger logger = new();

        public AbstractControllerData_SO controllerData_SO;

        [HideInInspector] public UMI3DControlManager controlManager;
        [HideInInspector] public UMI3DToolManager toolManager;
        [HideInInspector] public UMI3DProjectionManager projectionManager;
        [HideInInspector] public MonoBehaviour context;
        public IControllerDelegate controllerDelegate;

        public readonly static List<UMI3DController> activeControllers = new();
        /// <summary>
        /// The last controller that received an input.
        /// </summary>
        public static UMI3DController lastControllerUsed;
        /// <summary>
        /// The last controller that hovered a tool.
        /// </summary>
        public static UMI3DController lastControllerHovering;

        /// <summary>
        /// Init this controller.<br/>
        /// <br/>
        /// Warning: Delegate must be set before calling this method.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="controlManager"></param>
        public void Init(
            MonoBehaviour context, 
            UMI3DControlManager controlManager,
            UMI3DToolManager toolManager,
            UMI3DProjectionManager projectionManager
        )
        {
            logger.MainContext = context;
            logger.MainTag = nameof(UMI3DController);
            this.context = context;
            this.controlManager = controlManager;
            this.toolManager = toolManager;
            this.projectionManager = projectionManager;

            logger.Assert(controllerDelegate != null, $"{nameof(controllerDelegate)} is null");
        }

        public Transform Transform
        {
            get
            {
                return controllerDelegate.Transform;
            }
        }
        public uint BoneType
        {
            get
            {
                return controllerDelegate.BoneType;
            }
        }
        public Transform BoneTransform
        {
            get
            {
                return controllerDelegate.BoneTransform;
            }
        }
        /// <summary>
        /// Transform reference to track translation and rotation.
        /// </summary>
        public Transform ManipulationTransform
        {
            get
            {
                return controllerDelegate.ManipulationTransform;
            }
        }

        public void Enable()
        {
            activeControllers.Add(this);
        }

        public void Disable()
        {
            activeControllers.Remove(this);
        }

        /// <summary>
        /// Clear all menus and the projected tools
        /// </summary>
        public virtual void Clear()
        {
            //projectionManager.Release(null, null);

            //toolManager.ReleaseTool(UMI3DGlobalID.EnvironmentId, toolManager.toolDelegate.Tool.id);
        }
    }
}