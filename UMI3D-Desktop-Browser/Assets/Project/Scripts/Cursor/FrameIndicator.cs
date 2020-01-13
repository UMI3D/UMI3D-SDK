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
using UnityEngine;
using umi3d.common;

namespace BrowserDesktop.Cursor
{
    public class FrameIndicator : MonoBehaviour
    {
        [SerializeField]
        GameObject X = null;
        [SerializeField]
        GameObject Y = null;
        [SerializeField]
        GameObject Z = null;
        [SerializeField]
        GameObject RX = null;
        [SerializeField]
        GameObject RY = null;
        [SerializeField]
        GameObject RZ = null;

        [SerializeField]
        GameObject ProjectionResult = null;

        [SerializeField]
        bool display = false;
        [SerializeField]
        bool debug = false;

        DofGroupEnum dofGroup;
        public DofGroupEnum DofGroup
        {
            get { return dofGroup; }
            set {
                if (value == dofGroup) return;
                dofGroup = value;
                if (X.activeSelf != displayX()) X.SetActive(displayX());
                if (Y.activeSelf != displayY()) Y.SetActive(displayY());
                if (Z.activeSelf != displayZ()) Z.SetActive(displayZ());
                if (RX.activeSelf != displayRX()) RX.SetActive(displayRX());
                if (RY.activeSelf != displayRY()) RY.SetActive(displayRY());
                if (RZ.activeSelf != displayRZ()) RZ.SetActive(displayRZ());
                if (ProjectionResult.activeSelf != debug && display) ProjectionResult.SetActive(debug && display);
            }
        }

        public Transform Frame { set { transform.position = value.position; transform.rotation = value.rotation; } }


        public Vector3 Project(Ray ray)
        {
            return Project(ray, dofGroup);
        }

        public Vector3 Project(Ray ray, DofGroupEnum dofGroupEnum)
        {
            Vector3 result = transform.position;
            Vector3 projection;
            Plane projectionPlane;

            Plane XY = new Plane(transform.forward, transform.position);
            Plane XZ = new Plane(transform.up, transform.position);
            Plane YZ = new Plane(transform.right, transform.position);

            float d = float.MaxValue;

            switch (dofGroup)
            {
                case DofGroupEnum.X:
                    projection = Vector3.Project(ray.origin - transform.position, transform.right) + transform.position;
                    projectionPlane = new Plane(projection - ray.origin, projection);
                    if (projectionPlane.Raycast(ray, out d)) result = ray.origin + ray.direction * d;
                    break;
                case DofGroupEnum.Y:
                    projection = Vector3.Project(ray.origin - transform.position, transform.up) + transform.position;
                    projectionPlane = new Plane(projection - ray.origin, projection);
                    if (projectionPlane.Raycast(ray, out d)) result = ray.origin + ray.direction * d;
                    break;
                case DofGroupEnum.Z:
                    projection = Vector3.Project(ray.origin - transform.position, transform.forward) + transform.position;
                    projectionPlane = new Plane(projection - ray.origin, projection);
                    if (projectionPlane.Raycast(ray, out d)) result = ray.origin + ray.direction * d;
                    break;
                case DofGroupEnum.XY:
                    if (XY.Raycast(ray, out d)) { result = ray.origin + ray.direction * d; }
                    break;
                case DofGroupEnum.XZ:
                    if (XZ.Raycast(ray, out d)) { result = ray.origin + ray.direction * d; }
                    break;
                case DofGroupEnum.YZ:
                    if (YZ.Raycast(ray, out d)) { result = ray.origin + ray.direction * d; }
                    break;
                case DofGroupEnum.RX:
                    Vector2 axisX = new Vector2(0, 1);
                    if (YZ.Raycast(ray, out d)) { result = ray.origin + ray.direction * d; }
                    break;
                case DofGroupEnum.RY:
                    Vector2 axisY = new Vector2(0, 1);
                    if (XZ.Raycast(ray, out d)) { result = ray.origin + ray.direction * d; }
                    break;
                case DofGroupEnum.RZ:
                    Vector2 axisZ = new Vector2(0, 1);
                    if (XY.Raycast(ray, out d)) { result = ray.origin + ray.direction * d; }
                    break;
            }

            if (display && debug)
            {
                Debug.DrawLine(ray.origin, result, Color.black);
                ProjectionResult.transform.position = result;

            }

            return result;
        }


        bool displayX() { return display && (dofGroup == DofGroupEnum.X || dofGroup == DofGroupEnum.XY || dofGroup == DofGroupEnum.XYZ || dofGroup == DofGroupEnum.XZ || dofGroup == DofGroupEnum.X_RX || dofGroup == DofGroupEnum.ALL); }
        bool displayY() { return display && (dofGroup == DofGroupEnum.Y || dofGroup == DofGroupEnum.XY || dofGroup == DofGroupEnum.XYZ || dofGroup == DofGroupEnum.YZ || dofGroup == DofGroupEnum.Y_RY || dofGroup == DofGroupEnum.ALL); }
        bool displayZ() { return display && (dofGroup == DofGroupEnum.Z || dofGroup == DofGroupEnum.XZ || dofGroup == DofGroupEnum.XYZ || dofGroup == DofGroupEnum.YZ || dofGroup == DofGroupEnum.Z_RZ || dofGroup == DofGroupEnum.ALL); }
        bool displayRX() { return display && (dofGroup == DofGroupEnum.RX || dofGroup == DofGroupEnum.RX_RY || dofGroup == DofGroupEnum.RX_RY_RZ || dofGroup == DofGroupEnum.RX_RZ || dofGroup == DofGroupEnum.X_RX || dofGroup == DofGroupEnum.ALL); }
        bool displayRY() { return display && (dofGroup == DofGroupEnum.RY || dofGroup == DofGroupEnum.RX_RY || dofGroup == DofGroupEnum.RX_RY_RZ || dofGroup == DofGroupEnum.RY_RZ || dofGroup == DofGroupEnum.Y_RY || dofGroup == DofGroupEnum.ALL); }
        bool displayRZ() { return display && (dofGroup == DofGroupEnum.RZ || dofGroup == DofGroupEnum.RX_RZ || dofGroup == DofGroupEnum.RX_RY_RZ || dofGroup == DofGroupEnum.RY_RZ || dofGroup == DofGroupEnum.Z_RZ || dofGroup == DofGroupEnum.ALL); }
    }
}