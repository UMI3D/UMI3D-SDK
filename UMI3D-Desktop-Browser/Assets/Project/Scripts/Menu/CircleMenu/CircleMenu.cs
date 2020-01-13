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
using UnityEngine.UI;
using TMPro;
using BrowserDesktop.Controller;
using umi3d.cdk.menu.view;
using UnityEngine.Events;
using BrowserDesktop.Cursor;

namespace BrowserDesktop.Menu
{
    public class CircleMenu : umi3d.Singleton<CircleMenu>
    {
        public Transform Container;
        public int radius;
        public CircleMenuContainer circleMenuContainer;
        public RectTransform cursorAnchor;
        public bool IsExpanded { get { return circleMenuContainer.isExpanded; } }
        public MenuDisplayManager MenuDisplayManager;
        public RectTransform RectTransform;
        public Camera Camera;
        public MouseAndKeyboardController mouseAndKeyboardController;
        public UnityEvent MenuColapsed = new UnityEvent();
        public UnityEvent MenuExpand = new UnityEvent();

        Vector2 PositionToFollow;

        private void Start()
        {
            MenuDisplayManager.firstButtonBackButtonPressed.AddListener(Collapse);
            MenuDisplayManager.Display(true);
            //Collapse();
        }

        public void Update()
        {
            if (Input.GetKeyDown(InputLayoutManager.GetInputCode(InputLayoutManager.Input.LeaveContextualMenu)))
            {
                Collapse();
            }
            else if (Input.GetKeyDown(InputLayoutManager.GetInputCode(InputLayoutManager.Input.ContextualMenuNavigationBack)))
            {
                if (circleMenuContainer.isExpanded)
                {
                    MenuDisplayManager.Back();
                }
                else if(circleMenuContainer.Count() > 0)
                {
                    _Display();
                }
            }

            RectTransform.position = Vector2.Lerp(RectTransform.position, PositionToFollow, Time.deltaTime * 100);

            Vector2 localMousePosition = Input.mousePosition - cursorAnchor.position;
            cursorAnchor.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * Mathf.Atan2(-localMousePosition.x, localMousePosition.y));

            if(circleMenuContainer.isDisplayed && circleMenuContainer.isExpanded && circleMenuContainer.Count() == 0)
            {
                MenuDisplayManager.Back();
            }

            ManageOptionCursor();
        }

        void ManageOptionCursor()
        {
            if (CursorHandler.Exist)
            {
                bool display = false;
                if (!circleMenuContainer.isDisplayed || !circleMenuContainer.isExpanded)
                {
                    display = circleMenuContainer.Count() > 0;
                }
                CursorHandler.Instance.MenuIndicator = display;
            }
        }

        public static void Setup(CircleMenuContainer circleMenuContainer)
        {
            if (Exist)
            {
                Instance.circleMenuContainer = circleMenuContainer;
            }
        }

        void _Display()
        {
            if (true)
            {
                if (!circleMenuContainer.isDisplayed)
                {
                    circleMenuContainer.Display(true);
                }
                if (!circleMenuContainer.isExpanded)
                {
                    circleMenuContainer.Expand();
                    MenuExpand.Invoke();
                    CursorHandler.Movement = CursorHandler.CursorMovement.Free;
                }
            }
        }

        public void Display(Vector3 WorldPosition)
        {
            Follow(WorldPosition);
            _Display();
        }

        public void Display(Vector2 WorldPosition)
        {
            Follow(WorldPosition);
            _Display();
        }

        public void Collapse()
        {
            //circleMenuContainer?.Back();
            CursorHandler.Movement = CursorHandler.CursorMovement.Center;
            circleMenuContainer.Collapse();
            MenuColapsed.Invoke();
        }

        public void Follow(Vector3 WorldPosition)
        {
            setPosition(WorldPosition);
        }

        //public void Hide(bool clear)
        //{
        //    MenuDisplayManager.Hide(true);
        //    CursorHandler.Movement = CursorHandler.CursorMovement.Center;
        //    displaying = false;
        //}

        public void setPosition(Vector3 WorldPosition)
        {
            setPosition((Vector2)Camera.WorldToScreenPoint(WorldPosition));
        }

        public void setPosition(Vector2 ScreenPosition)
        {
            PositionToFollow = ScreenPosition;
            //RectTransform.position = ScreenPosition;
        }
    }
}