/*
Copyright 2019 - 2025 Inetum

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

namespace umi3d.cdk.interaction
{
    public class InteractableUIController : MonoBehaviour
    {
        public InteractableContainer interactableContainer;
        public InteractableHoverStateListener hoverStateListener;
        public InteractableVisibilityListener visibilityListener;

        public GameObject interactableUI;
        public object viewController;

        public static IInteractableUIControllerDataDelegate dataDelegate;

        void Start()
        {
            if ((dataDelegate?.TryGetInteractableUI(out interactableUI, out viewController, interactableContainer, this) ?? false) && interactableUI != null)
            {
                interactableUI.transform.SetParent(transform, false);
            }
        }

        public bool TryCastViewController<VC>(out VC viewController) where VC : class
        {
            if (this.viewController == null)
            {
                UnityEngine.Debug.LogError($"[InteractableUIController] Error: view controller is null.");
                viewController = null;
                return false;
            }

            if (this.viewController is not VC)
            {
                UnityEngine.Debug.LogError($"[InteractableUIController] Error: view controller is not {typeof(VC)}.");
                viewController = null;
                return false;
            }

            viewController = this.viewController as VC;
            return true;
        }
    }

    public interface IInteractableUIControllerDataDelegate
    {
        /// <summary>
        /// Return the UI gameObject to display interactable information to the user.<br/>
        /// This method is called once by renderer.
        /// </summary>
        /// <returns></returns>
        bool TryGetInteractableUI(out GameObject gameObject, out object viewController, InteractableContainer interactableContainer, InteractableUIController uiController) 
        {
            gameObject = null;
            viewController = null;
            return false; 
        }
    }
}