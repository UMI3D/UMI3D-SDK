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
using BrowserDesktop.Menu;

namespace BrowserDesktop.Cursor
{
    public class CursorHandler : umi3d.PersistentSingleton<CursorHandler>
    {
        //SpriteRenderer spriteRenderer;
        public Texture2D HintCursor;
        public RectTransform CrossCursor;
        public RectTransform CircleCursor;
        public RectTransform ClickedCursor;
        public RectTransform LeftClickOptionCursor;
        public RectTransform LeftClickExitCursor;
        public RectTransform FollowCursor;

        public enum CursorState { Default, Hover, Clicked }
        public enum CursorMovement { Free, Center, Confined, FreeHiden }

        public bool MenuIndicator = false;
        public bool ExitIndicator = false;

        CursorState state;
        CursorMovement cursorMovement;
        bool stateUpdated = true;
        bool movementUpdated = true;
        public static CursorState State { get { return Exist ? Instance.state : CursorState.Default; } set { if (Exist && Instance.state != value) { Instance.state = value; Instance.stateUpdated = true; } } }
        public static CursorMovement Movement { get { return Exist ? Instance.cursorMovement : CursorMovement.Free; } set { if (Exist && Instance.cursorMovement != value) { Instance.cursorMovement = value; Instance.movementUpdated = true; } } }

        public CursorMode cursorMode = CursorMode.Auto;
        public Vector2 hotSpot = Vector2.zero;

        [SerializeField]
        bool LastMenuState = false;

        private void Start()
        {
            stateUpdated = true;
            movementUpdated = true;
        }


        private void Update()
        {
            transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (movementUpdated)
            {
                switch (cursorMovement)
                {
                    case CursorMovement.Center:
                        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
                        UnityEngine.Cursor.visible = false;
                        //State = CursorState.Default;
                        break;
                    case CursorMovement.Free:
                        UnityEngine.Cursor.lockState = CursorLockMode.None;
                        UnityEngine.Cursor.visible = true;
                        break;
                    case CursorMovement.FreeHiden:
                        UnityEngine.Cursor.lockState = CursorLockMode.None;
                        UnityEngine.Cursor.visible = false;
                        break;
                    case CursorMovement.Confined:
                        UnityEngine.Cursor.lockState = CursorLockMode.Confined;
                        UnityEngine.Cursor.visible = true;
                        break;
                }
                movementUpdated = false;
            }
            bool newMenuState = ((!MainMenu.Exist || !MainMenu.IsDisplaying) && (cursorMovement == CursorMovement.Center || cursorMovement == CursorMovement.FreeHiden) );
            if (stateUpdated || LastMenuState != newMenuState)
            {
                LastMenuState = newMenuState;
                if (CrossCursor.gameObject.activeSelf != (cursorMovement == CursorMovement.Center && state == CursorState.Default && LastMenuState)) CrossCursor.gameObject.SetActive(cursorMovement == CursorMovement.Center && state == CursorState.Default && LastMenuState);
                if (CircleCursor.gameObject.activeSelf != (cursorMovement == CursorMovement.Center && state == CursorState.Hover && LastMenuState)) CircleCursor.gameObject.SetActive(cursorMovement == CursorMovement.Center && state == CursorState.Hover && LastMenuState);
                if (ClickedCursor.gameObject.activeSelf != (cursorMovement == CursorMovement.Center && state == CursorState.Clicked && LastMenuState)) ClickedCursor.gameObject.SetActive(cursorMovement == CursorMovement.Center && state == CursorState.Clicked && LastMenuState);
                if (FollowCursor.gameObject.activeSelf != (cursorMovement == CursorMovement.FreeHiden && LastMenuState)) FollowCursor.gameObject.SetActive(cursorMovement == CursorMovement.FreeHiden && LastMenuState);
                stateUpdated = false;
            }
            if (LeftClickOptionCursor.gameObject.activeSelf != (LastMenuState && MenuIndicator)) LeftClickOptionCursor.gameObject.SetActive(LastMenuState && MenuIndicator);
            if (LeftClickExitCursor.gameObject.activeSelf != (LastMenuState && ExitIndicator)) LeftClickExitCursor.gameObject.SetActive(LastMenuState && ExitIndicator);
            if (FollowCursor.gameObject.activeSelf) FollowCursor.position = Input.mousePosition - new Vector3(Screen.width/2,Screen.height/2,0);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Destroy(gameObject);
        }
    }
}