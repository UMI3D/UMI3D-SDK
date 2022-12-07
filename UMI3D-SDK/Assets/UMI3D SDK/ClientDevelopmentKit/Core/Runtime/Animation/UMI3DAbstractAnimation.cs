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

using MainThreadDispatcher;
using System.Collections;
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    /// <summary>
    /// Abstract class for all components that could be played.
    /// </summary>
    public abstract class UMI3DAbstractAnimation
    {
        /// <summary>
        /// Get an animation by id.
        /// </summary>
        /// <param name="id">UMI3D id of the animation.</param>
        /// <returns></returns>
        public static UMI3DAbstractAnimation Get(ulong id) { return UMI3DEnvironmentLoader.GetEntity(id)?.Object as UMI3DAbstractAnimation; }

        /// <summary>
        /// Update an UMI3D Property.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public virtual bool SetUMI3DProperty(UMI3DEntityInstance entity, SetEntityPropertyDto property)
        {
            switch (property.property)
            {
                case UMI3DPropertyKeys.AnimationPlaying:
                    bool old = dto.playing;
                    dto.playing = (bool)property.value;
                    if (old != dto.playing)
                    {
                        if (dto.playing)
                        {
                            if (dto.startTime == default)
                            {
                                (entity.Object as UMI3DAbstractAnimation).Start();
                            }
                            else
                            {
                                (entity.Object as UMI3DAbstractAnimation).Start(UMI3DClientServer.Instance.GetTime() - dto.startTime);
                            }
                        }
                        else
                        {
                            (entity.Object as UMI3DAbstractAnimation).Stop();
                        }
                    }
                    break;
                case UMI3DPropertyKeys.AnimationLooping:
                    dto.looping = (bool)property.value;
                    if (dto is UMI3DVideoPlayerDto)
                    {
                        (entity.Object as UMI3DVideoPlayer).SetLoopValue(dto.looping);
                    }
                    break;
                case UMI3DPropertyKeys.AnimationStartTime:
                    dto.startTime = (ulong)(long)property.value;
                    break;
                case UMI3DPropertyKeys.AnimationPauseFrame:
                    dto.pauseTime = (long)property.value;
                    SetProgress(dto.pauseTime);
                    break;
                default:
                    return false;
            }
            return true;
        }

        public virtual bool SetUMI3DProperty(UMI3DEntityInstance entity, uint operationId, uint propertyKey, ByteContainer container)
        {
            switch (propertyKey)
            {
                case UMI3DPropertyKeys.AnimationPlaying:
                    bool old = dto.playing;
                    dto.playing = UMI3DNetworkingHelper.Read<bool>(container);
                    if (old != dto.playing)
                    {
                        if (dto.playing)
                        {
                            if (dto.startTime == default)
                            {
                                (entity.Object as UMI3DAbstractAnimation).Start();
                            }
                            else
                            {
                                (entity.Object as UMI3DAbstractAnimation).Start(UMI3DClientServer.Instance.GetTime() - dto.startTime);
                            }
                        }
                        else
                        {
                            (entity.Object as UMI3DAbstractAnimation).Stop();
                        }
                    }
                    break;
                case UMI3DPropertyKeys.AnimationLooping:
                    dto.looping = UMI3DNetworkingHelper.Read<bool>(container);
                    if (dto is UMI3DVideoPlayerDto)
                    {
                        (entity.Object as UMI3DVideoPlayer).SetLoopValue(dto.looping);
                    }
                    break;
                case UMI3DPropertyKeys.AnimationStartTime:
                    dto.startTime = UMI3DNetworkingHelper.Read<ulong>(container);
                    break;
                case UMI3DPropertyKeys.AnimationPauseFrame:
                    dto.pauseTime = UMI3DNetworkingHelper.Read<long>(container);
                    SetProgress(dto.pauseTime);
                    break;
                default:
                    return false;
            }
            return true;
        }

        public static bool ReadUMI3DProperty(ref object value, uint propertyKey, ByteContainer container)
        {
            switch (propertyKey)
            {
                case UMI3DPropertyKeys.AnimationPlaying:
                    value = UMI3DNetworkingHelper.Read<bool>(container);
                    break;
                case UMI3DPropertyKeys.AnimationLooping:
                    value = UMI3DNetworkingHelper.Read<bool>(container);
                    break;
                case UMI3DPropertyKeys.AnimationStartTime:
                    value = UMI3DNetworkingHelper.Read<ulong>(container);
                    break;
                case UMI3DPropertyKeys.AnimationPauseFrame:
                    value = UMI3DNetworkingHelper.Read<long>(container);
                    break;
                default:
                    if (UMI3DAnimation.ReadMyUMI3DProperty(ref value, propertyKey, container))
                        return true;
                    if (UMI3DAudioPlayer.ReadMyUMI3DProperty(ref value, propertyKey, container))
                        return true;
                    return UMI3DNodeAnimation.ReadMyUMI3DProperty(ref value, propertyKey, container);

            }
            return true;
        }

        /// <summary>
        /// DTO local copy.
        /// </summary>
        protected UMI3DAbstractAnimationDto dto { get; set; }

        public UMI3DAbstractAnimation(UMI3DAbstractAnimationDto dto)
        {
            this.dto = dto;
            UMI3DEntityInstance node = UMI3DEnvironmentLoader.RegisterEntityInstance(dto.id, dto, this);
            if (dto.playing)
            {
                if (dto.startTime == default)
                    UnityMainThreadDispatcher.Instance().Enqueue(StartNextFrame());
                else
                    UnityMainThreadDispatcher.Instance().Enqueue(StartNextFrameAt(UMI3DClientServer.Instance.GetTime() - dto.startTime));
            }
            UnityMainThreadDispatcher.Instance().Enqueue(node.NotifyLoaded);
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
        /// Set the current progress of the animation between 0 (start) and 1 (end).
        /// </summary>
        /// <returns></returns>
        public abstract void SetProgress(long frame);

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
        /// Performed at the end of the animation.
        /// </summary>
        public virtual void OnEnd()
        {
            if (dto.looping)
            {
                Start();
            }
        }
    }
}