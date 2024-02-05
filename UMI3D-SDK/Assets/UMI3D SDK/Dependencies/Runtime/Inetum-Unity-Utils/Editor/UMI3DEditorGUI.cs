/*
Copyright 2019 - 2023 Inetum

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
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace inetum.unityUtils.editor
{
	public static class UMI3DEditorGUI
	{
		private delegate void PropertyFieldFunction(Rect rect, SerializedProperty property, GUIContent label, bool includeChildren);

		public static void PropertyField(Rect rect, SerializedProperty property, bool includeChildren)
		{
			PropertyField_Implementation(rect, property, includeChildren, DrawPropertyField);
		}

		public static void PropertyField_Layout(SerializedProperty property, bool includeChildren)
		{
			Rect dummyRect = new Rect();
			PropertyField_Implementation(dummyRect, property, includeChildren, DrawPropertyField_Layout);
		}

		private static void DrawPropertyField(Rect rect, SerializedProperty property, GUIContent label, bool includeChildren)
		{
			EditorGUI.PropertyField(rect, property, label, includeChildren);
		}

		private static void DrawPropertyField_Layout(Rect rect, SerializedProperty property, GUIContent label, bool includeChildren)
		{
			EditorGUILayout.PropertyField(property, label, includeChildren);
		}

		private static void PropertyField_Implementation(Rect rect, SerializedProperty property, bool includeChildren, PropertyFieldFunction propertyFieldFunction)
		{
			UMI3DSpecialAttribute specialCaseAttribute = PropertyUtility.GetAttribute<UMI3DSpecialAttribute>(property);
			if (specialCaseAttribute != null)
			{
				specialCaseAttribute.GetDrawer()?.OnGUI(rect, property);
			}
			else
			{
				EditorGUI.BeginChangeCheck();

				propertyFieldFunction.Invoke(rect, property, new GUIContent(property.displayName), includeChildren);

				if (EditorGUI.EndChangeCheck())
				{
					// TODO : value changed callback
				}
			}
		}
        public static void Button(UnityEngine.Object target, MethodInfo methodInfo)
        {
            if (methodInfo.GetParameters().All(p => p.IsOptional))
            {
                ButtonAttribute buttonAttribute = (ButtonAttribute)methodInfo.GetCustomAttributes(typeof(ButtonAttribute), true)[0];
                string buttonText = string.IsNullOrEmpty(buttonAttribute.Text) ? ObjectNames.NicifyVariableName(methodInfo.Name) : buttonAttribute.Text;

                if (GUILayout.Button(buttonText, GUI.skin.button))
                {
                    object[] defaultParams = methodInfo.GetParameters().Select(p => p.DefaultValue).ToArray();
                    IEnumerator methodResult = methodInfo.Invoke(target, defaultParams) as IEnumerator;

                    if (!Application.isPlaying)
                    {
                        EditorUtility.SetDirty(target);

                        PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
                        if (stage != null)
                        {
                            EditorSceneManager.MarkSceneDirty(stage.scene); // Prefabs mode
                        }
                        else
                        {
                            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene()); // Scene Mode
                        }
                    }
                    else if (methodResult != null && target is MonoBehaviour behaviour)
                    {
                        behaviour.StartCoroutine(methodResult);
                    }
                }
            }
            else
            {
                string warning = typeof(ButtonAttribute).Name + " works only on methods with no parameters";
                Debug.LogWarning(warning);
            }
        }
    }
}
#endif