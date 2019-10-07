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

namespace umi3d.example
{
    /// <summary>
    /// make the object move when the hover target is hovered.
    /// </summary>
    [RequireComponent(typeof(GenericObject3D))]
    public class TeleporterAnimation : MonoBehaviour
    {
        public Hover Hover;
        public float speed;
        [Range(0.1f,5)]
        public float amplitude;

        GenericObject3D obj;
        HashSet<UMI3DUser> HoveringUser =  new HashSet<UMI3DUser>();
        Vector3 defaultPosition;
        float yTmp;          


        private void Start()
        {
            obj = GetComponent<GenericObject3D>();
            defaultPosition = obj.transform.localPosition;
            // make sure the position can be diferent for each user. 
            obj.objectPosition.Sync(false);
            Hover.onHoverEnter.AddListener(onHoverEnter);
            Hover.onHoverExit.AddListener(onHoverExit);
        }

        /// <summary>
        /// make a simple up and down movement and asign it only to the users which are hovering the Hover target
        /// </summary>
        private void Update()
        {
            yTmp += speed * Time.deltaTime;
            float y = Mathf.PingPong(yTmp, amplitude);
            foreach (var user in HoveringUser)
            {
                obj.objectPosition.SetValue(user, defaultPosition + Vector3.up * (y));
            }
        }

        //add a user who start hovering the target into the list 
        void onHoverEnter(UMI3DUser user)
        {
            HoveringUser.Add(user);
        }

        //remove a user who stop hovering the target from the list
        void onHoverExit(UMI3DUser user)
        {
            if(HoveringUser.Contains(user)) HoveringUser.Remove(user);
        }
    }
}