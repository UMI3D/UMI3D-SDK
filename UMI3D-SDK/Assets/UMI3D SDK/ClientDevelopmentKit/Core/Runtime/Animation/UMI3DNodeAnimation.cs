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
    /// Animation applied on a node through several operations.
    /// </summary>
    public class UMI3DNodeAnimation : UMI3DAbstractAnimation
    {
        private const DebugScope scope = DebugScope.CDK | DebugScope.Core | DebugScope.Loading;

        /// <summary>
        /// Get an <see cref="UMI3DNodeAnimation"/> by id.
        /// </summary>
        /// <param name="id">UMI3D id</param>
        /// <returns></returns>
        public static new UMI3DNodeAnimation Get(ulong id) { return UMI3DAbstractAnimation.Get(id) as UMI3DNodeAnimation; }
        /// <summary>
        /// DTO local copy.
        /// </summary>
        protected new UMI3DNodeAnimationDto dto { get => base.dto as UMI3DNodeAnimationDto; set => base.dto = value; }

        /// <summary>
        /// Operation to apply as an animation node.
        /// </summary>
        public class OperationChain
        {
            public AbstractOperationDto operation;
            public ByteContainer byteOperation;
            public float startOnProgress;

            public bool IsByte => byteOperation != null;

            public OperationChain(AbstractOperationDto operation, float startOnProgress)
            {
                this.operation = operation;
                this.startOnProgress = startOnProgress;
            }

            public OperationChain(UMI3DNodeAnimationDto.OperationChainDto chainDto)
            {
                this.operation = chainDto.operation;
                this.startOnProgress = chainDto.startOnProgress;
            }

            public OperationChain(ByteContainer operation, float startOnProgress)
            {
                this.byteOperation = operation;
                this.startOnProgress = startOnProgress;
            }
        }

        private List<OperationChain> operationChains;
        private readonly List<Coroutine> Coroutines = new List<Coroutine>();
        private Coroutine PlayingCoroutines;
        private float progress;
        private bool started = false;

        public UMI3DNodeAnimation(UMI3DNodeAnimationDto dto) : base(dto)
        {
            operationChains = dto.animationChain?.Select(chain => new OperationChain(chain)).ToList() ?? new List<OperationChain>();
        }

        /// <inheritdoc/>
        public override float GetProgress()
        {
            return progress;
        }

        /// <inheritdoc/>
        public override void Start()
        {
            Start(0);
        }

        /// <inheritdoc/>
        public override void Stop()
        {
            if (!started) return;
            if (PlayingCoroutines != null)
            {
                UMI3DAnimationManager.StopCoroutine(PlayingCoroutines);
                PlayingCoroutines = null;
            }

            foreach (Coroutine c in Coroutines)
                UMI3DAnimationManager.StopCoroutine(c);
            Coroutines.Clear();
            started = false;
        }

        /// <inheritdoc/>
        public override void OnEnd()
        {
            PlayingCoroutines = null;
            foreach (Coroutine c in Coroutines)
                UMI3DAnimationManager.StopCoroutine(c);
            Coroutines.Clear();
            started = false;
            base.OnEnd();
        }

        private IEnumerator Playing(Action action)
        {
            var l = operationChains.OrderBy(c => c.startOnProgress).ToList();
            int i = 0;
            var p = GetProgress();

            while (i < l.Count && l[i].startOnProgress < p) { i++; }

            var fixUpdate = new WaitForFixedUpdate();
            while ((p = GetProgress()) < dto.duration)
            {
                while (i < l.Count && l[i].startOnProgress <= p)
                {
                    PerformChain(l[i]);
                    i++;
                }

                yield return fixUpdate;
                if (dto.playing) progress += Time.fixedDeltaTime;
            }
            if(dto.playing)
                while (i < l.Count && l[i].startOnProgress <= dto.duration)
                {
                    PerformChain(l[i]);
                    i++;
                }
            action.Invoke();
        }

        void PerformChain(OperationChain chain)
        {
            if (chain.IsByte)
                UMI3DTransactionDispatcher.PerformOperation(new ByteContainer(chain.byteOperation));
            else
                UMI3DTransactionDispatcher.PerformOperation(chain.operation);
        }

        /// <inheritdoc/>
        public override bool SetUMI3DProperty(UMI3DEntityInstance entity, SetEntityPropertyDto property)
        {
            if (base.SetUMI3DProperty(entity, property)) return true;
            UMI3DNodeAnimationDto ADto = dto;
            if (ADto == null) return false;
            switch (property.property)
            {
                case UMI3DPropertyKeys.AnimationDuration:
                    ADto.duration = (float)(Double)property.value;
                    break;
                case UMI3DPropertyKeys.AnimationChain:
                    return UpdateChain(dto, property);
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
        public static bool ReadMyUMI3DProperty(ref object value, uint propertyKey, ByteContainer container) { return false; }

        private bool UpdateChain(UMI3DNodeAnimationDto dto, SetEntityPropertyDto property)
        {
            switch (property)
            {
                case SetEntityListAddPropertyDto add:
                    operationChains.Add(new OperationChain((UMI3DNodeAnimationDto.OperationChainDto)add.value));
                    break;
                case SetEntityListRemovePropertyDto rem:
                    operationChains.RemoveAt((int)(Int64)rem.index);
                    break;
                case SetEntityListPropertyDto set:
                    operationChains[(int)(Int64)set.index] = new OperationChain((UMI3DNodeAnimationDto.OperationChainDto)set.value);
                    break;
                default:
                    operationChains = ((List<object>)property.value).Select(o => o as UMI3DNodeAnimationDto.OperationChainDto).Select(o => new OperationChain(o)).ToList();
                    break;
            }
            return true;
        }

        private bool UpdateChain(uint operationId, uint propertyKey, ByteContainer container)
        {
            if (operationChains == null)
                operationChains = new List<OperationChain>();
            UMI3DNetworkingHelper.ReadList(operationId, container, operationChains);
            Stop();
            Start(progress);
            return true;
        }

        /// <inheritdoc/>
        public override void Start(float atTime)
        {
            if (started) OnEnd();
            if (started) return;
            started = true;
            progress = atTime / 1000;
            if (PlayingCoroutines != null)
                UMI3DAnimationManager.StopCoroutine(PlayingCoroutines);
            PlayingCoroutines = UMI3DAnimationManager.StartCoroutine(Playing(() => { OnEnd(); }));
        }

        /// <inheritdoc/>
        public override void SetProgress(long frame)
        {
        }
    }
}