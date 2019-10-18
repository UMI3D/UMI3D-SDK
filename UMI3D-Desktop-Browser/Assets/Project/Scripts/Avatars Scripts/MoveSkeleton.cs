using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSkeleton : MonoBehaviour
{
    public Camera camera;
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
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);

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
