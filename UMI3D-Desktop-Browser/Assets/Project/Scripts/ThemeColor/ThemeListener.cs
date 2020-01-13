/*
Copyright 2019 Gfi Informatique

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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrowserDesktop.Theme
{
    public abstract class ThemeListener : MonoBehaviour
    {
        private void Start()
        {
            if (gameObject.activeInHierarchy)
                StartCoroutine(_ApplyTheme());
            else ApplyTheme();

        }

        private void OnEnable()
        {
            if (gameObject.activeInHierarchy)
                StartCoroutine(_ApplyTheme());
            else ApplyTheme();

        }

        private void OnValidate()
        {
            if (gameObject.activeInHierarchy)
                StartCoroutine(_ApplyTheme());
            else ApplyTheme();
        }

        bool running = false;
        IEnumerator _ApplyTheme()
        {
            if (running) yield break;
            running = true;
            var wait = new WaitForFixedUpdate();
            while (!Theme.Exist)
            {
                yield return wait;
            }
            ApplyTheme();
            running = false;
        }

        public abstract void ApplyTheme();
    }

}
