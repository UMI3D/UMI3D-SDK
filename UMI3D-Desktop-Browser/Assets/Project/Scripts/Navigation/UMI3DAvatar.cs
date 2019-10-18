using umi3d.cdk;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using umi3d.common;

public class UMI3DAvatar : MonoBehaviour {

    public UIFader fader;
    public Transform viewpoint;
    public Transform world;
    public AbstractNavigation orbitation;
    public AbstractNavigation walk;
    public AbstractNavigation fly;
    AbstractNavigation currentNav = null;

    public AvatarDto avatar;
    //public GameObject SceneAnchor;

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
            fader.gameObject.SetActive(true);
            if (currentNav != null)
                currentNav.Disable();
            if (newnav != null)
                newnav.Setup(world, viewpoint);
             currentNav = newnav;
            UMI3DBrowser.Navigation = currentNav;
        }
        UMI3DBrowser.isEnterTeleportationAllowed = true;
        UMI3DBrowser.viewpointOffsetForEnterTeleportation = /*viewpoint; */   new Vector3(viewpoint.position.x, 0, viewpoint.position.z);



    }

    private void BonesIterator(AvatarDto avatar)
        {
            List<BoneDto> BonesList = new List<BoneDto>();
            List<Transform> Children = new List<Transform>();

            this.transform/*.GetChild(0)*/.GetComponentsInChildren<Transform>(Children);

            foreach (Transform Item in Children)
            {
                string tag = Item.gameObject.tag;
                BoneDto boneInstance = null;

                switch (tag) // voir si on ajoute des tags / types ?
            {
                    case "Head": 
                        boneInstance = new BoneDto();
                        this.set_bone(boneInstance, BoneType.Head, Item);
                        break;
                    case "Chest":
                        boneInstance = new BoneDto();
                        this.set_bone(boneInstance, BoneType.Chest, Item);
                        break;
                    case "Base":
                        boneInstance = new BoneDto();
                        this.set_bone(boneInstance, BoneType.Base, Item);
                        break;
                    case "Sphere":
                        boneInstance = new BoneDto();
                        this.set_bone(boneInstance, BoneType.Sphere, Item);
                        break;
                    case "Cylinder":
                        boneInstance = new BoneDto();
                        this.set_bone(boneInstance, BoneType.Cylinder, Item);
                        break;
                    case "Finger":
                        boneInstance = new BoneDto();
                        this.set_bone(boneInstance, BoneType.Finger, Item);
                        break;
                    case "Capsule":
                        boneInstance = new BoneDto();
                        this.set_bone(boneInstance, BoneType.Capsule, Item);
                        break;
                    case "Foot":
                        boneInstance = new BoneDto();
                        this.set_bone(boneInstance, BoneType.Foot, Item);
                        break;
                    case "Joint_Foot":
                        boneInstance = new BoneDto();
                        this.set_bone(boneInstance, BoneType.Joint_Foot, Item);
                        break;
                    case "LHand":
                        boneInstance = new BoneDto();
                        this.set_bone(boneInstance, BoneType.LHand, Item);
                        break;
                    case "RHand":
                        boneInstance = new BoneDto();
                        this.set_bone(boneInstance, BoneType.RHand, Item);
                        break;
                    default:
                        break;
                }
                if (boneInstance != null)
                {
                    BonesList.Add(boneInstance);
                }
            }
            avatar.BoneList = BonesList;
        }

        private void set_bone(BoneDto bone, BoneType type, Transform item)
        {
            bone.type = type;
            bone.Position = this.viewpoint.transform.InverseTransformPoint(item.position); //item.localPosition.x
            bone.Position.X *= this.viewpoint.lossyScale.x;
            bone.Position.Y *= this.viewpoint.lossyScale.y;
            bone.Position.Z *= this.viewpoint.lossyScale.z;
            bone.Rotation = (Quaternion.Inverse(this.viewpoint.transform.rotation) * item.transform.rotation);
            bone.Scale = item.lossyScale;
        }

        /**
         * Rotate the camera or zoom depending on the input of the player.
         */

    void LateUpdate()
    {
        UpdateNavigation();
    }
}