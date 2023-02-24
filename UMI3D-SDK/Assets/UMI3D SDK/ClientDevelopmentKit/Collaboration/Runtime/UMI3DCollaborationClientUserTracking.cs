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

using System;
using umi3d.cdk.userCapture;
using umi3d.common.collaboration;
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.cdk.collaboration
{
    /// <summary>
    /// Manager for client user tracking event for other users in a collaborative context.
    /// </summary>
    public class UMI3DCollaborationClientUserTracking : UMI3DClientUserTracking
    {
        public new static UMI3DCollaborationClientUserTracking Instance => Instance as UMI3DCollaborationClientUserTracking;

        /// <summary>
        /// Skeleton of the tracked user.
        /// </summary>
        public GameObject UnitSkeleton;

        private Coroutine forceSendTrackingCoroutine;

        private int lastNbUsers = 0;

        protected override void Start()
        {
            base.Start();
            avatarEvent.AddListener(UMI3DCollaborativeUserAvatar.SkeletonCreation);
            UMI3DCollaborationClientServer.Instance.OnRedirection.AddListener(() => embodimentDict.Clear());
            UMI3DCollaborationClientServer.Instance.OnReconnect.AddListener(() => embodimentDict.Clear());
        }

        public class EmotesConfigEvent : UnityEvent<UMI3DEmotesConfigDto> { };
        public class EmoteEvent : UnityEvent<UMI3DEmoteDto> { };

        /// <summary>
        /// Triggered when an EmoteConfig file have been loaded
        /// </summary>
        public event Action<UMI3DEmotesConfigDto> EmotesLoadedEvent;
        public void OnEmoteConfigLoaded(UMI3DEmotesConfigDto dto) => EmotesLoadedEvent?.Invoke(dto);

        /// <summary>
        /// Triggered when an emote changed on availability
        /// </summary>
        public event Action<UMI3DEmoteDto> EmoteUpdatedEvent;
        public void OnEmoteUpdated(UMI3DEmoteDto dto) => EmoteUpdatedEvent?.Invoke(dto);

        /// <summary>
        /// Emote configuration on the server, with al available emotes for the user.
        /// </summary>
        protected UMI3DEmotesConfigDto emoteConfig;

        ///// <inheritdoc/>
        //protected override IEnumerator DispatchTracking()
        //{
        //    while (sendTracking)
        //    {
        //        if (targetTrackingFPS > 0)
        //        {
        //            if (UMI3DCollaborationClientServer.Connected())
        //            {
        //                BonesIterator();
        //                if (LastFrameDto != null)
        //                {
        //                    UMI3DCollaborationClientServer.SendTracking(LastFrameDto);
        //                }
        //            }
        //            yield return new WaitForSeconds(1f / targetTrackingFPS);
        //        }
        //        else
        //        {
        //            yield return new WaitUntil(() => targetTrackingFPS > 0 || !sendTracking);
        //        }
        //    }
        //}

        /// <inheritdoc/>
        //protected override IEnumerator DispatchCamera()
        //{
        //    yield return new WaitUntil(() => UMI3DCollaborationClientServer.Connected());

        //    base.DispatchCamera();
        //}

    }
}
