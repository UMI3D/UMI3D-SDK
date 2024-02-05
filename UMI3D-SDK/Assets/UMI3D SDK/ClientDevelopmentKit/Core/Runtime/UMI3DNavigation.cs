/*
Copyright 2019 - 2021 Inetum

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
using System.Linq;
using umi3d.common;

namespace umi3d.cdk
{
    /// <summary>
    /// Navigation manager for user displacement in the environment. 
    /// </summary>
    public class UMI3DNavigation : inetum.unityUtils.SingleBehaviour<UMI3DNavigation>
    {
        /// <summary>
        /// Current navigation system.
        /// </summary>
        public AbstractNavigation currentNav { get; protected set; } = null;
        /// <summary>
        /// Different available navigation system.
        /// </summary>
        public List<AbstractNavigation> navigations;

        public delegate void OnEmbarkVehicleDelegate(ulong vehicleId);

        public static event OnEmbarkVehicleDelegate onUpdateFrameDelegate;

        // Start is called before the first frame update
        private void Start()
        {
            currentNav = navigations.FirstOrDefault();
            currentNav.Activate();
        }

        public static void SetFrame(FrameRequestDto frameRequest)
        {
            UnityEngine.Debug.LogError("Need to handle rescaling");
            if (Exists && Instance.currentNav != null)
            {
                onUpdateFrameDelegate?.Invoke(frameRequest.FrameId);

                Instance.currentNav.UpdateFrame(frameRequest);

                var fConfirmation = new FrameConfirmationDto()
                {
                    userId = UMI3DClientServer.Instance.GetUserId()
                };

                UMI3DClientServer.SendRequest(fConfirmation, true);
            }
        }

        /// <summary>
        /// Move the user acording to a <see cref="NavigateDto"/>.
        /// </summary>
        /// <param name="dto"></param>
        public static IEnumerator Navigate(NavigateDto dto)
        {
            if (Exists && Instance.currentNav != null)
            {
                switch (dto)
                {
                    case TeleportDto teleportDto:
                        Instance.currentNav.Teleport(teleportDto);
                        break;
                    default:
                        Instance.currentNav.Navigate(dto);
                        break;
                }
            }

            yield break;
        }
    }
}