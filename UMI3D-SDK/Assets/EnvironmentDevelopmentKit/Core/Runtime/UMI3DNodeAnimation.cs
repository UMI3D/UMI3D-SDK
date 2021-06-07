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
    public class UMI3DNodeAnimation : UMI3DAbstractAnimation
    {
        [Serializable]
        public class OperationChain : IByte
        {
            public Operation Operation;
            public float progress;

            public UMI3DNodeAnimationDto.OperationChainDto Todto(UMI3DUser user)
            {
                return new UMI3DNodeAnimationDto.OperationChainDto() { operation = Operation.ToOperationDto(user), startOnProgress = progress };
            }

            public (int, Func<byte[], int, int, (int, int)>) ToBytes(int baseSize, UMI3DUser user)
            {
                var op = Operation.ToBytes(baseSize, user);

                int size =  sizeof(float) + op.Item1;
                Func<byte[], int, int, (int, int)> func = (b, i, bs) => {
                    bs += UMI3DNetworkingHelper.Write(progress, b, ref i);
                    return op.Item2(b, i, bs);
                };
                return (size, func);
            }

            (int, Func<byte[], int, int, (int, int)>) IByte.ToByteArray(int baseSize, params object[] parameters)
            {
                if (parameters.Length < 1)
                    return ToBytes(baseSize,null);
                return ToBytes(baseSize,parameters[0] as UMI3DUser);
            }
        }

        [SerializeField, EditorReadOnly]
        float duration = 10f;
        [SerializeField, EditorReadOnly]
        List<OperationChain> animationChain = null;
        private UMI3DAsyncProperty<float> objectDuration;
        private UMI3DAsyncListProperty<OperationChain> objectAnimationChain;

        public UMI3DAsyncProperty<float> ObjectDuration { get { Register(); return objectDuration; } protected set => objectDuration = value; }
        public UMI3DAsyncListProperty<OperationChain> ObjectAnimationChain { get { Register(); return objectAnimationChain; } protected set => objectAnimationChain = value; }

        ///<inheritdoc/>
        protected override UMI3DAbstractAnimationDto CreateDto()
        {
            return new UMI3DNodeAnimationDto();
        }

        ///<inheritdoc/>
        protected override void InitDefinition(ulong id)
        {
            var equality = new UMI3DAsyncPropertyEquality();
            base.InitDefinition(id);
            ObjectDuration = new UMI3DAsyncProperty<float>(id, UMI3DPropertyKeys.AnimationDuration, duration, null, equality.FloatEquality);
            ObjectAnimationChain = new UMI3DAsyncListProperty<OperationChain>(id, UMI3DPropertyKeys.AnimationChain, animationChain, (o, u) => o?.Todto(u));

            ObjectDuration.OnValueChanged += (d) => duration = d;
            ObjectAnimationChain.OnValueChanged += (l) => animationChain = l;
        }

        ///<inheritdoc/>
        protected override void WriteProperties(UMI3DAbstractAnimationDto dto, UMI3DUser user)
        {
            base.WriteProperties(dto, user);
            var NAdto = dto as UMI3DNodeAnimationDto;
            NAdto.animationChain = ObjectAnimationChain.GetValue(user)?.Select(op => op?.Todto(user)).ToList();
            NAdto.duration = ObjectDuration.GetValue(user);
        }

        protected override (int, Func<byte[], int, int, (int, int)>) ToBytesAux(UMI3DUser user)
        {

            var funcChain = UMI3DNetworkingHelper.ToBytes(animationChain, 0, user);

            int size = sizeof(float) + funcChain.Item1;
            Func<byte[], int, int, (int, int)> func = (b, i, bs) =>
            {
                bs += UMI3DNetworkingHelper.Write(objectDuration.GetValue(user), b, ref i);
                (i, bs) = funcChain.Item2(b, i, bs);
                return (i, bs);
            };
            return (size, func);
        }
    }
}