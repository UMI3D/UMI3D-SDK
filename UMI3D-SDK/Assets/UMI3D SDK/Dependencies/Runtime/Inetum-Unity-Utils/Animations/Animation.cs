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

using System;
using System.Collections;
using UnityEngine;

namespace inetum.unityUtils
{
    public class Animation 
    {
        const float _epsilon = .001f;

        /// <summary>
        /// Time of the animation
        /// </summary>
        public float animationTime { get; private set; } = 1f;
        /// <summary>
        /// Percentage [0,1] of completion of the animation.
        /// </summary>
        public float completionPercentage { get; private set; } = 0f;
        /// <summary>
        /// Limit of the difference between two floats.
        /// </summary>
        public float epsilon { get; private set; } = _epsilon;

        /// <summary>
        /// The initial value;
        /// </summary>
        System.Object initial;
        /// <summary>
        /// The final value.
        /// </summary>
        System.Object final;
        /// <summary>
        /// Apply the current interpolated value.
        /// </summary>
        Action<System.Object> applyValue;

        /// <summary>
        /// The easing method.
        /// </summary>
        Func<float, float, float> easing = Easings.EaseInCirc;
        /// <summary>
        /// Linearly interpolates a value between two floats.<br/>
        /// </summary>
        Func<System.Object, System.Object, float, System.Object> lerp;

        /// <summary>
        /// The animation.
        /// </summary>
        /// <returns></returns>
        IEnumerator Animate()
        {
            float elapsedTime = animationTime * completionPercentage;

            while (epsilon < animationTime - elapsedTime)
            {
                float t = easing(elapsedTime, animationTime);
                System.Object x = lerp(initial, final, t);

                applyValue?.Invoke(x);

                completionPercentage = elapsedTime / animationTime;

                yield return null;
                elapsedTime += Time.deltaTime;
            }

            completionPercentage = 0f;

            applyValue?.Invoke(final);

            yield return null;
        }

        /// <summary>
        /// Set the animation time.
        /// </summary>
        /// <param name="animationTime"></param>
        /// <returns></returns>
        public Animation SetAnimationTime(float animationTime)
        {
            this.animationTime = animationTime;
            return this;
        }

        /// <summary>
        /// Set the <see cref="initial"/> and <see cref="final"/> value of the animation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="initial"></param>
        /// <param name="final"></param>
        /// <returns></returns>
        public Animation SetInitAndFinalValue<T>(T initial, T final)
        {
            this.initial = initial;
            this.final = final;
            return this;
        }

        /// <summary>
        /// Set the <see cref="applyValue"/> method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="setValue"></param>
        /// <returns></returns>
        public Animation SetApplyValue<T>(Action<T> setValue)
        {
            this.applyValue = value =>
            {
                setValue?.Invoke((T)value);
            };

            return this;
        }

        /// <summary>
        /// Set the <see cref="lerp"/> method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lerp"></param>
        /// <returns></returns>
        public Animation SetLerp<T>(Func<T, T, float, T> lerp)
        {
            this.lerp = (a, b, t) =>
            {
                return lerp((T)a, (T)b, t);
            };

            return this;
        }

        /// <summary>
        /// Set the <see cref="easing"/> method.
        /// </summary>
        /// <param name="easing"></param>
        /// <returns></returns>
        public Animation SetEasing(Func<float, float, float> easing)
        {
            this.easing = easing;
            return this;
        }

        /// <summary>
        /// Set the <see cref="epsilon"/>.
        /// </summary>
        /// <param name="epsilon"></param>
        /// <returns></returns>
        public Animation SetEpsilon(float epsilon = _epsilon)
        {
            this.epsilon = epsilon;
            return this;
        }

        /// <summary>
        /// To start the animation.
        /// </summary>
        /// <returns></returns>
        public IEnumerator Start()
        {
            return Animate();
        }

        /// <summary>
        /// Apply a value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        public void ApplyValue<T>(T value)
        {
            applyValue?.Invoke(value);
        }
    }
}