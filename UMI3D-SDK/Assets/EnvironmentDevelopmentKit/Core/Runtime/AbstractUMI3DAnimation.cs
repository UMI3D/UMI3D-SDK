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

    public class UMI3DAbstractAnimation : MonoBehaviour, UMI3DLoadableEntity
    {
        string animationID;

        [SerializeField]
        bool playing;
        [SerializeField]
        bool looping;
        [SerializeField]
        DateTime startTime;
        private UMI3DAsyncProperty<bool> _objectPlaying;
        private UMI3DAsyncProperty<bool> _objectLooping;
        private UMI3DAsyncProperty<DateTime> _objectStartTime;

        public UMI3DAsyncProperty<bool> objectPlaying { get { Register();  return _objectPlaying; } protected set => _objectPlaying = value; }
        public UMI3DAsyncProperty<bool> objectLooping { get { Register(); return _objectLooping; } protected set => _objectLooping = value; }
        public UMI3DAsyncProperty<DateTime> objectStartTime { get { Register(); return _objectStartTime; } protected set => _objectStartTime = value; }

        /// <summary>
        /// Get the Id of the animation.
        /// </summary>
        /// <returns></returns>
        public string Id()
        {
            Register();
            return animationID;
        }


        /// <summary>
        /// Check if the AbstractObject3D has been registered to to the UMI3DScene and do it if not
        /// </summary>
        /// <returns>Return a LoadEntity</returns>
        public virtual LoadEntity Register()
        {
            if (animationID == null && UMI3DEnvironment.Exists)
            {
                animationID = UMI3DEnvironment.Register(this);
                InitDefinition(animationID);
            }
            return GetLoadEntity();
        }

        /// <summary>
        /// Initialize object's properties.
        /// </summary>
        protected virtual void InitDefinition(string id)
        {
            objectPlaying = new UMI3DAsyncProperty<bool>(id, UMI3DPropertyKeys.AnimationPlaying, playing);
            objectLooping = new UMI3DAsyncProperty<bool>(id, UMI3DPropertyKeys.AnimationLooping, looping);
            objectStartTime = new UMI3DAsyncProperty<DateTime>(id, UMI3DPropertyKeys.AnimationStartTime, startTime);

            objectPlaying.OnValueChanged += (b) => playing = b;
            objectLooping.OnValueChanged += (b) => looping = b;
            objectStartTime.OnValueChanged += (d) => startTime = d;
        }

        /// <summary>
        /// Return load operation
        /// </summary>
        /// <returns></returns>
        protected virtual LoadEntity GetLoadEntity()
        {
            var operation = new LoadEntity()
            {
                entity = this,
                users = new HashSet<UMI3DUser>(UMI3DEnvironment.GetEntities<UMI3DUser>())
            };
            return operation;
        }

        /// <summary>
        /// Write dto properties values.
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="user"></param>
        protected virtual void WriteProperties(UMI3DAbstractAnimationDto dto, UMI3DUser user)
        {
            dto.id = Id();
            dto.playing = objectPlaying.GetValue(user);
            dto.looping = objectLooping.GetValue(user);
            dto.startTime = objectStartTime.GetValue(user);
        }

        /// <summary>
        /// Convert to dto for a given user.
        /// </summary>
        /// <param name="user">User to convert for</param>
        /// <returns></returns>
        public UMI3DAbstractAnimationDto ToAnimationDto(UMI3DUser user)
        {
            UMI3DAbstractAnimationDto dto = CreateDto();
            WriteProperties(dto, user);
            return dto;
        }

        /// <summary>
        /// Create an empty Dto.
        /// </summary>
        /// <returns></returns>
        protected virtual UMI3DAbstractAnimationDto CreateDto()
        {
            return new UMI3DAbstractAnimationDto();
        }


        public IEntity ToEntityDto(UMI3DUser user)
        {
            return ToAnimationDto(user);
        }
    }
}