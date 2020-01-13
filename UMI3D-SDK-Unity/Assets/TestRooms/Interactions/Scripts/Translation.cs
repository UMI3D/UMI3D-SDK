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
using UnityEngine;
using umi3d.edk;

public class Translation : MonoBehaviour
{
    public GameObject frameOfReference;

    /// <summary>
    /// This have on purpose to be call by a OnManipulated Event.
    /// </summary>
    /// <param name="user">The who performed the manipulation</param>
    /// <param name="trans">The position delta of the manipulation</param>
    /// <param name="rot">The rotation delta of the manipulation</param>
    public void OnUserManipulation(UMI3DUser user, Vector3 trans, Quaternion rot)
    {
        this.transform.position += frameOfReference.transform.TransformDirection(trans);
    }

}
