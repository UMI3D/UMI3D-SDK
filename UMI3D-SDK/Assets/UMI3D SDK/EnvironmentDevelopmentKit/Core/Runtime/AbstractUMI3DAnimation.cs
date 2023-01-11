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
using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using UnityEngine;

namespace umi3d.edk
{
    /// <summary>
    /// Abstract base for all animated media, such as animations, videoplayers or audioplayers.
    /// </summary>
    public class UMI3DAbstractAnimation : MonoBehaviour, UMI3DLoadableEntity
    {
        /// <summary>
        /// Entity UMI3D id.
        /// </summary>
        private ulong animationID;
        
        /// <summary>
        /// Is the animation playing?
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("Is the animation playing?")]
        private bool playing;
        /// <summary>
        /// Should the animation loop?
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("Should the animation loop?")]
        private bool looping;

        /// <summary>
        /// Animation start time in milliseconds.
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("Animation start time in milliseconds.")]
        private ulong startTime;

        /// <summary>
        /// Animation last pause time in milliseconds.
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("Animation last pause time in milliseconds.")]
        private long pauseFrame;

        /// <summary>
        /// See <see cref="playing"/>.
        /// </summary>
        private UMI3DAsyncProperty<bool> _objectPlaying;
        /// <summary>
        /// See <see cref="looping"/>.
        /// </summary>
        private UMI3DAsyncProperty<bool> _objectLooping;
        /// <summary>
        /// See <see cref="startTime"/>.
        /// </summary>
        private UMI3DAsyncProperty<ulong> _objectStartTime;
        /// <summary>
        /// See <see cref="pauseFrame"/>.
        /// </summary>
        private UMI3DAsyncProperty<long> _objectPauseFrame;

        /// <summary>
        /// See <see cref="playing"/>.
        /// </summary>
        public UMI3DAsyncProperty<bool> objectPlaying { get { Register(); return _objectPlaying; } protected set => _objectPlaying = value; }
        /// <summary>
        /// See <see cref="looping"/>.
        /// </summary>
        public UMI3DAsyncProperty<bool> objectLooping { get { Register(); return _objectLooping; } protected set => _objectLooping = value; }
        /// <summary>
        /// See <see cref="startTime"/>.
        /// </summary>
        public UMI3DAsyncProperty<ulong> objectStartTime { get { Register(); return _objectStartTime; } protected set => _objectStartTime = value; }
        /// <summary>
        /// See <see cref="pauseFrame"/>.
        /// </summary>
        public UMI3DAsyncProperty<long> objectPauseTime { get { Register(); return _objectPauseFrame; } protected set => _objectPauseFrame = value; }

        /// <inheritdoc/>
        public ulong Id()
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
            if (animationID == 0 && UMI3DEnvironment.Exists)
            {
                animationID = UMI3DEnvironment.Register(this);
                InitDefinition(animationID);
            }
            return GetLoadEntity();
        }

        /// <summary>
        /// Initialize object's properties.
        /// </summary>
        protected virtual void InitDefinition(ulong id)
        {
            BeardedManStudios.Forge.Networking.Unity.MainThreadManager.Run(() =>
            {
                if (this != null)
                {
                    foreach (UMI3DUserFilter f in GetComponents<UMI3DUserFilter>())
                        AddConnectionFilter(f);
                }
            });

            objectPlaying = new UMI3DAsyncProperty<bool>(id, UMI3DPropertyKeys.AnimationPlaying, playing);
            objectLooping = new UMI3DAsyncProperty<bool>(id, UMI3DPropertyKeys.AnimationLooping, looping);
            objectStartTime = new UMI3DAsyncProperty<ulong>(id, UMI3DPropertyKeys.AnimationStartTime, startTime);
            objectPauseTime = new UMI3DAsyncProperty<long>(id, UMI3DPropertyKeys.AnimationPauseFrame, pauseFrame);

            objectPlaying.OnValueChanged += (b) => playing = b;
            objectLooping.OnValueChanged += (b) => looping = b;
            objectStartTime.OnValueChanged += (d) => startTime = d;
            objectPauseTime.OnValueChanged += (v) => pauseFrame = v;
        }

        /// <inheritdoc/>
        public virtual LoadEntity GetLoadEntity(HashSet<UMI3DUser> users = null)
        {
            var operation = new LoadEntity()
            {
                entities = new List<UMI3DLoadableEntity>() { this },
                users = users != null ? new HashSet<UMI3DUser>(users) : UMI3DServer.Instance.UserSetWhenHasJoined()
            };
            return operation;
        }

        /// <inheritdoc/>
        public DeleteEntity GetDeleteEntity(HashSet<UMI3DUser> users = null)
        {
            var operation = new DeleteEntity()
            {
                entityId = Id(),
                users = users != null ? new HashSet<UMI3DUser>(users) : UMI3DServer.Instance.UserSetWhenHasJoined()
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
            dto.pauseTime = objectPauseTime.GetValue(user);
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

        /// <inheritdoc/>
        public IEntity ToEntityDto(UMI3DUser user)
        {
            return ToAnimationDto(user);
        }

        /// <inheritdoc/>
        public Bytable ToBytes(UMI3DUser user)
        {
            return UMI3DSerializer.Write(Id())
                + UMI3DSerializer.Write(objectPlaying.GetValue(user))
                + UMI3DSerializer.Write(objectLooping.GetValue(user))
                + UMI3DSerializer.Write(objectStartTime.GetValue(user))
                + UMI3DSerializer.Write(objectPauseTime.GetValue(user))
                + ToBytesAux(user);
        }

        /// <inheritdoc/>
        protected virtual Bytable ToBytesAux(UMI3DUser user)
        {
            return new Bytable();
        }

        #region filter
        private readonly HashSet<UMI3DUserFilter> ConnectionFilters = new HashSet<UMI3DUserFilter>();

        /// <inheritdoc/>
        public bool LoadOnConnection(UMI3DUser user)
        {
            return ConnectionFilters.Count == 0 || !ConnectionFilters.Any(f => !f.Accept(user));
        }

        /// <inheritdoc/>
        public bool AddConnectionFilter(UMI3DUserFilter filter)
        {
            return ConnectionFilters.Add(filter);
        }

        /// <inheritdoc/>
        public bool RemoveConnectionFilter(UMI3DUserFilter filter)
        {
            return ConnectionFilters.Remove(filter);
        }

        #endregion
    }
}