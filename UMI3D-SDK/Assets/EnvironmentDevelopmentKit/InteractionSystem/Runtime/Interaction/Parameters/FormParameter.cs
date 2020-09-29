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

using System;
using System.Collections.Generic;
using umi3d.common.interaction;
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.edk.interaction
{
    public class FormParameter : UMI3DTool
    {
        public List<AbstractInteraction> Fields = new List<AbstractInteraction>();

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        protected override AbstractToolDto CreateDto()
        {
            return new FormDto();
        }

        public override AbstractToolDto ToDto(UMI3DUser user)
        {
            FormDto dto = (base.ToDto(user) as FormDto);

            //foreach (AbstractInteraction field in Fields)
            //{
            //    dto.Fields.Add(field.ToDto(user));
            //}

            return dto;
        }


    }
}