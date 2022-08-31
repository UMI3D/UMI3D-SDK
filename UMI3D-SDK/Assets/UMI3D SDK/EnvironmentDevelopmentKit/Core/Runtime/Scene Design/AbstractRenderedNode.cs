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

using inetum.unityUtils;
using System.Collections.Generic;
using umi3d.common;
using UnityEngine;

namespace umi3d.edk
{
    /// <summary>
    /// Abstract base for <see cref="UMI3DNode"/> that should be be rendered.
    /// </summary>
    public abstract class AbstractRenderedNode : UMI3DNode
    {
        /// <summary>
        /// Are the model materials being overridden?
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("Are the model materials being overridden?")]
        protected bool overrideModelMaterials = false;
        /// <summary>
        /// List of the <see cref="MaterialOverrider"/> applied to object.
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("List of the MaterialOverriders applied to object")]
        protected List<MaterialOverrider> materialsOverrider = new List<MaterialOverrider>();
        /// <summary>
        /// Should the object cast shadows on other objects?
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("Should the object cast shadows on other objects?")]
        protected bool castShadow = true;
        /// <summary>
        /// Should the object receive shadows from other objects?
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("Should the object receive shadows from other objects?")]
        protected bool receiveShadow = true;

        /// <summary>
        /// See <see cref="overrideModelMaterials"/>
        /// </summary>
        public UMI3DAsyncProperty<bool> objectMaterialsOverrided { get { Register(); return _objectMaterialsOverrided; } protected set => _objectMaterialsOverrided = value; }
        /// <summary>
        /// See <see cref="materialsOverrider"/>
        /// </summary>
        public UMI3DAsyncListProperty<MaterialOverrider> objectMaterialOverriders { get { Register(); return _objectMaterialOverriders; } protected set => _objectMaterialOverriders = value; }
        /// <summary>
        /// See <see cref="castShadow"/>
        /// </summary>
        public UMI3DAsyncProperty<bool> objectCastShadow { get { Register(); return _objectCastShadow; } protected set => _objectCastShadow = value; }
        /// <summary>
        /// See <see cref="receiveShadow"/>
        /// </summary>
        public UMI3DAsyncProperty<bool> objectReceiveShadow { get { Register(); return _objectReceiveShadow; } protected set => _objectReceiveShadow = value; }

        protected UMI3DAsyncProperty<bool> _objectMaterialsOverrided;
        protected UMI3DAsyncListProperty<MaterialOverrider> _objectMaterialOverriders;
        protected UMI3DAsyncProperty<bool> _objectCastShadow;
        protected UMI3DAsyncProperty<bool> _objectReceiveShadow;

        /// <inheritdoc/>
        protected override void InitDefinition(ulong id)
        {
            base.InitDefinition(id);

            objectMaterialsOverrided = new UMI3DAsyncProperty<bool>(objectId, UMI3DPropertyKeys.ApplyCustomMaterial, this.overrideModelMaterials);
            objectMaterialsOverrided.OnValueChanged += (bool value) => overrideModelMaterials = value;

            objectMaterialOverriders = new UMI3DAsyncListProperty<MaterialOverrider>(objectId, UMI3DPropertyKeys.OverideMaterialId, this.materialsOverrider, (x, u) => x.ToDto(), (a, b) => { return a.GetHashCode().Equals(b.GetHashCode()); });

            objectMaterialOverriders.OnValueChanged += (List<MaterialOverrider> value) => materialsOverrider = value;

            objectCastShadow = new UMI3DAsyncProperty<bool>(objectId, UMI3DPropertyKeys.CastShadow, castShadow);
            objectCastShadow.OnValueChanged += v => castShadow = v;

            objectReceiveShadow = new UMI3DAsyncProperty<bool>(objectId, UMI3DPropertyKeys.ReceiveShadow, receiveShadow);
            objectReceiveShadow.OnValueChanged += v => castShadow = v;

        }

        /// <inheritdoc/>
        protected override UMI3DNodeDto CreateDto()
        {
            return new UMI3DRenderedNodeDto();
        }

        /// <inheritdoc/>
        protected override void WriteProperties(UMI3DAbstractNodeDto dto, UMI3DUser user)
        {
            base.WriteProperties(dto, user);
            var meshDto = dto as UMI3DRenderedNodeDto;

            meshDto.receiveShadow = objectReceiveShadow.GetValue(user);
            meshDto.castShadow = objectCastShadow.GetValue(user);

            meshDto.applyCustomMaterial = overrideModelMaterials;
            meshDto.overridedMaterials = objectMaterialOverriders.GetValue(user).ConvertAll((mat) => mat.ToDto());
        }

        /// <inheritdoc/>
        public override Bytable ToBytes(UMI3DUser user)
        {
            return base.ToBytes(user)
                + UMI3DNetworkingHelper.Write(objectReceiveShadow.GetValue(user))
                + UMI3DNetworkingHelper.Write(objectCastShadow.GetValue(user))
                + UMI3DNetworkingHelper.Write(objectActive.GetValue(user))
                + UMI3DNetworkingHelper.WriteIBytableCollection(objectMaterialOverriders.GetValue(user));
        }
    }
}