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
using umi3d.cdk.interaction.selection.zoneselection;
using UnityEngine;

namespace umi3d.cdk.interaction.selection.intentdetector.method
{
    /// <summary>
    /// Implementation of the IntenSelect detector of intention, from de Haan et al. 2005
    /// </summary>
    public class IntenSelectDetectionMethod<T> : AbstractDetectionMethod<T> where T : MonoBehaviour
    {
        /// <summary>
        /// Methods parameters
        /// </summary>
        private IntenSelectParameters param;

        /// <summary>
        /// Dictionnary where objects considered by the Intenselect algorithm are stored
        /// The key is the id of the object, the value is its score
        /// </summary>
        private Dictionary<T, float> objectsToConsiderScoresDict;

        public IntenSelectDetectionMethod(IntenSelectParameters param)
        {
            this.param = param;
        }

        /// <inheritdoc/>
        public override void Init(AbstractController controller)
        {
            base.Init(controller);
            objectsToConsiderScoresDict = new Dictionary<T, float>();
        }

        /// <inheritdoc/>
        public override void Reset()
        {
            objectsToConsiderScoresDict.Clear();
        }

        /// <summary>
        /// Clear the detector except for the passed object
        /// </summary>
        /// <param name="obj"></param>
        private void ResetExceptOne(T obj)
        {
            Reset();
            objectsToConsiderScoresDict.Add(obj, 0);
        }

        /// <summary>
        /// Find the target of the use intention according to the IntentSelect algorithm
        /// </summary>
        /// <returns>The intended object or null</returns>
        public override T PredictTarget()
        {
            var coneSelector = new ConicSelectionZone<T>(controllerTransform.position, controllerTransform.forward, param.coneAngle);

            var interactableObjectsInZone = coneSelector.GetObjectsInZone();

            foreach (var obj in interactableObjectsInZone)
            {
                if (!objectsToConsiderScoresDict.ContainsKey(obj))
                    objectsToConsiderScoresDict.Add(obj, 0);
                objectsToConsiderScoresDict[obj] = ComputeScore(obj);
            }

            var objectsToRemove = new List<T>();
            foreach (var obj in objectsToConsiderScoresDict.Keys)
            {
                if (!coneSelector.IsObjectInZone(obj)) //  || objectsToConsiderScoresDict[obj] < param.scoreMin
                    objectsToRemove.Add(obj);
            }
            foreach (var obj in objectsToRemove)
                objectsToConsiderScoresDict.Remove(obj);

            if (objectsToConsiderScoresDict.Values.Count == 0)
                return null;
            var maxScore = objectsToConsiderScoresDict.Values.Max();
            var estimatedTargetPair = objectsToConsiderScoresDict.FirstOrDefault(o => o.Value == maxScore); //find the object with the highest score

            return estimatedTargetPair.Key;
        }

        /// <summary>
        /// Compute the cumulative score according to the formula of IntentSelect
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>Object's score</returns>
        private float ComputeScore(in T obj)
        {
            var vectorToObject = obj.transform.position - controllerTransform.position;

            float projectedDistance = Mathf.Abs(Vector3.Dot(vectorToObject, controllerTransform.forward));
            float distanceToRay = Vector3.Cross(vectorToObject, controllerTransform.forward).magnitude;

            if (projectedDistance != 0)
            {
                var correctedProjectedDistance = Mathf.Pow(projectedDistance, param.corrective_k);
                var correctedAngle = Mathf.Atan2(distanceToRay, correctedProjectedDistance) * 180 / Mathf.PI;
                var variation = 1 - (correctedAngle / param.coneAngle);
                var newScore = objectsToConsiderScoresDict[obj] * param.stickinessRate + variation * param.snappinessRate;
                if (newScore > param.scoreMax) //Avoid float overflow and give reactivity to the detector
                {
                    ResetExceptOne(obj);
                    return 1;
                }
                else
                    return newScore;
            }
            else
                return 0;
        }
    }
}