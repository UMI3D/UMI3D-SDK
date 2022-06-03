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
using System.Linq;
using UnityEngine;

namespace umi3d.cdk.interaction.selection.zoneselection
{
    /// <summary>
    /// Seleciton zone defined by a ray
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RaySelectionZone<T> : AbstractSelectionZone<T> where T : Component
    {
        /// <summary>
        /// Orientation of the ray
        /// </summary>
        public Vector3 direction;

        /// <summary>
        /// Origin of the ray
        /// </summary>
        public Vector3 origin;

        public RaySelectionZone(Vector3 origin, Vector3 direction)
        {
            this.origin = origin;
            this.direction = direction;
        }

        public RaySelectionZone(Transform originTransform)
        {
            this.origin = originTransform.position;
            this.direction = originTransform.forward;
        }

        /// <summary>
        /// Hit object and associated raycastHit
        /// </summary>
        public struct RaycastData
        {
            public T obj;
            public RaycastHit raycastHit;
        }

        /// <inheritdoc/>
        public override List<T> GetObjectsInZone()
        {
            var rayCastHits = umi3d.common.Physics.RaycastAll(origin, direction);
            var objectsOnRay = new List<T>();

            foreach (var hit in rayCastHits)
            {
                var interContainer = hit.transform.GetComponent<T>();
                if (interContainer == null)
                    interContainer = hit.transform.GetComponentInParent<T>();
                if (interContainer != null)
                    objectsOnRay.Add(interContainer);
            }

            return objectsOnRay;
        }

        /// <summary>
        /// Return closest object on ray
        /// </summary>
        /// <returns></returns>
        public override T GetClosestInZone()
        {
            var objectWithDistances = GetObjectsOnRayWithRayCastHits();
            var objList = objectWithDistances.Keys.ToList();
            if (objList.Count == 0)
                return null;

            var activeObjects = objList.Where(obj => (obj != null && obj.gameObject.activeInHierarchy)).DefaultIfEmpty();
            if (activeObjects == default)
                return null;

            var minDist = (from obj in activeObjects
                           select objectWithDistances[obj].distance).Min();

            var closestActiveInteractable = objList.FirstOrDefault(obj => objectWithDistances[obj].distance == minDist);
            return closestActiveInteractable;
        }

        /// <summary>
        /// Return closest raycast hit of desired type
        /// </summary>
        /// <returns></returns>
        public RaycastData GetClosestAndRaycastHit()
        {
            var objectWithDistances = GetObjectsOnRayWithRayCastHits();
            var objList = objectWithDistances.Keys.ToList();
            if (objList.Count == 0)
                return default;

            var activeObjects = objList.Where(obj => (obj != null && obj.gameObject.activeInHierarchy)).DefaultIfEmpty();
            if (activeObjects == default)
                return default;

            var minDist = (from obj in activeObjects
                           select objectWithDistances[obj].distance).Min();

            var closestActiveInteractable = objList.FirstOrDefault(obj => objectWithDistances[obj].distance == minDist);
            return new RaycastData() { obj = closestActiveInteractable, raycastHit = objectWithDistances[closestActiveInteractable] };
        }

        /// <inheritdoc/>
        public override bool IsObjectInZone(T obj)
        {
            return GetObjectsInZone().Contains(obj);
        }

        /// <summary>
        /// Returns the closest object to the ray (minimal orthogonal projection)
        /// </summary>
        /// <param name="objList"></param>
        /// <returns></returns>
        public T GetClosestToRay(List<T> objList)
        {
            if (objList.Count == 0)
                return null;

            System.Func<T, float> distToRay = obj =>
            {
                var vectorToObject = obj.transform.position - origin;
                return Vector3.Cross(vectorToObject.normalized, direction).magnitude;
            };

            var minDistance = objList.Select(distToRay)?.Min();

            return objList.FirstOrDefault(o => distToRay(o) == minDistance);
        }

        /// <summary>
        /// Returns objects that are along the ray with their RayCastHit object
        /// </summary>
        /// <returns></returns>
        public Dictionary<T, RaycastHit> GetObjectsOnRayWithRayCastHits()
        {
            var rayCastHits = umi3d.common.Physics.RaycastAll(origin, direction);
            var objectsOnRay = new Dictionary<T, RaycastHit>();
            foreach (var hit in rayCastHits)
            {
                var obj = hit.transform.GetComponent<T>();
                if (obj == null) //looking for a componenent in parent
                    obj = hit.transform.GetComponentInParent<T>();
                if (obj != null)
                {
                    if (objectsOnRay.ContainsKey(obj))
                    {
                        if (objectsOnRay[obj].distance > hit.distance) //only consider the first hit if there are several
                            objectsOnRay[obj] = hit;
                    }
                    else
                        objectsOnRay.Add(obj, hit);
                }
            }

            return objectsOnRay;
        }
    }
}