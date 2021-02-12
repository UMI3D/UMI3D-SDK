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

namespace umi3d.cdk.interaction
{
    public abstract class AbstractSelector : MonoBehaviour
    {
        public List<int> deactivationRequesters = new List<int>();

        public bool activated { get; protected set; }


        protected virtual void ActivateInternal() { activated = true; }
        protected virtual void DeactivateInternal() { activated = false; }

        protected virtual void Awake()
        {
            ActivateInternal();
        }


        /// <summary>
        /// Activate the Selector.
        /// </summary>
        public virtual void Activate(int id)
        {
            if (deactivationRequesters.Contains(id))
                deactivationRequesters.Remove(id);

            if ((deactivationRequesters.Count == 0) && !activated)
                ActivateInternal();
        }

        /// <summary>
        /// Deactivate the Selector.
        /// </summary>
        public virtual void Deactivate(int id)
        {
            if (!deactivationRequesters.Contains(id))
                deactivationRequesters.Add(id);
            if (activated)
                DeactivateInternal();
        }

        /// <summary>
        /// Select the object currently pointed at.
        /// </summary>
        public abstract void Select();




    }
}