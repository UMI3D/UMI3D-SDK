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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using umi3d.common;
using umi3d.common.userCapture;
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.edk.userCapture
{
    /// <summary>
    /// Manager for al embodiment related events. Handles skeletons, avatars, animations, and poses.
    /// </summary>
    public class UMI3DEmbodimentManager : PersistentSingleBehaviour<UMI3DEmbodimentManager>
    {
        private const DebugScope scope = DebugScope.EDK | DebugScope.UserCapture | DebugScope.User;

        /// <summary>
        /// Should the embodiment system be active?
        /// </summary>
        /// Should not be changed at runtime.
        [Tooltip("Should the embodiment system be active?\n" +
            "Warning: Should not be changed when Play Mode is running.")]
        public bool ActivateEmbodiments = true;

        public UMI3DScene EmbodimentsScene;
        /// <summary>
        /// Unity's prefab of the skeleton.
        /// </summary>
        [Tooltip("Prefab of the skeleton of the user.")]
        public GameObject SkeletonPrefab;

        public Dictionary<ulong, UMI3DAvatarNode> embodimentInstances = new Dictionary<ulong, UMI3DAvatarNode>();
        public Dictionary<ulong, Vector3> embodimentSize = new Dictionary<ulong, Vector3>();
        public Dictionary<ulong, Dictionary<uint, bool>> embodimentTrackedBonetypes = new Dictionary<ulong, Dictionary<uint, bool>>();

        public class EmbodimentEvent : UnityEvent<UMI3DAvatarNode> { };
        public class EmbodimentBoneEvent : UnityEvent<UMI3DUserEmbodimentBone> { };

        public EmbodimentEvent NewEmbodiment;

        public EmbodimentBoneEvent CreationEvent;
        public EmbodimentBoneEvent UpdateEvent;
        public EmbodimentBoneEvent DeletionEvent;

        /// <summary>
        /// Emote configuration for the environment.
        /// </summary>
        [SerializeField, Tooltip("Emote configuration for the environment.")]
        public UMI3DEmotesConfig emotesConfig;

        /// <inheritdoc/>
        protected override void Awake()
        {
            base.Awake();
            NewEmbodiment = new EmbodimentEvent();
            CreationEvent = new EmbodimentBoneEvent();
            UpdateEvent = new EmbodimentBoneEvent();
            DeletionEvent = new EmbodimentBoneEvent();
        }

        /// <inheritdoc/>
        protected virtual void Start()
        {
            if (ActivateEmbodiments)
            {
                UMI3DServer.Instance.OnUserLeave.AddListener(DeleteEmbodiment);
                WriteCollections();
            }
        }

        #region Tracking Data

        public virtual bool BoneTrackedInformation(ulong userId, uint bonetype)
        {
            if (embodimentTrackedBonetypes.ContainsKey(userId))
                return embodimentTrackedBonetypes[userId][bonetype];
            else
                return false;
        }

        /// <summary>
        /// Lock for  <see cref="JoinDtoReception(UMI3DUser, SerializableVector3, Dictionary{uint, bool})"/>.
        /// </summary>
        static object joinLock = new object();

        public async Task JoinDtoReception(UMI3DUser user, SerializableVector3 userSize, Dictionary<uint, bool> trackedBonetypes)
        {
            if (ActivateEmbodiments && user is UMI3DTrackedUser trackedUser)
            {
                ulong userId = user.Id();

                lock (joinLock)
                {
                    UMI3DLogger.Log("EmbodimentManager.JoinDtoReception before " + userId, scope);

                    if (embodimentSize.ContainsKey(userId))
                        UMI3DLogger.LogWarning("Internal error : the user size is already registered", scope);
                    else
                        embodimentSize.Add(userId, (Vector3)userSize);

                    if (embodimentTrackedBonetypes.ContainsKey(userId))
                        UMI3DLogger.LogWarning("Internal error : the user tracked data are already registered", scope);
                    else
                        embodimentTrackedBonetypes.Add(userId, trackedBonetypes);

                    if (embodimentInstances.ContainsKey(userId))
                    {
                        UMI3DLogger.LogWarning("Internal error : the user is already registered", scope);
                        return;
                    }

                    var embd = new GameObject("Embodiment" + userId, typeof(UMI3DAvatarNode));
                    embd.transform.position = UMI3DEnvironment.objectStartPosition.GetValue(user);
                    embd.transform.rotation = UMI3DEnvironment.objectStartOrientation.GetValue(user);

                    if (EmbodimentsScene != null)
                        embd.transform.SetParent(EmbodimentsScene.transform);
                    else
                        UMI3DLogger.LogWarning("The embodiments scene is not set !", scope);

                    //trackedUser.Avatar = embd.GetComponent<UMI3DAvatarNode>();

                    //LoadAvatarNode(trackedUser.Avatar);

                    GameObject skeleton = Instantiate(SkeletonPrefab, embd.transform);

                    if (embodimentSize.ContainsKey(userId))
                        skeleton.transform.localScale = embodimentSize[userId];
                    else
                        UMI3DLogger.LogError("EmbodimentSize does not contain key " + userId, scope);

                    //trackedUser.Avatar.userId = userId;
                    //trackedUser.Avatar.skeletonAnimator = skeleton.GetComponentInChildren<Animator>();

                    //embodimentInstances.Add(userId, trackedUser.Avatar);
                }

                await UMI3DAsyncManager.Yield();

                //NewEmbodiment.Invoke(trackedUser.Avatar);

                UMI3DLogger.Log("EmbodimentManager.JoinDtoReception end " + userId, scope);
            }
        }

        /// <summary>
        /// Update the Embodiment from the received Dto.
        /// </summary>
        /// <param name="dto">a dto containing the tracking data</param>
        public void UserTrackingReception(UserTrackingFrameDto dto, ulong userId, float timestep)
        {
            if (ActivateEmbodiments)
            {
                if (!embodimentInstances.ContainsKey(userId))
                {
                    UMI3DLogger.LogWarning($"Internal error : the user [{userId}] is not registered", scope);
                    return;
                }

                UMI3DAvatarNode userEmbd = embodimentInstances[userId];
                userEmbd.transform.localPosition = dto.position;
                userEmbd.transform.localRotation = dto.rotation;

                userEmbd.skeletonAnimator.transform.parent.position = userEmbd.transform.position + new Vector3(0, dto.skeletonHighOffset, 0);

                UpdateNodeTransform(userEmbd);

                userEmbd.UpdateEmbodiment(dto);
            }
        }

        /// <summary>
        /// Update the camera properties of a UMI3DUser
        /// </summary>
        /// <param name="dto">a dto containing the camera properties</param>
        /// <param name="user">the concerned user</param>
        public void UserCameraReception(UserCameraPropertiesDto dto, UMI3DUser user)
        {
            if (ActivateEmbodiments)
                StartCoroutine(_UserCameraReception(dto, user));
        }

        public void UserCameraReception(uint operationKey, ByteContainer container, UMI3DUser user)
        {
            if (ActivateEmbodiments)
                StartCoroutine(_UserCameraReception(operationKey, container, user));
        }

        private IEnumerator _UserCameraReception(UserCameraPropertiesDto dto, UMI3DUser user)
        {
            while (!embodimentInstances.ContainsKey(user.Id()))
            {
                UMI3DLogger.LogWarning($"Internal error : the user [{user.Id()}] is not registered", scope);
                yield return new WaitForFixedUpdate();
            }

            UMI3DAvatarNode userEmbd = embodimentInstances[user.Id()];
            userEmbd.userCameraPropertiesDto = dto;
        }

        private IEnumerator _UserCameraReception(uint operationKey, ByteContainer container, UMI3DUser user)
        {
            while (!embodimentInstances.ContainsKey(user.Id()))
            {
                UMI3DLogger.LogWarning($"Internal error : the user [{user.Id()}] is not registered", scope);
                yield return new WaitForFixedUpdate();
            }

            UMI3DAvatarNode userEmbd = embodimentInstances[user.Id()];
            userEmbd.userCameraPropertiesDto = UMI3DSerializer.Read<UserCameraPropertiesDto>(container);
        }

        /// <summary>
        /// Request the other browsers than the user's one to trigger/interrupt the emote of the corresponding id.
        /// </summary>
        /// <param name="emoteId">Emote to trigger UMI3D id.</param>
        /// <param name="user">Sending emote user.</param>
        /// <param name="trigger">True for triggering, false to interrupt.</param>
        public void DispatchChangeEmoteReception(ulong emoteId, UMI3DUser user, bool trigger)
        {
            //? avoid the data channel filtering
            var targetUsers = new HashSet<UMI3DUser>(UMI3DServer.Instance.Users());
            targetUsers.Remove(user);
            var req = new EmoteDispatchRequest()
            {
                sendingUserId = user.Id(),
                shouldTrigger = trigger,
                emoteId = emoteId,
                users = targetUsers
            };

            req.ToTransaction(true).Dispatch();
        }

        /// <summary>
        /// Delete the User's Embodiment.
        /// </summary>
        /// <param name="user">the concerned user</param>
        protected void DeleteEmbodiment(UMI3DUser user)
        {
            if (ActivateEmbodiments)
            {
                if (!embodimentInstances.ContainsKey(user.Id()))
                {
                    UMI3DLogger.LogWarning($"Internal error : the user [{user.Id()}] is  not registered", scope);
                    return;
                }

                UMI3DAvatarNode embd = embodimentInstances[user.Id()];

                DeleteEmbodimentObj(embd);

                Destroy(embd.transform.gameObject);

                if (user?.Id() != null)
                {
                    embodimentInstances.Remove(user.Id());
                    embodimentSize.Remove(user.Id());
                }
            }
        }

        /// <summary>
        /// Load an Avatar Node with an important update
        /// </summary>
        /// <param name="node">the avatar node to load</param>
        public void LoadAvatarNode(UMI3DAbstractNode node)
        {
            node.Register();
            LoadEntity op = node.GetLoadEntity();
            var tr = new Transaction() { reliable = true };
            tr.AddIfNotNull(op);
            UMI3DServer.Dispatch(tr);
        }

        /// <summary>
        /// Remove an Avatar Node with an important update
        /// </summary>
        /// <param name="id"></param>
        protected void DeleteEmbodimentObj(UMI3DAvatarNode node)
        {
            var tr = new Transaction() { reliable = true };
            tr.AddIfNotNull(node.GetDeleteEntity());
            UMI3DServer.Dispatch(tr);
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
        #endregion

        #region Bindings

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
            var operations = new List<SetEntityProperty>();

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
            var operations = new List<SetEntityProperty>();

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
            var operations = new List<SetEntityProperty>();

            if (keepWorldPosition && newparent != null)
            {
                UMI3DBinding binding = obj.bindings.GetValue(index);

                binding.node.transform.SetParent(newparent.transform, true);
                SetEntityProperty op = binding.node.objectParentId.SetValue(newparent.GetComponent<UMI3DAbstractNode>());

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
            var operations = new List<SetEntityProperty>();

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

        #endregion

        #region Body & Hand Animation

        [HideInInspector]
        public List<ulong> handPoseIds = new List<ulong>();

        [EditorReadOnly]
        public List<UMI3DHandPose> PreloadedHandPoses = new List<UMI3DHandPose>();

        [HideInInspector]
        public List<ulong> bodyPoseIds = new List<ulong>();

        [EditorReadOnly]
        public List<UMI3DBodyPose> PreloadedBodyPoses = new List<UMI3DBodyPose>();

        #region Runtime referencing of handpose
        /// <summary>
        /// Add a new handpose at run time to subscribe
        /// </summary>
        /// <param name="Hp">A umi3D hand pose to add</param>
        public void AddAnHandPoseRef(UMI3DHandPose Hp)
        {
            if (!PreloadedHandPoses.Contains(Hp))
            {
                PreloadedHandPoses.Add(Hp);
                ReWriteHandPoseCollection();
            }
        }
        /// <summary>
        /// Remove An handpose at run time
        /// </summary>
        /// <param name="hp">A umi3D hand pose to remove</param>
        public void RemoveHandPose(UMI3DHandPose hp)
        {
            PreloadedHandPoses.Remove(hp);
            ReWriteHandPoseCollection();
        }
        /// <summary>
        /// Rewrite the collection after modifiction of the handpose
        /// </summary>
        public void ReWriteHandPoseCollection()
        {
            handPoseIds.Clear();
            handPoseIds.AddRange(PreloadedHandPoses.Select(hp => hp.Id()).ToList());
        }
        #endregion

        protected virtual void WriteCollections()
        {
            handPoseIds.Clear();
            handPoseIds.AddRange(PreloadedHandPoses.Select(hp => hp.Id()).ToList());

            bodyPoseIds.Clear();
            bodyPoseIds.AddRange(PreloadedBodyPoses.Select(bp => bp.Id()).ToList());
        }

        public virtual void WriteNodeCollections(UMI3DAvatarNodeDto avatarNodeDto, UMI3DUser user)
        {
            if (avatarNodeDto.userId.Equals(user.Id()))
            {
                avatarNodeDto.handPoses = PreloadedHandPoses.Select(hp => hp.ToDto()).ToList();
                avatarNodeDto.bodyPoses = PreloadedBodyPoses.Select(bp => bp.ToDto()).ToList();

                if (emotesConfig != null)
                    avatarNodeDto.emotesConfigDto = (UMI3DEmotesConfigDto)emotesConfig.ToEntityDto(user);
            }
        }
        #endregion

        #region BoardedVehicle

        /// <summary>
        /// <UserId, (embarkment confirmation received, vehicle)>.
        /// </summary>
        Dictionary<ulong, (bool, UMI3DAbstractNode)> Embarkments = new Dictionary<ulong, (bool, UMI3DAbstractNode)>();

        private bool setTransform = false;
        private Vector3 localPosition;
        private Quaternion localRotation;

        public void ConfirmEmbarkment(VehicleConfirmation dto, UMI3DUser user)
        {
            StartCoroutine(_ConfirmeEmbarkment(user));
        }

        public void ConfirmEmbarkment(uint operationKey, ByteContainer container, UMI3DUser user)
        {
            StartCoroutine(_ConfirmeEmbarkment(user));
        }

        private IEnumerator _ConfirmeEmbarkment(UMI3DUser user)
        {
            while (!embodimentInstances.ContainsKey(user.Id()))
            {
                UMI3DLogger.LogWarning($"Internal error : the user [{user.Id()}] is not registered", scope);
                yield return new WaitForFixedUpdate();
            }

            Embarkments[user.Id()] = (true, Embarkments[user.Id()].Item2);

            Transaction tr = new Transaction();
            //tr.AddIfNotNull((user as UMI3DTrackedUser).Avatar.objectParentId.SetValue(Embarkments[user.Id()].Item2.GetComponent<UMI3DAbstractNode>()));
            //if (setTransform)
            //{
            //    tr.AddIfNotNull((user as UMI3DTrackedUser).Avatar.objectPosition.SetValue(localPosition));
            //    tr.AddIfNotNull((user as UMI3DTrackedUser).Avatar.objectRotation.SetValue(localRotation));
            //}
            //tr.Dispatch();
        }

        public void VehicleEmbarkment(UMI3DUser user, UMI3DAbstractNode vehicle = null)
        {
            if (user == null)
                return;

            setTransform = false;

            VehicleRequest vr;

            if (vehicle != null)
            {
                Embarkments[user.Id()] = (false, vehicle);

                if (vehicle != EmbodimentsScene)
                    vr = new VehicleRequest(vehicle.Id());
                else
                    vr = new VehicleRequest(EmbodimentsScene.Id());
            }
            else
            {
                Embarkments[user.Id()] = (false, EmbodimentsScene);

                vr = new VehicleRequest(EmbodimentsScene.Id());
            }

            vr.users = new HashSet<UMI3DUser>() { user };

            vr.ToTransaction(true).Dispatch();
        }

        public void VehicleEmbarkment(UMI3DUser user, ulong bodyAnimationId = 0, bool changeBonesToStream = false, List<uint> bonesToStream = null, UMI3DAbstractNode vehicle = null, bool stopNavigation = false, Vector3 position = new Vector3(), Quaternion rotation = new Quaternion())
        {
            if (user == null)
                return;

            BoardedVehicleRequest vr;

            if (vehicle != null)
            {
                Embarkments[user.Id()] = (false, vehicle);

                if (vehicle != EmbodimentsScene)
                    vr = new BoardedVehicleRequest(bodyAnimationId, changeBonesToStream, bonesToStream, vehicle.Id(), stopNavigation, position, rotation);
                else
                    vr = new BoardedVehicleRequest(bodyAnimationId, changeBonesToStream, bonesToStream, EmbodimentsScene.Id(), stopNavigation, position, rotation);
            }
            else
            {
                Embarkments[user.Id()] = (false, EmbodimentsScene);

                vr = new BoardedVehicleRequest(bodyAnimationId, changeBonesToStream, bonesToStream, EmbodimentsScene.Id(), stopNavigation, position, rotation);
            }

            localPosition = position;
            localRotation = rotation;
            setTransform = true;

            vr.users = new HashSet<UMI3DUser>() { user };

            vr.ToTransaction(true).Dispatch();
        }

        #endregion
    }
}