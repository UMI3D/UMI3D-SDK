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

namespace BrowserDesktop.Controller
{
    [CreateAssetMenu(fileName = "InputLayout", menuName = "UMI3D/Input Layout", order = 1)]
    public class InputLayout : ScriptableObject
    {
        public string LayoutName;

        #region Menu

        public KeyCode MainMenuToggle;
        public KeyCode ContextualMenuNavigationDirect;

        public KeyCode MainActionKey;
        public KeyCode LeaveContextualMenu;
        public KeyCode ContextualMenuNavigationBack;

        #endregion
        #region Navigation

        public KeyCode Forward;
        public KeyCode Backward;
        public KeyCode Left;
        public KeyCode Right;

        public KeyCode Sprint;
        public KeyCode Squat;
        public KeyCode Jump;
        public KeyCode FreeView;

        #endregion
        #region Action Key

        public KeyCode Action1;
        public KeyCode Action2;
        public KeyCode Action3;
        public KeyCode Action4;
        public KeyCode Action5;
        public KeyCode Action6;

        #endregion


        public void Set(InputLayout layout)
        {
            LayoutName = layout.LayoutName;
            MainMenuToggle = layout.MainMenuToggle;
            ContextualMenuNavigationDirect = layout.ContextualMenuNavigationDirect;
            MainActionKey = layout.MainActionKey;
            LeaveContextualMenu = layout.LeaveContextualMenu;
            ContextualMenuNavigationBack = layout.ContextualMenuNavigationBack;
            Forward = layout.Forward;
            Backward = layout.Backward;
            Left = layout.Left;
            Right = layout.Right;
            Sprint = layout.Sprint;
            Squat = layout.Squat;
            Jump = layout.Jump;
            FreeView = layout.FreeView;
            Action1 = layout.Action1;
            Action2 = layout.Action2;
            Action3 = layout.Action3;
            Action4 = layout.Action4;
            Action5 = layout.Action5;
            Action6 = layout.Action6;
        }
    }
}