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

namespace BrowserDesktop.Avatar
{
    public class MoveSkeleton : MonoBehaviour
    {
        public Camera _camera;
        public float speed = 1.0F;
        public Transform Shoulder;
        public Transform Hand;
        public float ArmLength = 1;

        private Vector3 RestPos;

        private bool isTouching;
        private bool isTouched;

        public float toVel = 12.5f;
        public float maxVel = 20f;
        public float maxForce = 500f;
        public float gain = 800f;

        public GameObject GO1;

        // Start is called before the first frame update
        void Start()
        {
            RestPos = this.transform.InverseTransformPoint(Hand.position);
            isTouching = false;
            isTouched = false;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            RaycastHit hit;
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                Transform objectHit = hit.transform;
                float shoulderDist = Vector3.Distance(hit.point, Shoulder.position);
                if (shoulderDist <= ArmLength)
                {
                    if (!isTouching)
                    {
                        isTouching = true;
                    }

                    if (!isTouched)
                    {
                        Vector3 n = hit.point - Hand.transform.position;
                        Vector3 tgtVel = Vector3.ClampMagnitude(toVel * n, maxVel);
                        Vector3 error = tgtVel - Hand.GetComponent<Rigidbody>().velocity;
                        Vector3 force = Vector3.ClampMagnitude(gain * error, maxForce);
                        Hand.GetComponent<Rigidbody>().AddForce(force);
                        if (n.magnitude < 0.1f)
                        {
                            Hand.transform.position = hit.point;
                        }
                    }
                    else
                    {
                        Hand.position = hit.point;
                    }
                }
                else
                {
                    if (isTouching)
                    {
                        isTouching = false;
                    }

                    if (isTouched)
                    {
                        isTouched = false;
                    }

                    Vector3 m = this.transform.TransformPoint(RestPos) - Hand.transform.position;
                    Vector3 tgtVel = Vector3.ClampMagnitude(toVel * m, maxVel);
                    Vector3 error = tgtVel - Hand.GetComponent<Rigidbody>().velocity;
                    Vector3 force = Vector3.ClampMagnitude(gain * error, maxForce);
                    Hand.GetComponent<Rigidbody>().AddForce(force);

                    if (m.magnitude < 0.1f)
                    {
                        Hand.transform.position = this.transform.TransformPoint(RestPos);
                    }
                }
            }
            else
            {
                if (isTouching)
                {
                    isTouching = false;
                }

                if (isTouched)
                {
                    isTouched = false;
                }

                Vector3 m = this.transform.TransformPoint(RestPos) - Hand.transform.position;

                if (m.magnitude > 0.1f)
                {
                    Vector3 tgtVel = Vector3.ClampMagnitude(toVel * m, maxVel);
                    Vector3 error = tgtVel - Hand.GetComponent<Rigidbody>().velocity;
                    Vector3 force = Vector3.ClampMagnitude(gain * error, maxForce);
                    Hand.GetComponent<Rigidbody>().AddForce(force);
                }
                else
                {
                    Hand.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    GO1.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                    Hand.transform.position = this.transform.TransformPoint(RestPos);
                }
            }
        }
    }
}