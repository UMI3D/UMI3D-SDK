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
using UnityEngine;

namespace umi3d.cdk
{
    public abstract class AbstractSelector : MonoBehaviour
    {
        public static AbstractSelector current { get; protected set; }

        public bool activated { get; protected set; }

        public bool activateOnLoad;

        /// <summary>
        /// Activate the Selector.
        /// </summary>
        public virtual void Activate()
        {
            if (current.activated && (current != this))
                current.Desactivate();

            current = this;
            activated = true;            
        }

        /// <summary>
        /// Deactivate the Selector.
        /// </summary>
        public virtual void Desactivate()
        {
            activated = false;
        }

        /// <summary>
        /// Select the object currently pointed at.
        /// </summary>
        public abstract void Select();


        protected virtual void Awake()
        {
            if (activateOnLoad)
                Activate();
        }
    }
}