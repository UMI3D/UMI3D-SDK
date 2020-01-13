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
using System.Collections.Generic;
using UnityEngine;
using BrowserDesktop.Controller;
using BrowserDesktop.Interaction;
using BrowserDesktop.Menu;

namespace BrowserDesktop.Cursor
{
    public class CursorKeyInput : KeyInput
    {
        // Start is called before the first frame update
        private bool swichOnDown = false;
        private bool constrainDistanceChange = false;
        public Transform Cursor;
        public Transform AvatarParent;
        public Transform Head;
        public float distCursor = 1;
        public float MaxDistCursorDelta = 0.3f;
        public float MinimumCursorDistance = 0.5f;
        public float ScrollToDistSpeed = 20f;
        umi3d.cdk.Interactable lastObject;

        public bool SwichOnDown { get => swichOnDown; protected set => swichOnDown = value; }

        protected override void Start()
        {
            base.Start();
            onInputDown.AddListener(() =>
            {
                SwichOnDown = (CursorHandler.State == CursorHandler.CursorState.Hover);
                if (SwichOnDown)
                {
                    CursorHandler.State = CursorHandler.CursorState.Clicked;
                }
                lastObject = null;
                constrainDistanceChange = true;
            });
            onInputUp.AddListener(() =>
            {
                if (SwichOnDown && CursorHandler.State == CursorHandler.CursorState.Clicked)
                    CursorHandler.State = CursorHandler.CursorState.Hover;
                constrainDistanceChange = false;
                //sdistCursor = 1;
            });
        }

        List<Transform> ignore;
        Transform lasthit;
        protected override void Update()
        {
            base.Update();

            if (!CircleMenu.Exist || !CircleMenu.Instance.IsExpanded)
            {
                distCursor += Input.mouseScrollDelta.y * Time.deltaTime * ScrollToDistSpeed;
                if (distCursor < MinimumCursorDistance) distCursor = MinimumCursorDistance;
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = umi3d.Physics.RaycastAll(ray, constrainDistanceChange ? distCursor + MaxDistCursorDelta : 100);

            ignore = new List<Transform>();
            ignore.AddRange(AvatarParent.gameObject.GetComponentsInChildren<Transform>());
            lastObject = (controller as MouseAndKeyboardController)?.mouseData.CurentHovered;
            if (lastObject != null)
                ignore.AddRange(lastObject.gameObject.GetComponentsInChildren<Transform>());
            bool ok = false;
            if (hits.Length > 0)
            {
                foreach (var hit in hits)
                {
                    if (!ignore.Contains(hit.transform))
                    {
                        Cursor.position = hit.point;
                        distCursor = hit.distance;
                        lasthit = hit.transform;
                        ok = true;
                        break;
                    }
                }
            }
            if (!ok)
            {
                Cursor.position = ray.GetPoint(distCursor);
            }
            Cursor.LookAt(Head);
        }
    }
}