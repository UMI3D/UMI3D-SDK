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
    public class UMI3DNodeAnimation : UMI3DAbstractAnimation
    {
        new public static UMI3DNodeAnimation Get(string id) { return UMI3DAbstractAnimation.Get(id) as UMI3DNodeAnimation; }
        protected new UMI3DNodeAnimationDto dto { get => base.dto as UMI3DNodeAnimationDto; set => base.dto = value; }

        List<Coroutine> Coroutines = new List<Coroutine>();
        Coroutine PlayingCoroutines;
        float progress;
        bool started = false;

        public UMI3DNodeAnimation(UMI3DNodeAnimationDto dto) : base(dto)
        {

        }

        ///<inheritdoc/>
        public override float GetProgress()
        {
            return progress;
        }

        ///<inheritdoc/>
        public override void Start()
        {
            if (started) return;
            progress = 0;
            if (PlayingCoroutines != null) UMI3DAnimationManager.Instance.StopCoroutine(PlayingCoroutines);
            foreach (var chain in dto.animationChain)
                if (GetProgress() < chain.startOnProgress)
                    Coroutines.Add(UMI3DAnimationManager.Instance.StartCoroutine(WaitForProgress(chain.startOnProgress, () => { UMI3DTransactionDispatcher.PerformOperation(chain.operation, null); })));
            PlayingCoroutines = UMI3DAnimationManager.Instance.StartCoroutine(Playing(() => { OnEnd(); }));
        }

        ///<inheritdoc/>
        public override void Stop()
        {
            if (!started) return;
            if (PlayingCoroutines != null) UMI3DAnimationManager.Instance.StopCoroutine(PlayingCoroutines);
            foreach (var c in Coroutines)
                UMI3DAnimationManager.Instance.StopCoroutine(c);
        }

        ///<inheritdoc/>
        public override void OnEnd()
        {
            PlayingCoroutines = null;
            started = false;
            base.OnEnd();
        }

        bool LaunchAnimation(float waitFor)
        {
            return dto.playing && GetProgress() >= waitFor;
        }

        IEnumerator WaitForProgress(float waitFor, Action action)
        {
            yield return new WaitUntil(() => LaunchAnimation(waitFor));
            action.Invoke();
        }

        IEnumerator Playing(Action action)
        {
            var fixUpdate = new WaitForFixedUpdate();
            while (GetProgress() < dto.duration)
            {
                yield return fixUpdate;
                if (dto.playing) progress += Time.fixedDeltaTime;
            }
            action.Invoke();
        }

        ///<inheritdoc/>
        public override bool SetUMI3DProperty(UMI3DEntityInstance entity, SetEntityPropertyDto property)
        {
            if (base.SetUMI3DProperty(entity, property)) return true;
            var ADto = dto as UMI3DNodeAnimationDto;
            if (ADto == null) return false;
            switch (property.property)
            {
                case UMI3DPropertyKeys.AnimationDuration:
                    ADto.duration = (float)property.value;
                    break;
                case UMI3DPropertyKeys.AnimationChain:
                    return UpdateChain(dto, property);
                default:
                    return false;
            }

            return true;
        }

        bool UpdateChain(UMI3DNodeAnimationDto dto, SetEntityPropertyDto property)
        {
            switch (property)
            {
                case SetEntityListAddPropertyDto add:
                    dto.animationChain.Add((UMI3DNodeAnimationDto.OperationChainDto)add.value);
                    break;
                case SetEntityListRemovePropertyDto rem:
                    dto.animationChain.RemoveAt((int)(Int64)rem.index);
                    break;
                case SetEntityListPropertyDto set:
                    dto.animationChain[(int)(Int64)set.index] = (UMI3DNodeAnimationDto.OperationChainDto)set.value;
                    break;
                default:
                    dto.animationChain = ((List<object>)property.value).Select(o => o as UMI3DNodeAnimationDto.OperationChainDto).ToList();
                    break;
            }
            return true;
        }

        ///<inheritdoc/>
        public override void Start(float atTime)
        {
            if (started) return;
            progress = atTime;
            if (PlayingCoroutines != null) UMI3DAnimationManager.Instance.StopCoroutine(PlayingCoroutines);
            foreach (var chain in dto.animationChain)
                if (GetProgress() < chain.startOnProgress)
                    Coroutines.Add(UMI3DAnimationManager.Instance.StartCoroutine(WaitForProgress(chain.startOnProgress, () => { UMI3DTransactionDispatcher.PerformOperation(chain.operation, null); })));
            PlayingCoroutines = UMI3DAnimationManager.Instance.StartCoroutine(Playing(() => { OnEnd(); }));
        }

        public override void SetProgress(long frame)
        {
        }
    }
}