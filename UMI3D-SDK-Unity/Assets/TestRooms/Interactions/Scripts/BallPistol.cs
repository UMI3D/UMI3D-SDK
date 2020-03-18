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
using umi3d.common;
using umi3d.edk;
using UnityEngine;

public class BallPistol : MonoBehaviour
{
    public CVEEquipable equipable;
    public Transform defaultParent;

    public Transform shootPosition;
    public GameObject bulletPrefab;
    public float strength = 10;


    protected virtual void Awake()
    {
        equipable.onUnequiped.AddListener(() =>
        {
            this.transform.parent = defaultParent;
            this.transform.localPosition = Vector3.zero;
            this.transform.localRotation = Quaternion.Euler(Vector3.zero);
        });
    }

    /// <summary>
    /// This have on purpose to be call by a OnTrigger Event.
    /// Shoot a bullet.
    /// </summary>
    /// <param name="user">user who triggered</param>
    /// <param name="bone">bone the user used to trigger</param>
    public void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, UMI3D.Scene.transform);
        bullet.transform.position = shootPosition.position;
        bullet.GetComponent<Rigidbody>().velocity = strength * shootPosition.transform.forward;
        Destroy(bullet, 2);
    }

    public void Equipe(UMI3DUser user, string bone)
    {
        BoneType boneType = UMI3DAvatarBone.instancesByUserId[user.UserId][bone].boneType;

        if ((boneType == BoneType.Hand_Left) || (boneType == BoneType.Hand_Right))
        {
            equipable.RequestEquip(user, bone);
        }
        else
        {
            UMI3DAvatarBone userBone = UMI3DAvatarBone.GetUserBoneByType(user.UserId, BoneType.Hand_Right);
            equipable.RequestEquip(user, (userBone != null) ? userBone.boneId : bone);
        }
    }


}
