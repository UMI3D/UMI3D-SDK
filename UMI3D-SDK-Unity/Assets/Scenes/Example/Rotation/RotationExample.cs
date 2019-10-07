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
    public class RotationExample : MonoBehaviour
    {

        public Manipulation Manipulation;

        public CVEPrimitive A, B, C;
        bool allOut = true;

        private void Start()
        {
            //the onManipulated event is sent when a user is manipulated the manipulation target.
            Manipulation.onManipulated.AddListener(OnManipulated);
            defaultRotA = A.transform.localRotation;
            defaultPosB = B.transform.localPosition;
            yTmp = 0;
        }
        Quaternion defaultRotA;
        Vector3 defaultPosB;
        float yTmp;

        Vector3 colorTmp = new Vector3();

        /// <summary>
        /// </summary>
        /// <param name="user">the user which is manipulating the target</param>
        /// <param name="deltaXYZ">a manipulation vector on Axes X Y and Z</param>
        /// <param name="deltaAngle">a manipulation quaternion on Axes X Y Z</param>
        void OnManipulated(UMI3DUser user, Vector3 deltaXYZ, Quaternion deltaAngle)
        {
            // directly set the deltaANgle to the object rotation;
            A.transform.localRotation *= deltaAngle;
            // Doing some weird thing to change the deltaAngle to a height.
            yTmp += Quaternion.Dot(defaultRotA, deltaAngle) * Time.deltaTime;
            float y = Mathf.PingPong(yTmp,1f);
            B.transform.localPosition = defaultPosB + new Vector3(0,y-0.5f,0);
            // calculating a color based on the deltaAngle
            colorTmp += deltaAngle.eulerAngles;
            C.Material.AlbedoColor = new Color(0.5f + Mathf.Cos(colorTmp.x), 0.5f + Mathf.Cos(colorTmp.y + Mathf.PI / 2), 0.5f + Mathf.Sin(colorTmp.z));
            C.Material.NotifyUpdate();
        }


        /// check if the example need to be reset
        private void Update()
        {
            Vector2 center = new Vector2(transform.position.x, transform.position.z);

            bool wasAllOut = allOut;
            allOut = true;
            foreach (var user in UMI3D.UserManager.GetUsers())
            {
                Vector3 userPos = user.avatar.viewpoint.gameObject.transform.position;
                if (Vector2.Distance(new Vector2(userPos.x, userPos.z), center) <= 2)
                {
                    allOut = false;
                    break;
                }
            }
            if (allOut && !wasAllOut)
            {
                A.transform.localRotation = defaultRotA;
                B.transform.localPosition = defaultPosB;
                yTmp = 0;
                colorTmp = Vector3.zero;
                C.Material.AlbedoColor = new Color(0.5f + Mathf.Cos(colorTmp.x), 0.5f + Mathf.Cos(colorTmp.y), 0.5f + Mathf.Cos(colorTmp.z));
                C.Material.NotifyUpdate();
            }
        }

    }
}