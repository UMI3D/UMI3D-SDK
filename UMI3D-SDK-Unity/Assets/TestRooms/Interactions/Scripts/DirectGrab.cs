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

public class DirectGrab : MonoBehaviour
{
    private Transform originalParent;

    /// <summary>
    /// This have on purpose to be call by a OnTrigger Event.
    /// Move an object under a user bone.
    /// </summary>
    /// <param name="user">User who triggered the Event</param>
    /// <param name="bone">Bone the User used to trigger the Event</param>
    public void Hold(UMI3DUser user, BoneDto bone)
    {
        originalParent = this.transform.parent;
        UMI3DAvatarBone grabBone;
        if (UMI3DAvatarBone.instancesByUserId[user.UserId].TryGetValue(bone.id, out grabBone))
        {
            Transform grabTransform = UMI3D.Scene.GetObject(grabBone.boneId).transform;
            this.transform.parent = grabTransform;
            this.transform.localPosition = new Vector3(0, 0, 0);
            this.transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
    }

    /// <summary>
    /// This have on purpose to be call by a OnRelease Event.
    /// Release the object and set it back under its parent.
    /// </summary>
    public void Release()
    {
        this.transform.parent = originalParent;
        this.transform.localPosition = new Vector3(0, 0, 0);
        this.transform.localRotation = Quaternion.Euler(0, 0, 0);
    }
}
