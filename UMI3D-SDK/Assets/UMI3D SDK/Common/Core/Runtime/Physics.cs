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
        /// Returns all the hits from a raycast algorithm, sorted by their distance.
        /// </summary>
        /// <param name="ray">Position and direction combined as a ray.</param>
        /// <param name="maxDistance">mMax length of the ray. Default is infinity.</param>
        /// <param name="layerMask">Layermask concerned by the raycasting.</param>
        /// <param name="queryTriggerInteraction"></param>
        /// <returns></returns>
        public static RaycastHit[] RaycastAll(Ray ray, float maxDistance = Mathf.Infinity, int layerMask = UnityEngine.Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            return Sort(UnityEngine.Physics.RaycastAll(ray, maxDistance, layerMask, queryTriggerInteraction));
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
        public static RaycastHit[] RaycastAll(Vector3 origin, Vector3 direction, float maxDistance = Mathf.Infinity, int layerMask = UnityEngine.Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            return Sort(UnityEngine.Physics.RaycastAll(origin, direction, maxDistance, layerMask, queryTriggerInteraction));
        }

        /// <summary>
        /// Sort the raycast hits from the closest to the farthest.
        /// </summary>
        /// <param name="array">Array of raycast hits.</param>
        /// <returns>Sorted array of raycastHits</returns>
        private static RaycastHit[] Sort(RaycastHit[] array)
        {
            var l = new List<RaycastHit>();
            foreach (RaycastHit r in array)
                l.Add(r);
            l.Sort(Comparison);
            return l.ToArray();
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