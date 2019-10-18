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
using UnityEngine;

public class Orbitation : AbstractNavigation
{
    public InteractionMapper mapper;
    private bool used = false;
    
    public float _distance = 1.0f;
    public float _initalY = 30f;
    public float _initalX = 180f;
    public float _xSpeed = 0.2f;
    public float _ySpeed = 0.2f;
    public float _zoomStep = 0.05f;
    float DT = 0f;
    

    Vector3 _distanceVector;
    float _x = 0.0f;
    float _y = 0.0f;

    public override void Setup (Transform world, Transform viewpoint)
    {
        base.Setup(world, viewpoint);
        _distanceVector = new Vector3(0.0f, 0.0f, 0.0f);// -_distance);
        Vector2 angles = viewpoint.localEulerAngles;
        _x = _initalX;
        _y = _initalY;
        this.Rotate(_x, _y);
        used = true;
    }

    /*
     * Transform the cursor mouvement in rotation and in a new position
     * for the camera.
     */
    public void Rotate(float x, float y)
    {
        if (UMI3DBrowser.Scene == null)
            return;
        //Transform angle in degree in quaternion form used by Unity for rotation.
        Quaternion rotation = Quaternion.Euler(y, x, 0.0f);
        //The new position is the target position + the distance vector of the camera
        //rotated at the specified angle.
        Vector3 position = rotation * _distanceVector + UMI3DBrowser.Scene.transform.position;
        //Update the rotation and position of the camera.
        viewpoint.rotation = rotation;
        viewpoint.position = position;
    }

    public void ZoomIn()
    {
        _distance -= _zoomStep;
        _distanceVector = new Vector3(0.0f, 0.0f, -_distance);
        this.Rotate(_x, _y);
    }

    public void ZoomOut()
    {
        _distance += _zoomStep;
        _distanceVector = new Vector3(0.0f, 0.0f, -_distance);
        this.Rotate(_x, _y);
    }

    void LateUpdate()
    {
        if (UIDetector.isUI)
            return;

        DT = Time.deltaTime;

        if (!used)
            return;

        if (Input.GetMouseButton(1))
        {
            _x += Input.GetAxis("Mouse X") * _xSpeed;
            _y += -Input.GetAxis("Mouse Y") * _ySpeed;
        }
        Rotate(_x, _y);
        if (Input.GetAxis("Mouse ScrollWheel") < 0.0f)
        {
            this.ZoomOut();
        }
        else if (Input.GetAxis("Mouse ScrollWheel") > 0.0f)
        {
            this.ZoomIn();
        }
    }

    public override void Disable()
    {
        used = false;
    }
}
