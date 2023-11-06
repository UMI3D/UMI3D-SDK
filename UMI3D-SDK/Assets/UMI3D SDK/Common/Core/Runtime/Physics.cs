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

using System;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.common
{
    /// <summary>
    /// Helper class for physics related algorithms such as ray-casting.
    /// </summary>
    public class Physics : MonoBehaviour
    {
        /// <summary>
        /// Size of the raycast hit buffer.
        /// </summary>
        const int size = 100;
        /// <summary>
        /// Raycast hit buffer.
        /// </summary>
        static RaycastHit[] hits = new RaycastHit[size];
        /// <summary>
        /// Distance comparer.
        /// </summary>
        static Comparer<RaycastHit> comparer = Comparer<RaycastHit>.Create(Comparison);

        /// <summary>
        /// Returns all the hits from a raycast algorithm, sorted by their distance.
        /// </summary>
        /// <param name="ray">Position and direction combined as a ray.</param>
        /// <param name="maxDistance">mMax length of the ray. Default is infinity.</param>
        /// <param name="layerMask">Layermask concerned by the raycasting.</param>
        /// <param name="queryTriggerInteraction"></param>
        /// <returns></returns>
        public static (RaycastHit[] hits, int hitCount) RaycastAll(Ray ray, float maxDistance = Mathf.Infinity, int layerMask = UnityEngine.Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            int hitCount = UnityEngine.Physics.RaycastNonAlloc(ray, hits, maxDistance, layerMask, queryTriggerInteraction);
            Array.Sort(hits, 0, hitCount, comparer);
            if (hitCount < size)
            {
                Array.Clear(hits, hitCount, size - hitCount);
            }
            return (hits, hitCount);
        }

        /// <summary>
        /// Returns all the hits from a raycast algorithm, sorted by their distance.
        /// </summary>
        /// <param name="origin">Starting position of the ray.</param>
        /// /// <param name="direction">Direction in which the ray should be cast.</param>
        /// <param name="maxDistance">Max length of the ray. Default is infinity.</param>
        /// <param name="layerMask">Layermask concerned by the raycasting.</param>
        /// <param name="queryTriggerInteraction"></param>
        /// <returns></returns>
        public static (RaycastHit[] hits, int hitCount) RaycastAll(Vector3 origin, Vector3 direction, float maxDistance = Mathf.Infinity, int layerMask = UnityEngine.Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            int hitCount = UnityEngine.Physics.RaycastNonAlloc(origin, direction, hits, maxDistance, layerMask, queryTriggerInteraction);
            Array.Sort(hits, 0, hitCount, comparer);
            if (hitCount < size)
            {
                Array.Clear(hits, hitCount, size - hitCount);
            }
            return (hits, hitCount);
        }

        /// <summary>
        /// Compare two raycastHits accoridng to their distances.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private static int Comparison(RaycastHit x, RaycastHit y)
        {
            return x.distance == y.distance ? 0 : x.distance < y.distance ? -1 : 1;
        }
    }
}