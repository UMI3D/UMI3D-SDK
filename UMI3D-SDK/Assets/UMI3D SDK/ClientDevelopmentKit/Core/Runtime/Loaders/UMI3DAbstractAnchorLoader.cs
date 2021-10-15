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
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    /// <summary>
    /// Loader for Abstract Anchor
    /// </summary>
    public class UMI3DAbstractAnchorLoader
    {

        public virtual void ReadUMI3DExtension(UMI3DDto dto, GameObject node, Action finished, Action<Umi3dExecption> failed)
        {
            finished.Invoke();
        }

        public virtual bool SetUMI3DProperty(UMI3DEntityInstance entity, SetEntityPropertyDto property)
        {
            return false;
        }

        public virtual bool SetUMI3DPorperty(UMI3DEntityInstance entity, uint operationId, uint propertyKey, ByteContainer container)
        {
            return false;
        }

        public virtual bool ReadUMI3DProperty(ref object value, uint propertyKey, ByteContainer container)
        {
            return false;
        }
    }
}