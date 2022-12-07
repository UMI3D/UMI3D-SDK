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
using System;
using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using UnityEngine;

namespace umi3d.edk
{
    /// <summary>
    /// Animation that triggers others animations.
    /// </summary>
    /// Animations of a same group could be triggered at the same time and executed in a particular order that way.
    public class UMI3DAnimation : UMI3DAbstractAnimation
    {
        /// <summary>
        /// A piece of animation to play.
        /// </summary>
        /// Animations in animation chain could be played simultaneously ou one after another by setting up the <see cref="Progress"/> field.
        [Serializable]
        public class AnimationChain : IBytable
        {
            /// <summary>
            /// Animation in the animation chain.
            /// </summary>
            public UMI3DAbstractAnimation Animation;
            /// <summary>
            /// Progress of the animation at which to start the animation.
            /// </summary>
            public float Progress;

            /// <inheritdoc/>
            public Bytable ToBytableArray(params object[] parameters)
            {
                return ToByte(null);
            }

            /// <inheritdoc/>
            public Bytable ToByte(UMI3DUser user)
            {
                return UMI3DNetworkingHelper.Write(Animation.Id())
                    + UMI3DNetworkingHelper.Write(Progress);
            }

            public UMI3DAnimationDto.AnimationChainDto Todto(UMI3DUser user)
            {
                return new UMI3DAnimationDto.AnimationChainDto() { animationId = Animation.Id(), startOnProgress = Progress };
            }

            /// <inheritdoc/>
            bool IBytable.IsCountable()
            {
                return true;
            }

            /// <inheritdoc/>
            Bytable IBytable.ToBytableArray(params object[] parameters)
            {
                return ToByte(null);
            }
        }

        /// <summary>
        ///  Max duration of an animation chain.
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("Max duration of an animation chain.")]
        private float duration = 10f;
        /// <summary>
        /// List of animations to play.
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("List of animations to play.")]
        private List<AnimationChain> animationChain = null;

        /// <summary>
        /// See <see cref="duration"/>.
        /// </summary>
        private UMI3DAsyncProperty<float> _objectDuration;
        /// <summary>
        /// See <see cref="animationChain"/>.
        /// </summary>
        private UMI3DAsyncListProperty<AnimationChain> _objectAnimationChain;

        /// <summary>
        /// See <see cref="duration"/>.
        /// </summary>
        public UMI3DAsyncProperty<float> ObjectDuration { get { Register(); return _objectDuration; } protected set => _objectDuration = value; }
        /// <summary>
        /// See <see cref="animationChain"/>.
        /// </summary>
        public UMI3DAsyncListProperty<AnimationChain> ObjectAnimationChain { get { Register(); return _objectAnimationChain; } protected set => _objectAnimationChain = value; }

        /// <inheritdoc/>
        protected override UMI3DAbstractAnimationDto CreateDto()
        {
            return new UMI3DAnimationDto();
        }

        /// <inheritdoc/>
        protected override void InitDefinition(ulong id)
        {
            var equality = new UMI3DAsyncPropertyEquality();
            base.InitDefinition(id);
            ObjectDuration = new UMI3DAsyncProperty<float>(id, UMI3DPropertyKeys.AnimationDuration, duration, null, equality.FloatEquality);
            ObjectAnimationChain = new UMI3DAsyncListProperty<AnimationChain>(id, UMI3DPropertyKeys.AnimationChain, animationChain, (o, u) => UMI3DEnvironment.Instance.useDto ? o?.Todto(u) : (object)o);

            ObjectDuration.OnValueChanged += (d) => duration = d;
            ObjectAnimationChain.OnValueChanged += (l) => animationChain = l;
        }

        /// <inheritdoc/>
        protected override void WriteProperties(UMI3DAbstractAnimationDto dto, UMI3DUser user)
        {
            base.WriteProperties(dto, user);
            var Adto = dto as UMI3DAnimationDto;
            Adto.animationChain = ObjectAnimationChain.GetValue(user)?.Select(op => op?.Todto(user)).ToList();
            Adto.duration = ObjectDuration.GetValue(user);
        }

        /// <inheritdoc/>
        protected override Bytable ToBytesAux(UMI3DUser user)
        {
            return base.ToBytesAux(user)
                + UMI3DNetworkingHelper.WriteIBytableCollection(ObjectAnimationChain.GetValue(user))
                + UMI3DNetworkingHelper.Write(ObjectDuration.GetValue(user));
        }
    }
}