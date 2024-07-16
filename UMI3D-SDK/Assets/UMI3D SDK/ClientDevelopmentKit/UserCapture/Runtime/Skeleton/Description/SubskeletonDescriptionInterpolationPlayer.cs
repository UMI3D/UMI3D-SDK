/*
Copyright 2019 - 2024 Inetum

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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using umi3d.common.userCapture;
using umi3d.common.userCapture.description;

using UnityEngine;

namespace umi3d.cdk.userCapture.description
{
    /// <summary>
    /// Compute a subskeleton pose from a subskeleton descriptor.
    /// </summary>
    /// Mainly used for interpolations.
    public class SubskeletonDescriptionInterpolationPlayer : ISubskeletonDescriptionInterpolationPlayer
    {
        /// <summary>
        /// Complete skeleton affected by the player.
        /// </summary>
        public ISkeleton Skeleton { get; protected set; }

        /// <summary>
        /// True if the player is currently applying a pose.
        /// </summary>
        /// Note that this value is still true after some time 
        /// after calling <see cref="End(bool)"/> because of end of application handling.
        public bool IsPlaying { get; protected set; } = false;

        /// <summary>
        /// True when the player is in the ending interpolation phase.
        /// </summary>
        public bool IsEnding { get; protected set; } = false;

        public bool ShouldInterpolate => shouldInterpolate;

        protected bool shouldInterpolate;

        public ISubskeletonDescriptor Descriptor { get; protected set; }

        protected PlayingData playingData;

        public event System.Action EndStarted;

        public event System.Action Stopped;

        protected struct PlayingData
        {
            public readonly float startTime;
            public readonly ISubskeletonDescriptionInterpolationPlayer.PlayingParameters parameters;
            public float endAskedTime;

            public PlayingData(float startTime, ISubskeletonDescriptionInterpolationPlayer.PlayingParameters parameters)
            {
                this.startTime = startTime;
                this.parameters = parameters;
                endAskedTime = -1f;
            }
        }

        #region DI

        private readonly ICoroutineService coroutineService;


        public SubskeletonDescriptionInterpolationPlayer(ISubskeletonDescriptor descriptor, bool shouldInterpolate, ISkeleton parentSkeleton, ICoroutineService coroutineService)
        {
            Descriptor = descriptor ?? throw new System.ArgumentNullException(nameof(descriptor));
            this.shouldInterpolate = shouldInterpolate;
            this.Skeleton = parentSkeleton ?? throw new System.ArgumentNullException(nameof(parentSkeleton));
            this.coroutineService = coroutineService;
        }

        public SubskeletonDescriptionInterpolationPlayer(ISubskeletonDescriptor descriptor, bool shouldInterpolate, ISkeleton parentSkeleton) : this(descriptor, shouldInterpolate, parentSkeleton, CoroutineManager.Instance)
        {
        }

        #endregion DI

        public void Play()
        {
            Play(null);
        }

        /// <inheritdoc/>
        public virtual void Play(ISubskeletonDescriptionInterpolationPlayer.PlayingParameters parameters = null)
        {
            if (IsPlaying)
                return;

            IsPlaying = true;

            playingData = new(UnityEngine.Time.time, parameters ?? new());

            if (parameters?.previousDescriptor != null)
                SwitchFrom(parameters?.startTransitionDuration ?? 0.25f);
        }

        /// <inheritdoc/>
        public virtual void End(bool shouldStopImmediate = false)
        {
            if (!IsPlaying)
                return;

            EndStarted?.Invoke();

            if (!shouldStopImmediate && ShouldInterpolate)
            {
                IsEnding = true;
                playingData.endAskedTime = Time.time;
                endRoutine = coroutineService.AttachCoroutine(EndRoutine());
            }
            else
            {
                if (IsEnding) // force end while ending
                {
                    IsEnding = false;
                    coroutineService.DetachCoroutine(endRoutine);
                    endRoutine = null;
                }
                Stop();
            }
        }

        private Coroutine endRoutine;



        private bool IsInTransition;
        private Coroutine transitionCoroutine;

        public event System.Action SwitchTransitionFinished;

        public void SwitchFrom(float transitionDuration = 0.25f)
        {
            IsInTransition = true;
            transitionCoroutine = coroutineService.AttachCoroutine(SwitchTransitionRoutine(transitionDuration));
        }

        private IEnumerator SwitchTransitionRoutine(float transitionDuration)
        {
            yield return new WaitForSeconds(transitionDuration);
            IsInTransition = false;
            if (transitionCoroutine != null)
                coroutineService.DetachCoroutine(transitionCoroutine);
            transitionCoroutine = null;
            SwitchTransitionFinished?.Invoke();
        }

        /// <summary>
        /// Coroutine handling the waiting for the ending phase.
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator EndRoutine()
        {
            yield return new WaitForSeconds(playingData.parameters.endTransitionDuration);
            IsEnding = false;
            Stop();
            coroutineService.DetachCoroutine(endRoutine);
            endRoutine = null;
        }

        protected virtual void Stop()
        {
            IsPlaying = false;
            playingData = default;
            Stopped?.Invoke();
        }

        /// <inheritdoc/>
        public virtual SubskeletonPose GetPose(UMI3DSkeletonHierarchy hierarchy)
        {
            if (hierarchy == null)
                throw new ArgumentNullException(nameof(hierarchy));

            if (!IsPlaying)
                return null;

            SubskeletonPose pose = Descriptor.GetPose(hierarchy);

            if (IsInTransition)
                return InterpolateSwitchTransitionFrom(hierarchy, playingData.parameters.previousDescriptor);

            if (pose == null
                || !ShouldInterpolate 
                || (!IsEnding && (Time.time - playingData.startTime) > playingData.parameters.startTransitionDuration)) // no interpolation needed
                return pose;

            return InterpolateTransitionPose(pose);
        }

        /// <summary>
        /// Interpolate pose with the current pose of the parent skeleton.
        /// </summary>
        /// <param name="pose"></param>
        /// <returns></returns>
        protected SubskeletonPose InterpolateTransitionPose(SubskeletonPose pose)
        {
            PoseAnchor anchor = pose.boneAnchor;

            float tEnterPhase = playingData.parameters.startTransitionDuration <= 0 ? 1 : (Time.time - playingData.startTime) / playingData.parameters.startTransitionDuration;
            float tEndPhase = playingData.parameters.endTransitionDuration <= 0 ? 1 : (Time.time - playingData.endAskedTime) / playingData.parameters.endTransitionDuration;

            // manages Hips position interpolation if it is the anchor
            if (anchor.bone is not BoneType.None && anchor.bone == BoneType.Hips)
            {
                Vector3 skeletonHipsPosition = new(0, Skeleton.Bones[BoneType.Hips].Position.y, 0);
                if (!IsEnding) // start transition
                {
                    anchor.position = Vector3.Lerp(skeletonHipsPosition, anchor.position, tEnterPhase);
                    anchor.rotation = Quaternion.Slerp(Skeleton.Bones[BoneType.Hips].Rotation, anchor.rotation, tEnterPhase);
                }
                else // end transition
                {
                    anchor.position = Vector3.Lerp(anchor.position, skeletonHipsPosition, tEndPhase);
                    anchor.rotation = Quaternion.Slerp(anchor.rotation, Skeleton.Bones[BoneType.Hips].Rotation, tEndPhase);
                }
            }

            // manages local rotation interpolation for all bones of the poses
            Dictionary<uint, SubskeletonBonePose> bonePoses = new();
            foreach (SubskeletonBonePose bonePoseOriginal in pose.bones)
            {
                if (bonePoses.ContainsKey(bonePoseOriginal.boneType) || !Skeleton.Bones.ContainsKey(bonePoseOriginal.boneType))
                    continue;

                Quaternion interpolatedLocalRotation;
                if (ShouldInterpolate && bonePoseOriginal.boneType != BoneType.Hips && bonePoseOriginal.boneType != pose.boneAnchor.bone)
                {
                    if (!IsEnding) // start transition
                        interpolatedLocalRotation = Quaternion.Slerp(Skeleton.Bones[bonePoseOriginal.boneType].LocalRotation, bonePoseOriginal.localRotation, tEnterPhase);
                    else // end transition
                        interpolatedLocalRotation = Quaternion.Slerp(bonePoseOriginal.localRotation, Skeleton.Bones[bonePoseOriginal.boneType].LocalRotation, tEndPhase);
                }
                else
                {
                    interpolatedLocalRotation = Quaternion.identity;
                }

                bonePoses[bonePoseOriginal.boneType] = new SubskeletonBonePose(bonePoseOriginal.boneType, interpolatedLocalRotation);
            }

            return new SubskeletonPose(anchor, bonePoses.Values.ToList());
        }


        private SubskeletonPose InterpolateSwitchTransitionFrom(UMI3DSkeletonHierarchy hierarchy, ISubskeletonDescriptor oldDescriptor)
        {
            SubskeletonPose newDescription = Descriptor.GetPose(hierarchy);

            SubskeletonPose currentDescription = oldDescriptor.GetPose(hierarchy);

            Dictionary<uint, SubskeletonBonePose> currentBonePoses = currentDescription.bones.ToDictionary(x => x.boneType, y => y);

            Dictionary<uint, SubskeletonBonePose> bonePoses = new();

            float normalizedTransitionTime = playingData.startTime <= 0 ? 1 : (Time.time - playingData.startTime) / playingData.parameters.startTransitionDuration;
            foreach (SubskeletonBonePose newDescriptionBonePose in newDescription.bones)
            {

                uint boneType = newDescriptionBonePose.boneType;
                if (bonePoses.ContainsKey(boneType))
                    continue;

                Quaternion interpolatedLocalRotation;
                if (currentBonePoses.ContainsKey(boneType)) // defined in current pose so interpolate
                {
                    interpolatedLocalRotation = Quaternion.Slerp(currentBonePoses[boneType].localRotation, newDescriptionBonePose.localRotation, normalizedTransitionTime);
                }
                else
                {
                    if (ShouldInterpolate && boneType != BoneType.Hips && boneType != newDescription.boneAnchor.bone)
                    {
                        if (!IsEnding) // start transition
                            interpolatedLocalRotation = Quaternion.Slerp(Skeleton.Bones[boneType].LocalRotation, newDescriptionBonePose.localRotation, normalizedTransitionTime);
                        else // end transition
                            interpolatedLocalRotation = Quaternion.Slerp(newDescriptionBonePose.localRotation, Skeleton.Bones[boneType].LocalRotation, normalizedTransitionTime);
                    }
                    else
                    {
                        interpolatedLocalRotation = Quaternion.identity;
                    }
                }

                bonePoses[boneType] = new SubskeletonBonePose(newDescriptionBonePose.boneType, interpolatedLocalRotation);
            }

            return new SubskeletonPose(newDescription.boneAnchor, bonePoses.Values.ToList());
        }
    }
}

