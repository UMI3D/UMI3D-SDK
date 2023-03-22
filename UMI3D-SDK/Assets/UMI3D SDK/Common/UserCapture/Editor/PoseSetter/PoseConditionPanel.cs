using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace umi3d.common.userCapture
{
    public class PoseConditionPanel : VisualElement
    {
        public class Uxmlfactory : UxmlFactory<PoseConditionPanel, PoseConditionPanel.UxmlTraits> { }

        public event Action<UMI3DPoseOveridder_so> onConditionCreated;

        Toggle tg_isInterpolationable;
        Toggle tg_isComposable;
        TextField duration_field;

        Button add_condition;
        Button remove_condition;

        UMI3DPoseOveridder_so poseOveridder_So;

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
            poseOveridder_So = null;
        }

        private void GetRef()
        {
            tg_isInterpolationable = this.Q<Toggle>("tg_isInterpolationable");
            tg_isComposable = this.Q<Toggle>("tg_isComposable");

            duration_field = this.Q<TextField>("duration_field");
            add_condition = this.Q<Button>("add_condition");
            remove_condition = this.Q<Button>("remove_condition");
        }

        private void BindUI()
        {
            SetUpToggles();
        }

        private void SetUpToggles()
        {
            //tg_isComposable.RegisterCallback(data => poseOveridder_So.)
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
            poseOveridder_So = ScriptableObject.CreateInstance("UMI3DPoseOveridder_so") as UMI3DPoseOveridder_so;
            onConditionCreated?.Invoke(poseOveridder_So);
        }
    }
}

