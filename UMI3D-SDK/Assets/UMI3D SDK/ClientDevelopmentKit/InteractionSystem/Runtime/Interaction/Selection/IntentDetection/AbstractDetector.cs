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

using umi3d.cdk.interaction.selection.intentdetector.method;
using UnityEngine;

namespace umi3d.cdk.interaction.selection.intentdetector
{
    /// <summary>
    /// Abstract parent class for all intention of selection detectors.
    /// </summary>
    // Separated from detection method because of Unity serialization of templates
    public abstract class AbstractDetector<T> : MonoBehaviour where T : MonoBehaviour
    {
        /// <summary>
        /// Transform associated with the pointing device
        /// </summary>
        protected Transform controllerTransform;

        /// <summary>
        /// Detection method logic
        /// </summary>
        public AbstractDetectionMethod<T> detectionMethod;

        /// <summary>
        /// True is the detector is currently looking for selection intent
        /// </summary>
        [HideInInspector]
        public bool isRunning;

        protected virtual void Awake()
        {
            controllerTransform = GetComponentInParent<AbstractController>().transform;
        }

        /// <summary>
        /// Initialize the detector
        /// </summary>
        public virtual void Init(AbstractController controller)
        {
            SetDetectionMethod();
            detectionMethod.Init(controller);
            controllerTransform = controller.transform;
            isRunning = true;
        }

        /// <summary>
        /// Method to override to set a detection method
        /// </summary>
        protected abstract void SetDetectionMethod();


        /// <summary>
        /// Reset parameters of the detector.
        /// To be called after an object has be selected.
        /// </summary>
        public virtual void Reinit()
        {
            detectionMethod.Reset();
        }

        /// <summary>
        /// Stop the detector
        /// </summary>
        public virtual void Stop()
        {
            detectionMethod.Stop();
            isRunning = false;
        }

        /// <summary>
        /// Predict the target of the user selection intention
        /// </summary>
        /// <returns>An interactable object or null</returns>
        public virtual T PredictTarget()
        {
            return detectionMethod.PredictTarget();
        }
    }
}