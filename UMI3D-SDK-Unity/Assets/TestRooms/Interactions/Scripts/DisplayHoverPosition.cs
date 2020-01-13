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
public class DisplayHoverPosition : MonoBehaviour
{
    public GenericObject3D interactbleObject;

    /// <summary>
    /// Gameobject placed at the hovered position.
    /// </summary>
    public GameObject hoverCursor;

    private void Awake()
    {
        if (!interactbleObject.isInteractable)
        {
            Debug.LogWarning("Warning : the hovered object has to be interactable");
        }
        hoverCursor.transform.SetParent(interactbleObject.transform, true);
    }

    /// <summary>
    /// Move the hoverCursor according to the hovered position
    /// This have on purpose to be call by a OnHovered Event.
    /// </summary>
    /// <param name="user">the user who triggered the event</param>
    /// <param name="pos">the position he is hovering in local space</param>
    /// <param name="norm">the normal vector on the hovering point</param>
    public void onHovered(UMI3DUser user, Vector3 pos, Vector3 norm)
    {
        hoverCursor.transform.localPosition = pos;
        hoverCursor.transform.LookAt(
            hoverCursor.transform.position
            + interactbleObject.transform.TransformDirection(norm));
    }
}
