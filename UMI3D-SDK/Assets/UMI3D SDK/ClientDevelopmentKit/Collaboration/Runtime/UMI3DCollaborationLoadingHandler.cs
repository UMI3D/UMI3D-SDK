/*
Copyright 2019 - 2023 Inetum

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

using UnityEngine;

namespace umi3d.cdk.collaboration
{
    /// <summary>
    /// <inheritdoc/> 
    /// Specialized for collaborative browsers.
    /// </summary>
    public class UMI3DCollaborationLoadingHandler : UMI3DLoadingHandler
    {
        #region UserTracking
        private UMI3DCollaborationClientUserTracking userTrackingService;
        #endregion UserTracking

        #region Emotes
        [Header("Emotes")]
        [SerializeField, Tooltip("Is the browser supporting emotes?")]
        public bool areEmotesSupported;
        private EmoteManager emoteService;

        /// <summary>
        /// Icon displayed by default when no icon is defined for an emote.
        /// </summary>
        [SerializeField, Tooltip("Icon displayed by default when no icon is defined for an emote.")]
        private Sprite defaultEmoteIcon;

        #endregion Emotes

        protected override void Awake()
        {
            // LOADING SERVICE
            environmentLoaderService = UMI3DCollaborationEnvironmentLoader.Instance;

            // USER TRACKING SERVICE
            userTrackingService = UMI3DCollaborationClientUserTracking.Instance;

            // EMOTES SERVICE
            if (areEmotesSupported)
            {
                emoteService = EmoteManager.Instance;
                emoteService.Initialize(defaultEmoteIcon);
            }

            base.Awake();
        }
    }
}