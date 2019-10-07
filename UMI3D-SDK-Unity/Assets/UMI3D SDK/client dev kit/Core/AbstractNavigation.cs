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
using umi3d.common;

namespace umi3d.cdk
{
    /// <summary>
    /// Abstract class for user navigation methods in scene.
    /// </summary>
    public abstract class AbstractNavigation : MonoBehaviour
    {
        /// <summary>
        /// Maximum tanslation speed for navigation request from server.
        /// </summary>
        [Tooltip("Maximum tanslation speed for navigation request from server")]
        public float maxNavTranslationSpeed = 1f;

        /// <summary>
        /// True if navigation is controlled by server request, false otherwise.
        /// </summary>
        protected bool managed;

        /// <summary>
        /// World origin.
        /// </summary>
        protected Transform world;

        /// <summary>
        /// User viewpoint transform.
        /// </summary>
        protected Transform viewpoint;

        /// <summary>
        /// Target position for navigation request from server.
        /// </summary>
        protected Vector3 navPosition = Vector3.zero;


        /// <summary>
        /// Setup and enable this navigation method.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="viewpoint"></param>
        public virtual void Setup(Transform world, Transform viewpoint)
        {
            world.position = new Vector3(0, 0, 0);
            this.world = world;
            this.viewpoint = viewpoint;
        }


        /// <summary>
        /// Disable this navigation method.
        /// </summary>
        public abstract void Disable();


        /// <summary>
        /// Apply navigation request from server.
        /// </summary>
        /// <param name="data"></param>
        /// <seealso cref="Teleport(TeleportDto)"/>
        public void Navigate(NavigateDto data)
        {
            managed = true;
            navPosition = - (Vector3) data.Position;
        }

        /// <summary>
        /// Apply teleport request from server.
        /// </summary>
        /// <param name="data"></param>
        /// <seealso cref="Navigate(NavigateDto)"/>
        public void Teleport(TeleportDto data)
        {
            managed = false;
            world.localPosition = -(Vector3)data.Position;
        }

        /// <summary>
        /// Navigate in the scene according to navigation request from server if any.
        /// </summary>
        /// <seealso cref="Navigate(NavigateDto)"/>
        protected void Move()
        {
            if (managed)
            {
                Vector3 vect = navPosition - world.position;
                float distance = vect.magnitude;
                float reacheableDistance = Time.deltaTime * maxNavTranslationSpeed;

                if (distance > reacheableDistance)
                    world.localPosition += reacheableDistance * vect.normalized;
                else
                {
                    managed = false;
                    world.localPosition = navPosition;
                }

            }
        }
    }
}