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
using umi3d.common.utils;
using UnityEngine;

namespace umi3d.cdk.userCapture.animation
{
    /// <summary>
    /// Subskeleton that is the target if a skeleton animation, using an Animator.
    /// </summary>
    public class AnimatedSubskeleton : IAnimatedSubskeleton
    {
        private const DebugScope DEBUG_SCOPE = DebugScope.CDK | DebugScope.Animation | DebugScope.UserCapture;

        #region Properties

        /// <inheritdoc/>
        public virtual ISkeletonMapper Mapper { get; protected set; }

        /// <inheritdoc/>
        public virtual int Priority { get; protected set; }

        /// <inheritdoc/>
        public virtual IReadOnlyList<UMI3DAnimatorAnimation> Animations => animations;

        private readonly List<UMI3DAnimatorAnimation> animations;

        /// <inheritdoc/>
        public virtual IReadOnlyList<SkeletonAnimationParameter> SelfUpdatedAnimatorParameters => selfUpdatedAnimatorParameters;

        private readonly List<SkeletonAnimationParameter> selfUpdatedAnimatorParameters;

        #endregion Properties

        /// <summary>
        /// See <see cref="SkeletonAnimationParameterDto"/>.
        /// </summary>
        public class SkeletonAnimationParameter
        {
            public SkeletonAnimationParameter(SkeletonAnimationParameterDto dto)
            {
                parameterName = dto.parameterName;
                parameterKey = dto.parameterKey;
                ranges = dto.ranges?.Select(dtoRange => new Range()
                {
                    startBound = dtoRange.startBound,
                    endBound = dtoRange.endBound,
                    useRawValue = dtoRange.rawValue,
                    result = dtoRange.result
                }).ToList() ?? new(0);
            }

            public string parameterName;
            public uint parameterKey;

            public readonly List<Range> ranges;

            public Range? lastRangeWithin;

            public struct Range
            {
                public float startBound;
                public float endBound;
                public float result;
                public bool useRawValue;

                public readonly bool Contains(in float value)
                {
                    return startBound <= value && value <= endBound;
                }

                public readonly float GetOutput(in float value)
                {
                    return useRawValue ? value : result;
                }
            }

            /// <summary>
            ///  Applies range parameter settings.
            /// </summary>
            /// <param name="parameter"></param>
            /// <param name="value"></param>
            /// <returns></returns>
            public float ApplyRanges(in float value)
            {
                if (lastRangeWithin.HasValue)
                {
                    Range range = lastRangeWithin.Value;
                    if (range.Contains(value))
                        return range.GetOutput(value);
                    else
                        lastRangeWithin = null;
                }

                if (value < MinRange)
                    return value;

                if (value > MaxRange)
                    return value;

                foreach (Range range in ranges)
                {
                    if (range.Contains(value))
                    {
                        lastRangeWithin = range;
                        return range.GetOutput(value);
                    }
                }
                return value;
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

        public AnimatedSubskeleton(ISkeletonMapper mapper, IEnumerable<UMI3DAnimatorAnimation> animations, int priority, IEnumerable<SkeletonAnimationParameterDto> selfUpdatedAnimatorParameters,
                                    ICoroutineService coroutineService, IUnityMainThreadDispatcher unityMainThreadDispatcher)
        {
            Mapper = mapper;
            Priority = priority;
            this.animations = animations?.ToList() ?? throw new System.ArgumentNullException();
            this.selfUpdatedAnimatorParameters = selfUpdatedAnimatorParameters?.Select(dto => new SkeletonAnimationParameter(dto)).ToList() ?? new(0);
            this.coroutineService = coroutineService;
            this.unityMainThreadDispatcher = unityMainThreadDispatcher;
        }

        #endregion Dependency Injection

        public AnimatedSubskeleton(ISkeletonMapper mapper, IEnumerable<UMI3DAnimatorAnimation> animations, int priority = 0, IEnumerable<SkeletonAnimationParameterDto> selfUpdatedAnimatorParameters = null)
        {
            Mapper = mapper;
            Priority = priority;
            this.animations = animations?.ToList() ?? throw new System.ArgumentNullException();
            this.selfUpdatedAnimatorParameters = selfUpdatedAnimatorParameters?.Select(dto => new SkeletonAnimationParameter(dto)).ToList() ?? new(0);
            coroutineService = CoroutineManager.Instance;
            unityMainThreadDispatcher = UnityMainThreadDispatcherManager.Instance;
        }

        /// <summary>
        /// Get the skeleton pose based on the position of this AnimationSkeleton.
        /// </summary>
        /// <returns></returns>
        public virtual SubSkeletonPoseDto GetPose(UMI3DSkeletonHierarchy hierarchy)
        {
            foreach (var anim in Animations)
            {
                if (anim?.IsPlaying() ?? false)
                {
                    return Mapper.GetPose(hierarchy);
                }
            }
            return null;
        }

        #region ParameterSelfUpdate

        /// <inheritdoc/>
        public virtual void StartParameterSelfUpdate(ISkeleton skeleton)
        {
            if (skeleton == null)
                throw new System.ArgumentNullException("Skeleton to auto update is not defined.");

            if (SelfUpdatedAnimatorParameters.Count > 0)
            {
                // coroutine is modifying an animator and thus require to be put on main thread
                unityMainThreadDispatcher.Enqueue(() =>
                {
                    updateParameterRoutine = coroutineService.AttachCoroutine(UpdateParametersRoutine(skeleton));
                    UMI3DClientServer.Instance.OnLeavingEnvironment.AddListener(StopParameterSelfUpdate);
                });
            }
        }

        /// <inheritdoc/>
        public virtual void StopParameterSelfUpdate()
        {
            unityMainThreadDispatcher.Enqueue(() =>
            {
                if (updateParameterRoutine is not null)
                    coroutineService.DetachCoroutine(updateParameterRoutine);

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
            Dictionary<string, object> previousValues = new();

            while (skeleton != null)
            {
                var lastFrame = skeleton.LastFrame;
                if (lastFrame == null || lastFrame.speed == null)
                {
                    yield return null;
                    continue;
                }
                foreach (var parameter in SelfUpdatedAnimatorParameters)
                {
                    (UMI3DAnimatorParameterType typeKey, object valueParameter) = parameter.parameterKey switch
                    {
                        (uint)SkeletonAnimatorParameterKeys.SPEED => (UMI3DAnimatorParameterType.Float, lastFrame.speed.Struct().magnitude),
                        (uint)SkeletonAnimatorParameterKeys.SPEED_X => (UMI3DAnimatorParameterType.Float, lastFrame.speed.X),
                        (uint)SkeletonAnimatorParameterKeys.SPEED_ABS_X => (UMI3DAnimatorParameterType.Float, Mathf.Abs(lastFrame.speed.X)),
                        (uint)SkeletonAnimatorParameterKeys.SPEED_Y => (UMI3DAnimatorParameterType.Float, lastFrame.speed.Y),
                        (uint)SkeletonAnimatorParameterKeys.SPEED_ABS_Y => (UMI3DAnimatorParameterType.Float, Mathf.Abs(lastFrame.speed.Y)),
                        (uint)SkeletonAnimatorParameterKeys.SPEED_Z => (UMI3DAnimatorParameterType.Float, lastFrame.speed.Z),
                        (uint)SkeletonAnimatorParameterKeys.SPEED_ABS_Z => (UMI3DAnimatorParameterType.Float, Mathf.Abs(lastFrame.speed.Z)),
                        (uint)SkeletonAnimatorParameterKeys.SPEED_X_Z => (UMI3DAnimatorParameterType.Float, Vector3.ProjectOnPlane(lastFrame.speed.Struct(), Vector3.up).magnitude),
                        (uint)SkeletonAnimatorParameterKeys.GROUNDED => (UMI3DAnimatorParameterType.Bool, (object)lastFrame.grounded),
                        (uint)SkeletonAnimatorParameterKeys.CROUCH => (UMI3DAnimatorParameterType.Bool, (object)lastFrame.crouching),
                        (uint)SkeletonAnimatorParameterKeys.JUMP => (UMI3DAnimatorParameterType.Bool, (object)lastFrame.jumping),
                        _ => default
                    };

                    if (parameter.parameterName != null)
                    {
                        bool inited = previousValues.TryGetValue(parameter.parameterName, out object previousValue);
                        if (!inited || IsChangeSignificant(previousValue, valueParameter, typeKey))
                        {
                            if (parameter.ranges.Count > 0 && typeKey == UMI3DAnimatorParameterType.Float)
                            {
                                valueParameter = parameter.ApplyRanges((float)valueParameter);
                            }

                            if (!inited || valueParameter != previousValues[parameter.parameterName])
                            {
                                UpdateParameter(parameter.parameterName, (uint)typeKey, valueParameter);
                                previousValues[parameter.parameterName] = valueParameter;
                            }
                        }
                    }
                }

                yield return null;
            }
        }

        /// <summary>
        /// Tell if a value has changed significantly compared to a threshold.
        /// </summary>
        /// <param name="previousValue"></param>
        /// <param name="newValue"></param>
        /// <param name="threshold"></param>
        /// <returns></returns>
        private bool IsChangeSignificant(object previousValue, object newValue, UMI3DAnimatorParameterType typeKey, float threshold = 0.05f)
        {
            if (previousValue == newValue)
                return false;

            if (typeKey == UMI3DAnimatorParameterType.Float)
                return IsChangeSignificant((float)previousValue, (float)newValue, threshold);
            switch (typeKey)
            {
                case UMI3DAnimatorParameterType.Bool:
                    return IsChangeSignificant((bool)previousValue, (bool)newValue);

                case UMI3DAnimatorParameterType.Float:
                    return IsChangeSignificant((float)previousValue, (float)newValue, threshold);

                case UMI3DAnimatorParameterType.Integer:
                    return IsChangeSignificant((int)previousValue, (int)newValue);
            }

            return true;
        }

        private bool IsChangeSignificant<T>(T previousValue, T newValue)
        {
            if (previousValue.Equals(newValue))
                return false;
            return true;
        }

        private bool IsChangeSignificant(float previousValue, float newValue, float threshold = 0.05f)
        {
            if (previousValue == newValue)
                return false;

            if (newValue == 0f && previousValue != 0f) // avoid slow movement persistance
                return true;

            float den = previousValue == 0 ? 1f : Mathf.Abs(previousValue);

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