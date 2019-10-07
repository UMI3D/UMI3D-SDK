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
using umi3d.edk;
using umi3d.common;

namespace umi3d.example
{
    public class Teleporter : MonoBehaviour
    {

        public Transform teleportationPosition;
        public Pick Pick;

        /// an example on how to change a user position.
        public void TeleportUser(umi3d.edk.UMI3DUser user)
        {
            Vector3 spawnPos = new Vector3(
                teleportationPosition.position.x + user.avatar.transform.position.x - user.avatar.viewpoint.transform.position.x,
                teleportationPosition.position.y,
                teleportationPosition.position.z + user.avatar.transform.position.z - user.avatar.viewpoint.transform.position.z
                );
            user.Send(new TeleportDto() { Position = spawnPos });
        }

        void Start()
        {
            Pick.onPicked.AddListener((user) =>
            {
                TeleportUser(user);
            });
        }

    }
}