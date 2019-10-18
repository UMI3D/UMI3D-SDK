using umi3d.cdk;
using UnityEngine;

public class Walk : AbstractNavigation
{
    public InteractionMapper mapper;

    private bool used = false;

    public float _xSpeed = 0.2f;
    public float _ySpeed = 0.2f;
    public float _height = 1.8f;
    public float _runSpeed = 0.2f;
    public float _stepSpeed = 0.1f;
    public float _heightSpeed = 0.05f;

    float _x = 0.0f;
    float _y = 0.0f;

    float DT = 0f;

    public override void Setup(Transform trackingZone, Transform viewpoint)
    {
        base.Setup(trackingZone, viewpoint);
        var euler = viewpoint.localEulerAngles;
        _x = euler.x;
        _y = euler.y;
        _height = viewpoint.localPosition.y;
        Move(Vector3.zero, 0, 0);
        used = true;
    }


    public void ZoomIn()
    {
        world.position += new Vector3(0, _heightSpeed, 0);
    }

    public void ZoomOut()
    {
        world.position -= new Vector3(0, _heightSpeed, 0);
    }

    void Move(Vector3 translation, float rx, float ry)
    {
        if (managed)
        {
            base.Move();
        }
        else
        {
            _x += rx;
            _y += ry;
            _x = Mathf.Clamp(_x, -60f, 60f);
            world.position -= translation;
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
            //_y = Mathf.Clamp(_y, -60f, 60f);
        }

        if (Input.GetAxis(UMI3DBrowser.navigationAxis_Y) < 0.0f)
        {
            this.ZoomOut();
        }
        else if (Input.GetAxis(UMI3DBrowser.navigationAxis_Y) > 0.0f)
        {
            this.ZoomIn();
        }

        var fr = viewpoint.forward;
        fr.y = 0f;
        fr.Normalize();
        var rg = viewpoint.right;
        rg.y = 0f;
        rg.Normalize();

        
        translate += Input.GetAxis(UMI3DBrowser.navigationAxis_X) * Time.deltaTime * rg * _runSpeed;
        translate += Input.GetAxis(UMI3DBrowser.navigationAxis_Z) * Time.deltaTime * fr * _runSpeed;

        Move(translate, rx, ry);
    }

    public override void Disable()
    {
        used = false;
    }
}
