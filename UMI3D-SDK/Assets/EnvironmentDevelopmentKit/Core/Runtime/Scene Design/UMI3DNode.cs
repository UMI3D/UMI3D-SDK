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

using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using UnityEngine;

namespace umi3d.edk
{
    /// <summary>
    /// UMI3D empty object.
    /// </summary>
    [DisallowMultipleComponent]
    [SelectionBase]
    public class UMI3DNode : UMI3DAbstractNode
    {

        #region properties

        /// <summary>
        /// Indicates if the object is permanently facing the users XBillboard
        /// </summary>
        public UMI3DAsyncProperty<bool> objectXBillboard;
        /// <summary>
        /// Indicates if the object is permanently facing the users YBillboard
        /// </summary>
        public UMI3DAsyncProperty<bool> objectYBillboard;



        /// <summary>
        /// An editor field to modify default objectXBillboard value
        /// </summary>
        [SerializeField]
        public bool xBillboard = false;
        /// <summary>
        /// An editor field to modify default objectYBillboard value
        /// </summary>
        [SerializeField]
        public bool yBillboard = false;


        //public ColliderDto colliderDto = null;
        public bool hasCollider = false;

        #region collider fields for editor

        /// <summary>
        /// Type of the collider generated in front end.
        /// </summary>
        public ColliderType colliderType = ColliderType.Mesh;

        /// <summary>
        /// In case of a mesh collider, should it be convex ?
        /// </summary>
        public bool convex = false;

        /// <summary>
        /// Center of the collider for box, sphere and capsule collider
        /// </summary>
        public Vector3 colliderCenter;

        /// <summary>
        /// The radius for sphere and capsule collider
        /// </summary>
        public float colliderRadius = 1f;

        /// <summary>
        /// The box scale for boxCollider
        /// </summary>
        public Vector3 colliderBoxSize = Vector3.one;

        /// <summary>
        /// The height of le collider for capsule collider
        /// </summary>
        public float colliderHeight = 1f;

        /// <summary>
        /// The collider direction for capsule collider
        /// </summary>
        public DirectionalType colliderDirection = DirectionalType.Y_Axis;

        /// <summary>
        /// true if un custom mesh is used for the MeshCollider
        /// </summary>
        public bool isMeshCustom = false;

        /// <summary>
        /// Custom MeshCollider used if isMeshCustom
        /// </summary>
        public UMI3DResource customMeshCollider;


        // UMI3DAsyncProperties for 
        public UMI3DAsyncProperty<bool> objectHasCollider;
        public UMI3DAsyncProperty<ColliderType> objectColliderType;
        public UMI3DAsyncProperty<bool> objectIsConvexe;
        public UMI3DAsyncProperty<Vector3> objectColliderCenter;
        public UMI3DAsyncProperty<float> objectColliderRadius;
        public UMI3DAsyncProperty<Vector3> objectColliderBoxSize;
        public UMI3DAsyncProperty<float> objectColliderHeight;
        public UMI3DAsyncProperty<DirectionalType> objectColliderDirection;
        public UMI3DAsyncProperty<bool> objectIsMeshCustom;
        // custom mesh collider is not synchronized at runtime
        public UMI3DAsyncProperty<UMI3DResource> objectCustomMeshCollider;


        #endregion

        public UMI3DAsyncProperty<UMI3DKHRLight> objectLight;

        /// <summary>
        /// Indicates if the visibility state of the object has been checked on this frame.
        /// </summary>
        public Dictionary<UMI3DUser, bool> hasVisibilityBeenChecked = new Dictionary<UMI3DUser, bool>();

        /// <summary>
        /// Indicates the visibility state of this frame.
        /// </summary>
        public Dictionary<UMI3DUser, bool> visibilityOnFrame = new Dictionary<UMI3DUser, bool>();

        /// <summary>
        /// Indicates the visibility state of a user for the last frame check of visibility.
        /// </summary>
        public Dictionary<UMI3DUser, bool> visibleLastFrame = new Dictionary<UMI3DUser, bool>();
        
        #endregion


        #region initialization

        /// <summary>
        /// Unity MonoBehaviour Start method.
        /// </summary>
        //protected virtual void Start()
        //{
        //    //SyncProperties();
        //}

        /// <summary>
        /// Initialize object's properties.
        /// </summary>
        protected override void InitDefinition(string id)
        {
            //PropertiesHandler = new AsyncPropertiesHandler();
            //PropertiesHandler.DelegateBroadcastUpdate += BroadcastUpdates;
            //PropertiesHandler.DelegatebroadcastUpdateForUser += BroadcastUpdates;
            base.InitDefinition(id);

            objectXBillboard = new UMI3DAsyncProperty<bool>(objectId, UMI3DPropertyKeys.XBillboard, this.xBillboard);
            objectXBillboard.OnValueChanged += (bool b) => xBillboard = b;

            objectYBillboard = new UMI3DAsyncProperty<bool>(objectId, UMI3DPropertyKeys.YBillboard, this.yBillboard);
            objectYBillboard.OnValueChanged += (bool b) => yBillboard = b;

            objectHasCollider = new UMI3DAsyncProperty<bool>(objectId, UMI3DPropertyKeys.HasCollider, this.hasCollider);
            objectColliderType = new UMI3DAsyncProperty<ColliderType>(objectId, UMI3DPropertyKeys.ColliderType, this.colliderType);
            objectIsConvexe = new UMI3DAsyncProperty<bool>(objectId, UMI3DPropertyKeys.Convex, this.convex);
            objectColliderCenter = new UMI3DAsyncProperty<Vector3>(objectId, UMI3DPropertyKeys.ColliderCenter, this.colliderCenter, ToUMI3DSerializable.ToSerializableVector3, (v1,v2)=> UMI3DAsyncPropertyEquality.Equals(v1,v2));
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

            var light = GetComponent<Light>();
            objectLight = new UMI3DAsyncProperty<UMI3DKHRLight>(objectId, UMI3DPropertyKeys.Light,light ? new UMI3DKHRLight(objectId,light):null,(l,u)=>l?.ToDto(u));

            /*if (ARTracker)
            {
                ARTracker.initDefinition();
            }*/

        }


        #endregion


        #region updates
        /*
        private float clock = 0;
        protected virtual void Update()
        {
            Register();
            if (!isStatic)
                SyncTransform();

            if (clock > 1f / UMI3D.TargetFrameRate)
            {
                PropertiesHandler.BroadcastUpdates();
                clock = 0;
            }
            clock += Time.deltaTime;
        }*/

        /// <summary>
        /// Unity MonoBehaviour LateUpdate method.
        /// </summary>
        protected virtual void LateUpdate()
        {
            hasVisibilityBeenChecked.Clear();
        }

        /// <summary>
        /// Unity Monobehaviour OnDisable method.
        /// </summary>
        private void OnDisable()
        {
            hasVisibilityBeenChecked.Clear();
        }

        /*
        /// <summary>
        /// Unity MonoBehaviour OnValidate method.
        /// </summary>
        public virtual void OnValidate()
        {
            SyncProperties();
        }

        /// <summary>
        /// Update object's properties.
        /// </summary>
        protected virtual void SyncProperties()
        {
            if (inited)
            {
                objectXBillboard.SetValue(Xbillboard);
                objectYBillboard.SetValue(Ybillboard);
                SyncTransform();
            }
        }

        /// <summary>
        /// Synchronise default local positionment from GameObject transform.
        /// </summary>
        void SyncTransform()
        {
            var t = gameObject.transform;
            var p = new Vector3(
                t.localPosition.x,
                t.localPosition.y,
                t.localPosition.z);
            var s = new Vector3(
                t.localScale.x,
                t.localScale.y,
                t.localScale.z);
            var q = new Quaternion(
                t.localRotation.x,
                t.localRotation.y,
                t.localRotation.z,
                t.localRotation.w);
            objectPosition.SetValue(p);
            objectRotation.SetValue(q);
            objectScale.SetValue(s);
        }

        /// <summary>
        /// Send updates to concerned users.
        /// </summary>
        private void BroadcastUpdates()
        {
            foreach (UMI3DUser user in UMI3D.UserManager.GetUsers().Where(u => VisibleFor(u)))
                PropertiesHandler.BroadcastUpdates(user);
        }

        /// <summary>
        /// Send updates to concerned users.
        /// </summary>
        /// <param name="user">the user</param>
        private void BroadcastUpdates(UMI3DUser user)
        {
            if (!VisibleFor(user))
                return;
            var res = new UpdateObjectDto();
            res.Entity = ConvertToDto(user) as EmptyObject3DDto;
            user.Send(res);
        }
        */
        #endregion


        #region destroy
        /*
        protected virtual void OnDestroy()
        {
            if (UMI3D.Scene)
                UMI3D.Scene.Remove(this);

            if (UMI3D.Exist)
            {
                foreach (UMI3DUser user in UMI3D.UserManager.GetUsers())
                    if (VisibleLastFrame.ContainsKey(user) && VisibleLastFrame[user])
                        user.ObjectsToRemove.Add(new RemoveObjectDto() { id = this.Id });
            }

        }
        */
        #endregion


        #region visibility

        /// <summary>
        /// Return true if the object is available for a given user, false otherwise.
        /// </summary>
        /// <param name="user">User to check for</param>
        /// <returns></returns>
        public bool VisibleFor(UMI3DUser user)
        {
            //TODO remove this later
            if (user == null)
                return true;

            if (hasVisibilityBeenChecked.ContainsKey(user))
                return visibilityOnFrame[user];

            hasVisibilityBeenChecked.Add(user, true);

            if (!visibilityOnFrame.ContainsKey(user))
                visibilityOnFrame.Add(user, true);

            if (!gameObject.activeInHierarchy || !enabled || (!user.hasImmersiveDevice && immersiveOnly))
            {
                visibilityOnFrame[user] = false;
                return false;
            }

            foreach (var filter in GetComponents<VisibilityFilter>().Where(f => f.enabled))
                if (!filter.Accept(user))
                {
                    visibilityOnFrame[user] = false;
                    return false;
                }

            UMI3DNode p = objectParentId.GetValue(user) as UMI3DNode;
            if (p != null && !p.VisibleFor(user))
            {
                visibilityOnFrame[user] = false;
                return false;
            }

            visibilityOnFrame[user] = true;
            return true;
        }
        #endregion



        /// <summary>
        /// Convert to GlTFNodeDto for a given user.
        /// </summary>
        /// <param name="user">User to convert for</param>
        /// <returns></returns>
        internal GlTFNodeDto ToGlTFNodeDto(UMI3DUser user)
        {
            GlTFNodeDto dto = new GlTFNodeDto();
            dto.name = gameObject.name;
            dto.position = objectPosition.GetValue(user);
            dto.scale = objectScale.GetValue(user);
            dto.rotation = objectRotation.GetValue(user);
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
            Debug.LogError("GetGlTFMaterialsFor is unimplemented!");
            return new List<GlTFMaterialDto>();
        }

        /// <summary>
        /// List all attached materials for a user.
        /// </summary>
        /// <param name="user">The concerned User.</param>
        /// <returns></returns>
        internal List<UMI3DAbstractAnimationDto> GetAnimationsFor(UMI3DUser user)
        {
            var anim = GetComponents<UMI3DAbstractAnimation>();
            return anim?.Select(a=>a.ToAnimationDto(user))?.ToList();
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
        /// Writte the UMI3DNode properties in an object UMI3DNodeDto is assignable from.
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
        }

        public override IEntity ToEntityDto(UMI3DUser user)
        {
            return ToGlTFNodeDto(user);
        }

        private ColliderDto GetColliderDto()
        {
            //Debug.Log("has collider : " + hasCollider);
            if (!hasCollider)
                return null;
            ColliderDto res = new ColliderDto();
            res.colliderType = colliderType;
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
                    if(isMeshCustom)
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


        public void SearchCollider()
        {
            var c = GetComponent<Collider>();
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


    }

}

