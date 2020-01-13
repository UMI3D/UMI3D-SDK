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

//#define PERFTEST        //For testing performance of parse/stringify.  Turn on editor profiling to see how we're doing
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class JSONChecker : EditorWindow {
	string JSON = @"{
	""TestObject"": {
		""SomeText"": ""Blah"",
		""SomeObject"": {
			""SomeNumber"": 42,
			""SomeBool"": true,
			""SomeNull"": null
		},
		
		""SomeEmptyObject"": { },
		""SomeEmptyArray"": [ ],
		""EmbeddedObject"": ""{\""field\"":\""Value with \\\""escaped quotes\\\""\""}""
	}
}";	  //dat string literal...
	umi3d.common.JSONObject j;
	[UnityEditor.MenuItem("Window/JSONChecker")]
	static void Init() {
		GetWindow(typeof(JSONChecker));
	}
	void OnGUI() {
		JSON = EditorGUILayout.TextArea(JSON);
		GUI.enabled = !string.IsNullOrEmpty(JSON);
		if(GUILayout.Button("Check JSON")) {
#if PERFTEST
            Profiler.BeginSample("JSONParse");
			j = umi3d.common.JSONObject.Create(JSON);
            Profiler.EndSample();
            Profiler.BeginSample("JSONStringify");
            j.ToString(true);
            Profiler.EndSample();
#else
			j = umi3d.common.JSONObject.Create(JSON);
#endif
			Debug.Log(j.ToString(true));
		}
		if(j) {
			//Debug.Log(System.GC.GetTotalMemory(false) + "");
			if(j.type == umi3d.common.JSONObject.Type.NULL)
				GUILayout.Label("JSON fail:\n" + j.ToString(true));
			else
				GUILayout.Label("JSON success:\n" + j.ToString(true));

		}
	}
}
#endif