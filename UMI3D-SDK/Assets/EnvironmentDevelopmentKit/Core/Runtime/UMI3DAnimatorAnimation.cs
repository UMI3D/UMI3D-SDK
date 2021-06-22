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

using System;
using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using UnityEngine;

namespace umi3d.edk
{
    public class UMI3DAnimatorAnimation : UMI3DAbstractAnimation
    {
        [SerializeField, EditorReadOnly]
        UMI3DNode node = null;
        [SerializeField, EditorReadOnly]
        string stateName = "";
        
        private UMI3DAsyncProperty<UMI3DNode> _objectNode;
        private UMI3DAsyncProperty<string> _objectStateName;


        public UMI3DAsyncProperty<UMI3DNode> ObjectNode { get { Register(); return _objectNode; } protected set => _objectNode = value; }
        public UMI3DAsyncProperty<string> ObjectStateName { get { Register(); return _objectStateName; } protected set => _objectStateName = value; }
        
        ///<inheritdoc/>
        protected override UMI3DAbstractAnimationDto CreateDto()
        {
            return new UMI3DAnimatorAnimationDto();
        }

        ///<inheritdoc/>
        protected override void InitDefinition(string id)
        {
            var equality = new UMI3DAsyncPropertyEquality();
            base.InitDefinition(id);
            ObjectNode = new UMI3DAsyncProperty<UMI3DNode>(id, UMI3DPropertyKeys.AnimationNodeId, node, null, (o, u) => o.Equals(u));
            ObjectStateName = new UMI3DAsyncProperty<string>(id, UMI3DPropertyKeys.AnimationStateName, stateName, null, (o, u) => o.Equals(u));
            ObjectNode.OnValueChanged += (d) => node = d;
            ObjectStateName.OnValueChanged += (d) => stateName = d;
        }

        ///<inheritdoc/>
        protected override void WriteProperties(UMI3DAbstractAnimationDto dto, UMI3DUser user)
        {
            base.WriteProperties(dto, user);
            var Adto = dto as UMI3DAnimatorAnimationDto;
            Adto.nodeId = ObjectNode.GetValue(user).Id();
            Adto.stateName = ObjectStateName.GetValue(user);
        }
    }
}