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

using inetum.unityUtils;
using System.Collections.Generic;
using umi3d.common;
using UnityEngine;

namespace umi3d.edk
{
    /// <summary>
    /// Animation using an animator to animate objects.
    /// </summary>
    public class UMI3DAnimatorAnimation : UMI3DAbstractAnimation
    {
        /// <summary>
        /// Node where the animator can be found on.
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("Node where the animator can be found on.")]
        private UMI3DNode node = null;
        /// <summary>
        /// Animation state's name in the animator controller.
        /// </summary>
        /// An empty state name corresponds to a self-caring animator.
        [SerializeField, EditorReadOnly, Tooltip("Current state's name in the animator controller. \n" +
                                                 "An empty state name corresponds to a self-caring animator.")]
        private string stateName = string.Empty;
        
        /// <summary>
        /// Animation normalized time at start. 
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("Animation normalized time at start.")]
        private float normalizedTime = 0f;

        /// <summary>
        /// See <see cref="node"/>.
        /// </summary>
        private UMI3DAsyncProperty<UMI3DNode> _objectNode;
        /// <summary>
        /// See <see cref="stateName"/>.
        /// </summary>
        private UMI3DAsyncProperty<string> _objectStateName;
        /// <summary>
        /// See <see cref="normalizedTime"/>.
        /// </summary>
        private UMI3DAsyncProperty<float> _objectNormalizedTime;
        /// <summary>
        /// <see cref="objectParameters"/>.
        /// </summary>
        private UMI3DAsyncDictionnaryProperty<string, object> _objectParameters;

        /// <summary>
        /// See <see cref="node"/>.
        /// </summary>
        public UMI3DAsyncProperty<UMI3DNode> objectNode { get { Register(); return _objectNode; } protected set => _objectNode = value; }
        /// <summary>
        /// See <see cref="stateName"/>.
        /// </summary>
        public UMI3DAsyncProperty<string> objectStateName { get { Register(); return _objectStateName; } protected set => _objectStateName = value; }
        /// <summary>
        /// See <see cref="normalizedTime"/>.
        /// </summary>
        public UMI3DAsyncProperty<float> objectNormalizedTime { get { Register(); return _objectNormalizedTime; } protected set => _objectNormalizedTime = value; }
        /// <summary>
        /// Property to change <see cref="Animator"/> parameters. Allowed values are float, integer, bool (value true for trigger parameter).
        /// </summary>
        public UMI3DAsyncDictionnaryProperty<string, object> objectParameters { get { Register(); return _objectParameters; } protected set => _objectParameters = value; }

        /// <inheritdoc/>
        protected override UMI3DAbstractAnimationDto CreateDto()
        {
            return new UMI3DAnimatorAnimationDto();
        }

        /// <inheritdoc/>
        protected override void InitDefinition(ulong id)
        {
            var equality = new UMI3DAsyncPropertyEquality();
            base.InitDefinition(id);

            objectNode = new UMI3DAsyncProperty<UMI3DNode>(id, UMI3DPropertyKeys.AnimationNodeId, node, null, (o, u) => o.Equals(u));
            objectStateName = new UMI3DAsyncProperty<string>(id, UMI3DPropertyKeys.AnimationStateName, stateName, null, (o, u) => o.Equals(u));
            objectNormalizedTime = new UMI3DAsyncProperty<float>(id, UMI3DPropertyKeys.AnimationAnimatorNormalizedTime, normalizedTime, null, (o, u) => o.Equals(u));
            objectParameters = new UMI3DAsyncDictionnaryProperty<string, object>(id, UMI3DPropertyKeys.AnimationAnimatorParameters,
                new Dictionary<string, object>(), null, (o, u) => UMI3DAnimatorParameter.Create(o), null, d =>
                {
                    return new Dictionary<string, object>(d);
                });

            objectNode.OnValueChanged += (d) => node = d;
            objectStateName.OnValueChanged += (d) => stateName = d;
            objectNormalizedTime.OnValueChanged += (d) => normalizedTime = d;
        }

        /// <inheritdoc/>
        protected override void WriteProperties(UMI3DAbstractAnimationDto dto, UMI3DUser user)
        {
            base.WriteProperties(dto, user);
            var Adto = dto as UMI3DAnimatorAnimationDto;
            Adto.nodeId = objectNode.GetValue(user).Id();
            Adto.stateName = objectStateName.GetValue(user);
            Adto.normalizedTime = objectNormalizedTime.GetValue(user);
            Adto.parameters = objectParameters.GetValue(user);
        }

        /// <inheritdoc/>
        protected override Bytable ToBytesAux(UMI3DUser user)
        {
            return base.ToBytesAux(user)
                + UMI3DSerializer.Write(objectNode.GetValue(user).Id())
                + UMI3DSerializer.Write(objectStateName.GetValue(user))
                + UMI3DSerializer.Write(objectNormalizedTime.GetValue(user))
                + UMI3DSerializer.Write(objectParameters.GetValue(user));
        }
    }
}