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
using System.Linq;
using umi3d.common;
using UnityEngine;

namespace umi3d.edk
{
    /// <summary>
    /// Abstract base UMI3D empty object to load on the clients.
    /// </summary>
    [DisallowMultipleComponent]
    [SelectionBase]
    public abstract class UMI3DAbstractNode : MonoBehaviour, UMI3DLoadableEntity
    {

        #region properties

        /// <summary>
        /// Indicates if InitDefinition has been called.
        /// </summary>
        protected bool inited = false;

        /// <summary>
        /// The objects's unique id. 
        /// </summary>
        protected ulong objectId;

        /// <summary>
        /// The public Getter for objectId.
        /// </summary>
        public ulong Id()
        {
            Register();
            return objectId;
        }

        /// <summary>
        /// The public Getter for object's parent Id.
        /// </summary>
        /// <returns></returns>
        private UMI3DAbstractNode Parent
        {
            get
            {
                if (transform.parent != null)
                {
                    UMI3DAbstractNode p = transform.parent.gameObject.GetComponent<UMI3DAbstractNode>();
                    return p;
                }
                return null;
            }
        }

        /// <summary>
        /// Get the id of the parent object if any.
        /// </summary>
        public UMI3DAsyncProperty<UMI3DAbstractNode> objectParentId { get { Register(); return _objectParentId; } protected set => _objectParentId = value; }

        /// <summary>
        /// True if the object is not allowed to move during the application exectution.
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("True if the object is not allowed to move during the application exectution.")]
        protected bool isStatic = false;
        /// <summary>
        /// See <see cref="isStatic"/>.
        /// </summary>
        public UMI3DAsyncProperty<bool> objectIsStatic { get { Register(); return _objectIsStatic; } protected set => _objectIsStatic = value; }


        /// <summary>
        /// True if the object should be active is the scene graph.
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("True if the object should be active is the scene graph.")]
        protected bool active = true;
        /// <summary>
        /// See <see cref="active"/>.
        /// </summary>
        public UMI3DAsyncProperty<bool> objectActive { get { Register(); return _objectActive; } protected set => _objectActive = value; }

        /// <summary>
        /// Anchor of the object in the real world, if any, for AR design.
        /// </summary>
        /// Nodes that possess an Anchor are placed according to their anchor in the real world, instaed of its position in the scene.
        [SerializeField, EditorReadOnly, Tooltip("Anchor of the object in the real world, if any, for AR design.")]
        protected UMI3DAnchor UMI3DAnchor;
        /// <summary>
        /// See <see cref="UMI3DAnchor"/>.
        /// </summary>
        public UMI3DAsyncProperty<UMI3DAnchorDto> objectAnchor { get { Register(); return _objectAnchor; } protected set => _objectAnchor = value; }


        /// <summary>
        /// True if the object is only vissible in full 3D media displayers (sush as Computer or Virtual Reality headset).
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("True if the object is only vissible in full 3D media displayers (sush as Computer or Virtual Reality headset).")]
        protected bool immersiveOnly = false;
        /// <summary>
        /// See <see cref="immersiveOnly"/>.
        /// </summary>
        public UMI3DAsyncProperty<bool> objectImmersiveOnly { get { Register(); return _objectImmersiveOnly; } protected set => _objectImmersiveOnly = value; }




        /// <summary>
        /// The object local position in the scene graph.
        /// </summary>
        public UMI3DAsyncProperty<Vector3> objectPosition { get { Register(); return _objectPosition; } protected set => _objectPosition = value; }

        /// <summary>
        /// The object local orientation in the scene graph.
        /// </summary>
        public UMI3DAsyncProperty<Quaternion> objectRotation { get { Register(); return _objectRotation; } protected set => _objectRotation = value; }

        /// <summary>
        /// The object local scale in the scene graph.
        /// </summary>
        public UMI3DAsyncProperty<Vector3> objectScale { get { Register(); return _objectScale; } protected set => _objectScale = value; }

        private UMI3DAsyncProperty<UMI3DAbstractNode> _objectParentId;
        private UMI3DAsyncProperty<bool> _objectIsStatic;
        private UMI3DAsyncProperty<bool> _objectActive;
        private UMI3DAsyncProperty<UMI3DAnchorDto> _objectAnchor;
        private UMI3DAsyncProperty<bool> _objectImmersiveOnly;
        private UMI3DAsyncProperty<Vector3> _objectPosition;
        private UMI3DAsyncProperty<Quaternion> _objectRotation;
        private UMI3DAsyncProperty<Vector3> _objectScale;

        /// <summary>
        /// Id of the Volume (see <seealso cref="volume.IVolume"/>) in which the node is present
        /// </summary>
        public ulong VolumeId;

        /// <summary>
        /// Room object used to relay data
        /// </summary>
        public ICollaborationRoom RelayRoom;

        #endregion


        #region initialization

        /// <summary>
        /// Check if the AbstractObject3D has been registered to to the UMI3DScene and do it if not
        /// </summary>
        public virtual void Register()
        {
            if (objectId == 0 && UMI3DEnvironment.Exists)
            {
                objectId = UMI3DEnvironment.Register(this);
                InitDefinition(objectId);
            }
        }

        /// <summary>
        /// Return load operation
        /// </summary>
        /// <returns></returns>
        public virtual LoadEntity GetLoadEntity(HashSet<UMI3DUser> users = null)
        {
            var operation = new LoadEntity()
            {
                entities = new List<UMI3DLoadableEntity>() { this },
                users = users != null ? new HashSet<UMI3DUser>(users) : UMI3DServer.Instance.UserSetWhenHasJoined()
            };
            return operation;
        }

        /// <summary>
        /// Return delete operation
        /// </summary>
        /// <returns></returns>
        public DeleteEntity GetDeleteEntity(HashSet<UMI3DUser> users = null)
        {
            var operation = new DeleteEntity()
            {
                entityId = Id(),
                users = users != null ? new HashSet<UMI3DUser>(users) : UMI3DServer.Instance.UserSetWhenHasJoined()
            };
            return operation;
        }

        /// <summary>
        /// Initialize object's properties.
        /// </summary>
        protected virtual void InitDefinition(ulong id)
        {
            BeardedManStudios.Forge.Networking.Unity.MainThreadManager.Run(() =>
            {
                if (this != null)
                {
                    foreach (UMI3DUserFilter f in GetComponents<UMI3DUserFilter>())
                        AddConnectionFilter(f);
                }
            });

            objectId = id;

            objectIsStatic = new UMI3DAsyncProperty<bool>(objectId, UMI3DPropertyKeys.Static, this.isStatic);
            objectActive = new UMI3DAsyncProperty<bool>(objectId, UMI3DPropertyKeys.Active, this.active);
            objectIsStatic.OnValueChanged += (bool s) => isStatic = s;
            objectActive.OnValueChanged += (bool a) => active = a;

            objectParentId = new UMI3DAsyncProperty<UMI3DAbstractNode>(objectId, UMI3DPropertyKeys.ParentId, Parent, (UMI3DAbstractNode node, UMI3DUser user) => node?.Id());
            objectParentId.OnValueChanged += (UMI3DAbstractNode node) => { if (transform.parent != node?.transform) transform.SetParent(node?.transform); };

            var PropertyEquality = new UMI3DAsyncPropertyEquality
            {
                epsilon = 0.000001f
            };

            objectPosition = new UMI3DAsyncProperty<Vector3>(objectId, UMI3DPropertyKeys.Position, transform.localPosition, ToUMI3DSerializable.ToSerializableVector3, PropertyEquality.Vector3Equality);
            objectPosition.OnValueChanged += (Vector3 p) => transform.localPosition = p;

            objectRotation = new UMI3DAsyncProperty<Quaternion>(objectId, UMI3DPropertyKeys.Rotation, transform.localRotation, ToUMI3DSerializable.ToSerializableVector4, PropertyEquality.QuaternionEquality);

            objectRotation.OnValueChanged += (Quaternion r) => transform.localRotation = r;

            objectScale = new UMI3DAsyncProperty<Vector3>(objectId, UMI3DPropertyKeys.Scale, transform.localScale, ToUMI3DSerializable.ToSerializableVector3);
            objectScale.OnValueChanged += (Vector3 s) => transform.localScale = s;

            objectImmersiveOnly = new UMI3DAsyncProperty<bool>(objectId, UMI3DPropertyKeys.VROnly, this.immersiveOnly);
            objectImmersiveOnly.OnValueChanged += (bool b) => immersiveOnly = b;

            objectAnchor = new UMI3DAsyncProperty<UMI3DAnchorDto>(objectId, UMI3DPropertyKeys.Anchor, UMI3DAnchor?.ToDto());
            objectAnchor.OnValueChanged += (UMI3DAnchorDto a) => {
                if (a != null)
                {
                    UMI3DAnchor.PositionOffset = a.positionOffset.Struct();
                    UMI3DAnchor.RotationOffset = a.rotationOffset;
                    UMI3DAnchor.ScaleOffset = a.scaleOffset.Struct();
                }
            };

            inited = true;
        }


        #endregion

        /// <inheritdoc/>
        protected virtual void WriteProperties(UMI3DAbstractNodeDto dto, UMI3DUser user)
        {
            dto.id = Id();
            dto.pid = objectParentId.GetValue(user)?.Id() ?? 0;
            dto.active = objectActive.GetValue(user);
            dto.isStatic = objectIsStatic.GetValue(user);
            dto.immersiveOnly = objectImmersiveOnly.GetValue(user);
            dto.anchorDto = objectAnchor.GetValue(user);
        }

        /// <inheritdoc/>
        public virtual Bytable ToBytes(UMI3DUser user)
        {
            UMI3DAnchorDto anchor = objectAnchor.GetValue(user);
            return UMI3DSerializer.Write(Id())
                + UMI3DSerializer.Write(objectParentId.GetValue(user)?.Id() ?? 0)
                + UMI3DSerializer.Write(objectActive.GetValue(user))
                + UMI3DSerializer.Write(objectIsStatic.GetValue(user))
                + UMI3DSerializer.Write(objectImmersiveOnly.GetValue(user))
                + UMI3DSerializer.Write(anchor.positionOffset)
                + UMI3DSerializer.Write(anchor.rotationOffset)
                + UMI3DSerializer.Write(anchor.scaleOffset);
        }


        #region sub objects

        /// <summary>
        /// Get sub objects for a given user.
        /// </summary>
        /// <param name="user">User to give children to</param>
        /// <returns></returns>
        public List<UMI3DAbstractNode> GetChildren(UMI3DUser user)
        {
            var res = new List<UMI3DAbstractNode>();
            foreach (Transform ct in transform)
            {
                UMI3DAbstractNode child = ct.gameObject.GetComponent<UMI3DAbstractNode>();
                if (child != null)
                {
                    if (child is UMI3DScene)
                    {
                        res.Add(child);
                    }
                    else if (child is UMI3DNode)
                    {
                        res.Add(child);
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// Get all sub objects for a given user and scene.
        /// </summary>
        /// <param name="user">User to give children to</param>
        /// <returns></returns>
        protected List<UMI3DNode> GetAllChildrenInThisScene(UMI3DUser user)
        {
            var res = new List<UMI3DNode>();
            foreach (Transform ct in transform)
            {
                UMI3DAbstractNode child = ct.gameObject.GetComponent<UMI3DAbstractNode>();
                if (child != null)
                {
                    if (child.LoadOnConnection(user) && child is UMI3DNode obj)
                    {
                        res.Add(obj);
                        res.AddRange(child.GetAllChildrenInThisScene(user));
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// Retrieve all the <see cref="UMI3DLoadableEntity"/> instances on a node and its children.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public List<UMI3DLoadableEntity> GetAllLoadableEntityUnderThisNode(UMI3DUser user, Transform transform = null)
        {
            var res = new List<UMI3DLoadableEntity>();

            if (transform == null) transform = this.transform;
            else if (transform.gameObject.GetComponent<UMI3DAbstractNode>() != null) return res;

            IEnumerable<UMI3DLoadableEntity> others = transform.gameObject.GetComponents<UMI3DLoadableEntity>()?.Where(i => !(i is UMI3DAbstractNode) && i.LoadOnConnection(user));
            if (others != null)
            {
                res.AddRange(others);
            }
            foreach (Transform ct in transform)
            {
                res.AddRange(GetAllLoadableEntityUnderThisNode(user, ct));
            }
            return res;
        }

        /// <inheritdoc/>
        public abstract IEntity ToEntityDto(UMI3DUser user);

        #endregion

        #region filter
        private readonly HashSet<UMI3DUserFilter> ConnectionFilters = new HashSet<UMI3DUserFilter>();

        public bool LoadOnConnection(UMI3DUser user)
        {
            return ConnectionFilters.Count == 0 || !ConnectionFilters.Any(f => !f.Accept(user));
        }

        public bool AddConnectionFilter(UMI3DUserFilter filter)
        {
            return ConnectionFilters.Add(filter);
        }

        public bool RemoveConnectionFilter(UMI3DUserFilter filter)
        {
            return ConnectionFilters.Remove(filter);
        }

        #endregion

    }
}

