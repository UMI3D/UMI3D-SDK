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

using inetum.unityUtils.observation;
using UnityEngine;

namespace umi3d.cdk.interaction
{
    public class InteractableVisibilityListener : MonoBehaviour
    {
        public new Renderer renderer;
        public InteractableContainer interactableContainer;
        public GameObject interactableUI;

        new Camera camera;

        public bool isVisible { get; private set; } = false;
        public static readonly Delegates<IInteractableVisibilityDelegate> delegates = new();
        public static IInteractableVisibilityDataDelegate dataDelegate;

        Ray ray;
        RaycastHit hit;

        void Start()
        {
            camera = Camera.main;
            
            interactableUI = dataDelegate?.GetInteractableUI();
            if (interactableUI != null)
            {
                interactableUI.transform.SetParent(transform, false);
            }

            if (renderer.isVisible)
            {
                OnBecameVisible();
            }

            UMI3DEnvironmentLoader.Instance.onEnvironmentLoaded.AddListener(CallOnBecameVisibleWhenEnvironmentIsLoaded);
        }

        private void OnDestroy()
        {
            UMI3DEnvironmentLoader.Instance.onEnvironmentLoaded.RemoveListener(CallOnBecameVisibleWhenEnvironmentIsLoaded);
        }

        void OnBecameVisible()
        {
            isVisible = true;
            delegates.ForEach(@delegate =>
            {
                @delegate.OnBecameVisible(renderer, interactableContainer, this);
                return Flow.Continue;
            });
        }

        void OnBecameInvisible()
        {
            isVisible = false;
            delegates.ForEach(@delegate =>
            {
                @delegate.OnBecameInvisible(renderer, interactableContainer, this);
                return Flow.Continue;
            });
        }

        void CallOnBecameVisibleWhenEnvironmentIsLoaded()
        {
            if (renderer.isVisible)
            {
                OnBecameVisible();
            }
        }
    }

    public interface IInteractableVisibilityDelegate
    {
        /// <summary>
        /// This method is called when the <paramref name="renderer"/> became visible by the camera.<br/>
        /// <b>Warning:</b> The renderer can be visible by the camera but not by the user as this method is called even if an obstacle block the view.
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="interactableContainer"></param>
        /// <param name="visibilityListener"></param>
        void OnBecameVisible(Renderer renderer, InteractableContainer interactableContainer, InteractableVisibilityListener visibilityListener) { }
        /// <summary>
        /// This method is called when the <paramref name="renderer"/> became invisible by the camera.<br/>
        /// <b>Warning:</b> The renderer can be visible by the camera but not by the user as this method is called even if an obstacle block the view.
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="interactableContainer"></param>
        /// <param name="visibilityListener"></param>
        void OnBecameInvisible(Renderer renderer, InteractableContainer interactableContainer, InteractableVisibilityListener visibilityListener) { }
    }

    public interface IInteractableVisibilityDataDelegate
    {
        /// <summary>
        /// Return the UI gameObject to display interactable information to the user.<br/>
        /// This method is called once by renderer.
        /// </summary>
        /// <returns></returns>
        GameObject GetInteractableUI();
    }
}