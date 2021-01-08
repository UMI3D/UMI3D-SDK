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

using UnityEditor;
using UnityEngine;
using System;
using Newtonsoft.Json;
using System.Text;
using System.IO;

namespace umi3d.edk.collaboration.editor
{
    [CustomEditor(typeof(IceServers), true)]
    [CanEditMultipleObjects]
    public class IceServersEditor : Editor
    {
        ///<inheritdoc/>
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (GUILayout.Button("Save"))
            {
                string path = EditorUtility.SaveFilePanel("Save Ice Servers", "", $"{target.name}.json", "json");
                if (!String.IsNullOrEmpty(path))
                {
                    var ice = target as IceServers;
                    try
                    {
                        string json = JsonConvert.SerializeObject(ice.iceServers, Formatting.Indented, new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.None
                        });
                        if(json != null)
                            File.WriteAllText(path, json, Encoding.UTF8);
                    }
                    catch (Exception e)
                    {
                        Debug.Log("Error on exporting " + e);
                    }
                }
            }
        }
    }
}
#endif