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

        /// <inheritdoc/>
        public override Task ReadUMI3DExtension(ReadUMI3DExtensionData value)
        {
            UMI3DAbstractAnimation animationInstance = value.dto switch
            {
                UMI3DAnimationDto anim         => new UMI3DAnimation(anim),
                UMI3DAnimatorAnimationDto anim => new UMI3DAnimatorAnimation(anim),
                UMI3DNodeAnimationDto anim     => new UMI3DNodeAnimation(anim),
                UMI3DVideoPlayerDto anim       => UMI3DVideoPlayerLoader.LoadVideoPlayer(anim),
                UMI3DAudioPlayerDto anim       => new UMI3DAudioPlayer(anim),
                _ => null
            };

            if (animationInstance is not null)
            {
                UMI3DEnvironmentLoader.Instance.RegisterEntity(animationInstance.Id, value.dto, animationInstance).NotifyLoaded();
                animationInstance.Init();
            }
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyData value)
        {
            if (value.entity?.Object is not UMI3DAbstractAnimation anim) return false;
            return await anim.SetUMI3DProperty(value);
        }

        /// <inheritdoc/>
        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyContainerData value)
        {
            if (value.entity?.Object is not UMI3DAbstractAnimation anim) return false;
            return await anim.SetUMI3DProperty(value);
        }

        /// <inheritdoc/>
        public override async Task<bool> ReadUMI3DProperty(ReadUMI3DPropertyData value)
        {
            value.result = value.propertyKey switch
            {
                UMI3DPropertyKeys.AnimationPlaying or
                UMI3DPropertyKeys.AnimationLooping => UMI3DSerializer.Read<bool>(value.container),
                UMI3DPropertyKeys.AnimationStartTime => UMI3DSerializer.Read<ulong>(value.container),
                UMI3DPropertyKeys.AnimationPauseFrame => UMI3DSerializer.Read<long>(value.container),
                _ => null
            };
            if (value.result is not null)
                return true;

            if (await UMI3DAnimation.ReadMyUMI3DProperty(value))
                    return true;
            if (await UMI3DAudioPlayer.ReadMyUMI3DProperty(value))
                    return true;
             return await UMI3DNodeAnimation.ReadMyUMI3DProperty(value);
        }
    }
}