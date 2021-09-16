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

namespace umi3d.cdk
{
    static public class UMI3DAnimationLoader
    {
        static public void ReadUMI3DExtension(UMI3DAbstractAnimationDto dto, GameObject node, Action finished, Action<Umi3dExecption> failed)
        {
            if (dto == null)
            {
                failed?.Invoke(new Umi3dExecption("dto shouldn't be null"));
                return;
            }

            switch (dto)
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
                    new UMI3DVideoPlayer(videoPlayer);
                    break;
                case UMI3DAudioPlayerDto audioPlayer:
                    new UMI3DAudioPlayer(audioPlayer);
                    break;
            }
            finished?.Invoke();
        }


        static public bool SetUMI3DProperty(UMI3DEntityInstance entity, SetEntityPropertyDto property)
        {
            var anim = entity?.Object as UMI3DAbstractAnimation;
            if (anim == null) return false;
            return anim.SetUMI3DProperty(entity, property);
        }

        static public bool SetUMI3DProperty(UMI3DEntityInstance entity, uint operationId, uint propertyKey, ByteContainer container)
        {
            var anim = entity?.Object as UMI3DAbstractAnimation;
            if (anim == null) return false;
            return anim.SetUMI3DProperty(entity, operationId, propertyKey, container);
        }

        static public bool ReadUMI3DProperty(ref object value, uint propertyKey, ByteContainer container)
        {
            return UMI3DAbstractAnimation.ReadUMI3DProperty(ref value, propertyKey, container);
        }

    }
}