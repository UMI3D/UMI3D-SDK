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
using System.Linq;
using umi3d.common;
using umi3d.common.userCapture;
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.edk.userCapture
{
    public class UMI3DEmbodimentManager : PersistentSingleton<UMI3DEmbodimentManager>
    {
        public float time = 0f;
        float timeTmp = 0;
        public int max = 0;

        Transaction transaction = new Transaction();

        public Dictionary<string, UMI3DAvatarNode> embodimentInstances = new Dictionary<string, UMI3DAvatarNode>();

        public class EmbodimentEvent : UnityEvent<UMI3DAvatarNode> { };
        public class EmbodimentBoneEvent : UnityEvent<UMI3DUserEmbodimentBone> { };

        public EmbodimentEvent NewEmbodiment;

        public EmbodimentBoneEvent CreationEvent;
        public EmbodimentBoneEvent UpdateEvent;
        public EmbodimentBoneEvent DeletionEvent;

        public UMI3DScene embodimentsScene;

        protected override void Awake()
        {
            base.Awake();
            NewEmbodiment = new EmbodimentEvent();
            CreationEvent = new EmbodimentBoneEvent();
            UpdateEvent = new EmbodimentBoneEvent();
            DeletionEvent = new EmbodimentBoneEvent();

        }

        protected virtual void Start()
        {
            UMI3DServer.Instance.OnUserJoin.AddListener(CreateEmbodiment);
            UMI3DServer.Instance.OnUserLeave.AddListener(DeleteEmbodiment);
        }

        //Update is called once per frame
        void Update()
        {
            if (UMI3DServer.Exists)
                Dispatch();
        }

        /// <summary>
        /// Create an Embodiment for a User.
        /// </summary>
        protected void CreateEmbodiment(UMI3DUser user)
        {
            UMI3DTrackedUser trackedUser = user as UMI3DTrackedUser;
            if (embodimentInstances.ContainsKey(user.Id()))
                throw new Exception("Internal error : the user is already registered");

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
        public void UserTrackingReception(UserTrackingFrameDto dto)
        {
            if (!embodimentInstances.ContainsKey(dto.userId))
            {
                throw new Exception("Internal error : the user is not registered");
            }

            UMI3DAvatarNode userEmbd = embodimentInstances[dto.userId];
            userEmbd.transform.localPosition = dto.position;
            userEmbd.transform.localRotation = dto.rotation;
            userEmbd.transform.localScale = dto.scale;

            UpdateUserEmbodiment(userEmbd);

            userEmbd.UpdateEmbodiment(dto);
        }

        public void UserCameraReception(UserCameraPropertiesDto dto, UMI3DUser user)
        {
            if (!embodimentInstances.ContainsKey(user.Id()))
            {
                throw new Exception("Internal error : the user is not registered");
            }

            UMI3DAvatarNode userEmbd = embodimentInstances[user.Id()];
            userEmbd.userCameraPropertiesDto = dto;

            Debug.LogWarning("bonetype : " + dto.boneType);
        }

        /// <summary>
        /// Delete the User's Embodiment.
        /// </summary>
        protected void DeleteEmbodiment(UMI3DUser user)
        {
            if (!embodimentInstances.ContainsKey(user.Id()))
                throw new Exception("Internal error : the user is not registered");

            UMI3DAvatarNode embd = embodimentInstances[user.Id()];

            DeleteEmbodimentObj(embd.Id());

            Destroy(embd.transform.gameObject);
            embodimentInstances.Remove(user.Id());
        }

        /// <summary>
        /// Load an Avatar Node.
        /// </summary>
        /// <param name="node">the avatar node to load.</param>
        public void LoadAvatarNode(UMI3DAbstractNode node)
        {
            LoadEntity op = node.Register();
            transaction.Operations.Add(op);
        }

        protected void DeleteEmbodimentObj(string id)
        {
            transaction.Operations.RemoveAll(o =>
            {
                if (o is SetEntityProperty)
                {
                    return (o as SetEntityProperty).entityId == id;
                }
                return false;

            });

            DeleteEntity op = new DeleteEntity();
            op.entityId = id;
            op += UMI3DEnvironment.GetEntities<UMI3DUser>();
            transaction.Operations.Add(op);
        }

        /// <summary>
        /// Update an Avatar Node.
        /// </summary>
        /// <param name="obj">the avatar node to update.</param>
        public void UpdateUserEmbodiment(UMI3DAvatarNode obj)
        {
            UpdateNodeTransform(obj);
        }

        /// <summary>
        /// Update an Avatar Node for a given user.
        /// </summary>
        /// <param name="user">the user</param>
        /// <param name="obj">the avatar node to update.</param>
        public void UpdateUserEmbodiment(UMI3DUser user, UMI3DAvatarNode obj)
        {
            UpdateNodeTransform(user, obj);
        }

        /// <summary>
        /// Update a Node.
        /// </summary>
        /// <param name="obj">the node to update</param>
        public void UpdateNodeTransform(UMI3DNode obj)
        {
            setOperation(obj.objectParentId.SetValue(obj.transform.parent.GetComponent<UMI3DAbstractNode>()));
            setOperation(obj.objectPosition.SetValue(obj.transform.localPosition));
            setOperation(obj.objectRotation.SetValue(obj.transform.localRotation));
            setOperation(obj.objectScale.SetValue(obj.transform.localScale));
        }

        /// <summary>
        /// Update a Node for a given user.
        /// </summary>
        /// <param name="user">the user</param>
        /// <param name="obj">the node to update</param>
        public void UpdateNodeTransform(UMI3DUser user, UMI3DNode obj)
        {
            setOperation(obj.objectPosition.SetValue(user, obj.transform.localPosition));
            setOperation(obj.objectRotation.SetValue(user, obj.transform.localRotation));
            setOperation(obj.objectScale.SetValue(user, obj.transform.localScale));
        }

        /// <summary>
        /// Set the activation of Bindings of an Avatar Node.
        /// </summary>
        /// <param name="obj">the avatar node</param>
        /// <param name="b">the activation value</param>
        public void UpdateBindingActivation(UMI3DAvatarNode obj, bool b)
        {
            setOperation(obj.activeBindings.SetValue(b));
            UMI3DAvatarNode.onActivationValueChanged.Invoke(obj.userId, b);
        }

        /// <summary>
        /// Set the activation of Bindings of an Avatar Node for a given user.
        /// </summary>
        /// <param name="user">the user</param>
        /// <param name="obj">the avatar node</param>
        /// <param name="b">the activation value</param>
        public void UpdateBindingActivation(UMI3DUser user, UMI3DAvatarNode obj, bool b)
        {
            setOperation(obj.activeBindings.SetValue(user, b));
            UMI3DAvatarNode.onActivationValueChanged.Invoke(obj.userId, b);
        }

        /// <summary>
        /// Set the list of Bindings of an Avatar Node.
        /// </summary>
        /// <param name="obj">the avatar node</param>
        /// <param name="bindings">the list of bindings</param>
        public void UpdateBindingList(UMI3DAvatarNode obj, List<UMI3DBinding> bindings)
        {
            setOperation(obj.bindings.SetValue(bindings));
        }

        /// <summary>
        /// Set the list of Bindings of an Avatar Node for a given user.
        /// </summary>
        /// <param name="user">the user</param>
        /// <param name="obj">the avatar node</param>
        /// <param name="bindings">the list of bindings</param>
        public void UpdateBindingList(UMI3DUser user, UMI3DAvatarNode obj, List<UMI3DBinding> bindings)
        {
            setOperation(obj.bindings.SetValue(user, bindings));
        }

        /// <summary>
        /// Update the Binding value of an Avatar Node at a given index.
        /// </summary>
        /// <param name="obj">the avatar node</param>
        /// <param name="index">the given index</param>
        /// <param name="binding">the new binding value</param>
        public void UpdateBinding(UMI3DAvatarNode obj, int index, UMI3DBinding binding)
        {
            setOperation(obj.bindings.SetValue(index, binding));
        }

        /// <summary>
        /// Update the Binding value of an Avatar Node at a given index for a given user.
        /// </summary>
        /// <param name="user">the user</param>
        /// <param name="obj">the avatar node</param>
        /// <param name="index">the given index</param>
        /// <param name="binding">the new binding value</param>
        public void UpdateBinding(UMI3DUser user, UMI3DAvatarNode obj, int index, UMI3DBinding binding)
        {
            setOperation(obj.bindings.SetValue(user, index, binding));
        }

        /// <summary>
        /// Add a new Binding for an Avatar Node.
        /// </summary>
        /// <param name="obj">the avatar node</param>
        /// <param name="binding">the new binding value</param>
        public void AddBinding(UMI3DAvatarNode obj, UMI3DBinding binding)
        {
            setOperation(obj.bindings.Add(binding));
        }

        /// <summary>
        /// Add a new Binding for an Avatar Node for a given user.
        /// </summary>
        /// <param name="user">the user</param>
        /// <param name="obj">the avatar node</param>
        /// <param name="binding">the new binding value</param>
        public void AddBinding(UMI3DUser user, UMI3DAvatarNode obj, UMI3DBinding binding)
        {
            setOperation(obj.bindings.Add(user, binding));
        }

        /// <summary>
        /// Remove a Binding for an Avatar Node.
        /// </summary>
        /// <param name="obj">the avatar node</param>
        /// <param name="binding">the new binding value</param>
        public void RemoveBinding(UMI3DAvatarNode obj, UMI3DBinding binding)
        {
            setOperation(obj.bindings.Remove(binding));
        }

        /// <summary>
        /// Remove a Binding for an Avatar Node for a given user.
        /// </summary>
        /// <param name="user">the user</param>
        /// <param name="obj">the avatar node</param>
        /// <param name="binding">the new binding value</param>
        public void RemoveBinding(UMI3DUser user, UMI3DAvatarNode obj, UMI3DBinding binding)
        {
            setOperation(obj.bindings.Remove(user, binding));
        }

        /// <summary>
        /// Remove a Binding at a given index for an Avatar Node.
        /// </summary>
        /// <param name="obj">the avatar node</param>
        /// <param name="index">the given index</param>
        public void RemoveBinding(UMI3DAvatarNode obj, int index)
        {
            setOperation(obj.bindings.RemoveAt(index));
        }

        /// <summary>
        /// Remove a Binding at a given index for an Avatar Node for a given user.
        /// </summary>
        /// <param name="user">the user</param>
        /// <param name="obj">the avatar node</param>
        /// <param name="index">the given index</param>
        public void RemoveBinding(UMI3DUser user, UMI3DAvatarNode obj, int index)
        {
            setOperation(obj.bindings.RemoveAt(user, index));
        }

        public void setOperation(SetEntityProperty operation)
        {
            if (operation != null)
            {
                transaction.Operations.Add(operation);
            }
        }

        bool checkTime()
        {
            timeTmp -= Time.deltaTime;
            if (time == 0 || timeTmp <= 0)
            {
                timeTmp = time;
                return true;
            }
            return false;
        }

        bool checkMax()
        {
            return max != 0 && transaction.Operations.Count() > max;
        }

        void Dispatch()
        {
            if (checkTime() || checkMax())
            {
                if (transaction.Operations.Count > 0)
                {
                    transaction.reliable = false;
                    UMI3DServer.Dispatch(transaction);
                    transaction.Operations.Clear();
                }

            }
        }
    }
}