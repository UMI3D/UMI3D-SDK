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

using System.Collections;
using System.Collections.Generic;
using umi3d.common;
using umi3d.common.userCapture;
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.edk.userCapture
{
    public class UMI3DEmbodimentManager : PersistentSingleton<UMI3DEmbodimentManager>
    {
        public UMI3DScene embodimentsScene;

        public Dictionary<ulong, UMI3DAvatarNode> embodimentInstances = new Dictionary<ulong, UMI3DAvatarNode>();
        public Dictionary<ulong, Vector3> embodimentSize = new Dictionary<ulong, Vector3>();
        public Dictionary<ulong, Dictionary<uint, bool>> embodimentTrackedBonetypes = new Dictionary<ulong, Dictionary<uint, bool>>();

        public class EmbodimentEvent : UnityEvent<UMI3DAvatarNode> { };
        public class EmbodimentBoneEvent : UnityEvent<UMI3DUserEmbodimentBone> { };

        public EmbodimentEvent NewEmbodiment;

        public EmbodimentBoneEvent CreationEvent;
        public EmbodimentBoneEvent UpdateEvent;
        public EmbodimentBoneEvent DeletionEvent;

        ///<inheritdoc/>
        protected override void Awake()
        {
            base.Awake();
            NewEmbodiment = new EmbodimentEvent();
            CreationEvent = new EmbodimentBoneEvent();
            UpdateEvent = new EmbodimentBoneEvent();
            DeletionEvent = new EmbodimentBoneEvent();
        }

        ///<inheritdoc/>
        protected virtual void Start()
        {
            UMI3DServer.Instance.OnUserJoin.AddListener(CreateEmbodiment);
            UMI3DServer.Instance.OnUserLeave.AddListener(DeleteEmbodiment);
        }

        public virtual bool BoneTrackedInformation(ulong userId, uint bonetype)
        {
            if (embodimentTrackedBonetypes.ContainsKey(userId))
                return embodimentTrackedBonetypes[userId][bonetype];
            else
                return false;
        }

        public void JoinDtoReception(ulong userId, SerializableVector3 userSize, Dictionary<uint, bool> trackedBonetypes)
        {
            if (embodimentSize.ContainsKey(userId))
                Debug.LogWarning("Internal error : the user size is already registered");
            else 
                embodimentSize.Add(userId, (Vector3)userSize);

            if (embodimentTrackedBonetypes.ContainsKey(userId))
                Debug.LogWarning("Internal error : the user tracked data are already registered");
            else
                embodimentTrackedBonetypes.Add(userId, trackedBonetypes);
        }

        /// <summary>
        /// Create an Embodiment for a User.
        /// </summary>
        /// <param name="user">the concerned UMI3DUser</param>
        protected void CreateEmbodiment(UMI3DUser user)
        {
            UMI3DTrackedUser trackedUser = user as UMI3DTrackedUser;
            if (embodimentInstances.ContainsKey(user.Id()))
            {
                Debug.LogWarning("Internal error : the user is already registered");
                return;
            }

            GameObject embd = new GameObject("Embodiment" + user.Id(), typeof(UMI3DAvatarNode));
            embd.transform.SetParent(embodimentsScene.transform);
            trackedUser.Avatar = embd.GetComponent<UMI3DAvatarNode>();

            LoadAvatarNode(trackedUser.Avatar);

            trackedUser.Avatar.userId = user.Id();

            embodimentInstances.Add(user.Id(), trackedUser.Avatar);

            NewEmbodiment.Invoke(trackedUser.Avatar);
        }


        /// <summary>
        /// Update the Embodiment from the received Dto.
        /// </summary>
        /// <param name="dto">a dto containing the tracking data</param>
        public void UserTrackingReception(UserTrackingFrameDto dto, ulong userId)
        {
            if (!embodimentInstances.ContainsKey(userId))
            {
                Debug.LogWarning($"Internal error : the user [{userId}] is not registered");
                return;
            }

            UMI3DAvatarNode userEmbd = embodimentInstances[userId];
            userEmbd.transform.localPosition = dto.position;
            userEmbd.transform.localRotation = dto.rotation;
            userEmbd.transform.localScale = Vector3.one;

            UpdateNodeTransform(userEmbd);

            userEmbd.UpdateEmbodiment(dto);
        }

        /// <summary>
        /// Update the camera properties of a UMI3DUser
        /// </summary>
        /// <param name="dto">a dto containing the camera properties</param>
        /// <param name="user">the concerned user</param>
        public void UserCameraReception(UserCameraPropertiesDto dto, UMI3DUser user)
        {
            StartCoroutine(_UserCameraReception(dto, user));
        }

        public void UserCameraReception(uint operationKey, ByteContainer container, UMI3DUser user)
        {
            StartCoroutine(_UserCameraReception(operationKey, container, user));
        }

        IEnumerator _UserCameraReception(UserCameraPropertiesDto dto, UMI3DUser user)
        {
            while (!embodimentInstances.ContainsKey(user.Id()))
            {
                Debug.LogWarning($"Internal error : the user [{user.Id()}] is not registered");
                yield return new WaitForFixedUpdate();
            }

            UMI3DAvatarNode userEmbd = embodimentInstances[user.Id()];
            userEmbd.userCameraPropertiesDto = dto;
        }

        IEnumerator _UserCameraReception(uint operationKey, ByteContainer container, UMI3DUser user)
        {
            while (!embodimentInstances.ContainsKey(user.Id()))
            {
                Debug.LogWarning($"Internal error : the user [{user.Id()}] is not registered");
                yield return new WaitForFixedUpdate();
            }

            UMI3DAvatarNode userEmbd = embodimentInstances[user.Id()];
            userEmbd.userCameraPropertiesDto = UMI3DNetworkingHelper.Read<UserCameraPropertiesDto>(container);
        }

        /// <summary>
        /// Delete the User's Embodiment.
        /// </summary>
        /// <param name="user">the concerned user</param>
        protected void DeleteEmbodiment(UMI3DUser user)
        {
            if (!embodimentInstances.ContainsKey(user.Id()))
            {
                Debug.LogWarning($"Internal error : the user [{user.Id()}] is  not registered");
                return;
            }

            UMI3DAvatarNode embd = embodimentInstances[user.Id()];

            DeleteEmbodimentObj(embd);

            Destroy(embd.transform.gameObject);
            embodimentInstances.Remove(user.Id());
        }

        /// <summary>
        /// Load an Avatar Node with an important update
        /// </summary>
        /// <param name="node">the avatar node to load</param>
        public void LoadAvatarNode(UMI3DAbstractNode node)
        {
            node.Register();
            LoadEntity op = node.GetLoadEntity();
            UMI3DServer.Dispatch(new Transaction
            {
                Operations = new List<Operation> { op },
                reliable = true
            });
        }

        /// <summary>
        /// Remove an Avatar Node with an important update
        /// </summary>
        /// <param name="id"></param>
        protected void DeleteEmbodimentObj(UMI3DAvatarNode node)
        {
            UMI3DServer.Dispatch(new Transaction
            {
                Operations = new List<Operation> { node.GetDeleteEntity() },
                reliable = true
            });
        }

        /// <summary>
        /// Update a Node.
        /// </summary>
        /// <param name="obj">the node to update</param>
        public void UpdateNodeTransform(UMI3DNode obj)
        {
            obj.objectPosition.SetValue(obj.transform.localPosition);
            obj.objectRotation.SetValue(obj.transform.localRotation);
            obj.objectScale.SetValue(obj.transform.localScale);
        }

        /// <summary>
        /// Set the activation of Bindings of an Avatar Node.
        /// </summary>
        /// <param name="obj">the avatar node</param>
        /// <param name="b">the activation value</param>
        /// <returns>The associated SetEntityProperty</returns>
        public SetEntityProperty UpdateBindingActivation(UMI3DAvatarNode obj, bool b)
        {
            UMI3DAvatarNode.onActivationValueChanged.Invoke(obj.userId, b);
            return obj.activeBindings.SetValue(b);
        }

        /// <summary>
        /// Set the activation of Bindings of an Avatar Node for a given user.
        /// </summary>
        /// <param name="user">the user</param>
        /// <param name="obj">the avatar node</param>
        /// <param name="b">the activation value</param>
        /// <returns>The associated SetEntityProperty</returns>
        public SetEntityProperty UpdateBindingActivation(UMI3DUser user, UMI3DAvatarNode obj, bool b)
        {
            UMI3DAvatarNode.onActivationValueChanged.Invoke(obj.userId, b);
            return obj.activeBindings.SetValue(user, b);
        }

        /// <summary>
        /// Set the list of Bindings of an Avatar Node.
        /// </summary>
        /// <param name="obj">the avatar node</param>
        /// <param name="bindings">the list of bindings</param>
        /// <returns>The associated SetEntityProperty</returns>
        public SetEntityProperty UpdateBindingList(UMI3DAvatarNode obj, List<UMI3DBinding> bindings)
        {
            return obj.bindings.SetValue(bindings);
        }

        /// <summary>
        /// Set the list of Bindings of an Avatar Node for a given user.
        /// </summary>
        /// <param name="user">the user</param>
        /// <param name="obj">the avatar node</param>
        /// <param name="bindings">the list of bindings</param>
        /// <returns>The associated SetEntityProperty</returns>
        public SetEntityProperty UpdateBindingList(UMI3DUser user, UMI3DAvatarNode obj, List<UMI3DBinding> bindings)
        {
            return obj.bindings.SetValue(user, bindings);
        }

        /// <summary>
        /// Update the Binding value of an Avatar Node at a given index.
        /// </summary>
        /// <param name="obj">the avatar node</param>
        /// <param name="index">the given index</param>
        /// <param name="binding">the new binding value</param>
        /// <returns>The associated SetEntityProperty</returns>
        public SetEntityProperty UpdateBinding(UMI3DAvatarNode obj, int index, UMI3DBinding binding)
        {
            return obj.bindings.SetValue(index, binding);
        }

        /// <summary>
        /// Update the Binding value of an Avatar Node at a given index for a given user.
        /// </summary>
        /// <param name="user">the user</param>
        /// <param name="obj">the avatar node</param>
        /// <param name="index">the given index</param>
        /// <param name="binding">the new binding value</param>
        /// <returns>The associated SetEntityProperty</returns>
        public SetEntityProperty UpdateBinding(UMI3DUser user, UMI3DAvatarNode obj, int index, UMI3DBinding binding)
        {
            return obj.bindings.SetValue(user, index, binding);
        }

        /// <summary>
        /// Add a new Binding for an Avatar Node.
        /// </summary>
        /// <param name="obj">the avatar node</param>
        /// <param name="binding">the new binding value</param>
        /// <returns>The associated SetEntityProperty</returns>
        public SetEntityProperty AddBinding(UMI3DAvatarNode obj, UMI3DBinding binding)
        {
            return obj.bindings.Add(binding);
        }

        /// <summary>
        /// Add a new Binding for an Avatar Node for a given user.
        /// </summary>
        /// <param name="user">the user</param>
        /// <param name="obj">the avatar node</param>
        /// <param name="binding">the new binding value</param>
        /// <returns>The associated SetEntityProperty</returns>
        public SetEntityProperty AddBinding(UMI3DUser user, UMI3DAvatarNode obj, UMI3DBinding binding)
        {
            return obj.bindings.Add(user, binding);
        }

        /// <summary>
        /// Remove a Binding for an Avatar Node.
        /// </summary>
        /// <param name="obj">the avatar node</param>
        /// <param name="binding">the new binding value</param>
        /// <param name="keepWorldPosition">the boolean to freeze the object in the world</param>
        /// <param name="newparent">a transform intended to be the new parent. If keepWorldPosition is true, newparent must be specified.</param>
        /// <returns>The list of associated SetEntityProperty.</returns>
        public List<SetEntityProperty> RemoveBinding(UMI3DAvatarNode obj, UMI3DBinding binding, bool keepWorldPosition = false, UMI3DAbstractNode newparent = null)
        {
            List<SetEntityProperty> operations = new List<SetEntityProperty>();

            if (keepWorldPosition && newparent != null)
            {
                binding.node.transform.SetParent(newparent.transform, true);
                operations.Add(binding.node.objectParentId.SetValue(newparent.GetComponent<UMI3DAbstractNode>()));

                operations.Add(binding.node.objectPosition.SetValue(binding.node.transform.localPosition));
                operations.Add(binding.node.objectRotation.SetValue(binding.node.transform.localRotation));
            }

            operations.Insert(0, obj.bindings.Remove(binding));
            return operations;
        }

        /// <summary>
        /// Remove a Binding for an Avatar Node for a given user.
        /// </summary>
        /// <param name="user">the user</param>
        /// <param name="obj">the avatar node</param>
        /// <param name="binding">the new binding value</param>
        /// <param name="keepWorldPosition">the boolean to freeze the object in the world</param>
        /// <param name="newparent">a transform intended to be the new parent. If keepWorldPosition is true, newparent must be specified.</param>
        /// <returns>The list of associated SetEntityProperty</returns>
        public List<SetEntityProperty> RemoveBinding(UMI3DUser user, UMI3DAvatarNode obj, UMI3DBinding binding, bool keepWorldPosition = false, UMI3DAbstractNode newparent = null)
        {
            List<SetEntityProperty> operations = new List<SetEntityProperty>();

            if (keepWorldPosition && newparent != null)
            {
                binding.node.transform.SetParent(newparent.transform, true);
                operations.Add(binding.node.objectParentId.SetValue(user, newparent.GetComponent<UMI3DAbstractNode>()));

                operations.Add(binding.node.objectPosition.SetValue(user, binding.node.transform.localPosition));
                operations.Add(binding.node.objectRotation.SetValue(user, binding.node.transform.localRotation));
            }

            operations.Insert(0, obj.bindings.Remove(user, binding));
            return operations;
        }

        /// <summary>
        /// Remove a Binding at a given index for an Avatar Node.
        /// </summary>
        /// <param name="obj">the avatar node</param>
        /// <param name="index">the given index</param>
        /// <param name="keepWorldPosition">the boolean to freeze the object in the world</param>
        /// <param name="newparent">a transform intended to be the new parent. If keepWorldPosition is true, newparent must be specified.</param>
        /// <returns>The list of associated SetEntityProperty</returns>
        public List<SetEntityProperty> RemoveBinding(UMI3DAvatarNode obj, int index, bool keepWorldPosition = false, UMI3DAbstractNode newparent = null)
        {
            List<SetEntityProperty> operations = new List<SetEntityProperty>();

            if (keepWorldPosition && newparent != null)
            {
                UMI3DBinding binding = obj.bindings.GetValue(index);

                binding.node.transform.SetParent(newparent.transform, true);
                var op = binding.node.objectParentId.SetValue(newparent.GetComponent<UMI3DAbstractNode>());

                operations.Add(op);

                operations.Add(binding.node.objectPosition.SetValue(binding.node.transform.localPosition));
                operations.Add(binding.node.objectRotation.SetValue(binding.node.transform.localRotation));
            }

            operations.Insert(0, obj.bindings.RemoveAt(index));
            return operations;
        }

        /// <summary>
        /// Remove a Binding at a given index for an Avatar Node for a given user.
        /// </summary>
        /// <param name="user">the user</param>
        /// <param name="obj">the avatar node</param>
        /// <param name="index">the given index</param>
        /// <param name="keepWorldPosition">the boolean to freeze the object in the world</param>
        /// <param name="newparent">a transform intended to be the new parent. If keepWorldPosition is true, newparent must be specified.</param>
        /// <returns>The list of associated SetEntityProperty</returns>
        public List<SetEntityProperty> RemoveBinding(UMI3DUser user, UMI3DAvatarNode obj, int index, bool keepWorldPosition = false, UMI3DAbstractNode newparent = null)
        {
            List<SetEntityProperty> operations = new List<SetEntityProperty>();

            if (keepWorldPosition && newparent != null)
            {
                UMI3DBinding binding = obj.bindings.GetValue(index);

                binding.node.transform.SetParent(newparent.transform, true);
                operations.Add(binding.node.objectParentId.SetValue(user, newparent.GetComponent<UMI3DAbstractNode>()));

                operations.Add(binding.node.objectPosition.SetValue(user, binding.node.transform.localPosition));
                operations.Add(binding.node.objectRotation.SetValue(user, binding.node.transform.localRotation));
            }

            operations.Insert(0, obj.bindings.RemoveAt(user, index));
            return operations;
        }
    }
}