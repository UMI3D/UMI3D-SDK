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
using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using umi3d.common.userCapture.animation;
using umi3d.common.userCapture.description;
using umi3d.common.userCapture.pose;
using umi3d.common.userCapture.tracking;
using umi3d.common.utils;
using UnityEngine;

namespace umi3d.cdk.userCapture.animation
{
    /// <summary>
    /// Subskeleton that is the target if a skeleton animation, using an Animator.
    /// </summary>
    public class AnimatedSubskeleton : ISubskeleton
    {
        private const DebugScope DEBUG_SCOPE = DebugScope.CDK | DebugScope.Animation | DebugScope.UserCapture;

        #region Properties

        /// <summary>
        /// Reference to the skeleton mapper that computes related links into a pose.
        /// </summary>
        public virtual ISkeletonMapper Mapper { get; protected set; }

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
        public virtual SkeletonAnimationParameter[] SelfUpdatedAnimatorParameters { get; protected set; }

        #endregion Properties

        /// <summary>
        /// See <see cref="SkeletonAnimationParameterDto"/>.
        /// </summary>
        public class SkeletonAnimationParameter
        {
            public SkeletonAnimationParameter(SkeletonAnimationParameterDto dto)
            {
                parameterKey = dto.parameterKey;
                ranges = dto.ranges?.Select(dtoRange => new Range()
                {
                    startBound = dtoRange.startBound,
                    endBound = dtoRange.endBound,
                    rawValue = dtoRange.rawValue,
                    result = dtoRange.result
                }).ToArray() ?? new Range[0];
            }

            public uint parameterKey;

            public Range[] ranges;

            public struct Range
            {
                public float startBound;
                public float endBound;
                public float result;
                public bool rawValue;
            }

            #region Optimization

            public float MinRange
            {
                get
                {
                    if (!hasLookedForMinRange)
                    {
                        _minRange = ranges.Select(x => x.startBound).Min();
                        hasLookedForMinRange = true;
                    }

                    return _minRange;
                }
            }

            private float _minRange;
            private bool hasLookedForMinRange;

            public float MaxRange
            {
                get
                {
                    if (!hasLookedForMaxRange)
                    {
                        _maxRange = ranges.Select(x => x.endBound).Max();
                        hasLookedForMaxRange = true;
                    }

                    return _maxRange;
                }
            }

            private float _maxRange;
            private bool hasLookedForMaxRange;

            #endregion Optimization
        }

        /// <summary>
        /// Cached coroutine of parameters self update.
        /// </summary>
        private Coroutine updateParameterRoutine;

        #region Dependency Injection

        private readonly ICoroutineService coroutineService;
        private readonly IUnityMainThreadDispatcher unityMainThreadDispatcher;

        public AnimatedSubskeleton(ISkeletonMapper mapper, UMI3DAnimatorAnimation[] animations, uint priority, SkeletonAnimationParameterDto[] selfUpdatedAnimatorParameters,
                                    ICoroutineService coroutineService, IUnityMainThreadDispatcher unityMainThreadDispatcher)
        {
            Mapper = mapper;
            Priority = priority;
            Animations = animations;
            SelfUpdatedAnimatorParameters = selfUpdatedAnimatorParameters?.Select(dto => new SkeletonAnimationParameter(dto)).ToArray()
                                                ?? new SkeletonAnimationParameter[0];
            this.coroutineService = coroutineService;
            this.unityMainThreadDispatcher = unityMainThreadDispatcher;
        }

        #endregion Dependency Injection

        public AnimatedSubskeleton(ISkeletonMapper mapper, UMI3DAnimatorAnimation[] animations, uint priority = 0, SkeletonAnimationParameterDto[] selfUpdatedAnimatorParameters = null)
        {
            Mapper = mapper;
            Priority = priority;
            Animations = animations;
            SelfUpdatedAnimatorParameters = selfUpdatedAnimatorParameters?.Select(dto => new SkeletonAnimationParameter(dto)).ToArray()
                                                ?? new SkeletonAnimationParameter[0];
            coroutineService = CoroutineManager.Instance;
            unityMainThreadDispatcher = UnityMainThreadDispatcherManager.Instance;
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

        #region ParameterSelfUpdate

        /// <summary>
        /// Start animation parameters self update. Parameters are recomputed each frame based on the <paramref name="skeleton"/> movement.
        /// </summary>
        public void StartParameterSelfUpdate(ISkeleton skeleton)
        {
            if (skeleton == null)
                throw new System.ArgumentNullException("Skeleton to auto update is not defined.");

            if (SelfUpdatedAnimatorParameters.Length > 0)
            {
                // coroutine is modifying an animator and thus require to be put on main thread
                unityMainThreadDispatcher.Enqueue(() =>
                {
                    updateParameterRoutine = coroutineService.AttachCoroutine(UpdateParametersRoutine(skeleton));
                    UMI3DClientServer.Instance.OnLeavingEnvironment.AddListener(StopParameterSelfUpdate);
                });
            }
        }

        /// <summary>
        /// Stop animation parameters self update.
        /// </summary>
        public void StopParameterSelfUpdate()
        {
            unityMainThreadDispatcher.Enqueue(() =>
            {
                if (updateParameterRoutine is not null)
                    coroutineService.DettachCoroutine(updateParameterRoutine);

                UMI3DClientServer.Instance.OnLeavingEnvironment.RemoveListener(StopParameterSelfUpdate);
            });
        }

        /// <summary>
        /// Update animator parameters directly from the browser, every frame, based on the <paramref name="skeleton"/> movement.
        /// </summary>
        /// <param name="skeleton"></param>
        /// <returns></returns>
        private IEnumerator UpdateParametersRoutine(ISkeleton skeleton)
        {
            Vector3 previousPosition = Vector3.zero;
            Dictionary<uint, float> previousValues = new();

            while (skeleton != null && skeleton.HipsAnchor != null)
            {
                foreach (var parameter in SelfUpdatedAnimatorParameters)
                {
                    (string name, UMI3DAnimatorParameterType typeKey, float valueParameter) = parameter.parameterKey switch
                    {
                        (uint)SkeletonAnimatorParameterKeys.SPEED => ("SPEED", UMI3DAnimatorParameterType.Float, (skeleton.HipsAnchor.position - previousPosition).magnitude / Time.deltaTime),
                        (uint)SkeletonAnimatorParameterKeys.SPEED_X => ("SPEED_X", UMI3DAnimatorParameterType.Float, Mathf.Abs(skeleton.HipsAnchor.position.x - previousPosition.x) / Time.deltaTime),
                        (uint)SkeletonAnimatorParameterKeys.SPEED_Y => ("SPEED_Y", UMI3DAnimatorParameterType.Float, Mathf.Abs(skeleton.HipsAnchor.position.y - previousPosition.y) / Time.deltaTime),
                        (uint)SkeletonAnimatorParameterKeys.SPEED_Z => ("SPEED_Z", UMI3DAnimatorParameterType.Float, Mathf.Abs(skeleton.HipsAnchor.position.z - previousPosition.z) / Time.deltaTime),
                        (uint)SkeletonAnimatorParameterKeys.SPEED_X_Y => ("SPEED_X_Y", UMI3DAnimatorParameterType.Float, (Vector3.ProjectOnPlane(skeleton.HipsAnchor.position, Vector3.up) - Vector3.ProjectOnPlane(previousPosition, Vector3.up)).magnitude / Time.deltaTime),
                        _ => default
                    };

                    bool inited = previousValues.TryGetValue(parameter.parameterKey, out float previousValue);
                    if (name != default
                                && (!inited || IsChangeSignificant(previousValue, valueParameter)))
                    {
                        if (parameter.ranges.Length > 0)
                        {
                            valueParameter = ApplyRanges(parameter, valueParameter);
                        }

                        if (!inited || valueParameter != previousValues[parameter.parameterKey])
                        {
                            UpdateParameter(name, (uint)typeKey, valueParameter);
                            previousValues[parameter.parameterKey] = valueParameter;
                        }
                    }
                }
                previousPosition = skeleton.HipsAnchor.transform.position;

                yield return null;
            }
        }

        /// <summary>
        ///  Applies range parameter settings.
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private float ApplyRanges(SkeletonAnimationParameter parameter, float value)
        {
            if (value < parameter.MinRange)
                return parameter.MinRange;

            if (value > parameter.MaxRange)
                return parameter.MaxRange;

            foreach (var range in parameter.ranges)
            {
                if (range.startBound <= value && value <= range.endBound)
                {
                    if (!range.rawValue)
                        value = range.result;
                    return value;
                }
            }
            return value;
        }

        /// <summary>
        /// Tell if a value has changed significantly compared to a threshold.
        /// </summary>
        /// <param name="previousValue"></param>
        /// <param name="newValue"></param>
        /// <param name="threshold"></param>
        /// <returns></returns>
        private bool IsChangeSignificant(float previousValue, float newValue, float threshold = 0.05f)
        {
            if (previousValue == newValue)
                return false;

            if (newValue == 0f && previousValue != 0f) // avoid slow movement persistance
                return true;

            var den = previousValue == 0 ? 1f : previousValue;

            if (Mathf.Abs(newValue - previousValue) / den > threshold)
                return true;

            return false;
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
                anim?.ApplyParameter(name, parameterDto);
            }
        }

        #endregion ParameterSelfUpdate
    }
}