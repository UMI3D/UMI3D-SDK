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
using System.Collections.Generic;
using umi3d.common;

public class CursorOnHover : MonoBehaviour
{
    protected static Dictionary<string, GameObject> cursorInstances = new Dictionary<string, GameObject>();

    /// <summary>
    /// Prefab of the cursor to instantiate.
    /// </summary>
    public GameObject cursorPrefab;


    /// <summary>
    /// Instanciate the Cursor.
    /// This have on purpose to be call by a OnHoverEnter Event.
    /// </summary>
    public void CreateCursor(UMI3DUser user, string bone)
    {
        if (cursorInstances.TryGetValue(user.UserId + bone, out GameObject oldCursor))
        {
            cursorInstances.Remove(user.UserId + bone);
            Destroy(oldCursor);
            Debug.LogWarning("Cursor already exists");
        }
        cursorInstances.Add(user.UserId + bone, Instantiate(cursorPrefab, this.transform));
    }

    /// <summary>
    /// Destroy the Cursor.
    /// This have on purpose to be call by a OnHoverExit Event.
    /// </summary>
    public void DestroyCursor(UMI3DUser user, string bone)
    {
        string id = user.UserId + bone;
        if (cursorInstances.TryGetValue(id, out GameObject cursor))
        {
            cursorInstances.Remove(id);
            Destroy(cursor);
        }
    }

    /// <summary>
    /// This will move the position of the Cursor
    /// This have on purpose to be call by a OnHovered Event.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="pos"></param>
    /// <param name="norm"></param>
    public void UpdateCursorPosition(UMI3DUser user, string boneId, Vector3 pos, Vector3 norm)
    {
        if (cursorInstances.TryGetValue(user.UserId + boneId, out GameObject cursor))
        {
            cursor.transform.localPosition = pos;

            cursor.transform.LookAt(
                cursor.transform.position
                + this.transform.TransformDirection(norm));
        }
    }
}
