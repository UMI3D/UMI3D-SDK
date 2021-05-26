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
using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using UnityEngine;

namespace umi3d.edk
{
    public class UMI3DAnimation : UMI3DAbstractAnimation
    {
        [Serializable]
        public class AnimationChain : IByte
        {
            public UMI3DAbstractAnimation Animation;
            public float Progress;

            public (int, Func<byte[], int, int>) ToByteArray(params object[] parameters)
            {
                return ToByte(null);
            }

            public (int, Func<byte[], int, int>) ToByte(UMI3DUser user)
            {
                int size = sizeof(ulong) + sizeof(float);
                Func<byte[], int, int> func = (b, i) => {
                    i += UMI3DNetworkingHelper.Write(Animation.Id(), b, i);
                    i += UMI3DNetworkingHelper.Write(Progress, b, i);
                    return size;
                };
                return (size, func);
            }

            public UMI3DAnimationDto.AnimationChainDto Todto(UMI3DUser user)
            {
                return new UMI3DAnimationDto.AnimationChainDto() { animationId = Animation.Id(), startOnProgress = Progress };
            }
        }

        [SerializeField, EditorReadOnly]
        float duration = 10f;
        [SerializeField, EditorReadOnly]
        List<AnimationChain> animationChain = null;

        private UMI3DAsyncProperty<float> _objectDuration;
        private UMI3DAsyncListProperty<AnimationChain> _objectAnimationChain;

        public UMI3DAsyncProperty<float> ObjectDuration { get { Register(); return _objectDuration; } protected set => _objectDuration = value; }
        public UMI3DAsyncListProperty<AnimationChain> ObjectAnimationChain { get { Register(); return _objectAnimationChain; } protected set => _objectAnimationChain = value; }

        ///<inheritdoc/>
        protected override UMI3DAbstractAnimationDto CreateDto()
        {
            return new UMI3DAnimationDto();
        }

        ///<inheritdoc/>
        protected override void InitDefinition(ulong id)
        {
            var equality = new UMI3DAsyncPropertyEquality();
            base.InitDefinition(id);
            ObjectDuration = new UMI3DAsyncProperty<float>(id, UMI3DPropertyKeys.AnimationDuration, duration, null, equality.FloatEquality);
            ObjectAnimationChain = new UMI3DAsyncListProperty<AnimationChain>(id, UMI3DPropertyKeys.AnimationChain, animationChain, (o, u) => o?.Todto(u));

            ObjectDuration.OnValueChanged += (d) => duration = d;
            ObjectAnimationChain.OnValueChanged += (l) => animationChain = l;
        }

        ///<inheritdoc/>
        protected override void WriteProperties(UMI3DAbstractAnimationDto dto, UMI3DUser user)
        {
            base.WriteProperties(dto, user);
            var Adto = dto as UMI3DAnimationDto;
            Adto.animationChain = ObjectAnimationChain.GetValue(user)?.Select(op => op?.Todto(user)).ToList();
            Adto.duration = ObjectDuration.GetValue(user);
        }

        protected override (int, Func<byte[], int, int>) ToBytesAux(UMI3DUser user)
        {
            var fbase = base.ToBytesAux(user);
            var fchain = UMI3DNetworkingHelper.ToBytes(ObjectAnimationChain.GetValue(user));
            var duration = ObjectDuration.GetValue(user);

            int size = fbase.Item1 + fchain.Item1 + UMI3DNetworkingHelper.GetSize(duration);
            Func<byte[], int, int> func = (b, i) => {
                i += fbase.Item2(b, i);
                i += fchain.Item2(b, i);
                i += UMI3DNetworkingHelper.Write(duration, b, i);
                return size;
            };
            return (size, func);
        }
    }
}