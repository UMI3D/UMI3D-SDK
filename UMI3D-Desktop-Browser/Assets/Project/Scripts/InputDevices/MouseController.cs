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
using System.Collections;
using System.Collections.Generic;
using umi3d.cdk;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseController : MonoBehaviour {


    public InteractionMapper player;

    public Vector3 speed = new Vector3(1f, 1f, 1f);
    public Vector3 dragDropSpeed = new Vector3(1f, 1f ,1f);
    public float hoveredFPS = 30f;

    HoverListener hoverred = null;
    float clickdelay = 0.2f;
    float lastmousedown = 0f;
    Vector3 lastmouseposition = new Vector3(0, 0, 0);
    Vector3 lastmousedownposition = new Vector3(0, 0, 0);

    bool mousedown = false;
    bool mousemoving = false;

    Vector3 lastHoveredPos;
    Vector3 lastHoveredNormal;
    bool sendHovered = false;


    void OnMouseDown( Vector3 mouse, float now , bool overViewport)
    {
        //if (!EventSystem.current.IsPointerOverGameObject())
        //{
            if (mousedown && now - lastmousedown > clickdelay)
            {
                if (!mousemoving)
                {
                    mousemoving = true;
                    player.OnDragStart(lastmousedownposition);
                }
                if (mousemoving && player.On2DMoved != null && !Input.GetKey(KeyCode.LeftShift))
                {
                    var distance = new Vector3(
                        speed.x * (mouse.x - lastmousedownposition.x) / Screen.width,
                        speed.y * (mouse.y - lastmousedownposition.y) / Screen.height,
                        0);
                    var move = new Vector2(
                        dragDropSpeed.x * (mouse.x - lastmouseposition.x) / Screen.width,
                        dragDropSpeed.y * (mouse.y - lastmouseposition.y) / Screen.height);
                    player.On2DMoved(move, distance);
                }
                if (mousemoving && player.On2DMoved != null && Input.GetKey(KeyCode.LeftShift))
                {
                    var distance = new Vector3(
                        speed.x * (mouse.x - lastmousedownposition.x) / Screen.width,
                        0,
                        speed.z * (mouse.y - lastmousedownposition.z) / Screen.height);
                    var move = new Vector2(
                        dragDropSpeed.x * (mouse.x - lastmouseposition.x) / Screen.width,
                        dragDropSpeed.z * (mouse.y - lastmouseposition.z) / Screen.height);
                    player.On2DMoved(move, distance);
                }


            }
            else if (!mousedown && overViewport)
            {
                lastmousedown = now;
                mousedown = true;
                lastmousedownposition.Set(mouse.x, mouse.y, mouse.y);
            }
        //}
    }

    void OnMouseUp(Vector3 mouse, float now, bool overViewport)
    {

        //if (!EventSystem.current.IsPointerOverGameObject())
        //{
        if (mousemoving)
        {
            mousemoving = false;
            player.OnDragStop();
        }
        if (!UIDetector.isUI && mousedown && now - lastmousedown < clickdelay)
            player.On2DPick(lastmousedownposition);
        mousedown = false;
        //}
    }

    void HoverObject(Vector3 mouse)
    {
        if (UIDetector.isUI)
        {
            if (hoverred != null)
                hoverred.OnHover(false);
            hoverred = null;
            sendHovered = false;
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(mouse);
        RaycastHit[] hits = umi3d.Physics.RaycastAll(ray, 100);
        var cast = false;
        foreach(RaycastHit hit in hits)
        {
            if (hit.collider.gameObject.GetComponentInParent<HDScene>() == null)
                break;
            var h = hit.collider.gameObject.GetComponent<HoverListener>();
            if (h == null)
                h = hit.collider.gameObject.GetComponentInParent<HoverListener>();
            if (h != null)
            {
                cast = true;
                if (h != hoverred)
                {
                    if (hoverred != null)
                    {
                        sendHovered = false;
                        hoverred.OnHover(false);
                    }
                    hoverred = h;
                    hoverred.OnHover(true);
                }
                else
                {
                    var p = h.transform.InverseTransformPoint(hit.point);
                    var n = h.transform.InverseTransformDirection(hit.normal);
                    if(p != lastHoveredPos || n != lastHoveredNormal)
                    {
                        lastHoveredPos = p; lastHoveredNormal = n;
                        sendHovered = true;
                        //hoverred.OnHovered( p, n);
                    }
                }
                break;
            }
        }
        if (!cast && hoverred != null)
        {
            hoverred.OnHover(false);
            hoverred = null;
        }
    }
    
    void Update () {
        var mouse = Input.mousePosition;
        float now = Time.time;
        bool overViewport = MouseOverViewport();
        if (Input.GetMouseButton(0))
            OnMouseDown(mouse, now, overViewport);
        else
            OnMouseUp(mouse, now, overViewport);
        lastmouseposition.Set(mouse.x, mouse.y, mouse.y);
        if (overViewport)
            HoverObject(mouse);
    }

    IEnumerator UpdateHovered ()
    {
        while(gameObject != null)
        {
            if (sendHovered)
            {
                sendHovered = false;
                if(hoverred != null)
                    hoverred.OnHovered( lastHoveredPos, lastHoveredNormal);
            }
            if(hoveredFPS == 0f)
                yield return new WaitForSeconds(1f / 60f);
            else
                yield return new WaitForSeconds(1f / hoveredFPS);
        }
        yield return null;
    }

    /*
    * Does viewport of the "local_cam" camera, which is inside the "main_cam" camera
    * currently contain the mouse?
    */
    bool MouseOverViewport()
    {
        return Camera.main.pixelRect.Contains(Input.mousePosition);
    }

    private void OnEnable()
    {
        StartCoroutine(UpdateHovered());
    }

    private void OnDisable()
    {
        StopCoroutine("UpdateHovered");
    }
}
