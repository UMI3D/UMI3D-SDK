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
using umi3d.cdk;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using umi3d.common;

namespace umi3d.cdk
{
    public class UMI3DBrowserNavigation : MonoBehaviour
    {
        public Transform viewpoint;
        public Camera viewpointCamera;
        public Transform world;
        public AbstractNavigation orbitation;
        public AbstractNavigation walk;
        public AbstractNavigation fly;
        AbstractNavigation currentNav = null;

        /*
         * 
         *      Main Behaviour
         * 
         */

        bool IsWalking() { return currentNav == walk; }
        bool IsFlying() { return currentNav == fly; }
        bool IsOrbitating() { return currentNav == orbitation; }

        void Start()
        {
            currentNav = walk;
            UMI3DBrowser.Navigation = currentNav;
            walk.Setup(world, viewpoint);
            StartCoroutine(LogUserPosition(1f / 30f));
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        private void OnApplicationQuit()
        {
            StopAllCoroutines();
        }

    /// <summary>
    /// Send the user tracking data to the server
    /// </summary>
    IEnumerator LogUserPosition(float dt)
    {
        bool connected = true;
        while (connected)
        {
            if (UMI3DBrowser.Scene != null)
            {
                var scene = UMI3DBrowser.Scene.transform;
                UMI3DBrowserAvatar.Instance.BonesIterator(UMI3DBrowserAvatar.Instance.avatar, this.viewpoint);
                UMI3DBrowserAvatar.Instance.avatar.ScaleScene = scene.transform.lossyScale.Unscaled(world.lossyScale);
                var data = new NavigationRequestDto();
                data.CameraDto = new CameraDto();
                data.CameraDto.position = world.InverseTransformPoint(this.viewpoint.position) - world.InverseTransformPoint(scene.transform.position);
                data.CameraDto.rotation = viewpoint.rotation * Quaternion.Inverse(world.rotation);
                data.CameraDto.projectionMatrix = viewpointCamera.projectionMatrix;
                data.Avatar = UMI3DBrowserAvatar.Instance.avatar;
                try
                {
                    UMI3DWebSocketClient.Navigate(data);
                }
                catch (Exception err)
                {
                    Debug.LogError(err);
                }
                finally { }
            }
            yield return new WaitForSeconds(dt);
        }
    }

        /// <summary>
        /// Update the browser's navigation type
        /// </summary>
        private void UpdateNavigation()
        {
            AbstractNavigation newnav = null;

            if (UMI3DBrowser.Media == null || UMI3DBrowser.Media.NavigationType == NavigationType.Walk)
                newnav = walk;
            else if (UMI3DBrowser.Media.NavigationType == NavigationType.Orbitation)
                newnav = orbitation;

            else if (UMI3DBrowser.Media.NavigationType == NavigationType.Fly)
                newnav = fly;

            if (newnav != currentNav)
            {
                if (currentNav != null)
                    currentNav.Disable();
                if (newnav != null)
                    newnav.Setup(world, viewpoint);
                currentNav = newnav;
                UMI3DBrowser.Navigation = currentNav;
            }
            UMI3DBrowser.isEnterTeleportationAllowed = true;
            UMI3DBrowser.viewpointOffsetForEnterTeleportation = new Vector3(viewpoint.position.x, 0, viewpoint.position.z);
        }

        /*
         * Rotate the camera or zoom depending on the input of the player.
         */

        void LateUpdate()
        {
            UpdateNavigation();
        }
    }
}