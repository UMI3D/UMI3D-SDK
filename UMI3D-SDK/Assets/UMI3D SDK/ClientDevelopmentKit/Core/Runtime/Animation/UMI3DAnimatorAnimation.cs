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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using umi3d.common;
using umi3d.common.utils;
using UnityEngine;

namespace umi3d.cdk
{
    /// <summary>
    /// Animation played on an <see cref="Animator"/> component. The animation is typically inside a state
    /// of the Mecanima Animator.
    /// </summary>
    public class UMI3DAnimatorAnimation : UMI3DAbstractAnimation
    {
        private UMI3DVersion.VersionCompatibility _version = new UMI3DVersion.VersionCompatibility("2.6", "*");
        public override UMI3DVersion.VersionCompatibility version => _version;

        #region Fields

        /// <summary>
        /// Get an <see cref="UMI3DAnimatorAnimation"/> by its UMI3D id.
        /// </summary>
        /// <param name="id">UMI3D id</param>
        /// <returns></returns>
        [Obsolete("Use EnvironmentLoader.Instance.GetEntityObject<UMI3DAnimatorAnimation>() instead.")]
        public static new UMI3DAnimatorAnimation Get(ulong id) => UMI3DAbstractAnimation.Get(id) as UMI3DAnimatorAnimation;
        /// <summary>
        /// DTO local copy.
        /// </summary>
        public new UMI3DAnimatorAnimationDto dto { get => base.dto as UMI3DAnimatorAnimationDto; protected set => base.dto = value; }

        /// <summary>
        /// Is the animation started?
        /// </summary>
        /// In the started state, the animation could be at any state of progress except at 0.
        private bool started = false;

        /// <summary>
        /// Last pause time in ms.
        /// </summary>
        private float lastPauseTime = 0;

        /// <summary>
        /// Is the animation started but was paused in the meanwhile?
        /// </summary>
        public bool IsPaused { get; protected set; }

        /// <summary>
        /// Node which has <see cref="animator"/>
        /// </summary>
        private UMI3DNodeInstance node;

        /// <summary>
        /// Animator used for animation.
        /// </summary>
        /// This animator could be shared by several <see cref="UMI3DAnimatorAnimation"/> as each animation
        /// corresponds to a state of the animator.
        protected Animator animator { get; private set; }

        #endregion Fields

        /// <summary>
        /// Length of the animation in ms.
        /// </summary>
        /// <returns></returns>
        public float Duration
        {
            get
            {
                if (animator == null)
                    return 0.0f;
                return animator.GetCurrentAnimatorStateInfo(0).length * 1_000;
            }
        }

        #region DI
        private ICoroutineService coroutineService;
        private readonly IUnityMainThreadDispatcher unityMainThreadDispatcher;

        public UMI3DAnimatorAnimation(UMI3DAnimatorAnimationDto dto) : base(dto)
        {
            coroutineService = CoroutineManager.Instance;
            unityMainThreadDispatcher = UnityMainThreadDispatcherManager.Instance;
        }

        public UMI3DAnimatorAnimation(UMI3DAnimatorAnimationDto dto,
                                      ICoroutineService coroutineService,
                                      IUnityMainThreadDispatcher unityMainThreadDispatcher) : base(dto)
        {
            this.coroutineService = coroutineService;
            this.unityMainThreadDispatcher = unityMainThreadDispatcher;
        }
        #endregion DI

        /// <inheritdoc/>
        public override bool IsPlaying() => started;

        /// <inheritdoc/>
        public override void Init()
        {
            base.Init();

            UMI3DEnvironmentLoader.WaitForAnEntityToBeLoaded(dto.nodeId,
                (n) =>
                {
                    unityMainThreadDispatcher.Enqueue(() =>
                    {
                        SetNode(dto.nodeId);

                        foreach (var entry in dto.parameters)
                        {
                            UMI3DAnimatorParameterDto param = UMI3DAnimatorParameter.Create(entry.Value);
                            ApplyParameter(entry.Key, param);
                        }

                        animator = (n as UMI3DNodeInstance)?.gameObject.GetComponentInChildren<Animator>();
                        if (animator != null)
                            animator.Rebind();
                    });
                }
            );
        }

        /// <inheritdoc/>
        public override float GetProgress()
        {
            if (!started)
                return 0;
            if (IsPaused)
                return lastPauseTime / Duration;

            var progress = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            if (progress >= 1)
                return 1;
            else
                return progress;
        }

        /// <inheritdoc/>
        public override void Start()
        {
            Start(0);
        }

        /// <inheritdoc/>
        public override void Start(float atTime)
        {
            if (started) return;
            Play(atTime);
            started = true;
        }

        /// <summary>
        /// Resume or start the animation.
        /// </summary>
        public void Play()
        {
            if (!started)
                Start();
            Play(IsPaused ? lastPauseTime : 0);
        }

        /// <summary>
        /// Resume the animation at a precise time.
        /// </summary>
        /// <param name="atTime">Resume time in ms.</param>
        public void Play(float atTime)
        {
            var nTime = dto.normalizedTime + atTime / Duration;

            if (animator == null)
            {
                UMI3DLogger.LogWarning($"No animator on node {node}. Animation paused.", DebugScope.CDK | DebugScope.Animation);
                IsPaused = true;
                return;
            }
            
            if (dto.stateName != string.Empty) // an empty state name corresponds to a self-caring animator.
            {
                animator.Play(dto.stateName, layer: 0, normalizedTime: nTime);
                trackingAnimationCoroutine ??= coroutineService.AttachCoroutine(TrackEnd());
            }
                
            IsPaused = false;
            UMI3DClientServer.Instance.OnLeavingEnvironment.AddListener(StopTracking);
        }

        /// <summary>
        /// Pause the animation, registering the current playing state.
        /// </summary>
        public void Pause()
        {
            lastPauseTime = GetProgress() * Duration;
            IsPaused = true;
            if (trackingAnimationCoroutine != null)
                coroutineService.DetachCoroutine(trackingAnimationCoroutine);
            trackingAnimationCoroutine = null;
            animator.Play(dto.stateName, layer: 0, normalizedTime: 1);
        }

        /// <inheritdoc/>
        public override void Stop()
        {
            IsPaused = false;
            OnEnd();
        }

        /// <inheritdoc/>
        public override void OnEnd()
        {
            started = false;

            if (trackingAnimationCoroutine != null)
            {
                coroutineService.DetachCoroutine(trackingAnimationCoroutine);
                trackingAnimationCoroutine = null;
            }

            base.OnEnd();
        }

        private Coroutine trackingAnimationCoroutine;

        /// <summary>
        /// Coroutine to track an animator animation end.
        /// </summary>
        /// This coroutine is useful since there is easy way to get 
        /// the end of an animation (a State) in an Animator componoment through the Mecanim API. <br/>
        /// Other way could involve an AnimationStateEvent to add on animation at design
        /// <returns></returns>
        private IEnumerator TrackEnd()
        {
            while (animator != null && GetProgress() < 1)
                yield return null;

            OnEnd();
        }

        private void StopTracking()
        {
            if (trackingAnimationCoroutine is not null)
                coroutineService.DetachCoroutine(trackingAnimationCoroutine);
            UMI3DClientServer.Instance.OnLeavingEnvironment.RemoveListener(StopTracking);
        }

        /// <inheritdoc/>
        public override void SetProgress(long time)
        {
            if (!started)
                return;
            if (IsPaused)
                lastPauseTime = time;
            else
                Play(time);
        }

        #region Setter

        /// <inheritdoc/>
        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyData value)
        {
            if (await base.SetUMI3DProperty(value)) return true;
            switch (value.property.property)
            {
                case UMI3DPropertyKeys.AnimationNodeId:
                    dto.nodeId = (ulong)(long)value.property.value;
                    SetNode(dto.nodeId);
                    break;

                case UMI3DPropertyKeys.AnimationStateName:
                    dto.stateName = (string)value.property.value;
                    break;

                case UMI3DPropertyKeys.AnimationAnimatorNormalizedTime:
                    dto.normalizedTime = (float)value.property.value;
                    break;

                case UMI3DPropertyKeys.AnimationAnimatorParameters:
                    switch (value.property)
                    {
                        case SetEntityDictionaryAddPropertyDto setAddProperty:
                            if (setAddProperty.key is string paramName && setAddProperty.value is UMI3DAnimatorParameterDto paramDto)
                                ApplyParameter(paramName, paramDto);
                            break;

                        case SetEntityPropertyDto setEntity:
                            if (setEntity.value is Dictionary<object, object> parametersDic)
                            {
                                foreach (var entry in parametersDic)
                                {
                                    ApplyParameter((string)entry.Key, (UMI3DAnimatorParameterDto)entry.Value);
                                }
                            }
                            break;

                        default:
                            break;
                    }
                    break;

                default:
                    return false;
            }

            return true;
        }


        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyContainerData value)
        {
            if (await base.SetUMI3DProperty(value)) return true;
            switch (value.propertyKey)
            {
                case UMI3DPropertyKeys.AnimationNodeId:
                    dto.nodeId = UMI3DSerializer.Read<uint>(value.container);
                    SetNode(dto.nodeId);
                    break;

                case UMI3DPropertyKeys.AnimationStateName:
                    dto.stateName = UMI3DSerializer.Read<string>(value.container);
                    break;

                case UMI3DPropertyKeys.AnimationAnimatorNormalizedTime:
                    dto.normalizedTime = UMI3DSerializer.Read<float>(value.container);
                    break;

                case UMI3DPropertyKeys.AnimationAnimatorParameters:
                    string key;
                    UMI3DAnimatorParameterDto parameter;
                    switch (value.operationId)
                    {
                        case UMI3DOperationKeys.SetEntityDictionnaryAddProperty:
                        case UMI3DOperationKeys.SetEntityDictionnaryProperty:
                            key = UMI3DSerializer.Read<string>(value.container);
                            parameter = UMI3DSerializer.Read<UMI3DAnimatorParameterDto>(value.container);
                            ApplyParameter(key, parameter);
                            break;

                        case UMI3DOperationKeys.SetEntityProperty:
                            var parameters = UMI3DSerializer.ReadDictionary<string, UMI3DAnimatorParameterDto>(value.container).Select(k => new KeyValuePair<string, object>(k.Key, k.Value));
                            foreach (var param in parameters)
                            {
                                ApplyParameter(param.Key, param.Value as UMI3DAnimatorParameterDto);
                            }
                            break;

                        case UMI3DOperationKeys.SetEntityDictionnaryRemoveProperty:
                            break;

                        default:
                            break;
                    }
                    break;

                default:
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Tries to apply a parameter to <see cref="animator"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parameterDto"></param>
        public void ApplyParameter(string name, UMI3DAnimatorParameterDto parameterDto)
        {
            if (animator == null)
                return;

            unityMainThreadDispatcher.Enqueue(() =>
            {
                UMI3DAnimatorParameterType type = (UMI3DAnimatorParameterType)parameterDto.type;
                switch (type)
                {
                    case UMI3DAnimatorParameterType.Bool:
                        var val = (bool)parameterDto.value;
                        if (val)
                        {
                            animator.SetTrigger(name);
                        }
                        animator.SetBool(name, val);
                        break;

                    case UMI3DAnimatorParameterType.Float:
                        animator.SetFloat(name, (float)parameterDto.value);
                        break;

                    case UMI3DAnimatorParameterType.Integer:
                        animator.SetInteger(name, (int)parameterDto.value);
                        break;

                    default:
                        break;
                }
            });
        }

        /// <summary>
        /// Setter for <see cref="node"/>.
        /// </summary>
        /// <param name="nodeId"></param>
        private void SetNode(ulong nodeId)
        {
            UMI3DEnvironmentLoader.WaitForAnEntityToBeLoaded(nodeId, (n) =>
            {
                node = n as UMI3DNodeInstance;
                if (node is not null)
                    animator = node.gameObject.GetComponentInChildren<Animator>();
                if (animator != null && dto.playing)
                    Start();

                MainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(debugClip);

            });
        }

        async void debugClip()
        {
            string name = null;
            while (true)
            {
                await UMI3DAsyncManager.Yield();
                try
                {
                    var m_CurrentClipInfo = animator.GetCurrentAnimatorClipInfo(0);
                    //Access the current length of the clip
                    if (m_CurrentClipInfo != null && m_CurrentClipInfo.Count() > 0)
                    {
                        var m_CurrentClipLength = m_CurrentClipInfo[0].clip.length;
                        //Access the Animation clip name
                        var m_ClipName = m_CurrentClipInfo[0].clip.name;

                        if (name != m_ClipName)
                        {
                            name = m_ClipName;
                        }
                    }
                }
                catch { }
            }
        }

        #endregion Setter

        /// <inheritdoc/>
        public override void Clear()
        {
        }
    }
}