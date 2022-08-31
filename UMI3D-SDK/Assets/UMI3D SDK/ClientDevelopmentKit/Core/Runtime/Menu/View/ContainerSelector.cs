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
using UnityEngine;

namespace umi3d.cdk.menu.view
{
    /// <summary>
    /// Choose the right container for a given menu.
    /// </summary>
    /// <see cref="MenuDisplayManager"/>
    [System.Serializable]
    public class ContainerSelector : MonoBehaviour
    {
        /// <summary>
        /// Menu display containers (sorted by hierarchy height).
        /// </summary>
        [Tooltip("Menu display containers (sorted by hierarchy height).")]
        public List<AbstractMenuDisplayContainer> containerPrefabs = new List<AbstractMenuDisplayContainer>();

        /// <summary>
        /// Specific container choice depending on menu's name.
        /// </summary>
        [SerializeField, Tooltip("Specific container choice depending on menu's name.")]
        public ContainerDictionary exceptions;


        /// <summary>
        /// Choose an apropriate container for a given menu displayed at a given depth.
        /// </summary>
        /// <param name="menu">Menu to display</param>
        /// <param name="depth">Depth of the menu to display</param>
        /// <returns></returns>
        public virtual AbstractMenuDisplayContainer ChooseContainer(AbstractMenu menu, int depth)
        {

            if (exceptions == null)
                exceptions = new ContainerDictionary();

            if (exceptions.TryGetValue(menu.Name, out AbstractMenuDisplayContainer exception))
            {
                return exception;
            }
            else
            {
                int index = Mathf.Min(depth, containerPrefabs.Count - 1);
                return containerPrefabs[index];
            }
        }
    }
}