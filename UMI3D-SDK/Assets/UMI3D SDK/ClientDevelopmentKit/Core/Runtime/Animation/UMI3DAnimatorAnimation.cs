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

using System;
using System.Collections;
using System.Threading.Tasks;
using umi3d.common;
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

        /// <summary>
        /// Get an <see cref="UMI3DAnimatorAnimation"/> by its UMI3D id.
        /// </summary>
        /// <param name="id">UMI3D id</param>
        /// <returns></returns>
        [Obsolete("Use EnvironmentLoader.Instance.GetEntityObject<UMI3DAnimatorAnimation>() instead.")]
        public new static UMI3DAnimatorAnimation Get(ulong id)
        { return UMI3DAbstractAnimation.Get(id) as UMI3DAnimatorAnimation; }

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
        /// Animator used for animation.
        /// </summary>
        /// This animator could be shared by several <see cref="UMI3DAnimatorAnimation"/> as each animation
        /// corresponds to a state of the animator.
        private Animator animator;

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

        public UMI3DAnimatorAnimation(UMI3DAnimatorAnimationDto dto) : base(dto)
        {
        }

        /// <inheritdoc/>
        public override bool IsPlaying() => started;

        /// <inheritdoc/>
        public override void Init()
        {
            base.Init();

            UMI3DEnvironmentLoader.WaitForAnEntityToBeLoaded(dto.nodeId,
                (n) =>
                {
                    MainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        animator = (n as UMI3DNodeInstance)?.gameObject.GetComponentInChildren<Animator>();
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
                return lastPauseTime;

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
            var nTime = atTime / Duration;

            animator.Play(dto.stateName, layer: 0, normalizedTime: nTime);
            IsPaused = false;
            trackingAnimationCoroutine ??= UMI3DLoadingHandler.Instance.AttachCoroutine(TrackEnd());
        }

        /// <summary>
        /// Pause the animation, registering the current playing state.
        /// </summary>
        public void Pause()
        {
            lastPauseTime = GetProgress() * Duration;
            IsPaused = true;
            UMI3DLoadingHandler.Instance.DettachCoroutine(trackingAnimationCoroutine);
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
                trackingAnimationCoroutine = null;
            base.OnEnd();
        }

        /// <inheritdoc/>
        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyData value)
        {
            if (await base.SetUMI3DProperty(value)) return true;
            switch (value.property.property)
            {
                case UMI3DPropertyKeys.AnimationNodeId:
                    dto.nodeId = (ulong)(long)value.property.value;
                    break;

                case UMI3DPropertyKeys.AnimationStateName:
                    dto.stateName = (string)value.property.value;
                    break;

                default:
                    return false;
            }

            return true;
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
            while (GetProgress() < 1)
                yield return null;

            OnEnd();
        }
    }
}