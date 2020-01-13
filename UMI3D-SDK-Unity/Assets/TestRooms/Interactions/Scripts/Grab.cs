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

[RequireComponent(typeof(Rigidbody))]
public class Grab : MonoBehaviour
{

    /// <summary>
    /// Should this object be snaped to the avatar bone on grab ?
    /// </summary>
    public bool snapToHand = false;

    public bool isGrabed { get; protected set; }

    /// <summary>
    /// <see cref="isGrabed"/> value on last frame.
    /// </summary>
    private bool isGrabed_lastframe;

    /// <summary>
    /// Avatar bone used for grab.
    /// </summary>
    private GameObject grabBone;

    private Vector3 offsetOnGrab;

    /// <summary>
    /// This have on purpose to be call by a OnTrigger Event.
    /// Setup the grab state of the object
    /// </summary>
    /// <param name="user"></param>
    /// <param name="bone"></param>
    public void Hold(UMI3DUser user, BoneDto bone)
    {
        if (user.avatar.instanciatedBones.TryGetValue(bone.type, out grabBone))
        {
            isGrabed = true;
            offsetOnGrab = grabBone.transform.InverseTransformDirection(this.transform.position - grabBone.transform.position);
        }
        else
        {
            Debug.LogError("Failed to grab, no avatar bone found");
        }
    }

    /// <summary>
    /// this will end the the grab
    /// </summary>
    public void Release()
    {
        isGrabed = false;
    }

    /// <summary>
    /// The Update method perform the grab when needed.
    /// </summary>
    private void Update()
    {
        if (isGrabed)
        {
            if (snapToHand)
                this.transform.position = grabBone.transform.position;
            else
                this.transform.position = grabBone.transform.position + grabBone.transform.TransformDirection(offsetOnGrab);
        }
        else if (isGrabed_lastframe)
        {
            this.transform.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }

        isGrabed_lastframe = isGrabed;
    }
}
