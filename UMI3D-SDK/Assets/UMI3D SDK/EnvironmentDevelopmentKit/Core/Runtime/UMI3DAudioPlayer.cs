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

using inetum.unityUtils;
using umi3d.common;
using UnityEngine;

namespace umi3d.edk
{
    /// <summary>
    /// Audio animation support. An audio player enables the playing of audio resources.
    /// </summary>
    public class UMI3DAudioPlayer : UMI3DAbstractAnimation
    {
        /// <summary>
        /// Node where the audio should come from.
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("Node where the audio should come from.")]
        private UMI3DNode node;
        /// <summary>
        /// Audio to play as a resource.
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip(" Audio to play as a resource.")]
        private UMI3DResource audioResources;
        /// <summary>
        /// Audio volume level.
        /// </summary>
        /// Used to reduce the audio volume. Default is max of value 1.
        [SerializeField, EditorReadOnly, Tooltip("Audio volume level. Use this to reduce the volume of a specific audio.")]
        [Range(0f, 1f)]
        private float volume = 1;
        /// <summary>
        /// Audio pitch level.
        /// </summary>
        /// Used to reduce the audio pitch. Default is max of value 1.
        [SerializeField, EditorReadOnly, Tooltip("Audio volume level. Use this to reduce the pitch of a specific audio.")]
        [Range(0f, 1f)]
        private float pitch = 1;
        /// <summary>
        /// Control the amplitude of spatialisation effects.
        /// </summary>
        /// A 0 value will result in a sound with no spatialisation, while 1 ends up with full 3D effects.
        [SerializeField, EditorReadOnly]
        [Tooltip("Control the amplitude of spatialisation effects.\n" +
                 "A 0 value will result in a sound with no spatialisation, while 1 ends up with full 3D effects.")]
        [Range(0f, 1f)]
        private float spatialBlend;
        /// <summary>
        /// See <see cref="node"/>.
        /// </summary>
        private UMI3DAsyncProperty<UMI3DNode> _objectNode;
        /// <summary>
        /// See <see cref="audioResources"/>.
        /// </summary>
        private UMI3DAsyncProperty<UMI3DResource> _objectAudioResource;
        /// <summary>
        /// See <see cref="volume"/>.
        /// </summary>
        private UMI3DAsyncProperty<float> _objectVolume;
        /// <summary>
        /// See <see cref="pitch"/>.
        /// </summary>
        private UMI3DAsyncProperty<float> _objectPitch;
        /// <summary>
        /// See <see cref="spatialBlend"/>.
        /// </summary>
        private UMI3DAsyncProperty<float> _objectSpacialBlend;

        /// <summary>
        /// See <see cref="node"/>.
        /// </summary>
        public UMI3DAsyncProperty<UMI3DNode> ObjectNode { get { Register(); return _objectNode; } protected set => _objectNode = value; }
        /// <summary>
        /// See <see cref="audioResources"/>.
        /// </summary>
        public UMI3DAsyncProperty<UMI3DResource> ObjectAudioResource { get { Register(); return _objectAudioResource; } protected set => _objectAudioResource = value; }
        /// <summary>
        /// See <see cref="volume"/>.
        /// </summary>
        public UMI3DAsyncProperty<float> ObjectVolume { get { Register(); return _objectVolume; } protected set => _objectVolume = value; }
        /// <summary>
        /// See <see cref="pitch"/>.
        /// </summary>
        public UMI3DAsyncProperty<float> ObjectPitch { get { Register(); return _objectPitch; } protected set => _objectPitch = value; }
        /// <summary>
        /// See <see cref="spatialBlend"/>.
        /// </summary>
        public UMI3DAsyncProperty<float> ObjectSpacialBlend { get { Register(); return _objectSpacialBlend; } protected set => _objectSpacialBlend = value; }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        protected override UMI3DAbstractAnimationDto CreateDto()
        {
            return new UMI3DAudioPlayerDto();
        }

        /// <inheritdoc/>
        protected override Bytable ToBytesAux(UMI3DUser user)
        {
            return UMI3DNetworkingHelper.Write(ObjectNode.GetValue(user)?.Id() ?? 0)
                + UMI3DNetworkingHelper.Write(ObjectVolume.GetValue(user))
                + UMI3DNetworkingHelper.Write(ObjectPitch.GetValue(user))
                + UMI3DNetworkingHelper.Write(ObjectSpacialBlend.GetValue(user))
                + ObjectAudioResource.GetValue(user).ToByte();
        }
    }
}