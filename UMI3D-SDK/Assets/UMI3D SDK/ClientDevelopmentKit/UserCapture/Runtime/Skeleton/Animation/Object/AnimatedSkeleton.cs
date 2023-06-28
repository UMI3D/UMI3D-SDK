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

using inetum.unityUtils;
using System.Collections;
using umi3d.common;
using umi3d.common.userCapture;
using umi3d.common.userCapture.animation;
using umi3d.common.userCapture.pose;
using UnityEngine;

namespace umi3d.cdk.userCapture.animation
{
    /// <summary>
    /// Subskeleton that is the target if a skeleton animation, using an Animator.
    /// </summary>
    public class AnimatedSkeleton : ISubSkeleton
    {
        private const DebugScope DEBUG_SCOPE = DebugScope.CDK | DebugScope.Animation | DebugScope.UserCapture;

        /// <summary>
        /// Reference to the skeleton mapper that computes related links into a pose.
        /// </summary>
        public virtual SkeletonMapper Mapper { get; protected set; }

        /// <summary>
        /// Priority level of the animated skeleton.
        /// </summary>
        public virtual uint Priority { get; protected set; }

        /// <summary>
        /// Animation id of the animated skeleton.
        /// </summary>
        public virtual UMI3DAnimatorAnimation[] Animations { get; protected set; }

        /// <summary>
        /// Parameters required by the animator that are updated by the browser itself.
        /// </summary>
        /// The key correspond to a key in <see cref="SkeletonAnimatorParameterKeys"/>.
        public virtual uint[] SelfUpdatedAnimatorParameters { get; protected set; }

        /// <summary>
        /// Cached coroutine of parameters self update.
        /// </summary>
        private Coroutine updateParameterRoutine;

        private readonly ICoroutineService coroutineService;

        public AnimatedSkeleton(SkeletonMapper mapper, UMI3DAnimatorAnimation[] animations, uint priority = 0, uint[] selfUpdatedAnimatorParameters = null)
        {
            Mapper = mapper;
            Priority = priority;
            Animations = animations;
            SelfUpdatedAnimatorParameters = selfUpdatedAnimatorParameters ?? new uint[0];
            coroutineService = CoroutineManager.Instance;
        }

        public AnimatedSkeleton(SkeletonMapper mapper, UMI3DAnimatorAnimation[] animations, uint priority, uint[] selfUpdatedAnimatorParameters, ICoroutineService coroutineService)
        {
            Mapper = mapper;
            Priority = priority;
            Animations = animations;
            SelfUpdatedAnimatorParameters = selfUpdatedAnimatorParameters;
            this.coroutineService = coroutineService;
        }

        ///<inheritdoc/>
        /// Always returns null for AnimatonSkeleton.
        public virtual UserCameraPropertiesDto GetCameraDto()
        {
            return null;
        }

        /// <summary>
        /// Get the skeleton pose based on the position of this AnimationSkeleton.
        /// </summary>
        /// <returns></returns>
        public virtual PoseDto GetPose()
        {
            foreach (var anim in Animations)
            {
                if (anim?.IsPlaying() ?? false)
                {
                    return Mapper.GetPose();
                }
            }
            return null;
        }

        /// <summary>
        /// Start animation parameters self update. Parameters are recomputed each frame based on the <paramref name="skeleton"/> movement.
        /// </summary>
        public void StartParameterSelfUpdate(ISkeleton skeleton)
        {
            if (skeleton == null)
                throw new System.ArgumentNullException("Skeleton to auto update is not defined.");

            if (SelfUpdatedAnimatorParameters.Length > 0)
            {
                updateParameterRoutine = coroutineService.AttachCoroutine(UpdateParametersRoutine(skeleton));
                UMI3DClientServer.Instance.OnLeavingEnvironment.AddListener(() => { if (updateParameterRoutine is not null) coroutineService.DettachCoroutine(updateParameterRoutine); });
            }
        }

        /// <summary>
        /// Stop animation parameters self update.
        /// </summary>
        public void StopParameterSelfUpdate()
        {
            if (updateParameterRoutine is not null)
                coroutineService.DettachCoroutine(updateParameterRoutine);
        }

        /// <summary>
        /// Update animator parameters directly from the browser, every frame, based on the <paramref name="skeleton"/> movement.
        /// </summary>
        /// <param name="skeleton"></param>
        /// <returns></returns>
        private IEnumerator UpdateParametersRoutine(ISkeleton skeleton)
        {
            Vector3 previousPosition = Vector3.zero;

            while (true)
            {
                foreach (var parameter in SelfUpdatedAnimatorParameters)
                {
                    (string name, UMI3DAnimatorParameterType type, object valueParameter) = parameter switch
                    {
                        (uint)SkeletonAnimatorParameterKeys.SPEED => ("SPEED", UMI3DAnimatorParameterType.Float, (skeleton.HipsAnchor.transform.position - previousPosition).magnitude / Time.deltaTime),
                        (uint)SkeletonAnimatorParameterKeys.SPEED_X => ("SPEED_X", UMI3DAnimatorParameterType.Float, Mathf.Abs(skeleton.HipsAnchor.transform.position.x - previousPosition.x) / Time.deltaTime),
                        (uint)SkeletonAnimatorParameterKeys.SPEED_Y => ("SPEED_Y", UMI3DAnimatorParameterType.Float, Mathf.Abs(skeleton.HipsAnchor.transform.position.y - previousPosition.y) / Time.deltaTime),
                        (uint)SkeletonAnimatorParameterKeys.SPEED_Z => ("SPEED_Z", UMI3DAnimatorParameterType.Float, Mathf.Abs(skeleton.HipsAnchor.transform.position.z - previousPosition.z) / Time.deltaTime),
                        (uint)SkeletonAnimatorParameterKeys.SPEED_X_Y => ("SPEED_X_Y", UMI3DAnimatorParameterType.Float, (Vector3.ProjectOnPlane(skeleton.HipsAnchor.transform.position, Vector3.up) - Vector3.ProjectOnPlane(previousPosition, Vector3.up)).magnitude / Time.deltaTime),
                        _ => default
                    };

                    if (name != default)
                        UpdateParameter(name, (uint)type, valueParameter);
                }
                previousPosition = skeleton.HipsAnchor.transform.position;

                yield return null;
            }
        }

        /// <summary>
        ///  Apply a parameter change to every animation of the associated animator
        /// </summary>
        /// <param name="name"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        private void UpdateParameter(string name, uint key, object value)
        {
            UMI3DAnimatorParameterDto parameterDto = new UMI3DAnimatorParameterDto() { type = (int)key, value = value };

            foreach (var anim in Animations)
            {
                anim.ApplyParameter(name, parameterDto);
            }
        }
    }
}