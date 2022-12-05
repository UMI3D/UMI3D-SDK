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

        /// <summary>
        /// Starts an emote on another user's avatar in the scene.
        /// </summary>
        /// <param name="emoteId">Emote to start UMI3D Id.</param>
        /// <param name="userId">Id of the user to start the emote for.</param>
        /// Don't use this for your own avatar's emotes.
        public void PlayEmoteOnOtherAvatar(ulong emoteId, ulong userId)
        {
            var otherUserAvatar = embodimentDict[userId];
            var animators = otherUserAvatar.GetComponentsInChildren<Animator>();
            var emoteAnimator = animators.Where(animator => animator.runtimeAnimatorController != null).FirstOrDefault();

            if (emoteAnimator == null || emoteConfig == null)
                return;
            var emoteToPlay = emoteConfig.emotes.Find(x => x.id == emoteId);
            if (emoteCoroutineDict.ContainsKey(userId) && emoteCoroutineDict[userId] != null) //an Emote is playing, need to interrupt it
            {
                StopCoroutine(emoteCoroutineDict[userId]);
                emoteAnimator.enabled = false;
                emoteAnimator.Update(0);
            }

            var coroutine = StartCoroutine(PlayEmote(emoteAnimator, emoteToPlay));
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
        protected IEnumerator PlayEmote(Animator animator, UMI3DEmoteDto emote)
        {
            animator.enabled = true;
            animator.Play(emote.stateName);
            animator.Update(0);
            yield return new WaitWhile(() =>
            {
                if (animator == null) return false;  // heppens when a user leaves the scene when playing an emote
                return animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1;
            });
            if (animator == null) // heppens when a user leaves the scene when playing an emote
                yield break;
            animator.enabled = false;
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

            if (emoteCoroutineDict.ContainsKey(userId)
                && emoteCoroutineDict[userId] != null)
            {
                StopCoroutine(emoteCoroutineDict[userId]);
                emoteCoroutineDict[userId] = null;

                if (UMI3DClientUserTracking.Instance.embodimentDict.TryGetValue(userId, out UserAvatar otherUserAvatar))
                {
                    if (otherUserAvatar == null) //the embodiment system lost the avatar
                        return;

                    var animators = otherUserAvatar.GetComponentsInChildren<Animator>();
                    if (animators == null) //no animator to desactive found on the avatar
                        return;

                    var emoteAnimator = animators.Where(animator => animator.runtimeAnimatorController != null).FirstOrDefault();
                    if (emoteAnimator == null) //no animator to desactive found on the avatar
                        return;

                    var emoteToStop = emoteConfig.emotes.Find(x => x.id == emoteId);
                    if (emoteAnimator == null) //the emote to stop doesn't exist
                        throw new Umi3dException("The emote to stop does not exist in emote configuration file.");

                    emoteAnimator.Update(0);
                    emoteAnimator.enabled = false;
                }
            }
        }
    }
}
