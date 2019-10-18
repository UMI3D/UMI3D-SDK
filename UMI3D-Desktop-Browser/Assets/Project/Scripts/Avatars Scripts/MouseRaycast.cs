using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MouseRaycast : MonoBehaviour
{
    public Camera camera;
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
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);

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
