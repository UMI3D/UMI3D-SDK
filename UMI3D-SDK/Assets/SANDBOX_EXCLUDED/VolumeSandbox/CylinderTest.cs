using umi3d.common.volume;
using UnityEngine;

public class CylinderTest : MonoBehaviour
{
    public MeshFilter filter;

    private void Start()
    {
        filter.mesh = GeometryTools.GetCylinder(this.transform.position, this.transform.rotation, this.transform.localScale, 1, 1);
        filter.sharedMesh = filter.mesh;
    }

}
