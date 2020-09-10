/*
Copyright 2019 Gfi Informatique

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
using umi3d.common;
using UnityEngine;

namespace umi3d.edk
{
    public class UMI3DAudioPlayer : UMI3DAbstractAnimation
    {
        [SerializeField]
        UMI3DNode node;
        [SerializeField]
        UMI3DResource audioResources;
        [SerializeField]
        [Range(0f,1f)]
        float volume = 1;
        [SerializeField]
        [Range(0f, 1f)]
        float pitch = 1;
        [SerializeField]
        [Range(0f, 1f)]
        float spatialBlend;


        public UMI3DAsyncProperty<UMI3DNode> ObjectNode;
        public UMI3DAsyncProperty<UMI3DResource> ObjectAudioResource;
        public UMI3DAsyncProperty<float> ObjectVolume;
        public UMI3DAsyncProperty<float> ObjectPitch;
        public UMI3DAsyncProperty<float> ObjectSpacialBlend;

        protected override void InitDefinition(string id)
        {
            var equality = new UMI3DAsyncPropertyEquality();

            base.InitDefinition(id);
            ObjectNode = new UMI3DAsyncProperty<UMI3DNode>(id, UMI3DPropertyKeys.AnimationNode, node,(n,u)=>n?.Id());
            ObjectAudioResource = new UMI3DAsyncProperty<UMI3DResource>(id,UMI3DPropertyKeys.AnimationResource,audioResources,(r,u)=>r?.ToDto());
            ObjectVolume = new UMI3DAsyncProperty<float>(id, UMI3DPropertyKeys.AnimationVolume,volume, null, equality.FloatEquality);
            ObjectPitch = new UMI3DAsyncProperty<float>(id, UMI3DPropertyKeys.AnimationPitch, pitch, null, equality.FloatEquality);
            ObjectSpacialBlend = new UMI3DAsyncProperty<float>(id, UMI3DPropertyKeys.AnimationSpacialBlend, spatialBlend, null, equality.FloatEquality);

            ObjectNode.OnValueChanged += (n) => node = n;
            ObjectAudioResource.OnValueChanged += (r) => audioResources = r;
            ObjectVolume.OnValueChanged += (f) => volume = f;
            ObjectPitch.OnValueChanged += (f) => pitch = f;
            ObjectSpacialBlend.OnValueChanged += (f) => spatialBlend = f;
        }

        protected override void WriteProperties(UMI3DAbstractAnimationDto dto, UMI3DUser user)
        {
            base.WriteProperties(dto, user);
            var Adto = dto as UMI3DAudioPlayerDto;
            Adto.nodeID = ObjectNode.GetValue(user)?.Id();
            Adto.audioResource = ObjectAudioResource.GetValue(user)?.ToDto();
            Adto.volume = ObjectVolume.GetValue(user);
            Adto.pitch = ObjectPitch.GetValue(user);
            Adto.spatialBlend = ObjectSpacialBlend.GetValue(user);
        }

        protected override UMI3DAbstractAnimationDto CreateDto()
        {
            return new UMI3DAudioPlayerDto();
        }
    }
}