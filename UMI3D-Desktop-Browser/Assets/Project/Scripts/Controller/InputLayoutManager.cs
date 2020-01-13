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
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

namespace BrowserDesktop.Controller
{
    public class InputLayoutManager : umi3d.PersistentSingleton<InputLayoutManager>
    {
        InputLayout CurrentLayout { get { return (inputLayouts != null && inputLayouts.Length > layoutId) ? inputLayouts[layoutId] : null; } }
        [SerializeField]
        InputLayout[] defaultInputLayout = null;
        InputLayout[] loadedInputLayout;
        InputLayout[] inputLayouts;
        int layoutId;

        public string LayoutsFileName;

        static public UnityEvent OnLayoutLoaded = new UnityEvent();
        static public UnityEvent OnLayoutChanged = new UnityEvent();

        static public bool LayoutLoaded { get; protected set; }

        public enum Input
        {
            MainMenuToggle,
            ContextualMenuNavigationDirect,
            MainActionKey,
            LeaveContextualMenu,
            ContextualMenuNavigationBack,
            Forward,
            Backward,
            Left,
            Right,
            Sprint,
            Squat,
            Jump,
            FreeView,
            Action1,
            Action2,
            Action3,
            Action4,
            Action5,
            Action6,
        }

        protected void Start()
        {
            LoadLayout();
            inputLayouts = new InputLayout[defaultInputLayout.Length + loadedInputLayout.Length];
            defaultInputLayout.CopyTo(inputLayouts, 0);
            loadedInputLayout.CopyTo(inputLayouts, defaultInputLayout.Length);
            OnLayoutLoaded.Invoke();
            LayoutLoaded = true;
        }

        static public KeyCode GetInputCode(Input input)
        {
            if (Exist)
            {
                switch (input)
                {
                    case Input.MainMenuToggle:
                        return Instance.CurrentLayout.MainMenuToggle;
                    case Input.ContextualMenuNavigationDirect:
                        return Instance.CurrentLayout.ContextualMenuNavigationDirect;
                    case Input.MainActionKey:
                        return Instance.CurrentLayout.MainActionKey;
                    case Input.LeaveContextualMenu:
                        return Instance.CurrentLayout.LeaveContextualMenu;
                    case Input.ContextualMenuNavigationBack:
                        return Instance.CurrentLayout.ContextualMenuNavigationBack;
                    case Input.Forward:
                        return Instance.CurrentLayout.Forward;
                    case Input.Backward:
                        return Instance.CurrentLayout.Backward;
                    case Input.Left:
                        return Instance.CurrentLayout.Left;
                    case Input.Right:
                        return Instance.CurrentLayout.Right;
                    case Input.Sprint:
                        return Instance.CurrentLayout.Sprint;
                    case Input.Squat:
                        return Instance.CurrentLayout.Squat;
                    case Input.Jump:
                        return Instance.CurrentLayout.Jump;
                    case Input.FreeView:
                        return Instance.CurrentLayout.FreeView;
                    case Input.Action1:
                        return Instance.CurrentLayout.Action1;
                    case Input.Action2:
                        return Instance.CurrentLayout.Action2;
                    case Input.Action3:
                        return Instance.CurrentLayout.Action3;
                    case Input.Action4:
                        return Instance.CurrentLayout.Action4;
                    case Input.Action5:
                        return Instance.CurrentLayout.Action5;
                    case Input.Action6:
                        return Instance.CurrentLayout.Action6;
                }
            }
            return KeyCode.None;
        }

        static public bool IsNameAvailable(string name)
        {
            return Exist ? Instance._IsNameAvailable(name) : false;
        }

        bool _IsNameAvailable(string name)
        {
            foreach (var input in defaultInputLayout)
                if (input.name == name) return false;
            return true;
        }

        static public void SetCurrentInputLayout(string name)
        {
            if (Exist) Instance._SetCurrentInputLayout(name);
        }
        void _SetCurrentInputLayout(string name)
        {
            int i = 0;
            foreach (var input in inputLayouts)
            {
                if (input.name == name)
                {
                    layoutId = i;
                    break;
                }

                i++;
            }
        }
        static public string[] GetInputsName() { return Exist ? Instance._GetInputsName() : new string[0]; }
        public string[] _GetInputsName()
        {
            string[] inputsName = new string[inputLayouts.Length];
            for (int i = 0; i < inputLayouts.Length; i++)
            {
                inputsName[i] = inputLayouts[i].name;
            }
            return inputsName;
        }

        static public InputLayout[] GetInputs() { return Exist ? Instance._GetInputs() : new InputLayout[0]; }
        public InputLayout[] _GetInputs() { return inputLayouts; }

        void LoadLayout()
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, LayoutsFileName);

            if (File.Exists(filePath))
            {
                string dataAsJson = File.ReadAllText(filePath);
                loadedInputLayout = JsonUtility.FromJson<InputLayout[]>(dataAsJson);
            }
            else
            {
                loadedInputLayout = new InputLayout[0];
            }
        }

        static public void SaveLayout(InputLayout inputLayout)
        {
            if (Exist) Instance._SaveLayout(inputLayout);
        }

        void _SaveLayout(InputLayout inputLayout)
        {
            bool found = false;
            foreach (InputLayout input in loadedInputLayout)
            {
                if (input.LayoutName == inputLayout.LayoutName)
                {
                    input.Set(inputLayout);
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                InputLayout[] inputs = new InputLayout[loadedInputLayout.Length + 1];
                loadedInputLayout.CopyTo(inputs, 0);
                inputs[loadedInputLayout.Length] = inputLayout;
                loadedInputLayout = inputs;
            }

            string filePath = Path.Combine(Application.streamingAssetsPath, LayoutsFileName);
            string dataAsJson = JsonUtility.ToJson(loadedInputLayout);
            File.WriteAllText(filePath, dataAsJson);

            inputLayouts = new InputLayout[defaultInputLayout.Length + loadedInputLayout.Length];
            defaultInputLayout.CopyTo(inputLayouts, 0);
            loadedInputLayout.CopyTo(inputLayouts, defaultInputLayout.Length);
            OnLayoutLoaded.Invoke();
        }
    }
}