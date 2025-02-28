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

using inetum.unityUtils.observation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace umi3d.common.core.target
{
    public class TargetManager : ITargetDataDelegate
    {
        #region Initialization

        public static TargetManager @default => _default.Value;
        static readonly Lazy<TargetManager> _default = new(() => new TargetManager());
        TargetManager() {}

        #endregion

        public ITargetDataDelegate dataDelegate;

        public Delegates<ITargetDelegate> delegates;

        #region ITargetDataDelegate

        public OperatingSystem GetOperatingSystem()
        {
            return dataDelegate.GetOperatingSystem();
        }

        public Platform GetPlatform()
        {
            return dataDelegate.GetPlatform();
        }

        public IReadOnlyList<Plugin> GetActivePlugins()
        {
            return dataDelegate.GetActivePlugins();
        }
        public IReadOnlyList<Feature> GetActiveFeatures()
        {
            return dataDelegate.GetActiveFeatures();
        }

        public IReadOnlyList<ImmersiveType> GetAuthorizedImmersiveTypes()
        {
            return dataDelegate.GetAuthorizedImmersiveTypes();
        }
        public ImmersiveType GetCurrentImmersiveType()
        {
            return dataDelegate.GetCurrentImmersiveType();
        }

        public IReadOnlyList<Controller> GetAuthorizedControllers()
        {
            return dataDelegate.GetAuthorizedControllers();
        }
        public IReadOnlyList<Controller> GetCurrentControllers()
        {
            return dataDelegate.GetCurrentControllers();
        }

        public bool TrySetCurrentControllers(IEnumerable<Controller> controllers)
        {
            return dataDelegate.TrySetCurrentControllers(controllers);
        }

        public bool TrySetCurrentImmersiveType(ImmersiveType immersiveType)
        {
            return dataDelegate.TrySetCurrentImmersiveType(immersiveType);
        }

        #endregion

        public static bool isWindows => @default.GetOperatingSystem().Equals(OperatingSystem.windows);
        public static bool isAndroid => @default.GetOperatingSystem().Equals(OperatingSystem.android);

        /// <summary>
        /// Whether the target is currently considered as a PC.
        /// </summary>
        public static bool isPC => @default.GetCurrentControllers().Contains(Controller.keyboardAndMouse);
        /// <summary>
        /// Whether the target is currently considered as an immersive plateforme.
        /// </summary>
        public static bool isImmersive
        {
            get
            {
                IReadOnlyList<Controller> controllers = @default.GetCurrentControllers();
                if (controllers.Contains(Controller.vrController)) { return true; }
                else if (controllers.Contains(Controller.hand)) { return true; }
                else { return false; }
            }
        }
        /// <summary>
        /// Whether the target is currently considered as a mobile device.
        /// </summary>
        public static bool isMobile => @default.GetCurrentControllers().Contains(Controller.screen);

        /// <summary>
        /// Whether the target can be a PC with the right controllers.
        /// </summary>
        public static bool canBePC => @default.GetAuthorizedControllers().Contains(Controller.keyboardAndMouse);
        /// <summary>
        /// Whether the target can be an immersive device with the right controllers.
        /// </summary>
        public static bool canBeImmersive
        {
            get
            {
                IReadOnlyList<Controller> controllers = @default.GetAuthorizedControllers();
                if (controllers.Contains(Controller.vrController)) { return true; }
                else if (controllers.Contains(Controller.hand)) { return true; }
                else { return false; }
            }
        }
        /// <summary>
        /// Whether the target can be a mobile device with the right controllers.
        /// </summary>
        public static bool canBeMobile => @default.GetAuthorizedControllers().Contains(Controller.screen);

        /// <summary>
        /// Whether the target is currently considered as a VR device.
        /// </summary>
        public static bool isVR => @default.GetCurrentImmersiveType() == ImmersiveType.VR;
        /// <summary>
        /// Whether the target is currently considered as an AR device.
        /// </summary>
        public static bool isAR => @default.GetCurrentImmersiveType() == ImmersiveType.AR;

        /// <summary>
        /// Whether the target can be a VR device.
        /// </summary>
        public static bool canBeVR => @default.GetAuthorizedImmersiveTypes().Contains(ImmersiveType.VR);
        /// <summary>
        /// Whether the target can be an AR device.
        /// </summary>
        public static bool canBeAR => @default.GetAuthorizedImmersiveTypes().Contains(ImmersiveType.AR);
    }
}