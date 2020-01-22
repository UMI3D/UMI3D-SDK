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
using umi3d.common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using UnityEngine.Events;

namespace umi3d.edk
{
    /// <summary>
    /// Abstract UMI3D scene object.
    /// </summary>
    public abstract class GenericObject3D : MonoBehaviour
    {

        #region properties

        /// <summary>
        /// The object preview in Unity Editor
        /// </summary>
        [HideInInspector]
        public GameObject preview;

        /// <summary>
        /// Indicates if InitDefinition has been called.
        /// </summary>
        protected bool inited = false;

        public AsyncPropertiesHandler PropertiesHandler { protected set; get; }

        /// <summary>
        /// The objects's unique id. 
        /// </summary>
        protected string objectId;

        /// <summary>
        /// The public Getter for objectId.
        /// </summary>
        public string Id
        {
            get
            {
                if (objectId == null && UMI3D.Scene != null)
                    objectId = UMI3D.Scene.Register(this);
                return objectId;
            }
        }

        /// <summary>
        /// The public Getter for object's parent Id.
        /// </summary>
        /// <returns></returns>
        public string ParentId
        {
            get
            {
                if (transform.parent != null)
                {
                    var p = transform.parent.gameObject.GetComponent<GenericObject3D>();
                    if (p != null)
                        return p.Id;
                }
                return null;
            }
        }

        /// <summary>
        /// Indicates if the object is permanently facing the users
        /// </summary>
        public UMI3DAsyncProperty<bool> objectBillboard;

        /// <summary>
        /// An editor field to modify default objectBillboard value
        /// </summary>
        [SerializeField]
        protected bool billboard = false;

        /// <summary>
        /// False if the object is alowed to move during the application exectution.
        /// </summary>
        [SerializeField]
        protected bool isStatic = false;


        /// <summary>
        /// Indicates if the object is only vissible in full 3D media displayers (sush as Computer or Virtual reality headset)
        /// </summary>
        public UMI3DAsyncProperty<bool> objectImmersiveOnly;

        /// <summary>
        /// An editor field to modify default objectImmersiveOnly value
        /// </summary>
        
        public bool immersiveOnly = false;


        /// <summary>
        /// The object local position in the scene graph.
        /// </summary>
        public UMI3DAsyncProperty<Vector3> objectPosition;

        /// <summary>
        /// The object local orientation in the scene graph.
        /// </summary>
        public UMI3DAsyncProperty<Quaternion> objectRotation;

        /// <summary>
        /// The object local scale in the scene graph.
        /// </summary>
        public UMI3DAsyncProperty<Vector3> objectScale;

        /// <summary>
        /// Indicates if the visibility state of the object has been checked on this frame.
        /// </summary>
        public Dictionary<UMI3DUser, bool> HasVisibilityBeenChecked = new Dictionary<UMI3DUser, bool>();

        /// <summary>
        /// Indicates the visibility state of this frame.
        /// </summary>
        public Dictionary<UMI3DUser, bool> VisibilityOnFrame = new Dictionary<UMI3DUser, bool>();

        /// <summary>
        /// Indicates the visibility state of a user for the last frame check of visibility.
        /// </summary>
        public Dictionary<UMI3DUser, bool> VisibleLastFrame = new Dictionary<UMI3DUser, bool>();


        protected ARTracker ARTracker { get { return GetComponent<ARTracker>(); } }

        #region Interactable
        /// <summary>
        /// Class for event rising on hover when <see cref="trackHoverPosition"/> is enabled. 
        /// The first argument is the hovering user
        /// The second argument is the hovered position of the object in the object's local frame, 
        /// the third argument is the normal to the object's surface at the hovered position in the object's local frame.
        /// </summary>
        [Serializable]
        public class HoverEvent : UnityEvent<UMI3DUser, string, Vector3, Vector3> { }


        public InteractableDto GetInteractableDto(UMI3DUser user)
        {
            if (!isInteractable) return null;

            InteractableDto dto = interactable.ConvertToDto(user) as InteractableDto;
            dto.objectId = this.Id;
            dto.trackHoverPosition = trackHoverPosition;
            
            return dto;
        }

        [SerializeField]
        protected CVEInteractable interactable;
        [SerializeField]
        protected bool trackHoverPosition;

        public UMI3DAsyncProperty<CVEInteractable> objectInteractable;
        public UMI3DAsyncProperty<bool> objectTrackHoverPosition;

        public bool isInteractable { get { return interactable != null; } }

        [SerializeField]
        public UMI3DUserBoneEvent onHoverEnter = new UMI3DUserBoneEvent();

        [SerializeField]
        public HoverEvent onHovered = new HoverEvent();

        [SerializeField]
        public UMI3DUserBoneEvent onHoverExit = new UMI3DUserBoneEvent();

        /// <summary>
        /// List of bones hovering this object (if any).
        /// </summary>
        public List<string> hoveringBones = new List<string>();

        public bool isHovered { get { return hoveringBones.Count > 0; } }

        #endregion

        #endregion


        #region initialization

        /// <summary>
        /// Unity MonoBehaviour Awake method.
        /// </summary>
        protected virtual void Awake()
        {
            initDefinition();
        }

        /// <summary>
        /// Unity MonoBehaviour Start method.
        /// </summary>
        protected virtual void Start()
        {
            SyncProperties();
        }

        /// <summary>
        /// Check if the GenericObject3D has been registered to to the UMI3DScene and do it if not
        /// </summary>
        void Register()
        {
            if (objectId == null && UMI3D.Scene != null)
                objectId = UMI3D.Scene.Register(this);
        }

        /// <summary>
        /// Initialize object's properties.
        /// </summary>
        protected virtual void initDefinition()
        {
            PropertiesHandler = new AsyncPropertiesHandler();
            PropertiesHandler.DelegateBroadcastUpdate += BroadcastUpdates;
            PropertiesHandler.DelegatebroadcastUpdateForUser += BroadcastUpdates;


            objectPosition = new UMI3DAsyncProperty<Vector3>(PropertiesHandler, new Vector3());
            objectPosition.OnValueChanged += (Vector3 p) => transform.localPosition = p;

            objectRotation = new UMI3DAsyncProperty<Quaternion>(PropertiesHandler, new Quaternion());
            objectRotation.OnValueChanged += (Quaternion r) => transform.localRotation = r;

            objectScale = new UMI3DAsyncProperty<Vector3>(PropertiesHandler, new Vector3());
            objectScale.OnValueChanged += (Vector3 s) => transform.localScale = s;

            objectBillboard = new UMI3DAsyncProperty<bool>(PropertiesHandler, this.billboard);
            objectBillboard.OnValueChanged += (bool b) => billboard = b;

            objectImmersiveOnly = new UMI3DAsyncProperty<bool>(PropertiesHandler, this.immersiveOnly);
            objectImmersiveOnly.OnValueChanged += (bool b) => immersiveOnly = b;

            objectInteractable = new UMI3DAsyncProperty<CVEInteractable>(PropertiesHandler, this.interactable);
            objectInteractable.OnValueChanged += (CVEInteractable i) => interactable = i;

            objectTrackHoverPosition = new UMI3DAsyncProperty<bool>(PropertiesHandler, this.trackHoverPosition);
            objectTrackHoverPosition.OnValueChanged += (bool b) => trackHoverPosition = b;

            if (ARTracker)
            {
                ARTracker.initDefinition();
            }

            Register();

            inited = true;
        }


        #endregion


        #region updates

        /// <summary>
        /// Unity MonoBehaviour Update method.
        /// </summary>
        protected virtual void Update()
        {
            Register();
            if (!isStatic)
                SyncTransform();
            PropertiesHandler.BroadcastUpdates();
        }

        /// <summary>
        /// Unity MonoBehaviour LateUpdate method.
        /// </summary>
        protected virtual void LateUpdate()
        {
            HasVisibilityBeenChecked.Clear();
        }

        /// <summary>
        /// Unity Monobehaviour OnDisable method.
        /// </summary>
        private void OnDisable()
        {
            HasVisibilityBeenChecked.Clear();
        }

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
                objectBillboard.SetValue(billboard);
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
            res.Entity = ConvertToDto(user) as AbstractObject3DDto;
            user.Send(res);
        }

        #endregion


        #region destroy

        protected virtual void OnDestroy()
        {
            if (UMI3D.Scene)
                UMI3D.Scene.Remove(this);

            if(UMI3D.Exist)
                foreach (UMI3DUser user in UMI3D.UserManager.GetUsers())
                    if (VisibleLastFrame.ContainsKey(user) && VisibleLastFrame[user])
                        user.ObjectsToRemove.Add(new RemoveObjectDto() { Id = this.Id });
        }

        #endregion


        #region visibility

        /// <summary>
        /// Return true if the object is available for a given user, false otherwise.
        /// </summary>
        /// <param name="user">User to check for</param>
        /// <returns></returns>
        public bool VisibleFor(UMI3DUser user)
        {
            if (HasVisibilityBeenChecked.ContainsKey(user))
                return VisibilityOnFrame[user];

            HasVisibilityBeenChecked.Add(user, true);

            if (!VisibilityOnFrame.ContainsKey(user))
                VisibilityOnFrame.Add(user, true);

            if (!gameObject.activeInHierarchy || !user.ImmersiveDeviceUser && immersiveOnly)
            {
                VisibilityOnFrame[user] = false;
                return false;
            }

            foreach (var filter in GetComponents<VisibilityFilter>().Where(f => f.enabled))
                if (!filter.Accept(user))
                {
                    VisibilityOnFrame[user] = false;
                    return false;
                }


            var p = UMI3D.Scene.GetObject(ParentId);
            if (p != null && !p.VisibleFor(user))
            {
                VisibilityOnFrame[user] = false;
                return false;
            }

            VisibilityOnFrame[user] = true;
            return true;
        }

        public void UpdateVisibilityLastFrame(UMI3DUser user)
        {
            if (VisibleLastFrame.ContainsKey(user))
                VisibleLastFrame[user] = VisibleFor(user);
            else
                VisibleLastFrame.Add(user, VisibleFor(user));
        }

        public void UpdateVisibilityForUser(UMI3DUser user)
        {
            bool wasVisible = (VisibleLastFrame.ContainsKey(user)) ? VisibleLastFrame[user] : false;
            bool visible = VisibleFor(user);

            if (wasVisible && !visible)
                user.ObjectsToRemove.Add(new RemoveObjectDto() { Id = this.Id });

            if (!wasVisible && visible)
            {
                GenericObject3D parent = UMI3D.Scene.GetObject(ParentId);

                if (parent == null || (parent.VisibleLastFrame.ContainsKey(user) && parent.VisibleLastFrame[user] && parent.VisibleFor(user)))
                    user.ObjectsToLoad.Add(ConvertToDto(user) as AbstractObject3DDto);
            }
        }

        #endregion


        #region sub objects

        /// <summary>
        /// Get sub objects for a given user.
        /// </summary>
        /// <param name="user">User to give children to</param>
        /// <returns></returns>
        public List<GenericObject3D> GetChildren(UMI3DUser user)
        {
            var res = new List<GenericObject3D>();
            foreach (Transform ct in transform)
            {
                var child = ct.gameObject.GetComponent<GenericObject3D>();
                if (child != null && child.VisibleFor(user))
                    res.Add(child);
            }
            return res;
        }

        #endregion


        /// <summary>
        /// Convert object to Data Transfer Object for a given user. 
        /// </summary>
        /// <param name="user">the user</param>
        /// <returns>an AbstractObject3DDto representing this object</returns>
        public abstract AbstractObject3DDto ConvertToDto(UMI3DUser user);

    }
}
