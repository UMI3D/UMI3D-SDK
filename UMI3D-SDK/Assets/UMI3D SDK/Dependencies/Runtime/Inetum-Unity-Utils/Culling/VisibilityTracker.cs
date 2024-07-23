/*
Copyright 2019 - 2024 Inetum

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
using UnityEngine.Assertions;

namespace inetum.unityUtils.culling
{
    /// <summary>
    /// Raise an event when the gameobject, which has a renderer, become visible / invisible.
    /// </summary>
    public class VisibilityTracker : MonoBehaviour
    {
        public bool IsVisible { get; private set; }

        private Renderer trackedRenderer;

        public event System.Action BecameVisible;

        public event System.Action BecameInvisible;

        public event System.Action<bool> VisibilityChanged;

        private void Start()
        {
            trackedRenderer = GetComponent<Renderer>();
            Assert.IsNotNull(trackedRenderer, "A visibility tracker should track the visibility of a renderer.");
        }

        private void OnBecameVisible()
        {
            IsVisible = true;
            BecameVisible?.Invoke();
            VisibilityChanged?.Invoke(true);
        }

        private void OnBecameInvisible()
        {
            IsVisible = false;
            BecameInvisible?.Invoke();
            VisibilityChanged?.Invoke(false);
        }

    }
}
