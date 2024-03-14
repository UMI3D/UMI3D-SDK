/*
Copyright 2019 - 2024 Inetum

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
using System.Collections.Generic;
using umi3d.common.interaction;
using UnityEngine;

namespace umi3d.cdk.interaction
{
    public class ControlModel 
    {
        debug.UMI3DLogger logger = new(mainTag: nameof(ControlModel));

        public Controls_SO controls_SO;

        public void Init(Controls_SO controls_SO)
        {
            this.controls_SO = controls_SO;
        }

        public PhysicalButtonControlEntity GetPhysicalButton()
        {
            return controls_SO
                .physicalButtonControls
                .Find(
                control =>
                    {
                        return !control.controlData.isUsed;
                    }
                );
        }

        public UIButtonControlEntity GetUIButton()
        {
            return GetUIControl(
                controls_SO.uIButtonControls,
                controls_SO.uIButtonControlPrefabs
            );
        }

        public PhysicalManipulationControlEntity GetPhysicalManipulation(DofGroupDto dof)
        {
            return controls_SO
                .physicalManipulationControls
                .Find(
                    control =>
                    {
                        bool isUsed = control.controlData.isUsed;
                        bool isDofCompatible = 
                            control
                            .manipulationData
                            .compatibleDofGroup
                            .FindIndex(
                                _dof =>
                                {
                                    return _dof == dof.dofs;
                                }
                            ) >= 0;
                        return !isUsed && isDofCompatible;
                    }
                );
        }

        public UIManipulationControlEntity GetUIManipulation(DofGroupDto dof)
        {
            var uiManipulation = controls_SO
                .uIManipulationControls
                .Find(
                    control =>
                    {
                        bool isUsed = control.controlData.isUsed;
                        bool isDofCompatible =
                            control
                            .manipulationData
                            .compatibleDofGroup
                            .FindIndex(
                                _dof =>
                                {
                                    return _dof == dof.dofs;
                                }
                            ) >= 0;
                        return !isUsed && isDofCompatible;
                    }
                );
            if (uiManipulation == null)
            {
                uiManipulation = controls_SO.uIManipulationControlPrefabs.Find(
                    control =>
                    {
                        return control
                            .manipulationData
                            .compatibleDofGroup
                            .FindIndex(
                                _dof =>
                                {
                                    return _dof == dof.dofs;
                                }
                            ) >= 0;
                    }
                );
                if (uiManipulation == null)
                {
                    throw new NoInputFoundException();
                }
            }
            return uiManipulation;
        }

        public TextControlEntity GetText()
        {
            return GetUIControl(
                controls_SO.textControls,
                controls_SO.textControlPrefabs
            );
        }

        public ToggleControlEntity GetToggle()
        {
            return GetUIControl(
                controls_SO.toggleControls,
                controls_SO.toggleControlPrefabs
            );
        }

        public EnumControlEntity GetEnum()
        {
            return GetUIControl(
                controls_SO.enumControls,
                controls_SO.enumControlPrefabs
            );
        }

        public SliderControlEntity GetSlider()
        {
            return GetUIControl(
                controls_SO.sliderControls,
                controls_SO.sliderControlPrefabs
            );
        }

        Entity GetUIControl<Entity>(List<Entity> controls, List<Entity> prefabs)
            where Entity: AbstractControlEntity
        {
            var control = 
                controls
                .Find(
                    control =>
                    {
                        return !control.controlData.isUsed;
                    }
                );
            if (control == null)
            {
                control = prefabs[0];
            }
            return control;
        }
    }
}