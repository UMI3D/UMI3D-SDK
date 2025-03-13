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

namespace umi3d.cdk.interaction
{
    /// <summary>
    /// Game object containing UMI3D <see cref="umi3d.cdk.interaction.Interactable"/>.
    /// </summary>
    public class InteractableContainer : MonoBehaviour
    {
        /// <summary>
        /// List of all <see cref="umi3d.cdk.interaction.Interactable"/> containers.
        /// </summary>
        public static List<InteractableContainer> containers = new List<InteractableContainer>();

        /// <summary>
        /// Interactable associated with the object.
        /// </summary>
        [Tooltip("Interactable associated with the object")]
        public Interactable Interactable;

        private void Awake()
        {
            if (!containers.Contains(this))
                containers.Add(this);
        }

        private void Start()
        {
            UMI3DEnvironmentLoader.Instance.onEnvironmentLoaded.AddListener(AddListenersToRendererChildren);
        }

        private void OnDestroy()
        {
            containers.Remove(this);
            UMI3DEnvironmentLoader.Instance.onEnvironmentLoaded.RemoveListener(AddListenersToRendererChildren);
        }

        void AddListenersToRendererChildren()
        {
            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (Collider collider in colliders)
            {
                // Add an hover listener to be notified when the user hover the interactable.
                InteractableHoverStateListener hoverStateListener = collider.GetComponent<InteractableHoverStateListener>();
                if (hoverStateListener == null)
                {
                    hoverStateListener = collider.gameObject.AddComponent<InteractableHoverStateListener>();
                    hoverStateListener.collider = collider;
                    hoverStateListener.interactableContainer = this;
                }

                Renderer renderer = collider.GetComponent<Renderer>();
                if (renderer == null)
                {
                    renderer = collider.gameObject.AddComponent<MeshRenderer>();
                }

                // Add a visibility listener to be notified when the interactable is visible by the camera user.
                InteractableVisibilityListener visibilityListener = renderer.GetComponent<InteractableVisibilityListener>();
                if (visibilityListener == null)
                {
                    visibilityListener = renderer.gameObject.AddComponent<InteractableVisibilityListener>();
                    visibilityListener.renderer = renderer;
                    visibilityListener.interactableContainer = this;
                }

                // Add a ui controller to manage the interaction indicator.
                InteractableUIController uiController = renderer.GetComponent<InteractableUIController>();
                if (uiController == null)
                {
                    uiController = renderer.gameObject.AddComponent<InteractableUIController>();
                    uiController.interactableContainer = this;
                    uiController.hoverStateListener = hoverStateListener;
                    uiController.visibilityListener = visibilityListener;
                }

                hoverStateListener.uiController = uiController;
                visibilityListener.uiController = uiController;
            }
        }
    }
}