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

using UnityEngine;

namespace umi3d.cdk.interaction.selection.intentdetector.method
{
    /// <summary>
    /// Base class for selection intent detection method.
    /// Used to detect <see cref="InteractableContainer"/> or <see cref="UnityEngine.UI.Selectable"/>
    /// </summary>
    /// <typeparam name="T">Type of the object to select.</typeparam>
    public abstract class AbstractDetectionMethod<T> where T : MonoBehaviour
    {
        /// <summary>
        /// Transform of the controller the detector is attached with
        /// </summary>
        protected Transform controllerTransform;

        /// <summary>
        /// Initialize the detection method
        /// </summary>
        /// <param name="controller"></param>
        public virtual void Init(AbstractController controller)
        {
            controllerTransform = controller.transform;
        }

        /// <summary>
        /// Reset parameters of the detector.
        /// To be called after an object has be selected.
        /// </summary>
        public virtual void Reset()
        { }

        /// <summary>
        /// Stop the detector
        /// </summary>
        public virtual void Stop()
        { }

        /// <summary>
        /// Predict the target of the user selection intention
        /// </summary>
        /// <returns>An interactable object or null</returns>
        public abstract T PredictTarget();
    }
}