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
using System;
using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using UnityEngine;

namespace umi3d.edk
{
    /// <summary>
    /// UMI3D empty object to load on the clients on their scene graph.
    /// </summary>
    [DisallowMultipleComponent]
    [SelectionBase]
    public class UMI3DNode : UMI3DAbstractNode
    {

        #region fields

        /// <summary>
        /// Name given to the node.
        /// </summary>
        public string nodeName;

        #region asyncproperties
        /// <summary>
        /// Should the object be permanently facing the users XBillboard?
        /// </summary>
        public UMI3DAsyncProperty<bool> objectXBillboard { get { Register(); return _objectXBillboard; } protected set => _objectXBillboard = value; }
        /// <summary>
        /// Should the object be permanently facing the users YBillboard?
        /// </summary>
        public UMI3DAsyncProperty<bool> objectYBillboard { get { Register(); return _objectYBillboard; } protected set => _objectYBillboard = value; }

        /// <summary>
        /// Contains a collection of UMI3DId refering entities with skinnedMeshRenderer and an interger that is the position of this node in the bones array of the skinnedMeshRenderer.
        /// Used only with Model with tracked sub object and skinnedMeshRenderer
        /// </summary>
        public Dictionary<ulong, int> skinnedRendererLinks = new Dictionary<ulong, int>();

        #endregion asyncproperties

        /// <summary>
        /// An editor field to modify default objectXBillboard value
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("Should the object be permanently facing the users XBillboard?")]
        private bool xBillboard = false;
        /// <summary>
        /// An editor field to modify default objectYBillboard value
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("Should the object be permanently facing the users YBillboard?")]
        private bool yBillboard = false;

        /// <summary>
        /// Does the node possess a collider?
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("Check this box if a collider is attached to that node.")]
        protected bool hasCollider = false;


        #region collider

        /// <summary>
        /// Type of the collider generated in front end.
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("Type of the collider generated in front end.")]
        protected ColliderType colliderType = ColliderType.Mesh;

        /// <summary>
        /// In case of a mesh collider, should it be convex?
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("In case of a mesh collider, should it be convex?")]
        protected bool convex = false;

        /// <summary>
        /// Center of the collider for box, sphere and capsule collider.
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("Center of the collider for box, sphere and capsule collider.")]
        protected Vector3 colliderCenter;

        /// <summary>
        /// The radius for sphere and capsule collider.
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("Radius for sphere and capsule collider.")]
        protected float colliderRadius = 1f;

        /// <summary>
        /// The box scale for boxCollider.
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("Box scale for boxCollider")]
        protected Vector3 colliderBoxSize = Vector3.one;

        /// <summary>
        /// The height of the collider for capsule collider.
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("Height of the collider for capsule collider.")]
        protected float colliderHeight = 1f;

        /// <summary>
        /// The collider direction for capsule collider.
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("The collider direction for capsule collider.")]
        protected DirectionalType colliderDirection = DirectionalType.Y_Axis;

        /// <summary>
        /// True if a custom mesh is used for the MeshCollider.
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("True if a custom mesh is used for the MeshCollider.")]
        protected bool isMeshCustom = false;

        /// <summary>
        /// Custom MeshCollider used if isMeshCustom.
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("Custom MeshCollider used if isMeshCustom.")]
        protected UMI3DResource customMeshCollider;

        #region asyncproperties

        /// <summary>
        /// See <see cref="hasCollider"/>.
        /// </summary>
        public UMI3DAsyncProperty<bool> objectHasCollider { get { Register(); return _objectHasCollider; } protected set => _objectHasCollider = value; }
        /// <summary>
        /// See <see cref="colliderType"/>.
        /// </summary>
        public UMI3DAsyncProperty<ColliderType> objectColliderType { get { Register(); return _objectColliderType; } protected set => _objectColliderType = value; }
        /// <summary>
        /// See <see cref="convex"/>.
        /// </summary>
        public UMI3DAsyncProperty<bool> objectIsConvexe { get { Register(); return _objectIsConvexe; } protected set => _objectIsConvexe = value; }
        /// <summary>
        /// See <see cref="colliderCenter"/>.
        /// </summary>
        public UMI3DAsyncProperty<Vector3> objectColliderCenter { get { Register(); return _objectColliderCenter; } protected set => _objectColliderCenter = value; }
        /// <summary>
        /// See <see cref="colliderRadius"/>.
        /// </summary>
        public UMI3DAsyncProperty<float> objectColliderRadius { get { Register(); return _objectColliderRadius; } protected set => _objectColliderRadius = value; }
        /// <summary>
        /// See <see cref="colliderBoxSize"/>.
        /// </summary>
        public UMI3DAsyncProperty<Vector3> objectColliderBoxSize { get { Register(); return _objectColliderBoxSize; } protected set => _objectColliderBoxSize = value; }
        /// <summary>
        /// See <see cref="colliderHeight"/>.
        /// </summary>
        public UMI3DAsyncProperty<float> objectColliderHeight { get { Register(); return _objectColliderHeight; } protected set => _objectColliderHeight = value; }
        /// <summary>
        /// See <see cref="colliderDirection"/>.
        /// </summary>
        public UMI3DAsyncProperty<DirectionalType> objectColliderDirection { get { Register(); return _objectColliderDirection; } protected set => _objectColliderDirection = value; }
        /// <summary>
        /// See <see cref="isMeshCustom"/>.
        /// </summary>
        public UMI3DAsyncProperty<bool> objectIsMeshCustom { get { Register(); return _objectIsMeshCustom; } protected set => _objectIsMeshCustom = value; }
        /// <summary>
        /// See <see cref="customMeshCollider"/>.
        /// </summary>
        /// Custom mesh collider is not synchronized at runtime
        public UMI3DAsyncProperty<UMI3DResource> objectCustomMeshCollider { get { Register(); return _objectCustomMeshCollider; } protected set => _objectCustomMeshCollider = value; }

        #endregion asyncproperties

        #endregion collider

        #region asyncproperties

        public UMI3DAsyncProperty<UMI3DKHRLight> objectLight { get { Register(); return _objectLight; } protected set => _objectLight = value; }

        private UMI3DAsyncProperty<bool> _objectXBillboard;
        private UMI3DAsyncProperty<bool> _objectYBillboard;
        private UMI3DAsyncProperty<bool> _objectHasCollider;
        private UMI3DAsyncProperty<ColliderType> _objectColliderType;
        private UMI3DAsyncProperty<bool> _objectIsConvexe;
        private UMI3DAsyncProperty<Vector3> _objectColliderCenter;
        private UMI3DAsyncProperty<float> _objectColliderRadius;
        private UMI3DAsyncProperty<Vector3> _objectColliderBoxSize;
        private UMI3DAsyncProperty<float> _objectColliderHeight;
        private UMI3DAsyncProperty<DirectionalType> _objectColliderDirection;
        private UMI3DAsyncProperty<bool> _objectIsMeshCustom;
        private UMI3DAsyncProperty<UMI3DResource> _objectCustomMeshCollider;
        private UMI3DAsyncProperty<UMI3DKHRLight> _objectLight;

        #endregion asyncproperties

        #endregion fields


        #region initialization

        /// <summary>
        /// Initialize object's properties.
        /// </summary>
        protected override void InitDefinition(ulong id)
        {
            base.InitDefinition(id);

            nodeName = gameObject.name;

            objectXBillboard = new UMI3DAsyncProperty<bool>(objectId, UMI3DPropertyKeys.XBillboard, this.xBillboard);
            objectXBillboard.OnValueChanged += (bool b) => xBillboard = b;

            objectYBillboard = new UMI3DAsyncProperty<bool>(objectId, UMI3DPropertyKeys.YBillboard, this.yBillboard);
            objectYBillboard.OnValueChanged += (bool b) => yBillboard = b;

            objectHasCollider = new UMI3DAsyncProperty<bool>(objectId, UMI3DPropertyKeys.HasCollider, this.hasCollider);
            objectColliderType = new UMI3DAsyncProperty<ColliderType>(objectId, UMI3DPropertyKeys.ColliderType, this.colliderType);
            objectIsConvexe = new UMI3DAsyncProperty<bool>(objectId, UMI3DPropertyKeys.Convex, this.convex);
            objectColliderCenter = new UMI3DAsyncProperty<Vector3>(objectId, UMI3DPropertyKeys.ColliderCenter, this.colliderCenter, ToUMI3DSerializable.ToSerializableVector3, (v1, v2) => UMI3DAsyncPropertyEquality.Equals(v1, v2));
            objectColliderRadius = new UMI3DAsyncProperty<float>(objectId, UMI3DPropertyKeys.ColliderRadius, this.colliderRadius, null, (f1, f2) => UMI3DAsyncPropertyEquality.Equals(f1, f2));
            objectColliderBoxSize = new UMI3DAsyncProperty<Vector3>(objectId, UMI3DPropertyKeys.ColliderBoxSize, this.colliderBoxSize, ToUMI3DSerializable.ToSerializableVector3, (v1, v2) => UMI3DAsyncPropertyEquality.Equals(v1, v2));
            objectColliderHeight = new UMI3DAsyncProperty<float>(objectId, UMI3DPropertyKeys.ColliderHeight, this.colliderHeight, null, (f1, f2) => UMI3DAsyncPropertyEquality.Equals(f1, f2));
            objectColliderDirection = new UMI3DAsyncProperty<DirectionalType>(objectId, UMI3DPropertyKeys.ColliderDirection, this.colliderDirection);
            objectIsMeshCustom = new UMI3DAsyncProperty<bool>(objectId, UMI3DPropertyKeys.IsMeshColliderCustom, this.isMeshCustom);
            objectCustomMeshCollider = new UMI3DAsyncProperty<UMI3DResource>(objectId, UMI3DPropertyKeys.ColliderCustomResource, this.customMeshCollider);


            objectHasCollider.OnValueChanged += (bool b) => { hasCollider = b; }; // a modifier
            objectColliderType.OnValueChanged += (ColliderType t) => { colliderType = t; };// a modifier
            objectIsConvexe.OnValueChanged += (bool b) => { convex = b; };
            objectColliderCenter.OnValueChanged += (Vector3 v) => { colliderCenter = v; };
            objectColliderRadius.OnValueChanged += (float f) => { colliderRadius = f; };
            objectColliderBoxSize.OnValueChanged += (Vector3 v) => { colliderBoxSize = v; };
            objectColliderHeight.OnValueChanged += (float f) => { colliderHeight = f; };
            objectColliderDirection.OnValueChanged += (DirectionalType dt) => { colliderDirection = dt; };
            objectIsMeshCustom.OnValueChanged += (bool b) => { isMeshCustom = b; };
            objectCustomMeshCollider.OnValueChanged += (UMI3DResource r) => { customMeshCollider = r; };

            Light light = GetComponent<Light>();
            objectLight = new UMI3DAsyncProperty<UMI3DKHRLight>(objectId, UMI3DPropertyKeys.Light, light ? new UMI3DKHRLight(objectId, light) : null, (l, u) => l?.ToDto(u));

            /*if (ARTracker)
            {
                ARTracker.initDefinition();
            }*/

        }


        #endregion

        /// <summary>
        /// Convert to GlTFNodeDto for a given user.
        /// </summary>
        /// <param name="user">User to convert for</param>
        /// <returns></returns>
        internal GlTFNodeDto ToGlTFNodeDto(UMI3DUser user)
        {
            if (gameObject == null)
                UMI3DLogger.LogError($"UMI3DNode.ToGLTFNodeDto(userId: {user.Id()}) : Gameobject null for {name}, should not happen. \n {Environment.StackTrace}", DebugScope.EDK);

            var dto = new GlTFNodeDto
            {
                name = nodeName,
                position = objectPosition.GetValue(user),
                scale = objectScale.GetValue(user),
                rotation = objectRotation.GetValue(user)
            };
            dto.extensions.umi3d = ToUMI3DNodeDto(user);
            dto.extensions.KHR_lights_punctual = objectLight.GetValue(user)?.ToDto(user);

            return dto;
        }


        /// <summary>
        /// Convert to dto for a given user.
        /// </summary>
        /// <param name="user">User to convert for</param>
        /// <returns></returns>
        protected UMI3DNodeDto ToUMI3DNodeDto(UMI3DUser user)
        {
            UMI3DNodeDto dto = CreateDto();
            WriteProperties(dto, user);
            return dto;
        }

        /// <summary>
        /// List all attached materials for a user.
        /// </summary>
        /// <param name="user">The concerned User.</param>
        /// <returns></returns>
        internal virtual List<GlTFMaterialDto> GetGlTFMaterialsFor(UMI3DUser user)
        {
            return new List<GlTFMaterialDto>();
        }

        /// <summary>
        /// List all attached materials for a user.
        /// </summary>
        /// <param name="user">The concerned User.</param>
        /// <returns></returns>
        internal List<UMI3DAbstractAnimationDto> GetAnimationsFor(UMI3DUser user)
        {
            UMI3DAbstractAnimation[] anim = GetComponents<UMI3DAbstractAnimation>();
            return anim?.Select(a => a.ToAnimationDto(user))?.ToList();
        }

        /// <summary>
        /// Create an empty Dto.
        /// </summary>
        /// <returns></returns>
        protected virtual UMI3DNodeDto CreateDto()
        {
            return new UMI3DNodeDto();
        }

        /// <summary>
        /// Write the UMI3DNode properties in an object UMI3DNodeDto is assignable from.
        /// </summary>
        /// <param name="scene">The UMI3DNodeDto to be completed</param>
        /// <param name="user">User to convert for</param>
        /// <returns></returns>
        protected override void WriteProperties(UMI3DAbstractNodeDto dto, UMI3DUser user)
        {
            base.WriteProperties(dto, user);
            var nodeDto = dto as UMI3DNodeDto;
            if (nodeDto == null) return;
            nodeDto.xBillboard = objectXBillboard.GetValue(user);
            nodeDto.yBillboard = objectYBillboard.GetValue(user);
            nodeDto.colliderDto = GetColliderDto();
            nodeDto.lodDto = GetLod();
            nodeDto.skinnedRendererLinks = skinnedRendererLinks;
        }

        /// <inheritdoc/>
        public override Bytable ToBytes(UMI3DUser user)
        {
            return base.ToBytes(user)
                + UMI3DNetworkingHelper.Write(objectXBillboard.GetValue(user))
                + UMI3DNetworkingHelper.Write(objectYBillboard.GetValue(user))
                + ColliderToBytes(user)
                + LodToBytes(user);
        }

        /// <summary>
        /// Compute UMI3DLodDto with LogGroup component on the node.
        /// </summary>
        /// <returns>null if not component</returns>
        private UMI3DLodDto GetLod()
        {
            LODGroup lod = GetComponent<LODGroup>();
            if (lod == null) return null;
            var lodg = new UMI3DLodDto
            {
                lods = new List<UMI3DLodDefinitionDto>()
            };
            foreach (LOD lofd in lod.GetLODs())
            {
                var loddef = new UMI3DLodDefinitionDto();
                Renderer[] renderers = lofd.renderers;
                loddef.nodes = transform.GetComponentsInChildren<Renderer>().Where(r => renderers.Contains(r)).Select(s => s.GetComponent<UMI3DNode>()).Where(s => s != null).Select(s => s.Id()).ToList();
                loddef.screenSize = lofd.screenRelativeTransitionHeight;
                loddef.fadeTransition = lofd.fadeTransitionWidth;
                lodg.lods.Add(loddef);
            }
            return lodg;
        }

        /// <summary>
        /// Get <see cref="UMI3DLodDto"/> as a byte array.
        /// Not implemented yet.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private Bytable LodToBytes(UMI3DUser user)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override IEntity ToEntityDto(UMI3DUser user)
        {
            return ToGlTFNodeDto(user);
        }

        /// <summary>
        /// Get the collider as a <see cref="ColliderDto"/>.
        /// </summary>
        /// <returns></returns>
        private ColliderDto GetColliderDto()
        {
            if (!hasCollider)
                return null;
            var res = new ColliderDto
            {
                colliderType = colliderType
            };
            switch (colliderType)
            {
                case ColliderType.Box:
                    res.colliderCenter = colliderCenter;
                    res.colliderBoxSize = colliderBoxSize;
                    break;
                case ColliderType.Sphere:
                    res.colliderCenter = colliderCenter;
                    res.colliderRadius = colliderRadius;
                    break;
                case ColliderType.Capsule:
                    res.colliderCenter = colliderCenter;
                    res.colliderDirection = colliderDirection;
                    res.colliderHeight = colliderHeight;
                    res.colliderRadius = colliderRadius;
                    break;
                case ColliderType.Mesh:
                    if (isMeshCustom)
                    {
                        res.customMeshCollider = customMeshCollider.ToDto();
                        res.convex = false;
                        res.isMeshCustom = true;
                    }
                    else
                    {
                        res.customMeshCollider = null;
                        res.convex = convex;
                        res.isMeshCustom = false;
                    }
                    break;
                default:
                    break;
            }


            return res;
        }

        /// <summary>
        /// Get <see cref="ColliderDto"/> as a byte array.
        /// Not implemented yet.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception
        private Bytable ColliderToBytes(UMI3DUser user)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Look for a collider in the node, and fills out the adequate fields.
        /// </summary>
        public void SearchCollider()
        {
            Collider c = GetComponent<Collider>();
            if (c == null)
            {
                hasCollider = false;
            }

            else
            {
                hasCollider = true;
                switch (c)
                {
                    case BoxCollider bc:
                        colliderType = ColliderType.Box;
                        colliderBoxSize = bc.size;
                        colliderCenter = bc.center;
                        break;
                    case SphereCollider sc:
                        colliderType = ColliderType.Sphere;
                        colliderRadius = sc.radius;
                        colliderCenter = sc.center;
                        break;
                    case CapsuleCollider cc:
                        colliderType = ColliderType.Capsule;
                        colliderCenter = cc.center;
                        colliderDirection = (DirectionalType)cc.direction;
                        colliderHeight = cc.height;
                        colliderRadius = cc.radius;
                        break;
                    case MeshCollider mc:
                        colliderType = ColliderType.Mesh;
                        customMeshCollider = null;
                        isMeshCustom = false;
                        convex = mc.convex;
                        break;

                    default:
                        //hasCollider = false;
                        break;
                }
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (hasCollider)
            {
                Gizmos.color = Color.green;
                Gizmos.matrix = transform.localToWorldMatrix;

                switch (colliderType)
                {
                    case ColliderType.Sphere:
                        Gizmos.DrawWireSphere(colliderCenter, colliderRadius);
                        break;
                    case ColliderType.Box:
                        Gizmos.DrawWireCube(colliderCenter, colliderBoxSize);
                        break;
                    case ColliderType.Capsule:

                        Quaternion axis = Quaternion.identity;
                        switch (colliderDirection)
                        {
                            case DirectionalType.X_Axis:
                                axis = Quaternion.Euler(0, 0, 90);
                                break;
                            case DirectionalType.Z_Axis:
                                axis = Quaternion.Euler(90, 0, 0);
                                break;
                            default:
                                break;
                        }

                        Matrix4x4 angleMatrix = Matrix4x4.TRS(transform.position + transform.rotation * colliderCenter, transform.rotation * axis, UnityEditor.Handles.matrix.lossyScale);

                        using (new UnityEditor.Handles.DrawingScope(angleMatrix))
                        {
                            UnityEditor.Handles.color = Gizmos.color;

                            var pointOffset = (Mathf.Max(colliderHeight, 2f) - (colliderRadius * 2)) / 2;

                            //draw sideways
                            UnityEditor.Handles.DrawWireArc(Vector3.up * pointOffset, Vector3.left, Vector3.back, -180, colliderRadius);
                            UnityEditor.Handles.DrawLine(new Vector3(0, pointOffset, -colliderRadius), new Vector3(0, -pointOffset, -colliderRadius));
                            UnityEditor.Handles.DrawLine(new Vector3(0, pointOffset, colliderRadius), new Vector3(0, -pointOffset, colliderRadius));
                            UnityEditor.Handles.DrawWireArc(Vector3.down * pointOffset, Vector3.left, Vector3.back, 180, colliderRadius);
                            //draw frontways
                            UnityEditor.Handles.DrawWireArc(Vector3.up * pointOffset, Vector3.back, Vector3.left, 180, colliderRadius);
                            UnityEditor.Handles.DrawLine(new Vector3(-colliderRadius, pointOffset, 0), new Vector3(-colliderRadius, -pointOffset, 0));
                            UnityEditor.Handles.DrawLine(new Vector3(colliderRadius, pointOffset, 0), new Vector3(colliderRadius, -pointOffset, 0));
                            UnityEditor.Handles.DrawWireArc(Vector3.down * pointOffset, Vector3.back, Vector3.left, -180, colliderRadius);
                            //draw center
                            UnityEditor.Handles.DrawWireDisc(Vector3.up * pointOffset, Vector3.up, colliderRadius);
                            UnityEditor.Handles.DrawWireDisc(Vector3.down * pointOffset, Vector3.up, colliderRadius);

                        }
                        break;
                }
            }
        }
#endif
    }
}

