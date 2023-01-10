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
using System.ComponentModel;
using System.Threading.Tasks;
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    /// <summary>
    /// Loader for <see cref="UMI3DAbstractAnimationDto"/>.
    /// </summary>
    public class UMI3DAnimationLoader : AbstractLoader
    {
        UMI3DVersion.VersionCompatibility _version = new UMI3DVersion.VersionCompatibility("2.6", "*");
        public override UMI3DVersion.VersionCompatibility version => _version;
        public override bool CanReadUMI3DExtension(ReadUMI3DExtensionData data)
        {
            return data.dto is UMI3DAbstractAnimationDto;
        }

        public override async Task ReadUMI3DExtension(ReadUMI3DExtensionData value)
        {
            switch (value.dto)
            {
                case UMI3DAnimationDto animation:
                    new UMI3DAnimation(animation);
                    break;
                case UMI3DAnimatorAnimationDto animatorAnimation:
                    new UMI3DAnimatorAnimation(animatorAnimation);
                    break;
                case UMI3DNodeAnimationDto nodeAnimation:
                    new UMI3DNodeAnimation(nodeAnimation);
                    break;
                case UMI3DVideoPlayerDto videoPlayer:
                    UMI3DVideoPlayerLoader.LoadVideo(videoPlayer);
                    break;
                case UMI3DAudioPlayerDto audioPlayer:
                    new UMI3DAudioPlayer(audioPlayer);
                    break;
            }
        }

        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyData value)
        {
            var anim = value.entity?.Object as UMI3DAbstractAnimation;
            if (anim == null) return false;
            return await anim.SetUMI3DProperty(value);
        }

        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyContainerData value)
        {
            var anim = value.entity?.Object as UMI3DAbstractAnimation;
            if (anim == null) return false;
            return await anim.SetUMI3DProperty(value);
        }

        public override async Task<bool> ReadUMI3DProperty(ReadUMI3DPropertyData value)
        {
            switch (value.propertyKey)
            {
                case UMI3DPropertyKeys.AnimationPlaying:
                    value.result = UMI3DSerializer.Read<bool>(value.container);
                    break;
                case UMI3DPropertyKeys.AnimationLooping:
                    value.result = UMI3DSerializer.Read<bool>(value.container);
                    break;
                case UMI3DPropertyKeys.AnimationStartTime:
                    value.result = UMI3DSerializer.Read<ulong>(value.container);
                    break;
                case UMI3DPropertyKeys.AnimationPauseFrame:
                    value.result = UMI3DSerializer.Read<long>(value.container);
                    break;
                default:
                    if (await UMI3DAnimation.ReadMyUMI3DProperty(value))
                        return true;
                    if (await UMI3DAudioPlayer.ReadMyUMI3DProperty(value))
                        return true;
                    return await UMI3DNodeAnimation.ReadMyUMI3DProperty(value);
            }
            return true;
        }
    }
}