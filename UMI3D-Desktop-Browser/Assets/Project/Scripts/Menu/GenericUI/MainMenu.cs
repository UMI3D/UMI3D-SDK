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
using umi3d;
using umi3d.cdk.menu.view;
using umi3d.cdk.menu.core;
using UnityEngine.UI;
using BrowserDesktop.Cursor;
using BrowserDesktop.Controller;
using BrowserDesktop;

namespace BrowserDesktop.Menu
{
    public class MainMenu : Singleton<MainMenu>
    {
        bool display;
        static public bool IsDisplaying { get { return Exist ? Instance.display : false; } }

        [SerializeField]
        Transform MenuTransform = null;
        public MenuAsset ToolboxMenu;
        //SimpleMenu KeyBoardInputMenu = null;
        public MenuDisplayManager menuDisplay;
        public Button optionButton;
        public RectTransform optionWindow;

        #region Menu

        public TextInputMenuItem MainMenuToggle;
        public TextInputMenuItem ContextualMenuNavigationDirect;

        public TextInputMenuItem MainActionKey;
        public TextInputMenuItem LeaveContextualMenu;
        public TextInputMenuItem ContextualMenuNavigationBack;

        #endregion
        #region Navigation

        public TextInputMenuItem Forward;
        public TextInputMenuItem Backward;
        public TextInputMenuItem Left;
        public TextInputMenuItem Right;

        public TextInputMenuItem Sprint;
        public TextInputMenuItem Squat;
        public TextInputMenuItem Jump;
        public TextInputMenuItem FreeView;

        #endregion
        #region Action Key

        public TextInputMenuItem Action1;
        public TextInputMenuItem Action2;
        public TextInputMenuItem Action3;
        public TextInputMenuItem Action4;
        public TextInputMenuItem Action5;
        public TextInputMenuItem Action6;

        #endregion

        void setupMenu(InputLayout inputLayout)
        {
            //MainMenuToggle.va = inputLayout.MainMenuToggle.ToString();
            //ContextualMenuNavigationDirect.text = inputLayout.ContextualMenuNavigationDirect.ToString();
            //MainActionKey.text = inputLayout.MainActionKey.ToString();
            //LeaveContextualMenu.text = inputLayout.LeaveContextualMenu.ToString();
            //ContextualMenuNavigationBack.text = inputLayout.ContextualMenuNavigationBack.ToString();
            //Forward.text = inputLayout.Forward.ToString();
            //Backward.text = inputLayout.Backward.ToString();
            //Left.text = inputLayout.Left.ToString();
            //Right.text = inputLayout.Right.ToString();
            //Sprint.text = inputLayout.Sprint.ToString();
            //Squat.text = inputLayout.Squat.ToString();
            //Jump.text = inputLayout.Jump.ToString();
            //FreeView.text = inputLayout.FreeView.ToString();
            //Action1.text = inputLayout.Action1.ToString();
            //Action2.text = inputLayout.Action2.ToString();
            //Action3.text = inputLayout.Action3.ToString();
            //Action4.text = inputLayout.Action4.ToString();
            //Action5.text = inputLayout.Action5.ToString();
            //Action6.text = inputLayout.Action6.ToString();
        }



        static public void Display(bool display = true)
        {
            if (Exist) Instance._Display(display);
        }


        private void Start()
        {
            InteractionMapper.Instance.toolboxMenu = ToolboxMenu.menu;
            //KeyBoardInputMenu = new SimpleMenu();
            //KeyBoardInputMenu.Name = "Keyboard Layout";
            InputLayoutManager.OnLayoutChanged.AddListener(layoutChanged);

            optionButton.onClick.AddListener(() => { optionWindow.gameObject.SetActive(!optionWindow.gameObject.activeSelf); });

            //ToolboxMenu.menu.AddSubMenu(KeyBoardInputMenu);

            _Display(false, true);
        }

        void layoutChanged()
        {
            //foreach(var input in InputLayoutManager.GetInputs())
            //{
            //    KeyBoardInputMenu.RemoveAllMenuItem();

            //}
        }

        void _Display(bool display, bool force = false)
        {
            if (display != this.display || force)
            {
                this.display = display;
                MenuTransform.gameObject.SetActive(display);
                CursorHandler.Movement = display ? CursorHandler.CursorMovement.Free : CursorHandler.CursorMovement.Center;
                if (display)
                {
                    menuDisplay.Display(true);
                    optionWindow.gameObject.SetActive(false);
                }
                else menuDisplay.Hide(true);
            }
        }

        protected override void OnDestroy()
        {
            InputLayoutManager.OnLayoutChanged.RemoveListener(layoutChanged);
            optionButton.onClick.RemoveAllListeners();
            base.OnDestroy();
        }

    }
}