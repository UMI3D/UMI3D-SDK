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
#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MainThreadDispatcher;
using umi3d.common;

namespace umi3d.edk.editor
{
    public class UMI3D_Wizard : ScriptableWizard
    {

        static Action<GameObject> _callback;

        public umi3d.edk.UMI3D.Protocol Protocol;
        public int TargetFrameRate = 30;
        public bool AutoRun = true;

        public bool CreateOrigin = true;
        public string EnvironementName = "default name";
        public umi3d.common.EnvironmentType EnvironmentType = common.EnvironmentType.Media;
        public umi3d.common.NavigationType NavigationType = common.NavigationType.Walk;

        /// <summary>
        /// Display the wizard
        /// </summary>
        /// <param name="callback"></param>
        static public void Display(Action<GameObject> callback)
        {
            _callback = callback;
            UMI3D_Wizard wizard = ScriptableWizard.DisplayWizard<UMI3D_Wizard>("Create a UMI3D Node", "Create");
            if (umi3d.edk.UMI3D.Instance)
            {
                wizard.Protocol = umi3d.edk.UMI3D.Instance.protocol;
                wizard.TargetFrameRate = umi3d.edk.UMI3D.TargetFrameRate;
                wizard.AutoRun = umi3d.edk.UMI3D.AutoRun;
                wizard.CreateOrigin = true;
                if (umi3d.edk.UMI3D.Scene || (umi3d.edk.UMI3D.Scene = umi3d.edk.UMI3D.Instance.GetComponent<umi3d.edk.UMI3DScene>()))
                {
                    wizard.EnvironementName = umi3d.edk.UMI3D.Scene._name;
                    wizard.EnvironmentType = umi3d.edk.UMI3D.Scene._type;
                    wizard.NavigationType = umi3d.edk.UMI3D.Scene._navigation;
                }
            }
}

        private void OnWizardCreate()
        {
            GameObject node;
            if (umi3d.edk.UMI3D.Exist)
            {
                if (umi3d.edk.UMI3D.Instance.isRunning)
                {
                    Debug.LogWarning("Trying to change the umi3d Node while the environement is Running is Prohibited");
                    return;
                }
                node = umi3d.edk.UMI3D.Instance.gameObject;
            }
            else if (!(node = GameObject.Find("UMI3D")))
            {
                node = new GameObject();
                node.name = "UMI3D";
            }
            if (!node.GetComponent<UnityMainThreadDispatcher>()) node.AddComponent<UnityMainThreadDispatcher>();
            if (!node.GetComponent<UMI3DMainThreadDispatcher>()) node.AddComponent<UMI3DMainThreadDispatcher>();
            umi3d.edk.UMI3D _umi3d = node.GetComponent<umi3d.edk.UMI3D>() ?? node.AddComponent<umi3d.edk.UMI3D>();
            if(!umi3d.edk.UMI3D.Exist) umi3d.edk.UMI3D.Instance = _umi3d;
            _umi3d.protocol = Protocol;
            umi3d.edk.UMI3D.TargetFrameRate = TargetFrameRate;
            umi3d.edk.UMI3D.AutoRun = AutoRun;
            umi3d.edk.UMI3D.Scene = node.GetComponent<umi3d.edk.UMI3DScene>() ?? node.AddComponent<umi3d.edk.UMI3DScene>();
            umi3d.edk.UMI3D.UserManager = node.GetComponent<umi3d.edk.UMI3DUserManager>() ?? node.AddComponent<umi3d.edk.UMI3DUserManager>();
            

            
            edk.UMI3D.Scene._name = EnvironementName;
            edk.UMI3D.Scene._navigation = NavigationType;
            edk.UMI3D.Scene._type = EnvironmentType;

            //just in case
            node.transform.position = Vector3.zero;
            node.transform.rotation =  Quaternion.identity;
            node.transform.localScale =  Vector3.one;
            if (CreateOrigin)
            {
                bool already = false;
                foreach (Transform child in node.transform)
                    if (child.name == "Origin")
                    {
                        already = true;
                        if (!child.gameObject.GetComponent<edk.EmptyObject3D>())
                            child.gameObject.AddComponent<edk.EmptyObject3D>();
                        break;
                    }
                if (!already)
                {
                    GameObject origin = Menu.CreateEmpty(node.transform);
                    origin.name = "Origin";
                    //just in case
                    origin.transform.position = Vector3.zero;
                    origin.transform.rotation = Quaternion.identity;
                    origin.transform.localScale = Vector3.one;
                }
            }
            
            if (_callback != null) _callback.Invoke(node);
        }

        private void OnWizardOtherButton()
        {

        }

        private void OnWizardUpdate()
        {
            //helpString = "Create a UMI3D Node";
        }

        protected override bool DrawWizardGUI()
        {
            minSize = new Vector2(EditorGUIUtility.singleLineHeight * 17, EditorGUIUtility.singleLineHeight * 23);
            maxSize = new Vector2(maxSize.x, EditorGUIUtility.singleLineHeight * 23);
            EditorGUI.indentLevel = 0;
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Connection Setup");
            EditorGUI.indentLevel = 1;
            Protocol = (umi3d.edk.UMI3D.Protocol)EditorGUILayout.EnumPopup("Protocol",Protocol);
            TargetFrameRate = EditorGUILayout.IntSlider("Target Frame Rate",TargetFrameRate, 0, 180);
            AutoRun = EditorGUILayout.Toggle("Sart at launch", AutoRun);
            EditorGUI.indentLevel = 0;
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Environement Setup");
            EditorGUI.indentLevel = 1;
            EnvironementName = EditorGUILayout.TextField("Environement Name", EnvironementName);
            EnvironmentType = (common.EnvironmentType)EditorGUILayout.EnumPopup("Type", EnvironmentType);
            NavigationType = (common.NavigationType)EditorGUILayout.EnumPopup("Navigation", NavigationType);
            CreateOrigin = EditorGUILayout.Toggle("Instantiate Origin", CreateOrigin);
            EditorGUI.indentLevel = 0;
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Server Setup");
            EditorGUI.indentLevel = 1;
            EditorGUI.indentLevel = 0;
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            return false;
            //return base.DrawWizardGUI();
        }
        
    }
}
#endif