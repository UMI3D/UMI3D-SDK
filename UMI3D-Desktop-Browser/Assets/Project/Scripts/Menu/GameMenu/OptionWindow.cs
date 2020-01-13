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
using System.Text.RegularExpressions;
using UnityEngine;
using BrowserDesktop.Controller;

namespace BrowserDesktop.Menu
{
    public class OptionWindow : MonoBehaviour
    {
        [SerializeField]
        InputLayoutElementDisplayer itemPrefab = null;

        Dictionary<InputLayoutManager.Input, InputLayoutElementDisplayer> inputs = new Dictionary<InputLayoutManager.Input, InputLayoutElementDisplayer>();

        public Transform Container;

        Coroutine setup = null;

        private void Start()
        {
            foreach (InputLayoutManager.Input Einput in (InputLayoutManager.Input[])InputLayoutManager.Input.GetValues(typeof(InputLayoutManager.Input)))
            {
                var instance = Instantiate(itemPrefab.transform, Container).GetComponent<InputLayoutElementDisplayer>();
                inputs.Add(Einput, instance);
                instance.Set(Regex.Replace(Einput.ToString(), @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0"));
            }
        }

        IEnumerator Setup()
        {
            var wait = new WaitForEndOfFrame();
            while (!InputLayoutManager.Exist)
            {
                yield return wait;
            }
            foreach (InputLayoutManager.Input Einput in (InputLayoutManager.Input[])InputLayoutManager.Input.GetValues(typeof(InputLayoutManager.Input)))
            {
                if (inputs.ContainsKey(Einput))
                {
                    inputs[Einput].Set(InputLayoutManager.GetInputCode(Einput));
                }
            }
            setup = null;
        }

        private void OnEnable()
        {
            if (setup == null)
            {
                setup = StartCoroutine(Setup());
            }
        }

        private void OnDisable()
        {
            if (setup != null)
            {
                StopCoroutine(setup);
            }
        }

        private void OnDestroy()
        {
            if (setup != null)
            {
                StopCoroutine(setup);
            }
        }

    }
}