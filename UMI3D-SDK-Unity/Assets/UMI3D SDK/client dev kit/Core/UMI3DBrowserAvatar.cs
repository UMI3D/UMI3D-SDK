using umi3d.cdk;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using umi3d.common;

public class UMI3DBrowserAvatar : MonoBehaviour
{
    public Transform viewpoint;
    public Transform world;
    public AbstractNavigation orbitation;
    public AbstractNavigation walk;
    public AbstractNavigation fly;
    AbstractNavigation currentNav = null;

    public List<BoneType> BonesToFilter;
    public AvatarDto avatar;

    /*
     * 
     *      Main Behaviour
     * 
     */

    bool IsWalking() { return currentNav == walk; }
    bool IsFlying() { return currentNav == fly; }
    bool IsOrbitating() { return currentNav == orbitation; }

    void Start()
    {
        currentNav = walk;
        UMI3DBrowser.Navigation = currentNav;
        walk.Setup(world, viewpoint);
        StartCoroutine(LogUserPosition(1f / 30f));
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    private void OnApplicationQuit()
    {
        StopAllCoroutines();
    }

    /// <summary>
    /// Send the user tracking data to the server
    /// </summary>
    IEnumerator LogUserPosition(float dt)
    {
        bool connected = true;
        while (connected)
        {
            if (UMI3DBrowser.Scene != null)
            {
                var scene = UMI3DBrowser.Scene.transform;

                this.BonesIterator(this.avatar);
                this.avatar.ScaleScene = scene.transform.lossyScale;

                var data = new NavigationRequestDto();
                data.TrackingZonePosition = -world.position;
                data.TrackingZoneRotation = Quaternion.Inverse(world.rotation);
                data.CameraPosition = world.InverseTransformPoint(this.viewpoint.position) - world.InverseTransformPoint(scene.transform.position);
                data.CameraRotation = viewpoint.localRotation;
                data.Avatar = this.avatar;

                try
                {
                    UMI3DWebSocketClient.Navigate(data);
                }
                catch (Exception err)
                {
                    Debug.LogError(err);
                }
                finally { }
            }
            yield return new WaitForSeconds(dt);
        }
    }

    /// <summary>
    /// Update the browser's navigation type
    /// </summary>
    private void UpdateNavigation()
    {
        AbstractNavigation newnav = null;

        if (UMI3DBrowser.Media == null || UMI3DBrowser.Media.NavigationType == NavigationType.Walk)
            newnav = walk;
        else if (UMI3DBrowser.Media.NavigationType == NavigationType.Orbitation)
            newnav = orbitation;

        else if (UMI3DBrowser.Media.NavigationType == NavigationType.Fly)
            newnav = fly;

        if (newnav != currentNav)
        {
            if (currentNav != null)
                currentNav.Disable();
            if (newnav != null)
                newnav.Setup(world, viewpoint);
            currentNav = newnav;
            UMI3DBrowser.Navigation = currentNav;
        }
        UMI3DBrowser.isEnterTeleportationAllowed = true;
        UMI3DBrowser.viewpointOffsetForEnterTeleportation = new Vector3(viewpoint.position.x, 0, viewpoint.position.z);
    }

    /// <summary>
    /// Iterate through the bones of the browser's skeleton to create BoneDto
    /// </summary>
    private void BonesIterator(AvatarDto avatar)
    {
        List<BoneDto> BonesList = new List<BoneDto>();
        List<UMI3DBoneType> Children = new List<UMI3DBoneType>();

        this.transform.GetComponentsInChildren<UMI3DBoneType>(Children);
        foreach (UMI3DBoneType Item in Children)
        {
            BoneDto boneInstance = null;

            if (Item.BoneType != BoneType.None)
            {
                boneInstance = new BoneDto();
                this.set_bone(boneInstance, Item.BoneType, Item);
            }
            if (boneInstance != null)
            {
                BonesList.Add(boneInstance);
            }
        }
        avatar.BoneList = BonesList;
    }

    /// <summary>
    /// Set a BoneDto at a Transform object properties
    /// </summary>
    private void set_bone(BoneDto bone, BoneType type, UMI3DBoneType item)
    {
        bone.type = type;
        bone.Position = this.viewpoint.transform.InverseTransformPoint(item.transform.position);
        bone.Position.X *= this.viewpoint.lossyScale.x;
        bone.Position.Y *= this.viewpoint.lossyScale.y;
        bone.Position.Z *= this.viewpoint.lossyScale.z;
        bone.Rotation = (Quaternion.Inverse(this.viewpoint.transform.rotation) * item.transform.rotation);
        bone.Scale = item.transform.lossyScale;
    }

    /*
     * Rotate the camera or zoom depending on the input of the player.
     */

    void LateUpdate()
    {
        UpdateNavigation();
    }
}