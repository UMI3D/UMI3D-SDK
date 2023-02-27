﻿/*
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
using System.Threading.Tasks;
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    public class UMI3DAnimatorAnimation : UMI3DAbstractAnimation
    {
        UMI3DVersion.VersionCompatibility _version = new UMI3DVersion.VersionCompatibility("2.6", "*");
        public override UMI3DVersion.VersionCompatibility version => _version;

        /// <summary>
        /// Get an <see cref="UMI3DAnimatorAnimation"/> by its UMI3D id.
        /// </summary>
        /// <param name="id">UMI3D id</param>
        /// <returns></returns>
        public static new UMI3DAnimatorAnimation Get(ulong id) { return UMI3DAbstractAnimation.Get(id) as UMI3DAnimatorAnimation; }
        /// <summary>
        /// DTO local copy.
        /// </summary>
        protected new UMI3DAnimatorAnimationDto dto { get => base.dto as UMI3DAnimatorAnimationDto; set => base.dto = value; }

        private bool started = false;

        public UMI3DAnimatorAnimation(UMI3DAnimatorAnimationDto dto) : base(dto)
        {
        }

        /// <inheritdoc/>
        public override float GetProgress()
        {
            return 0;
        }

        /// <inheritdoc/>
        public override void Start()
        {
            if (started) return;

            UMI3DEnvironmentLoader.WaitForAnEntityToBeLoaded(dto.nodeId,
                (n) =>
                {
                    MainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(WaitingForAnimator(n, dto));
                }
            );
        }

        protected IEnumerator WaitingForAnimator(UMI3DEntityInstance n, UMI3DAnimatorAnimationDto dto)
        {
            if (n == null)
                yield break;

            yield return null;

            (n as UMI3DNodeInstance)?.gameObject.GetComponentInChildren<Animator>().Play(dto.stateName);
        }

        /// <inheritdoc/>
        public override void Stop()
        {
        }

        /// <inheritdoc/>
        public override void OnEnd()
        {
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
            yield return null;
            action.Invoke();
        }

        /// <inheritdoc/>
        public override async  Task<bool> SetUMI3DProperty(SetUMI3DPropertyData value)
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

        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyContainerData value)
        {
            if (await base.SetUMI3DProperty(value)) return true;
            switch (value.propertyKey)
            {
                case UMI3DPropertyKeys.AnimationNodeId:
                    dto.nodeId = UMI3DSerializer.Read<uint>(value.container);
                    break;
                case UMI3DPropertyKeys.AnimationStateName:
                    dto.stateName = UMI3DSerializer.Read<string>(value.container);
                    break;
                default:
                    return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public override void Start(float atTime)
        {
        }

        /// <inheritdoc/>
        public override void SetProgress(long frame)
        {
        }
    }
}