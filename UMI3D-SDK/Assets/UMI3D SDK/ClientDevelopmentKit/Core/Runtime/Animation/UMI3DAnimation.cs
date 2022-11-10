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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    /// <summary>
    /// Animation composed of a chain of severam animations
    /// </summary>
    public class UMI3DAnimation : UMI3DAbstractAnimation
    {
        private const DebugScope scope = DebugScope.CDK | DebugScope.Core | DebugScope.Loading;

        /// <summary>
        /// Get an animation by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static new UMI3DAnimation Get(ulong id) { return UMI3DAbstractAnimation.Get(id) as UMI3DAnimation; }
        /// <summary>
        /// DTO local copy.
        /// </summary>
        protected new UMI3DAnimationDto dto { get => base.dto as UMI3DAnimationDto; set => base.dto = value; }

        private readonly List<Coroutine> Coroutines = new List<Coroutine>();
        private Coroutine PlayingCoroutines;
        private float progress;
        private bool started = false;

        public UMI3DAnimation(UMI3DAnimationDto dto) : base(dto)
        {

        }

        /// <inheritdoc/>
        public override float GetProgress()
        {
            return progress;
        }

        /// <inheritdoc/>
        public override void Start()
        {
            if (started) return;
            progress = 0;
            if (PlayingCoroutines != null) UMI3DAnimationManager.StopCoroutine(PlayingCoroutines);
            foreach (UMI3DAnimationDto.AnimationChainDto chain in dto.animationChain)
            {
                float p = GetProgress();
                if (p < chain.startOnProgress)
                    Coroutines.Add(UMI3DAnimationManager.StartCoroutine(WaitForProgress(chain.startOnProgress, () => { UMI3DAnimationManager.Start(chain.animationId); })));
                if (p == chain.startOnProgress)
                    UMI3DAnimationManager.Start(chain.animationId);
            }

            PlayingCoroutines = UMI3DAnimationManager.StartCoroutine(Playing(() => { OnEnd(); }));
        }

        /// <inheritdoc/>
        public override void Stop()
        {
            if (!started) return;
            if (PlayingCoroutines != null) UMI3DAnimationManager.StopCoroutine(PlayingCoroutines);
            foreach (UMI3DAnimationDto.AnimationChainDto chain in dto.animationChain)
                UMI3DAnimationManager.Stop(chain.animationId);
            foreach (Coroutine c in Coroutines)
                UMI3DAnimationManager.StopCoroutine(c);
        }

        /// <inheritdoc/>
        public override void OnEnd()
        {
            PlayingCoroutines = null;
            started = false;
            base.OnEnd();
        }

        private bool LaunchAnimation(float waitFor)
        {
            return dto.playing && GetProgress() >= waitFor;
        }

        private IEnumerator WaitForProgress(float waitFor, Action action)
        {
            yield return new WaitUntil(() => LaunchAnimation(waitFor));
            action.Invoke();
        }

        private IEnumerator Playing(Action action)
        {
            var fixUpdate = new WaitForFixedUpdate();
            while (GetProgress() < dto.duration)
            {
                yield return fixUpdate;
                if (dto.playing) progress += Time.fixedDeltaTime;
            }
            action.Invoke();
        }

        /// <inheritdoc/>
        public override bool SetUMI3DProperty(UMI3DEntityInstance entity, SetEntityPropertyDto property)
        {
            if (base.SetUMI3DProperty(entity, property)) return true;
            switch (property.property)
            {
                case UMI3DPropertyKeys.AnimationDuration:
                    dto.duration = (float)(Double)property.value;
                    break;
                case UMI3DPropertyKeys.AnimationChain:
                    return UpdateChain(property);
                default:
                    return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public override bool SetUMI3DProperty(UMI3DEntityInstance entity, uint operationId, uint propertyKey, ByteContainer container)
        {
            if (base.SetUMI3DProperty(entity, operationId, propertyKey, container)) return true;
            switch (propertyKey)
            {
                case UMI3DPropertyKeys.AnimationDuration:
                    dto.duration = UMI3DNetworkingHelper.Read<float>(container);
                    break;
                case UMI3DPropertyKeys.AnimationChain:
                    return UpdateChain(operationId, propertyKey, container);
                default:
                    return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public static bool ReadMyUMI3DProperty(ref object value, uint propertyKey, ByteContainer container)
        {
            switch (propertyKey)
            {
                case UMI3DPropertyKeys.AnimationDuration:
                    value = UMI3DNetworkingHelper.Read<float>(container);
                    break;
                case UMI3DPropertyKeys.AnimationChain:
                    return UpdateChain(ref value, propertyKey, container);
                default:
                    return false;
            }

            return true;
        }

        private bool UpdateChain(SetEntityPropertyDto property)
        {
            switch (property)
            {
                case SetEntityListAddPropertyDto add:
                    dto.animationChain.Add((UMI3DAnimationDto.AnimationChainDto)add.value);
                    break;
                case SetEntityListRemovePropertyDto rem:
                    dto.animationChain.RemoveAt((int)(Int64)rem.index);
                    break;
                case SetEntityListPropertyDto set:
                    dto.animationChain[(int)(Int64)set.index] = (UMI3DAnimationDto.AnimationChainDto)set.value;
                    break;
                default:
                    dto.animationChain = ((List<object>)property.value).Select(o => o as UMI3DAnimationDto.AnimationChainDto).ToList();
                    break;
            }
            return true;
        }

        private bool UpdateChain(uint operationId, uint propertyKey, ByteContainer container)
        {
            if (dto.animationChain == null)
                dto.animationChain = new List<UMI3DAnimationDto.AnimationChainDto>();
            UMI3DNetworkingHelper.ReadList(operationId, container, dto.animationChain);
            return true;
        }

        private static bool UpdateChain(ref object value, uint propertyKey, ByteContainer container)
        {
            value = UMI3DNetworkingHelper.ReadList<UMI3DAnimationDto.AnimationChainDto>(container);
            return true;
        }

        /// <inheritdoc/>
        public override void Start(float atTime)
        {
            if (started) return;
            progress = atTime;
            if (PlayingCoroutines != null) UMI3DAnimationManager.StopCoroutine(PlayingCoroutines);
            foreach (UMI3DAnimationDto.AnimationChainDto chain in dto.animationChain)
            {
                float p = GetProgress();
                if (p < chain.startOnProgress)
                    Coroutines.Add(UMI3DAnimationManager.StartCoroutine(WaitForProgress(chain.startOnProgress, () => { UMI3DAnimationManager.Start(chain.animationId); })));
                if (p == chain.startOnProgress)
                    UMI3DAnimationManager.Start(chain.animationId);
            }

            PlayingCoroutines = UMI3DAnimationManager.StartCoroutine(Playing(() => { OnEnd(); }));
        }

        /// <inheritdoc/>
        public override void SetProgress(long frame)
        {
            progress = frame;
        }
    }
}