/*
Copyright 2019 - 2023 Inetum
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

using MainThreadDispatcher;
using System;
using System.Collections;
using System.ComponentModel;
using System.Threading.Tasks;
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    /// <summary>
    /// Abstract class for all entities that could be played.
    /// </summary>
    public abstract class UMI3DAbstractAnimation : AbstractLoader, IEntity
    {
        /// <summary>
        /// Get an animation by id.
        /// </summary>
        /// <param name="id">UMI3D id of the animation.</param>
        /// <returns></returns>
        [Obsolete("Use UMI3DEnvironmentLoader.Instance.GetEntity<UMI3DAbstractAnimation> instead.")]
        public static UMI3DAbstractAnimation Get(ulong id) { return UMI3DEnvironmentLoader.GetEntity(id)?.Object as UMI3DAbstractAnimation; }

        /// <summary>
        /// Is the animation currently playing?
        /// </summary>
        /// <returns></returns>
        public abstract bool IsPlaying();

        /// <inheritdoc/>
        public override bool CanReadUMI3DExtension(ReadUMI3DExtensionData data)
        {
            return false;
        }

        /// <summary>
        /// This Method should not be called to load an AbstractAnimation.
        /// Please use <see cref="UMI3DAnimationLoader.ReadUMI3DExtension(ReadUMI3DExtensionData)"/> instead.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="Umi3dException"></exception>
        /// <see cref="UMI3DAnimationLoader.ReadUMI3DExtension(ReadUMI3DExtensionData)"/>
        public override Task ReadUMI3DExtension(ReadUMI3DExtensionData value)
        {
            throw new Umi3dException("This Method should not be called to load an AbstractAnimation.\nPlease use UMI3DAnimationLoader.ReadUMI3DExtension(<>) instead.");
        }

        /// <summary>
        /// This Method should not be called to Read an AbstractAnimation.
        /// Please use <see cref="UMI3DAnimationLoader.ReadUMI3DProperty(ReadUMI3DPropertyData)"/> instead.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="Umi3dException"></exception>
        /// <see cref="UMI3DAnimationLoader.ReadUMI3DProperty(ReadUMI3DPropertyData)"
        public override Task<bool> ReadUMI3DProperty(ReadUMI3DPropertyData value)
        {
            throw new Umi3dException("This Method should not be called to load an AbstractAnimation.\nPlease use UMI3DAnimationLoader.ReadUMI3DProperty(<>) instead.");
        }

        /// <inheritdoc/>
        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyData value)
        {
            switch (value.property.property)
            {
                case UMI3DPropertyKeys.AnimationPlaying:
                    bool old = dto.playing;
                    dto.playing = (bool)value.property.value;
                    if (old != dto.playing)
                    {
                        if (dto.playing)
                        {
                            if (dto.startTime == default)
                            {
                                (value.entity.Object as UMI3DAbstractAnimation).Start();
                            }
                            else
                            {
                                (value.entity.Object as UMI3DAbstractAnimation).Start(UMI3DClientServer.Instance.GetTime() - dto.startTime);
                            }
                        }
                        else
                        {
                            (value.entity.Object as UMI3DAbstractAnimation).Stop();
                        }
                    }
                    break;
                case UMI3DPropertyKeys.AnimationLooping:
                    dto.looping = (bool)value.property.value;
                    if (dto is UMI3DVideoPlayerDto)
                    {
                        (value.entity.Object as UMI3DVideoPlayer).SetLoopValue(dto.looping);
                    }
                    break;
                case UMI3DPropertyKeys.AnimationStartTime:
                    dto.startTime = (ulong)(long)value.property.value;
                    break;
                case UMI3DPropertyKeys.AnimationPauseFrame:
                    dto.pauseTime = (long)value.property.value;
                    SetProgress(dto.pauseTime);
                    break;
                default:
                    return await Task.FromResult(false);
            }
            return await Task.FromResult(true);
        }

        /// <inheritdoc/>
        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyContainerData value)
        {
            switch (value.propertyKey)
            {
                case UMI3DPropertyKeys.AnimationPlaying:
                    bool oldPlayingState = dto.playing;
                    dto.playing = UMI3DSerializer.Read<bool>(value.container);
                    if (oldPlayingState != dto.playing)
                    {
                        if (dto.playing)
                        {
                            if (dto.startTime == default)
                            {
                                (value.entity.Object as UMI3DAbstractAnimation).Start();
                            }
                            else
                            {
                                (value.entity.Object as UMI3DAbstractAnimation).Start(UMI3DClientServer.Instance.GetTime() - dto.startTime);
                            }
                        }
                        else
                        {
                            (value.entity.Object as UMI3DAbstractAnimation).Stop();
                        }
                    }
                    break;
                case UMI3DPropertyKeys.AnimationLooping:
                    dto.looping = UMI3DSerializer.Read<bool>(value.container);
                    if (dto is UMI3DVideoPlayerDto)
                    {
                        (value.entity.Object as UMI3DVideoPlayer).SetLoopValue(dto.looping);
                    }
                    break;
                case UMI3DPropertyKeys.AnimationStartTime:
                    dto.startTime = UMI3DSerializer.Read<ulong>(value.container);
                    break;
                case UMI3DPropertyKeys.AnimationPauseFrame:
                    dto.pauseTime = UMI3DSerializer.Read<long>(value.container);
                    SetProgress(dto.pauseTime);
                    break;
                default:
                    return await Task.FromResult(false);
            }
            return await Task.FromResult(true);
        }

        /// <summary>
        /// DTO local copy.
        /// </summary>
        protected UMI3DAbstractAnimationDto dto { get; set; }

        /// <summary>
        /// Animation UMI3D id.
        /// </summary>
        public ulong Id => dto.id;

        public UMI3DAbstractAnimation(UMI3DAbstractAnimationDto dto)
        {
            this.dto = dto;
        }

        /// <summary>
        /// Initialize the animation.
        /// </summary>
        /// Used to start directly animations or to find out usefuls components.
        public virtual void Init()
        {
#if !UNITY_EDITOR
            if (dto.playing)
            {
                if (dto.startTime == default)
                    UnityMainThreadDispatcher.Instance().Enqueue(StartNextFrame());
                else
                    UnityMainThreadDispatcher.Instance().Enqueue(StartNextFrameAt(UMI3DClientServer.Instance.GetTime() - dto.startTime));
            }
#endif
        }

        /// <summary>
        /// Call start method next frame.
        /// </summary>
        /// <returns></returns>
        private IEnumerator StartNextFrame()
        {
            yield return new WaitForEndOfFrame();

            if (dto.playing)
                Start();
        }


        /// <summary>
        /// Call start method next frame.
        /// </summary>
        /// <returns></returns>
        private IEnumerator StartNextFrameAt(float time)
        {
            yield return new WaitForEndOfFrame();
            Start(time);
        }

        /// <summary>
        /// Remove the animation from the environment.
        /// </summary>
        public async void Destroy()
        {
            await UMI3DEnvironmentLoader.DeleteEntity(dto.id);
        }

        /// <summary>
        /// Get the current progress of the animation between 0 (start) and 1 (end).
        /// </summary>
        /// <returns></returns>
        public abstract float GetProgress();

        /// <summary>
        /// Set the current progress of the animation by time in milliseconds.
        /// </summary>
        /// <returns></returns>
        public abstract void SetProgress(long time);

        /// <summary>
        /// Play the animation.
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// Play animation starting from <paramref name="atTime"/>, a value between 0 and 1.
        /// </summary>
        /// <param name="atTime">Time in ms</param>
        public abstract void Start(float atTime);

        /// <summary>
        /// Interrupt the animation.
        /// </summary>
        public abstract void Stop();

        /// <summary>
        /// Triggered when an animation reaches its end.
        /// </summary>
        public event Action AnimationEnded;

        /// <summary>
        /// Performed at the end of the animation.
        /// </summary>
        public virtual void OnEnd()
        {
            if (dto.looping)
                Start();
            else
                AnimationEnded?.Invoke();
        }
    }
}