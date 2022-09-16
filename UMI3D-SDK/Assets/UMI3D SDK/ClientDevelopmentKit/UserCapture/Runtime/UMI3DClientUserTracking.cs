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
using umi3d.common;
using umi3d.common.userCapture;
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.cdk.userCapture
{
    /// <summary>
    /// Manager for client user tracking related events
    /// </summary>
    public class UMI3DClientUserTracking : SingleBehaviour<UMI3DClientUserTracking>
    {
        /// <summary>
        /// Transform of the gameobject containing the user's skeleton.
        /// </summary>
        [Tooltip("Transform of the gameobject containing the user's skeleton.")]
        public Transform skeletonContainer;
        /// <summary>
        /// Transform associated with the viewpoint of the user.
        /// </summary>
        [Tooltip("Transform associated with the viewpoint of the user")]
        public Transform viewpoint;
        /// <summary>
        /// <see cref="BoneType"/> associated with the viewpoint of the user.
        [Tooltip("Bone associated with the viewpoint of the user.")]
        [ConstEnum(typeof(BoneType), typeof(uint))]
        public uint viewpointBonetype;

        /// <summary>
        /// True when the user's tracking has started.
        /// </summary>
        public bool trackingReception { get; protected set; }

        /// <summary>
        /// If true the avatar tracking is sent.
        /// </summary>
        public bool SendTracking => sendTracking;

        /// <summary>
        /// If true the avatar tracking is sent.
        /// </summary>
        [Tooltip("If true the avatar tracking is sent.")]
        protected bool sendTracking = true;

        /// <summary>
        /// Frequency indicating the number tracked frames send to the server per seconds.
        /// </summary>
        [field: SerializeField, Tooltip(" Frequency indicating the number tracked frames send to the server per seconds.")]
        public float targetTrackingFPS { get; protected set; } = 15;

        /// <summary>
        /// Collection of tracked bones by their BoneType id.
        /// </summary>
        private List<uint> streamedBonetypes = new List<uint>();
        /// <summary>
        /// Collection of instanciated <see cref="UserAvatar"/> by the user's id.
        /// </summary>
        /// This also includes avatars form other users in the environment.
        [ReadOnly, Tooltip("Collection of instanciated avatar  with their user's id.")]
        public Dictionary<ulong, UserAvatar> embodimentDict = new Dictionary<ulong, UserAvatar>();

        /// <summary>
        /// This event is raised after each analysis of the skeleton.
        /// </summary>
        [HideInInspector]
        [Tooltip("This event is raised after each analysis of the skeleton.")]
        public UnityEvent skeletonParsedEvent;

        /// <summary>
        /// This event has to be raised to send a CameraPropertiesDto. By default, it is raised at the beginning of Play Mode.
        /// </summary>
        [HideInInspector]
        [Tooltip("This event has to be raised to send a CameraPropertiesDto. By default, it is raised at the beginning of Play Mode.")]
        public UnityEvent sendingCameraProperties;

        /// <summary>
        /// This event has to be raised to start sending tracking data. The sending will stop if the Boolean \"sendTracking\" is false. By default, it is raised at the beginning of Play Mode.
        /// </summary>
        [HideInInspector]
        [Tooltip("This event has to be raised to start sending tracking data. The sending will stop if the Boolean \"sendTracking\" is false. By default, it is raised at the beginning of Play Mode.")]
        public UnityEvent startingSendingTracking;

        /// <summary>
        /// If true, always send tracking frames (according to \"targetTrackingFPS\"), otherwise, frames will be sent only if user has moved
        /// </summary>
        [Tooltip("If true, always send tracking frames (according to \"targetTrackingFPS\"), otherwise, frames will be sent only if user has moved.")]
        public bool alwaysSendTrackingFrame = false;

        /// <summary>
        /// Position delta to consider user has moved.
        /// </summary>
        [SerializeField]
        [Tooltip("Position delta to consider user has moved.")]
        private float detectionPositionDelta = .1f;

        /// <summary>
        /// Rotation delta (degrees) to consider user has moved.
        /// </summary>
        [SerializeField]
        [Tooltip("Rotation delta (degrees) to consider user has moved.")]
        private float detectionRotationDelta = 5f;

        /// <summary>
        /// Maximum time (in seconds) between two tracking frames sent, even if player has no moved.
        /// </summary>
        [SerializeField]
        [Tooltip("Maximum time (in seconds) between two tracking frames sent, even if player has no moved")]
        private float maximumTimeBetweenFramesSent = 5;

        private float lastTimeFrameSent = 0;


        public class HandPoseEvent : UnityEvent<UMI3DHandPoseDto> { };
        public class BodyPoseEvent : UnityEvent<UMI3DBodyPoseDto> { };

        public HandPoseEvent handPoseEvent = new HandPoseEvent();

        public BodyPoseEvent bodyPoseEvent = new BodyPoseEvent();

        public class EmotesConfigEvent : UnityEvent<UMI3DEmotesConfigDto> { };
        public class EmoteEvent : UnityEvent<UMI3DEmoteDto> { };

        /// <summary>
        /// Triggered when an EmoteConfig file have been loaded
        /// </summary>
        public EmotesConfigEvent EmotesLoadedEvent = new EmotesConfigEvent();
        /// <summary>
        /// Trigered when an emote changed on availability
        /// </summary>
        public EmoteEvent EmoteChangedEvent = new EmoteEvent();
        /// <summary>
        /// Trigered when an emote is played by the user
        /// </summary>
        public UnityEvent EmotePlayedSelfEvent = new UnityEvent();
        /// <summary>
        /// Trigered when an emote is finished to be played by the user
        /// </summary>
        public UnityEvent EmoteEndedSelfEvent = new UnityEvent();

        /// <summary>
        /// True when an emote is currently playing
        /// </summary>
        public bool IsEmotePlaying { get; protected set; } = false;

        /// <summary>
        /// Emote configuration on the server, with al available emotes for the user.
        /// </summary>
        private UMI3DEmotesConfigDto emoteConfig;

        /// <summary>
        /// Starts an emote on another user's avatar in the scene.
        /// </summary>
        /// <param name="emoteId">Emote to start UMI3D Id.</param>
        /// <param name="userId">Id of the user to start the emote for.</param>
        /// Don't use this for your own avatar's emotes.
        public void PlayEmoteOnOtherAvatar(ulong emoteId, ulong userId)
        {
            var otherUserAvatar = embodimentDict[userId];
            var animators = otherUserAvatar.GetComponentsInChildren<Animator>();
            var emoteAnimator = animators.Where(animator => animator.runtimeAnimatorController != null).FirstOrDefault();

            if (emoteAnimator == null || emoteConfig == null)
                return;
            var emoteToPlay = emoteConfig.emotes.Find(x => x.id == emoteId);
            StartCoroutine(PlayEmote(emoteAnimator, emoteToPlay));
        }

        private const string IdleStateName = "Idle";
        /// <summary>
        /// Plays an emote on an animator.
        /// </summary>
        /// <param name="animator">Animator to play the emote on.</param>
        /// <param name="emote">Emote to play.</param>
        /// <returns></returns>
        protected IEnumerator PlayEmote(Animator animator, UMI3DEmoteDto emote)
        {
            animator.enabled = true;
            animator.Update(0);
            animator.Play(emote.stateName);
            yield return new WaitWhile(() =>
            {
                if (animator == null) return false;  // heppens when a user leaves the scene when playing an emote
                return animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1;
            });
            if (animator == null) // heppens when a user leaves the scene when playing an emote
                yield break;
            animator.Play(IdleStateName);
            animator.Update(0);
            animator.enabled = false;
        }

        /// <summary>
        /// Stops an emote on another user's avatar in the scene.
        /// </summary>
        /// <param name="emoteId">Emote to stop UMI3D Id.</param>
        /// <param name="userId">Id of the user to stop the emote for.</param>
        /// Don't use this for your own avatar's emotes.
        public void StopEmoteOnOtherAvatar(ulong emoteId, ulong userId)
        {
            var otherUserAvatar = embodimentDict[userId];
            var animators = otherUserAvatar.GetComponentsInChildren<Animator>();
            var emoteAnimator = animators.Where(animator => animator.runtimeAnimatorController != null).FirstOrDefault();

            if (emoteAnimator == null || emoteConfig == null)
                return;
            var emoteToStop = emoteConfig.emotes.Find(x => x.id == emoteId);
            StopCoroutine(PlayEmote(emoteAnimator, emoteToStop));
            emoteAnimator.Play(IdleStateName);
            emoteAnimator.Update(0);
            emoteAnimator.enabled = false;
        }

        public class AvatarEvent : UnityEvent<ulong> { };

        public AvatarEvent avatarEvent = new AvatarEvent();

        protected UserTrackingFrameDto LastFrameDto = new UserTrackingFrameDto();
        protected UserCameraPropertiesDto CameraPropertiesDto = null;
        /// <summary>
        /// Should the client send <see cref="UserCameraPropertiesDto"/>?
        /// </summary>
        protected bool sendCameraProperties = false;

        #region User data

        /// <summary>
        /// Store last user position.
        /// </summary>
        private Vector3 lastPosition;

        /// <summary>
        /// Stores last user rotation
        /// </summary>
        private Quaternion lastRotation;

        /// <summary>
        /// Store last rotations for every bone.
        /// </summary>
        private Dictionary<uint, Quaternion> lastBoneRotations = new Dictionary<uint, Quaternion>();
        /// <summary>
        /// If true, disable the sending of bones rotations in frame dtos.
        /// </summary>
        /// Frame dtos withouth bones are much lighter.
        public bool IgnoreBones { get; protected set; } = false;

        #endregion

        /// <inheritdoc/>
        protected override void Awake()
        {
            base.Awake();
            skeletonParsedEvent = new UnityEvent();

        }

        protected virtual void Start()
        {
            streamedBonetypes = UMI3DClientUserTrackingBone.instances.Keys.ToList();
            sendingCameraProperties.AddListener(() => StartCoroutine(DispatchCamera()));
            startingSendingTracking.AddListener(() => { if (sendTracking) StartCoroutine(DispatchTracking()); });
            UMI3DEnvironmentLoader.Instance.onEnvironmentLoaded.AddListener(() => StartCoroutine(DispatchCamera()));
            UMI3DEnvironmentLoader.Instance.onEnvironmentLoaded.AddListener(() => { if (sendTracking) StartCoroutine(DispatchTracking()); });
            UMI3DEnvironmentLoader.Instance.onEnvironmentLoaded.AddListener(() => trackingReception = true);
            EmotesLoadedEvent.AddListener((UMI3DEmotesConfigDto dto) => { emoteConfig = dto; });
            EmotePlayedSelfEvent.AddListener(delegate
            {
                IgnoreBones = true;
                IsEmotePlaying = true;
            });
            EmoteEndedSelfEvent.AddListener(delegate
            {
                IgnoreBones = false;
                IsEmotePlaying = false;
            });
        }

        /// <summary>
        /// Dispatch User Tracking data through Tracking Channel
        /// </summary>
        protected virtual IEnumerator DispatchTracking()
        {
            while (sendTracking)
            {
                if (targetTrackingFPS > 0)
                {
                    BonesIterator();

                    if (UMI3DClientServer.Exists && UMI3DClientServer.Instance.GetUserId() != 0 && LastFrameDto != null)
                        UMI3DClientServer.SendTracking(LastFrameDto);

                    if (sendCameraProperties)
                        sendingCameraProperties.Invoke();

                    yield return new WaitForSeconds(1f / targetTrackingFPS);
                }
                else
                {
                    yield return new WaitUntil(() => targetTrackingFPS > 0 || !sendTracking);
                }
            }
        }

        /// <summary>
        /// Dispatch Camera data through Tracking Channel
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator DispatchCamera()
        {
            while (UMI3DClientServer.Instance.GetUserId() == 0)
            {
                yield return null;
            }

            var newCameraProperties = new UserCameraPropertiesDto()
            {
                scale = 1f,
                projectionMatrix = viewpoint.TryGetComponent(out Camera camera) ? camera.projectionMatrix : new Matrix4x4(),
                boneType = viewpointBonetype,
            };

            if (!newCameraProperties.Equals(CameraPropertiesDto))
            {
                UMI3DClientServer.SendData(newCameraProperties, true);
                CameraPropertiesDto = newCameraProperties;
            }
        }



        /// <summary>
        /// Iterate through the bones of the browser's skeleton to create BoneDto
        /// </summary>
        /// <param name="forceNotNullDto">If true, <see cref="LastFrameDto"/> won't be null.</param>
        protected void BonesIterator(bool forceNotNullDto = false)
        {
            if (UMI3DEnvironmentLoader.Exists)
            {
                var bonesList = new List<BoneDto>();
                if (!IgnoreBones)
                {
                    foreach (UMI3DClientUserTrackingBone bone in UMI3DClientUserTrackingBone.instances.Values)
                    {
                        if (streamedBonetypes.Contains(bone.boneType))
                        {
                            BoneDto dto = bone.ToDto();
                            if (dto != null)
                                bonesList.Add(dto);
                        }
                    }
                }

                Vector3 position = UMI3DNavigation.Instance.transform.localPosition;
                Quaternion rotation = UMI3DNavigation.Instance.transform.localRotation;

                if (!HasPlayerMoved(position, rotation, bonesList) && !forceNotNullDto && (Time.realtimeSinceStartup < lastTimeFrameSent + maximumTimeBetweenFramesSent))
                {
                    LastFrameDto = null;
                }
                else
                {
                    lastTimeFrameSent = Time.realtimeSinceStartup;

                    LastFrameDto = new UserTrackingFrameDto()
                    {
                        bones = bonesList,
                        skeletonHighOffset = skeletonContainer.localPosition.y,
                        position = position, //position relative to UMI3DEnvironmentLoader node
                        rotation = rotation, //rotation relative to UMI3DEnvironmentLoader node
                        refreshFrequency = targetTrackingFPS,
                        userId = UMI3DClientServer.Instance.GetUserId(),
                    };
                }

                skeletonParsedEvent.Invoke();
            }
        }

        /// <summary>
        /// Defines if user has moved since last time.
        /// </summary>
        /// <param name="position">User position</param>
        /// <param name="rotation">User rotation</param>
        /// <param name="bones">List of all bones</param>
        /// <returns></returns>
        protected bool HasPlayerMoved(Vector3 position, Quaternion rotation, List<BoneDto> bones)
        {
            if (alwaysSendTrackingFrame)
                return true;

            bool hasMoved = false;

            if ((Vector3.Distance(position, lastPosition) > detectionPositionDelta) || (Quaternion.Angle(lastRotation, rotation) > detectionRotationDelta))
            {
                hasMoved = true;
                lastRotation = rotation;
                lastPosition = position;
            }
            else
            {
                foreach (BoneDto bone in bones)
                {
                    if (lastBoneRotations.ContainsKey(bone.boneType))
                    {
                        if (Quaternion.Angle(bone.rotation, lastBoneRotations[bone.boneType]) > 5)
                        {
                            hasMoved = true;
                            lastBoneRotations[bone.boneType] = bone.rotation;
                        }
                    }
                    else
                    {
                        hasMoved = true;
                        lastBoneRotations[bone.boneType] = bone.rotation;
                    }
                }
            }

            return hasMoved;
        }

        /// <summary>
        /// Register the UserAvatar instance of a user in the concerned dictionary
        /// </summary>
        /// <param name="id">the id of the user</param>
        /// <param name="u">the UserAvatar instance to register</param>
        /// <returns>A bool indicating if the UserAvatar has been registered</returns>
        public virtual bool RegisterEmbd(ulong id, UserAvatar u)
        {
            if (embodimentDict.ContainsKey(id))
            {
                avatarEvent.Invoke(id);
                return false;
            }
            else
            {
                embodimentDict.Add(id, u);
                avatarEvent.Invoke(id);
                return true;
            }
        }

        /// <summary>
        /// Unregister the UserAvatar instance of a user in the concerned dictionary
        /// </summary>
        /// <param name="id">the id of the user</param>
        /// <returns>A bool indicating if the UserAvatar has been unregistered</returns>
        public virtual bool UnregisterEmbd(ulong id)
        {
            return embodimentDict.Remove(id);
        }

        /// <summary>
        /// Try to get the UserAvatar instance of a user from the concerned dictionary
        /// </summary>
        /// <param name="id">the id of the user</param>
        /// <param name="embd">the UserAvatar instance if found</param>
        /// <returns>A bool indicating if the UserAvatar has been found</returns>
        public virtual bool TryGetValue(ulong id, out UserAvatar embd)
        {
            return embodimentDict.TryGetValue(id, out embd);
        }

        /// <summary>
        /// Set the number of tracked frame per second that are sent to the server.
        /// </summary>
        /// <param name="newFPSTarget"></param>
        public void SetFPSTarget(int newFPSTarget)
        {
            if (newFPSTarget > 0)
            {
                targetTrackingFPS = newFPSTarget;
            }
            else
            {
                UMI3DLogger.LogError("Tracking frame Fps must be greater than 0.", DebugScope.CDK);
            }
        }

        /// <summary>
        /// Set the list of streamed bones.
        /// </summary>
        /// <param name="bonesToStream"></param>
        public void SetStreamedBones(List<uint> bonesToStream)
        {
            this.streamedBonetypes = bonesToStream;
        }

        /// <summary>
        /// Setter for <see cref="sendCameraProperties"/>.
        /// </summary>
        /// <param name="activeSending"></param>
        public void SetCameraPropertiesSending(bool activeSending)
        {
            this.sendCameraProperties = activeSending;
        }

        /// <summary>
        /// Setter for <see cref="sendTracking"/>.
        /// </summary>
        /// <param name="activeSending"></param>
        public void SetTrackingSending(bool activeSending)
        {
            this.sendTracking = activeSending;
            startingSendingTracking.Invoke();
        }

        /// <summary>
        /// Make a user board in in a vehicle that supports boarded users.
        /// </summary>
        /// <param name="vehicleDto"></param>
        public void EmbarkVehicle(BoardedVehicleDto vehicleDto)
        {
            if (vehicleDto.BodyAnimationId != 0)
            {
                var anim = UMI3DNodeAnimation.Get(vehicleDto.BodyAnimationId);
                if (anim != null)
                    anim.Start();
            }

            var bones = UMI3DClientUserTrackingBone.instances.Keys.ToList();

            bones.RemoveAll(item => vehicleDto.BonesToStream.Contains(item));

            SetStreamedBones(bones);
        }
    }
}