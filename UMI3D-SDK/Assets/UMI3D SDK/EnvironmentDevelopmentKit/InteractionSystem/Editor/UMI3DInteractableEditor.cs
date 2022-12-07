/*
Copyright 2019 - 2021 Inetum

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

using umi3d.edk.interaction;
using UnityEditor;

namespace umi3d.edk.editor
{
    /// <summary>
    /// <see cref="UMI3DInteractable"/> custom editor.
    /// </summary>
    [CustomEditor(typeof(UMI3DInteractable), true)]
    public class UMI3DInteractableEditor : UMI3DAbstractToolEditor
    {
        private SerializedProperty Node;
        private SerializedProperty NotifySubObject;
        private SerializedProperty NotifyHoverPosition;
        private SerializedProperty HasPriority;
        private SerializedProperty InteractionDistance;
        private SerializedProperty onHoverEnter;
        private SerializedProperty onHoverExit;
        private SerializedProperty onHovered;
        private SerializedProperty UseAnimations;
        private SerializedProperty HoverEnterAnimation;
        private SerializedProperty HoverExitAnimation;

        private bool showInteractionDistance;

        /// <inheritdoc/>
        protected override void OnEnable()
        {
            base.OnEnable();
            Node = _target.FindProperty("Node");
            NotifyHoverPosition = _target.FindProperty("NotifyHoverPosition");
            NotifySubObject = _target.FindProperty("NotifySubObject");
            HasPriority = _target.FindProperty("HasPriority");
            InteractionDistance = _target.FindProperty("InteractionDistance");
            onHoverEnter = serializedObject.FindProperty("onHoverEnter");
            onHovered = serializedObject.FindProperty("onHovered");
            onHoverExit = serializedObject.FindProperty("onHoverExit");
            UseAnimations = serializedObject.FindProperty("UseAnimations");
            HoverEnterAnimation = serializedObject.FindProperty("HoverEnterAnimation");
            HoverExitAnimation = serializedObject.FindProperty("HoverExitAnimation");

            showInteractionDistance = InteractionDistance.floatValue >= 0f;
        }

        private static bool displayEvent = false;

        /// <inheritdoc/>
        protected override void _OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(Node);
            EditorGUILayout.PropertyField(NotifyHoverPosition);
            EditorGUILayout.PropertyField(NotifySubObject);
            EditorGUILayout.PropertyField(HasPriority);

            showInteractionDistance = EditorGUILayout.Toggle("Has interaction distance", showInteractionDistance);

            if (showInteractionDistance)
            {
                if (InteractionDistance.floatValue < 0f)
                {
                    InteractionDistance.floatValue = 1;
                }
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(InteractionDistance);
                EditorGUI.indentLevel--;
            } else
            {
                InteractionDistance.floatValue = -1;
            }

            base._OnInspectorGUI();
            displayEvent = EditorGUILayout.Foldout(displayEvent, "Interaction Events", true);
            if (displayEvent)
            {
                EditorGUILayout.PropertyField(onHoverEnter, true);
                EditorGUILayout.PropertyField(onHoverExit, true);
                if (NotifyHoverPosition.boolValue)
                    EditorGUILayout.PropertyField(onHovered, true);
                EditorGUILayout.PropertyField(UseAnimations, true);
                if (UseAnimations.boolValue)
                {
                    EditorGUILayout.PropertyField(HoverEnterAnimation, true);
                    EditorGUILayout.PropertyField(HoverExitAnimation, true);
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif