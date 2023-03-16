using System;
using System.Collections;
using System.Collections.Generic;
using umi3d.edk.userCapture;
using UnityEngine;
using UnityEngine.UIElements;

namespace inetum.unityUtils
{
    public class PoseConditionPanel : VisualElement
    {
        public class Uxmlfactory : UxmlFactory<PoseConditionPanel, PoseConditionPanel.UxmlTraits> { }

        public event Action<UMI3DPoseOveridder_so> onConditionCreated;

        Toggle tg_isInterpolationable;
        Toggle tg_isComposable;

        UMI3DPoseOveridder_so poseOveridder_So;

        public void Init()
        {
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

        private void BindUI()
        {
            //throw new NotImplementedException();
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

