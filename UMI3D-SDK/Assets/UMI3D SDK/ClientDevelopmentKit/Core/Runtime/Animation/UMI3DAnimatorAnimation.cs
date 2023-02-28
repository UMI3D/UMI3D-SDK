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
using System.Threading.Tasks;
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    public class UMI3DAnimatorAnimation : UMI3DAbstractAnimation
    {
        UMI3DVersion.VersionCompatibility _version = new UMI3DVersion.VersionCompatibility("2.6", "*");
        public override UMI3DVersion.VersionCompatibility version => _version;

        #region Fields

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

        /// <summary>
        /// Node which has <see cref="animator"/>
        /// </summary>
        private UMI3DNodeInstance node;

        /// <summary>
        /// <see cref="node"/> animator.
        /// </summary>
        private Animator animator;

        #endregion

        #region Methods

        public UMI3DAnimatorAnimation(UMI3DAnimatorAnimationDto dto) : base(dto)
        {
            SetNode(dto.nodeId);

            foreach (var entry in dto.parameters)
            {
                UMI3DAnimatorParameterDto param = new UMI3DAnimatorParameterDto(entry.Value);
                ApplyParameter(entry.Key, param);
            }
        }

        #region Animation

        /// <inheritdoc/>
        public override void Start()
        {
            if (started) return;

            if (animator != null)
                animator.Play(dto.stateName);
        }

        /// <inheritdoc/>
        public override void Start(float atTime)
        {
            UMI3DLogger.LogWarning("Imposisble for now to set a specific time for UMI3DAnimatorAnimation", DebugScope.Animation);
            Start();
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
        public override float GetProgress()
        {
            return 0;
        }

        #endregion

        #region Setter

        /// <inheritdoc/>
        public override async  Task<bool> SetUMI3DProperty(SetUMI3DPropertyData value)
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
                case UMI3DPropertyKeys.AnimationAnimatorParameters:
                    UMI3DLogger.LogWarning("Setting animator parameters not handled in dto mode", DebugScope.Animation);
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
        private void ApplyParameter(string name, UMI3DAnimatorParameterDto parameterDto)
        {
            if (animator == null) return;

            UMI3DAnimatorParameyerType type = (UMI3DAnimatorParameyerType)parameterDto.type;

            switch (type)
            {
                case UMI3DAnimatorParameyerType.Bool:
                    var val = (bool)parameterDto.value;
                    if (val)
                    {
                        animator.SetTrigger(name);
                    }
                    animator.SetBool(name, val);
                    break;
                case UMI3DAnimatorParameyerType.Float:
                    animator.SetFloat(name, (float)parameterDto.value);
                    break;
                case UMI3DAnimatorParameyerType.Integer:
                    animator.SetInteger(name, (int)parameterDto.value);
                    break;
                default:
                    break;
            }
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
                if(node != null)
                    animator = node.gameObject.GetComponentInChildren<Animator>();
                if (animator != null && dto.playing)
                {
                    animator.Play(dto.stateName);
                }
            });
        }

        /// <inheritdoc/>
        public override void SetProgress(long frame)
        {
        }

        #endregion

        #endregion
    }
}