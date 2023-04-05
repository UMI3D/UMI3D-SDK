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
using umi3d.cdk.userCapture;
using umi3d.common;
using umi3d.common.userCapture;
using UnityEngine;

namespace umi3d.cdk.collaboration
{
    /// <summary>
    /// Manager for client user tracking event for other users in a collaborative context.
    /// </summary>
    public class UMI3DCollaborationClientUserTracking : UMI3DClientUserTracking
    {
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
            UMI3DCollaborationClientServer.Instance.OnLeaving.AddListener(() => emotesAnimators.Clear());
        }

        /// <inheritdoc/>
        protected override IEnumerator DispatchTracking()
        {
            while (sendTracking)
            {
                if (targetTrackingFPS > 0)
                {
                    if (UMI3DCollaborationClientServer.Connected())
                    {
                        BonesIterator();
                        if (LastFrameDto != null)
                        {
                            UMI3DCollaborationClientServer.SendTracking(LastFrameDto);
                        }
                    }
                    yield return new WaitForSeconds(1f / targetTrackingFPS);
                }
                else
                {
                    yield return new WaitUntil(() => targetTrackingFPS > 0 || !sendTracking);
                }
            }
        }

        /// <inheritdoc/>
        protected override IEnumerator DispatchCamera()
        {
            yield return new WaitUntil(() => UMI3DCollaborationClientServer.Connected());

            base.DispatchCamera();
        }

        /// <summary>
        /// Collection of emotes' coroutine related to playing for each user.
        /// </summary>
        private Dictionary<ulong, Coroutine> emoteCoroutineDict = new Dictionary<ulong, Coroutine>();

        private Dictionary<ulong, Animator> emotesAnimators = new();

        /// <summary>
        /// Starts an emote on another user's avatar in the scene.
        /// </summary>
        /// <param name="emoteId">Emote to start UMI3D Id.</param>
        /// <param name="userId">Id of the user to start the emote for.</param>
        /// Don't use this for your own avatar's emotes.
        public void PlayEmoteOnOtherAvatar(ulong emoteId, ulong userId)
        {
            if (emoteConfig == null)
                return;

            if (!emotesAnimators.ContainsKey(userId) || emotesAnimators[userId] == null)
            {
                var emoteAnimator = UnpackEmoteAnimator(userId);
                if (emoteAnimator == null) // no support for bundled animations on avatar
                    return;
                emotesAnimators[userId] = emoteAnimator;
            } 

            var emoteToPlay = emoteConfig.emotes.Find(x => x.id == emoteId);
            if (emoteCoroutineDict.ContainsKey(userId)) //an Emote is playing, need to interrupt it
            {
                if (emoteCoroutineDict[userId] != null)
                    StopCoroutine(emoteCoroutineDict[userId]);
                emoteCoroutineDict[userId] = null;
            }

            var coroutine = StartCoroutine(PlayEmote(userId, emotesAnimators[userId], emoteToPlay));
            if (emoteCoroutineDict.ContainsKey(userId))
                emoteCoroutineDict[userId] = coroutine;
            else
                emoteCoroutineDict.Add(userId, coroutine);
        }

        /// <summary>
        /// Plays an emote on an animator.
        /// </summary>
        /// <param name="animator">Animator to play the emote on.</param>
        /// <param name="emote">Emote to play.</param>
        /// <returns></returns>
        protected IEnumerator PlayEmote(ulong userId, Animator animator, UMI3DEmoteDto emote)
        {
            embodimentDict[userId].ForceDisablingBinding = true;

            animator.enabled = true;
            animator.Play(emote.stateName, layer: 0, 0f);
            animator.Update(0);

            float startTime = Time.time;
            float expectedLength = default;
            yield return new WaitUntil(() =>
            {
                var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if (expectedLength == default && stateInfo.IsName(emote.stateName)) // the animation state is not transitionned to the played one directly
                {
                    expectedLength = stateInfo.length; // the right length is available only when the animation stateMachine is in the right state
                    startTime = Time.time; // compensate the delay when switching animation
                }
                if (expectedLength != default && Time.time - startTime > expectedLength) //we may be in another state but we went through all the one we intended to
                    return true;
                return false;
            }); // wait for emote end of animation


            if (animator == null) // heppens when a user leaves the scene when playing an emote
                yield break;

            animator.Play(emoteConfig.defaultStateName, layer: 0, 0f);
            animator.enabled = false;
            animator.Update(0);

            emoteCoroutineDict.Remove(userId);
            embodimentDict[userId].ForceDisablingBinding = false;
        }

        /// <summary>
        /// Stops an emote on another user's avatar in the scene.
        /// </summary>
        /// <param name="emoteId">Emote to stop UMI3D Id.</param>
        /// <param name="userId">Id of the user to stop the emote for.</param>
        /// Don't use this for your own avatar's emotes.
        public void StopEmoteOnOtherAvatar(ulong emoteId, ulong userId)
        {
            if (emoteConfig == null) //no emote support in the scene
                return;

            if (!emoteCoroutineDict.ContainsKey(userId) // no support for bundled emotes
                || emoteCoroutineDict[userId] == null)
                return;
            
            StopCoroutine(emoteCoroutineDict[userId]);
            emoteCoroutineDict.Remove(userId);
                
            if (!embodimentDict.ContainsKey(userId))
                UMI3DLogger.LogWarning($"No embodiment for user {userId}", DebugScope.CDK);

            embodimentDict[userId].ForceDisablingBinding = false;

            var emoteToStop = emoteConfig.emotes.Find(x => x.id == emoteId);
            if (!emotesAnimators.ContainsKey(userId) || emotesAnimators[userId] == null) //the emote to stop doesn't exist
            {
                UMI3DLogger.LogWarning($"The user {userId} has no referenced emote animator", DebugScope.CDK);
                return;
            }

            emotesAnimators[userId].Play(emoteConfig.defaultStateName, layer: 0, 0f);
            emotesAnimators[userId].Update(0);
            emotesAnimators[userId].enabled = false;
        }
    }
}
