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
using umi3d.common;
using UnityEngine;

namespace umi3d.edk
{
    public class UMI3DAudioPlayer : UMI3DAbstractAnimation
    {
        [SerializeField, EditorReadOnly]
        UMI3DNode node;
        [SerializeField, EditorReadOnly]
        UMI3DResource audioResources;
        [SerializeField, EditorReadOnly]
        [Range(0f, 1f)]
        float volume = 1;
        [SerializeField, EditorReadOnly]
        [Range(0f, 1f)]
        float pitch = 1;
        [SerializeField, EditorReadOnly]
        [Range(0f, 1f)]
        float spatialBlend;
        private UMI3DAsyncProperty<UMI3DNode> _objectNode;
        private UMI3DAsyncProperty<UMI3DResource> _objectAudioResource;
        private UMI3DAsyncProperty<float> _objectVolume;
        private UMI3DAsyncProperty<float> _objectPitch;
        private UMI3DAsyncProperty<float> _objectSpacialBlend;

        public UMI3DAsyncProperty<UMI3DNode> ObjectNode { get { Register(); return _objectNode; } protected set => _objectNode = value; }
        public UMI3DAsyncProperty<UMI3DResource> ObjectAudioResource { get { Register(); return _objectAudioResource; } protected set => _objectAudioResource = value; }
        public UMI3DAsyncProperty<float> ObjectVolume { get { Register(); return _objectVolume; } protected set => _objectVolume = value; }
        public UMI3DAsyncProperty<float> ObjectPitch { get { Register(); return _objectPitch; } protected set => _objectPitch = value; }
        public UMI3DAsyncProperty<float> ObjectSpacialBlend { get { Register(); return _objectSpacialBlend; } protected set => _objectSpacialBlend = value; }

        ///<inheritdoc/>
        protected override void InitDefinition(ulong id)
        {
            var equality = new UMI3DAsyncPropertyEquality();

            base.InitDefinition(id);
            ObjectNode = new UMI3DAsyncProperty<UMI3DNode>(id, UMI3DPropertyKeys.AnimationNode, node, (n, u) => n?.Id());
            ObjectAudioResource = new UMI3DAsyncProperty<UMI3DResource>(id, UMI3DPropertyKeys.AnimationResource, audioResources, (r, u) => r?.ToDto());
            ObjectVolume = new UMI3DAsyncProperty<float>(id, UMI3DPropertyKeys.AnimationVolume, volume, null, equality.FloatEquality);
            ObjectPitch = new UMI3DAsyncProperty<float>(id, UMI3DPropertyKeys.AnimationPitch, pitch, null, equality.FloatEquality);
            ObjectSpacialBlend = new UMI3DAsyncProperty<float>(id, UMI3DPropertyKeys.AnimationSpacialBlend, spatialBlend, null, equality.FloatEquality);

            ObjectNode.OnValueChanged += (n) => node = n;
            ObjectAudioResource.OnValueChanged += (r) => audioResources = r;
            ObjectVolume.OnValueChanged += (f) => volume = f;
            ObjectPitch.OnValueChanged += (f) => pitch = f;
            ObjectSpacialBlend.OnValueChanged += (f) => spatialBlend = f;
        }

        ///<inheritdoc/>
        protected override void WriteProperties(UMI3DAbstractAnimationDto dto, UMI3DUser user)
        {
            base.WriteProperties(dto, user);
            var Adto = dto as UMI3DAudioPlayerDto;
            Adto.nodeID = ObjectNode.GetValue(user)?.Id() ?? 0;
            Adto.audioResource = ObjectAudioResource.GetValue(user)?.ToDto();
            Adto.volume = ObjectVolume.GetValue(user);
            Adto.pitch = ObjectPitch.GetValue(user);
            Adto.spatialBlend = ObjectSpacialBlend.GetValue(user);
        }

        ///<inheritdoc/>
        protected override UMI3DAbstractAnimationDto CreateDto()
        {
            return new UMI3DAudioPlayerDto();
        }

        protected override (int, Func<byte[], int, int, (int,int)>) ToBytesAux(UMI3DUser user)
        {
            var fr = ObjectAudioResource.GetValue(user).ToByte();
            int size = sizeof(ulong) + 3 * sizeof(float) + fr.Item1;
            Func<byte[], int, int, (int, int)> func = (b, i, bs) => {
                bs += UMI3DNetworkingHelper.Write(ObjectNode.GetValue(user)?.Id() ?? 0, b, ref i);
                bs += UMI3DNetworkingHelper.Write(ObjectVolume.GetValue(user), b, ref i);
                bs += UMI3DNetworkingHelper.Write(ObjectPitch.GetValue(user), b, ref i);
                bs += UMI3DNetworkingHelper.Write(ObjectSpacialBlend.GetValue(user), b, ref i);
                return fr.Item2(b, i, bs);
            };
            return (size, func);
        }
    }
}