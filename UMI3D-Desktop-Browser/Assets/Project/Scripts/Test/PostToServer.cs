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
using System.Text;
using umi3d.cdk;
using umi3d.common;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Networking;

public class PostToServer : MonoBehaviour
{
}

#if UNITY_EDITOR

[CustomEditor(typeof(PostToServer))]
public class ObjectBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PostToServer myScript = (PostToServer)target;
        if (UMI3DBrowser.Media != null && GUILayout.Button("interact"))
        {
            UMI3DHttpClient.Interact("fakeId");
        }
    }
}

#endif
