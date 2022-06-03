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
using umi3d.cdk.interaction.selection.zoneselection;
using UnityEngine;

namespace umi3d.cdk.interaction.selection.intentdetector.method
{
    /// <summary>
    /// Ray cast method for selecting objects according to their position on a ray
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RaycastDetectionMethod<T> : AbstractDetectionMethod<T> where T : MonoBehaviour
    {
        /// <inheritdoc/>
        public override T PredictTarget()
        {
            var raySelection = new RaySelectionZone<T>(controllerTransform.position, controllerTransform.forward);
            var closestActiveInteractable = raySelection.GetClosestInZone();
            return closestActiveInteractable;
        }
    }

    /// <summary>
    /// Ray cast method for selecting objects according to their position on a ray. 
    /// Specific rules for <see cref="InteractableContainer"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class InteractableRaycastDetectionMethod: RaycastDetectionMethod<InteractableContainer>
    {
        /// <inheritdoc/>
        public override InteractableContainer PredictTarget()
        {
            var raySelection = new RaySelectionZone<InteractableContainer>(controllerTransform.position, controllerTransform.forward);
            var objWithRaycastHits = raySelection.GetObjectsOnRayWithRayCastHits();
            var objWithRaycastHitsSorted = SortInteractable(objWithRaycastHits);
            if (objWithRaycastHitsSorted.Count > 0)
                return objWithRaycastHitsSorted[0];
            else
                return null;
        }

        /// <summary>
        /// Rule to sort interactables
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private List<InteractableContainer> SortInteractable(Dictionary<InteractableContainer, RaycastHit> source)
        {
            List<RaycastHit> hitsList = new List<RaycastHit>(source.Values);

            Transform t = controllerTransform;
            var interactablesPairs = new List<KeyValuePair<InteractableContainer, RaycastHit>>();
            foreach (var pair in source)
                interactablesPairs.Add(pair);

            interactablesPairs.Sort(delegate (KeyValuePair<InteractableContainer, RaycastHit> x, KeyValuePair<InteractableContainer, RaycastHit> y)
            {
                if (x.Key.Interactable.Active && !y.Key.Interactable.Active)
                    return -1;
                else if (!x.Key.Interactable.Active && y.Key.Interactable.Active)
                    return 1;
                else if (x.Key.Interactable.HasPriority && !y.Key.Interactable.HasPriority)
                    return -1;
                else if (!x.Key.Interactable.HasPriority && y.Key.Interactable.HasPriority)
                    return 1;
                else
                {
                    if (Vector3.Distance(t.position, x.Value.point) >= Vector3.Distance(t.position, y.Value.point))
                        return 1;
                    else
                        return -1;
                }
            });

            List<InteractableContainer> res = new List<InteractableContainer>();
            foreach (var e in interactablesPairs)
                res.Add(e.Key);

            return res;
        }
    }
}