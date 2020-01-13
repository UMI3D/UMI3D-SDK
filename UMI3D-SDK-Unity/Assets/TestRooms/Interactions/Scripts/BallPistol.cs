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
    public GameObject bulletPrefab;
    public float strength = 10;

    /// <summary>
    /// This have on purpose to be call by a OnTrigger Event.
    /// Shoot a bullet.
    /// </summary>
    /// <param name="user">user who triggered</param>
    /// <param name="bone">bone the user used to trigger</param>
    public void Shoot(UMI3DUser user, BoneDto bone)
    {
        GameObject bullet = Instantiate(bulletPrefab);
        bullet.GetComponent<Rigidbody>().velocity = strength * this.transform.forward;
    }
}
