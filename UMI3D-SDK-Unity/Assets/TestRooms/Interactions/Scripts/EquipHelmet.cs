using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using umi3d.common;
using umi3d.edk;

public class EquipHelmet : MonoBehaviour
{
    public CVEEquipable equipable;

    public BoneType boneType;

    private Vector3 startPosition;
    private Quaternion startRotation;


    private void Start()
    {
        startPosition = equipable.transform.position;
        startRotation = equipable.transform.rotation;

        equipable.onUnequiped.AddListener(() =>
        {
            if (equipable)
            {
                equipable.transform.position = startPosition;
                equipable.transform.rotation = startRotation;
            }
        });
    }

    public void SwitchEquipe(UMI3DUser user, string boneId)
    {
        UMI3DAvatarBone userBone = UMI3DAvatarBone.GetUserBoneByType(user.UserId, boneType);
        if (userBone != null)
        {
            if (!equipable.isEquiped)
            {
                equipable.RequestEquip(user, userBone.boneId);
            }
            else
            {
                equipable.RequestUnequip();
            }
        }
        else
        {
            NotificationDto notif = new NotificationDto()
            {
                title = "Equipment error.",
                content = "You can't equipe this object.",
                duration = 5f,
            };
            UMI3DNotifier.Notify(user, notif);
        }
    }
}