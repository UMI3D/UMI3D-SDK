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

using inetum.unityUtils;

using System;
using System.Collections.Generic;
using System.Linq;

using umi3d.common;

namespace umi3d.edk.collaboration.emotes
{
    /// <summary>
    /// Dispatch emote requests by triggering the right animation
    /// </summary>
    public interface IEmoteDispatcher
    {
        /// <summary>
        /// Request the other browsers than the user's one to trigger/interrupt the emote of the corresponding id.
        /// </summary>
        /// <param name="emoteId">Emote to trigger UMI3D id.</param>
        /// <param name="sendingUser">Sending emote user.</param>
        /// <param name="trigger">True for triggering, false to interrupt.</param>
        void DispatchEmoteTrigger(UMI3DUser sendingUser, ulong emoteId, bool trigger);

        /// <summary>
        /// Emote configuration for the environment for each user.
        /// </summary>
        IDictionary<ulong, UMI3DEmotesConfig> EmotesConfigs { get; }

        bool AutoLoadAnimations { get; set; }

        event Action<(UMI3DUser sendingUser, ulong emoteId, bool isTrigger)> EmoteTriggered;
    }

    /// <summary>
    /// Dispatch emote requests by triggering the right animation
    /// </summary>
    public class EmoteDispatcher : Singleton<EmoteDispatcher>, IEmoteDispatcher
    {
        private const DebugScope DEBUG_SCOPE = DebugScope.EDK | DebugScope.Collaboration;

        #region Dependency Injection

        private readonly IUMI3DServer umi3dServerService;
        private readonly IUMI3DEnvironmentManager umi3dEnvironmentService;

        public EmoteDispatcher() : this(UMI3DEnvironment.Instance, UMI3DServer.Instance)
        {
        }

        public EmoteDispatcher(IUMI3DEnvironmentManager umi3dEnvironmentService, IUMI3DServer umi3dServer) : base()
        {
            this.umi3dEnvironmentService = umi3dEnvironmentService;
            this.umi3dServerService = umi3dServer;

            Init();
        }

        #endregion Dependency Injection

        /// <summary>
        /// Emote configuration for the environment for each user. Key is user id.
        /// </summary>
        public IDictionary<ulong, UMI3DEmotesConfig> EmotesConfigs { get; protected set; } = new Dictionary<ulong, UMI3DEmotesConfig>();

        public event Action<(UMI3DUser sendingUser, ulong emoteId, bool isTrigger)> EmoteTriggered;

        public bool AutoLoadAnimations { get; set; } = true;

        protected void Init()
        {
            umi3dServerService.OnUserActive.AddListener(LoadEmotesAnimations);
            umi3dServerService.OnUserLeave.AddListener(CleanEmotesAnimations);
            umi3dServerService.OnUserMissing.AddListener(CleanEmotesAnimations);
        }

        protected virtual void LoadEmotesAnimations(UMI3DUser user)
        {
            if (!AutoLoadAnimations)
                return;

            // retrieve animations for each emote and their loading operation
            var operations = (from otherUser in umi3dServerService.Users()
                              where otherUser.Id() != user.Id() && EmotesConfigs.ContainsKey(otherUser.Id())
                              from emote in EmotesConfigs[otherUser.Id()].IncludedEmotes
                              let anim = umi3dEnvironmentService._GetEntityInstance<UMI3DAbstractAnimation>(emote.Id())
                              where anim != null
                              let op = anim.GetLoadEntity(new() { user })
                              where op is not null
                              select op).ToList();

            if (operations.Count == 0)
                return;

            Transaction t = new() { reliable = true };
            t.AddIfNotNull(operations);
            umi3dServerService.DispatchTransaction(t);
        }

        private void CleanEmotesAnimations(UMI3DUser user)
        {
            if (!AutoLoadAnimations || !EmotesConfigs.ContainsKey(user.Id()))
                return;

            var operations = (from emote in EmotesConfigs[user.Id()].IncludedEmotes
                              let anim = umi3dEnvironmentService._GetEntityInstance<UMI3DAbstractAnimation>(emote.Id())
                              where anim != null
                              let op = anim.GetDeleteEntity()
                              where op is not null
                              select op).ToList();

            if (operations.Count == 0)
                return;

            Transaction t = new() { reliable = true };
            t.AddIfNotNull(operations);
            umi3dServerService.DispatchTransaction(t);
        }

        /// <summary>
        /// Request the other browsers than the user's one to trigger/interrupt the emote of the corresponding id.
        /// </summary>
        /// <param name="emoteId">Emote to trigger UMI3D id.</param>
        /// <param name="sendingUser">Sending emote user.</param>
        /// <param name="trigger">True for triggering, false to interrupt.</param>
        public void DispatchEmoteTrigger(UMI3DUser sendingUser, ulong emoteId, bool trigger)
        {
            // lots of null handling here, but emote request can be invalid in many ways.
            /* to be valid :
             * An emote config should exist for the user, 
             * it should the required emote,
             * the emote should be available for the user,
             * and the emote animation should not have been destroyed in the meanwhile.
            */

            ulong sendingUserId = sendingUser.Id();
            if (!EmotesConfigs.ContainsKey(sendingUserId))
            {
                UMI3DLogger.LogWarning($"Cannot {(trigger ? "start" : "stop")} emote for user {sendingUserId}. User does not have an emote config. User {sendingUserId} should not be able to request emotes.", DEBUG_SCOPE);
                return;
            }
            else if (EmotesConfigs[sendingUserId].IncludedEmotes.Count == 0)
            {
                UMI3DLogger.LogWarning($"Cannot {(trigger ? "start" : "stop")} emote for user {sendingUserId}. Emote config is empty of emotes. User {sendingUserId} should not be able to request emotes.", DEBUG_SCOPE);
                return;
            }

            UMI3DEmote emote = EmotesConfigs[sendingUserId].IncludedEmotes.Find(x => x.Id() == emoteId);

            if (emote == null)
            {
                UMI3DLogger.LogWarning($"Cannot {(trigger ? "start" : "stop")} emote for user {sendingUserId}. Emote {emoteId} does not exist. Emote request is invalid or emote config has been reset.", DEBUG_SCOPE);
                return;
            }

            if (!emote.Available.GetValue(sendingUser))
            {
                UMI3DLogger.LogWarning($"Cannot {(trigger ? "start" : "stop")} emote for user {sendingUserId}. Emote {emote.label} exists but is not available for them.", DEBUG_SCOPE);
                return;
            }

            ulong animationId = emote.AnimationId.GetValue(sendingUser);
            
            if (animationId != default) // when animationId is default, trigger emote without triggering an animation
            {
                IAnimation animation = umi3dEnvironmentService._GetEntityInstance<IAnimation>(animationId);

                if (animation == null)
                {
                    UMI3DLogger.LogWarning($"Cannot {(trigger ? "start" : "stop")} emote for user {sendingUserId}. Associated animation {animationId} for emote {emote.label} does not exist. Animation {animationId} may have been destroyed.", DEBUG_SCOPE);
                    return;
                }

                Transaction t = new (true);
                SetEntityProperty op = animation.objectPlaying.SetValue(trigger);
                if (t.AddIfNotNull(op))
                    umi3dServerService.DispatchTransaction(t);
            }

            EmoteTriggered?.Invoke((sendingUser, emoteId, trigger));
        }
    }
}