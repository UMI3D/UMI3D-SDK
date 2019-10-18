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
using UnityEditor;
using UnityEngine;

public class MouseRaycast : MonoBehaviour
{
    public Camera Camera;
    public Transform Shoulder;
    public Transform Hand;
    public float ArmLength;
    public float speed = 1.0F;

    private Vector3 RestPos;
    private float startTime;
    private Vector3 CurrentLocalPos;
    private Vector3 CurrentPos;
    private bool isTouching;
    private bool isTouched;

    // Start is called before the first frame update
    void Start()
    {
        RestPos = Hand.localPosition;
        isTouching = false;
        isTouched = false;
        startTime = -10;
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        Ray ray = Camera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            Transform objectHit = hit.transform;
            float Distance = Vector3.Distance(hit.point, Shoulder.position);
            if (Distance <= ArmLength)
            {
                if (!isTouching)
                {
                    startTime = Time.time;
                    isTouching = true;
                }

                if (!isTouched)
                {
                    float distCovered = (Time.time - startTime) * speed;
                    float fracJourney = distCovered / Vector3.Distance(CurrentPos, hit.point);
                    Hand.position = Vector3.Lerp(CurrentPos, hit.point, fracJourney);
                    if (Hand.position == hit.point)
                    {
                        isTouched = true;
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
                    startTime = Time.time;
                    isTouching = false;
                }

                if (isTouched)
                {
                    isTouched = false;
                }

                float distCovered = (Time.time - startTime) * speed;
                float fracJourney = distCovered / Vector3.Distance(CurrentLocalPos, RestPos);
                Hand.localPosition = Vector3.Lerp(CurrentLocalPos, RestPos, fracJourney);               
            }
        }
        CurrentLocalPos = Hand.localPosition;
        CurrentPos = Hand.position;
    }
}
