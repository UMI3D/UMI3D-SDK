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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    /// <summary>
    /// Animation composed of a chain of severam animations
    /// </summary>
    public class UMI3DAnimation : UMI3DAbstractAnimation
    {
        UMI3DVersion.VersionCompatibility _version = new UMI3DVersion.VersionCompatibility("2.6", "*");
        public override UMI3DVersion.VersionCompatibility version => _version;

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

        /// <inheritdoc/>
        public override bool IsPlaying() => started;

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
            foreach (AnimationChainDto chain in dto.animationChain)
            {
                float p = GetProgress();
                if (p < chain.startOnProgress)
                    Coroutines.Add(UMI3DAnimationManager.StartCoroutine(WaitForProgress(chain.startOnProgress, () => { UMI3DAnimationManager.Instance.StartAnimation(chain.animationId); })));
                if (p == chain.startOnProgress)
                    UMI3DAnimationManager.Instance.StartAnimation(chain.animationId);
            }

            PlayingCoroutines = UMI3DAnimationManager.StartCoroutine(Playing(actionAfterPlaying: OnEnd));
        }

        /// <inheritdoc/>
        public override void Stop()
        {
            if (!started) return;
            if (PlayingCoroutines != null) UMI3DAnimationManager.StopCoroutine(PlayingCoroutines);
            foreach (AnimationChainDto chain in dto.animationChain)
                UMI3DAnimationManager.Instance.StopAnimation(chain.animationId);
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

        private IEnumerator Playing(Action actionAfterPlaying)
        {
            var fixUpdate = new WaitForFixedUpdate();
            while (GetProgress() < dto.duration)
            {
                yield return fixUpdate;
                if (dto.playing) progress += Time.fixedDeltaTime;
            }
            actionAfterPlaying.Invoke();
        }

        /// <inheritdoc/>
        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyData value)
        {
            if (await base.SetUMI3DProperty(value)) return true;
            switch (value.property.property)
            {
                case UMI3DPropertyKeys.AnimationDuration:
                    dto.duration = (float)(Double)value.property.value;
                    break;
                case UMI3DPropertyKeys.AnimationChain:
                    return UpdateChain(value.property);
                default:
                    return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyContainerData value)
        {
            if (await base.SetUMI3DProperty(value)) return true;
            switch (value.propertyKey)
            {
                case UMI3DPropertyKeys.AnimationDuration:
                    dto.duration = UMI3DSerializer.Read<float>(value.container);
                    break;
                case UMI3DPropertyKeys.AnimationChain:
                    return UpdateChain(value.operationId, value.propertyKey, value.container);
                default:
                    return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public static async Task<bool> ReadMyUMI3DProperty(ReadUMI3DPropertyData value)
        {
            switch (value.propertyKey)
            {
                case UMI3DPropertyKeys.AnimationDuration:
                    value.result = UMI3DSerializer.Read<float>(value.container);
                    break;
                case UMI3DPropertyKeys.AnimationChain:
                    return UpdateChain(value);
                default:
                    return await Task.FromResult(false);
            }

            return await Task.FromResult(true);
        }

        private static bool UpdateChain(ReadUMI3DPropertyData value)
        {
            value.result = UMI3DSerializer.ReadList<AnimationChainDto>(value.container);
            return true;
        }

        private bool UpdateChain(SetEntityPropertyDto property)
        {
            switch (property)
            {
                case SetEntityListAddPropertyDto add:
                    dto.animationChain.Add((AnimationChainDto)add.value);
                    break;
                case SetEntityListRemovePropertyDto rem:
                    dto.animationChain.RemoveAt((int)(Int64)rem.index);
                    break;
                case SetEntityListPropertyDto set:
                    dto.animationChain[(int)(Int64)set.index] = (AnimationChainDto)set.value;
                    break;
                default:
                    dto.animationChain = ((List<object>)property.value).Select(o => o as AnimationChainDto).ToList();
                    break;
            }
            return true;
        }

        private bool UpdateChain(uint operationId, uint propertyKey, ByteContainer container)
        {
            if (dto.animationChain == null)
                dto.animationChain = new List<AnimationChainDto>();
            UMI3DSerializer.ReadList(operationId, container, dto.animationChain);
            return true;
        }



        /// <inheritdoc/>
        public override void Start(float atTime)
        {
            if (started) return;
            progress = atTime;
            if (PlayingCoroutines != null) UMI3DAnimationManager.StopCoroutine(PlayingCoroutines);
            foreach (AnimationChainDto chain in dto.animationChain)
            {
                float p = GetProgress();
                if (p < chain.startOnProgress)
                    Coroutines.Add(UMI3DAnimationManager.StartCoroutine(WaitForProgress(chain.startOnProgress, () => { UMI3DAnimationManager.Instance.StartAnimation(chain.animationId); })));
                if (p == chain.startOnProgress)
                    UMI3DAnimationManager.Instance.StartAnimation(chain.animationId);
            }

            PlayingCoroutines = UMI3DAnimationManager.StartCoroutine(Playing(actionAfterPlaying: OnEnd));
        }

        /// <inheritdoc/>
        public override void SetProgress(long frame)
        {
            progress = frame;
        }
    }
}