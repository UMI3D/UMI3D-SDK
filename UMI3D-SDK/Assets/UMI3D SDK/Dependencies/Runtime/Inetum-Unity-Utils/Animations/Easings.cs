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
using UnityEngine;

namespace inetum.unityUtils
{
    public static class Easings 
    {
        /// <summary>
        /// Linearly interpolates a value between two floats.<br/>
        /// In this method <paramref name="t"/> clamped.
        /// </summary>
        /// <param name="initial">Start value</param>
        /// <param name="final">End value</param>
        /// <param name="t">Our progress or percentage.</param>
        /// <returns>Interpolated value between two floats</returns>
        public static float Lerp(float initial, float final, float t)
        {
            return (initial + (final - initial) * t);
        }

        /// <summary>
        /// Linearly interpolates a value between two floats.<br/>
        /// In this method <paramref name="t"/> clamped.
        /// </summary>
        /// <param name="initial">Start value</param>
        /// <param name="final">End value</param>
        /// <param name="t">Our progress or percentage.</param>
        /// <returns>Interpolated value between two floats</returns>
        public static Vector3 Lerp(Vector3 initial, Vector3 final, float t)
        {
            return (initial + (final - initial) * t);
        }

        /// <summary>
        /// A linear function.
        /// </summary>
        /// <param name="elapsedTime"></param>
        /// <param name="animationTime">Duration of the animation</param>
        /// <returns></returns>
        public static float Linear(float elapsedTime, float animationTime)
        {
            return elapsedTime / animationTime;
        }

        /// <summary>
        /// An ease in quad function.
        /// </summary>
        /// <param name="elapsedTime"></param>
        /// <param name="animationTime">Duration of the animation</param>
        /// <returns></returns>
        public static float EaseInQuad(float elapsedTime, float animationTime)
        {
            // The absolute progress of the animation in the bounds of 0 (beginning of the animation) and 1 (end of animation).
            float t = elapsedTime / animationTime;
            return t * t;
        }

        /// <summary>
        /// An ease in cubic function.
        /// </summary>
        /// <param name="elapsedTime"></param>
        /// <param name="animationTime">Duration of the animation</param>
        /// <returns></returns>
        public static float EaseInCubic(float elapsedTime, float animationTime)
        {
            // The absolute progress of the animation in the bounds of 0 (beginning of the animation) and 1 (end of animation).
            float t = elapsedTime / animationTime;
            return t * t * t;
        }

        /// <summary>
        /// An ease in cubic function.
        /// </summary>
        /// <param name="elapsedTime"></param>
        /// <param name="animationTime">Duration of the animation</param>
        /// <returns></returns>
        public static float EaseInQuart(float elapsedTime, float animationTime)
        {
            // The absolute progress of the animation in the bounds of 0 (beginning of the animation) and 1 (end of animation).
            float t = elapsedTime / animationTime;
            return t * t * t * t;
        }

        /// <summary>
        /// An ease in quint function.
        /// </summary>
        /// <param name="elapsedTime"></param>
        /// <param name="animationTime">Duration of the animation</param>
        /// <returns></returns>
        public static float EaseInQuint(float elapsedTime, float animationTime)
        {
            // The absolute progress of the animation in the bounds of 0 (beginning of the animation) and 1 (end of animation).
            float t = elapsedTime / animationTime;
            return t * t * t * t * t;
        }

        /// <summary>
        /// An ease in sine function.
        /// </summary>
        /// <param name="elapsedTime"></param>
        /// <param name="animationTime">Duration of the animation</param>
        /// <returns></returns>
        public static float EaseInSine(float elapsedTime, float animationTime)
        {
            // The absolute progress of the animation in the bounds of 0 (beginning of the animation) and 1 (end of animation).
            float t = elapsedTime / animationTime;
            return 1 - Mathf.Cos((t * Mathf.PI) / 2);
        }

        /// <summary>
        /// An ease in circ function.
        /// </summary>
        /// <param name="elapsedTime"></param>
        /// <param name="animationTime">Duration of the animation</param>
        /// <returns></returns>
        public static float EaseInCirc(float elapsedTime, float animationTime)
        {
            // The absolute progress of the animation in the bounds of 0 (beginning of the animation) and 1 (end of animation).
            float t = elapsedTime / animationTime;
            return 1 - Mathf.Sqrt(1 - Mathf.Pow(t, 2));
        }

        /// <summary>
        /// An ease in back function.
        /// </summary>
        /// <param name="elapsedTime"></param>
        /// <param name="animationTime">Duration of the animation</param>
        /// <returns></returns>
        public static float EaseInBack(float elapsedTime, float animationTime, float c1 = 1.70158f)
        {
            float c3 = c1 + 1f;

            // The absolute progress of the animation in the bounds of 0 (beginning of the animation) and 1 (end of animation).
            float t = elapsedTime / animationTime;
            return c3 * t * t * t - c1 * t * t;
        }

        /// <summary>
        /// An ease out quad function.
        /// </summary>
        /// <param name="elapsedTime"></param>
        /// <param name="animationTime">Duration of the animation</param>
        /// <returns></returns>
        public static float EaseOutQuad(float elapsedTime, float animationTime)
        {
            // The absolute progress of the animation in the bounds of 0 (beginning of the animation) and 1 (end of animation).
            float t = elapsedTime / animationTime;
            return 1 - (1 - t) * (1 - t);
        }

        /// <summary>
        /// An ease out cubic function.
        /// </summary>
        /// <param name="elapsedTime"></param>
        /// <param name="animationTime">Duration of the animation</param>
        /// <returns></returns>
        public static float EaseOutCubic(float elapsedTime, float animationTime)
        {
            // The absolute progress of the animation in the bounds of 0 (beginning of the animation) and 1 (end of animation).
            float t = elapsedTime / animationTime;
            return 1 - Mathf.Pow(1 - t, 3);
        }

        /// <summary>
        /// An ease out cubic function.
        /// </summary>
        /// <param name="elapsedTime"></param>
        /// <param name="animationTime">Duration of the animation</param>
        /// <returns></returns>
        public static float EaseOutQuart(float elapsedTime, float animationTime)
        {
            // The absolute progress of the animation in the bounds of 0 (beginning of the animation) and 1 (end of animation).
            float t = elapsedTime / animationTime;
            return 1 - Mathf.Pow(1 - t, 4);
        }

        /// <summary>
        /// An ease out quint function.
        /// </summary>
        /// <param name="elapsedTime"></param>
        /// <param name="animationTime">Duration of the animation</param>
        /// <returns></returns>
        public static float EaseOutQuint(float elapsedTime, float animationTime)
        {
            // The absolute progress of the animation in the bounds of 0 (beginning of the animation) and 1 (end of animation).
            float t = elapsedTime / animationTime;
            return 1 - Mathf.Pow(1 - t, 5);
        }

        /// <summary>
        /// An ease out sine function.
        /// </summary>
        /// <param name="elapsedTime"></param>
        /// <param name="animationTime">Duration of the animation</param>
        /// <returns></returns>
        public static float EaseOutSine(float elapsedTime, float animationTime)
        {
            // The absolute progress of the animation in the bounds of 0 (beginning of the animation) and 1 (end of animation).
            float t = elapsedTime / animationTime;
            return Mathf.Sin((t * Mathf.PI) / 2);
        }

        /// <summary>
        /// An ease out circ function.
        /// </summary>
        /// <param name="elapsedTime"></param>
        /// <param name="animationTime">Duration of the animation</param>
        /// <returns></returns>
        public static float EaseOutCirc(float elapsedTime, float animationTime)
        {
            // The absolute progress of the animation in the bounds of 0 (beginning of the animation) and 1 (end of animation).
            float t = elapsedTime / animationTime;
            return Mathf.Sqrt(1 - Mathf.Pow(t - 1, 2));
        }

        /// <summary>
        /// An ease out back function.
        /// </summary>
        /// <param name="elapsedTime"></param>
        /// <param name="animationTime">Duration of the animation</param>
        /// <returns></returns>
        public static float EaseOutBack(float elapsedTime, float animationTime, float c1 = 1.70158f)
        {
            float c3 = c1 + 1f;

            // The absolute progress of the animation in the bounds of 0 (beginning of the animation) and 1 (end of animation).
            float t = elapsedTime / animationTime;
            return 1 + c3 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t - 1, 2);
        }
    }
}