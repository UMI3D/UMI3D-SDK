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

public class Fly : AbstractNavigation
{
    private bool used = false;

    public float _xSpeed = 0.2f;
    public float _ySpeed = 0.2f;
    public float _flySpeed = 0.2f;
    public float _stepSpeed = 0.1f;
    float _x = 0.0f;
    float _y = 0.0f;
    float DT = 0f;

    

    public override void Setup (Transform world, Transform viewpoint)
    {
        base.Setup(world, viewpoint);
        var euler = viewpoint.eulerAngles;
        _x = euler.x;
        _y = euler.y;
        Move(Vector3.zero, 0, 0);
        used = true;
    }

    

    void Move( Vector3 translation , float rx, float ry) {
        if (managed)
        {
            base.Move();
        } else
        {
            _x += rx;
            _y += ry;
            viewpoint.position += translation;
            viewpoint.rotation = Quaternion.Euler(_x, _y, 0.0f);
        }
    }

    void LateUpdate()
    {
        if (UIDetector.isUI)
            return;
        
        DT = Time.deltaTime;

        if (!used)
            return;

        var rx = 0f;
        var ry = 0f;
        var translate = Vector3.zero;

        if (Input.GetMouseButton(1))
        {
            rx = -Input.GetAxis("Mouse Y") * _ySpeed;
            ry = Input.GetAxis("Mouse X") * _xSpeed;
        }
        
        var fr = viewpoint.forward;
        fr.Normalize();
        var rg = viewpoint.right;
        rg.Normalize();

        translate += Input.GetAxis(UMI3DBrowser.navigationAxis_X) * DT * rg * _stepSpeed;
        translate += Input.GetAxis(UMI3DBrowser.navigationAxis_Z) * DT * fr * _flySpeed;

        Move(translate, rx, ry);
    }

    public override void Disable()
    {
        used = false;
    }
}
