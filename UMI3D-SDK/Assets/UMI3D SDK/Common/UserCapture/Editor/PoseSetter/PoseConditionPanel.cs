/*
Copy 2019 - 2023 Inetum

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

using inetum.unityUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

using static umi3d.common.userCapture.pose.UMI3DPoseOverrider_so;

namespace umi3d.common.userCapture.pose.editor
{
    public class PoseConditionPanel : VisualElement
    {
        public class Uxmlfactory : UxmlFactory<PoseConditionPanel, UxmlTraits> { }

        public event Action<UMI3DPoseOverrider_so> onConditionCreated;

        Toggle tg_isInterpolable;
        Toggle tg_isComposable;
        UintField_UI_Elements duration;
        UintField_UI_Elements min_duration;
        UintField_UI_Elements max_duration;

        Button add_condition;
        Button remove_condition;

        VisualElement condition_container;

        List<ConditionField> condition_fields = new List<ConditionField>();

        UMI3DPoseOverrider_so poseOverrider_So;

        public void Init()
        {
            GetRef();
            BindUI();
            Hide();
        }

        public void Enable()
        {
            Show();
            CreatePoseOverriderInstance();
        }

        public void Disable()
        {
            Hide();
            poseOverrider_So = null;
        }

        public UMI3DPoseOverrider_so GetPoseOverrider_So()
        {
            var poseOverrider = (UMI3DPoseOverrider_so)ScriptableObject.CreateInstance(typeof(UMI3DPoseOverrider_so));
            poseOverrider_So.duration = new Duration() { min = (uint)min_duration.value, max = (uint)max_duration.value, duration = (uint)duration.value };
            poseOverrider_So.composable = tg_isComposable.value;
            poseOverrider_So.interpolable = tg_isInterpolable.value;
            poseOverrider_So.poseConditions = new AbstractPoseConditionDto[condition_fields.Count];


            for (int i = 0; i < condition_fields.Count; i++)
            {
                poseOverrider_So.poseConditions[i] = condition_fields[i].GetPoseConditionDto();
            }

            return poseOverrider_So;
        }

        private void GetRef()
        {
            tg_isInterpolable = this.Q<Toggle>("tg_isInterpolationable");
            tg_isComposable = this.Q<Toggle>("tg_isComposable");

            duration = this.Q<UintField_UI_Elements>("duration");
            min_duration = this.Q<UintField_UI_Elements>("min_duration");
            max_duration = this.Q<UintField_UI_Elements>("max_duration");

            add_condition = this.Q<Button>("add_condition");
            remove_condition = this.Q<Button>("remove_condition");

            condition_container = this.Q<VisualElement>("condition_container");
        }

        private void BindUI()
        {
            InitFields();
            BindButtons();
        }

        private void InitFields()
        {
            duration.Init();
            min_duration.Init();
            max_duration.Init();
        }

        private void BindButtons()
        {
            condition_container.Clear();

            add_condition.clicked += () =>
            {
                var condition = new ConditionField();
                condition_fields.Add(condition);
                condition_container.Add(condition);
            };

            remove_condition.clicked += () =>
            {
                if (condition_fields != null && condition_fields.Count > 0)
                {
                    VisualElement visualElement = condition_fields?.Last();
                    if (visualElement != null)
                    {
                        condition_fields.Remove(visualElement as ConditionField);
                        condition_container.Remove(visualElement);
                    }
                }

            };
        }


        private void Hide()
        {
            style.display = DisplayStyle.None;
        }

        private void Show()
        {
            style.display = DisplayStyle.Flex;
        }

        private void CreatePoseOverriderInstance()
        {
            poseOverrider_So = ScriptableObject.CreateInstance("UMI3DPoseOverrider_so") as UMI3DPoseOverrider_so;
            onConditionCreated?.Invoke(poseOverrider_So);
        }
    }
}

#endif