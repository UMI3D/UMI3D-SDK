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

namespace inetum.unityUtils.math
{
    public static class TranslationUtils 
    {
        /// <summary>
        /// Filter the <paramref name="translation"/> by <paramref name="filter"/>.
        /// </summary>
        /// <param name="translation">The translation.</param>
        /// <param name="filter">The filter. If a composant is 0 then the translation will be null in that composant.</param>
        /// <returns></returns>
        public static Vector3 FilterTranslation(this Vector3 translation, Vector3 filter)
        {
            return Vector3.Scale(translation, filter);
        }

        /// <summary>
        /// Filter the translation from <paramref name="originPosition"/> to <paramref name="endPosition"/> by <paramref name="filter"/>.
        /// </summary>
        /// <param name="originPosition">The origin of the translation.</param>
        /// <param name="endPosition">The position of the object after the translation.</param>
        /// <param name="filter">The filter. If a composant is 0 then the translation will be null in that composant.</param>
        /// <returns></returns>
        public static Vector3 FilterTranslation(this Vector3 originPosition, Vector3 endPosition, Vector3 filter)
        {
            Vector3 translation = endPosition - originPosition;
            return Vector3.Scale(translation, filter);
        }

        /// <summary>
        /// calculates a vector representing the relative distance between <paramref name="a"/> and <paramref name="b"/>.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Vector3 GetRelativeTranslationOfAToB(this Transform a, Transform b)
        {
            return b.position - a.position;
        }
    }
}