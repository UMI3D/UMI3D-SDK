using System.Collections;
using System.Collections.Generic;
using UnscentedKalmanFilter;
using UnityEngine;
using System;

public class MovingWithFilter_NewVersion : MonoBehaviour
{
    List<double[]> rightMeasurements = new List<double[]>();
    List<double[]> rightEstimations = new List<double[]>();

    List<double[]> leftMeasurements = new List<double[]>();
    List<double[]> leftEstimations = new List<double[]>();

    UKF rightFilter = new UKF();
    UKF leftFilter = new UKF();
    UKF headFilter = new UKF();

    public Transform RightController;
    public Transform LeftController;
    public Transform Headset;

    public Transform AvatarHead;
    public Transform AvatarRightHand;
    public Transform AvatarLeftHand;

    float avatarScale = 1;

    public float MeasurePerSecond = 60; // 16, 8, 4

    bool hasStarted = false;

    //public List<InverseKinematics> ScriptLists = new List<InverseKinematics>();

    double[] right_previous_prediction;
    double[] left_previous_prediction;

    double[] right_prediction;
    double[] left_prediction;

    Vector3 right_regressed_position;
    Vector3 left_regressed_position;

    float lastCheckTime = 0f;

    // Start is called before the first frame update
    void Start()
    {
        //GameEvents.sizeEvent.AddListener(setAvatarScale);
        //GameEvents.startEvent.AddListener(StartCoroutine);
    }

    // Update is called once per frame
    void Update() 
    {
        if (hasStarted)
        {
            Regression();

            AvatarRightHand.localPosition = right_regressed_position;
            AvatarLeftHand.localPosition = left_regressed_position;
        }
    }

    IEnumerator moveSkeleton()
    {
        while (true)
        {
            Vector3 rightRelativePosition = (RightController.position - Headset.position) / avatarScale;
            double[] rightMeasurement = new double[] { rightRelativePosition.x, rightRelativePosition.y, rightRelativePosition.z };

            Vector3 leftRelativePosition = (LeftController.position - Headset.position) / avatarScale;
            double[] leftMeasurement = new double[] { leftRelativePosition.x, leftRelativePosition.y, leftRelativePosition.z };

            rightFilter.Update(rightMeasurement);
            rightMeasurements.Add(rightMeasurement);

            leftFilter.Update(leftMeasurement);
            leftMeasurements.Add(leftMeasurement);

            double[] newRightState = rightFilter.getState();
            right_prediction = new double[] { newRightState[0], newRightState[1], newRightState[2] };

            double[] newLeftState = leftFilter.getState();
            left_prediction = new double[] { newLeftState[0], newLeftState[1], newLeftState[2] };

            if (rightEstimations.Count > 0)
                right_previous_prediction = rightEstimations[rightEstimations.Count - 1];
            else
                right_previous_prediction = rightMeasurement;

            if (leftEstimations.Count > 0)
                left_previous_prediction = leftEstimations[leftEstimations.Count - 1];
            else
                left_previous_prediction = leftMeasurement;

            lastCheckTime = Time.time;

            yield return new WaitForSeconds(1f / MeasurePerSecond);

            rightEstimations.Add(right_prediction);
            leftEstimations.Add(left_prediction);
        }
    }

    // depend du framerate du serveur

    void Regression()
    {
        double check = lastCheckTime;
        double now = Time.time;

        var delta = now - check;

        var right_value_x = (right_prediction[0] - right_previous_prediction[0]) * MeasurePerSecond * delta + right_previous_prediction[0];
        var right_value_y = (right_prediction[1] - right_previous_prediction[1]) * MeasurePerSecond * delta + right_previous_prediction[1];
        var right_value_z = (right_prediction[2] - right_previous_prediction[2]) * MeasurePerSecond * delta + right_previous_prediction[2];

        rightEstimations.Add(new double[] { right_value_x, right_value_y, right_value_z });
        right_regressed_position = new Vector3((float)right_value_x, (float)right_value_y, (float)right_value_z);

        var left_value_x = (left_prediction[0] - left_previous_prediction[0]) * MeasurePerSecond * delta + left_previous_prediction[0];
        var left_value_y = (left_prediction[1] - left_previous_prediction[1]) * MeasurePerSecond * delta + left_previous_prediction[1];
        var left_value_z = (left_prediction[2] - left_previous_prediction[2]) * MeasurePerSecond * delta + left_previous_prediction[2];

        leftEstimations.Add(new double[] { left_value_x, left_value_y, left_value_z });
        left_regressed_position = new Vector3((float)left_value_x, (float)left_value_y, (float)left_value_z);
    }

    public void StartCoroutine()
    {
        //foreach (InverseKinematics script in ScriptLists)
        //{
        //    script.enabled = true;
        //}
        StartCoroutine("moveSkeleton");
        hasStarted = true;
    }

    public void setAvatarScale(string str)
    {
        if (float.TryParse(str, out float value))
            avatarScale = value;
    }
}