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
using UnityEngine;
using umi3d.common;

namespace umi3d.edk
{
    public class UMI3DAvatar : MonoBehaviour
    {
        /// <summary>
        /// Contain the GameObjects associated with the BoneTypes.
        /// </summary>
        /// <see cref="instanciatedBones"/>
        public Dictionary<BoneType, GameObject> listOfPrefabs;

        /// <summary>
        /// Contain the BoneTypes not to use for self-representation.
        /// </summary>
        public List<BoneType> bonesToFilter;

        /// <summary>
        /// The user represented by the avatar.
        /// </summary>
        public UMI3DUser user;

        /// <summary>
        /// The user's viewpoint.
        /// </summary>
        public GameObject viewpoint;

        /// <summary>
        /// The default representation of the UMI3D environment.
        /// </summary>
        public GameObject defaultAvatar;

        public bool defaultDisplay = false;

        /// <summary>
        /// The display mode of the UMI3D environment.
        /// </summary>
        public AvatarDisplayMode displayMode;

        /// <summary>
        /// The avatar's anchor.
        /// </summary>
        public GameObject anchor;

        private AvatarDto avatarDto;

        private void Start()
        {
            CVEAvatar cveavt = anchor.AddComponent<CVEAvatar>();
            anchor.AddComponent<EmptyObject3D>();
            cveavt.UserId = user.UserId;
            UMI3DAvatarBone.instancesByUserId.Add(user.UserId, new Dictionary<string, UMI3DAvatarBone>());
        }

        /// <summary>
        /// Update the user's avatar according to the display mode and the received informations.
        /// </summary>
        public void UpdateAvatar(NavigationRequestDto navigation)
        {
            updateGobalPosition(navigation);

            if (this.displayMode == AvatarDisplayMode.DynamicDisplay)
            {
                this.RenewAppearance();
            }
            else if (this.displayMode == AvatarDisplayMode.DefaultDisplay && !this.defaultDisplay)
            {
                foreach (Transform child in this.anchor.transform)
                {
                    Destroy(child.gameObject);
                }

                if (this.defaultAvatar != null)
                {
                    Instantiate(this.defaultAvatar, this.anchor.transform);
                }

                this.defaultDisplay = !this.defaultDisplay;

                this.anchor.transform.localScale = new Vector3(
                    1 / avatarDto.ScaleScene.X,
                    1 / avatarDto.ScaleScene.Y,
                    1 / avatarDto.ScaleScene.Z);
            }
            else if (this.displayMode == AvatarDisplayMode.APIDisplay)
            {
                // Connect an API
            }
        }

        /// <summary>
        /// Update the position and rotation of the avatar in the scene from a NavigationRequestDto.
        /// </summary>
        private void updateGobalPosition(NavigationRequestDto navigation)
        {
            var parent = transform.parent;
            transform.SetParent(UMI3D.Scene.transform);
            transform.localPosition = navigation.TrackingZonePosition;
            transform.localRotation = navigation.TrackingZoneRotation;
            transform.SetParent(parent);

            viewpoint.transform.SetParent(transform);

            avatarDto = navigation.Avatar;

            Vector3 position = new Vector3(
                navigation.CameraPosition.X / avatarDto.ScaleScene.X,
                navigation.CameraPosition.Y / avatarDto.ScaleScene.Y,
                navigation.CameraPosition.Z / avatarDto.ScaleScene.Z);

            viewpoint.transform.position = position;
            viewpoint.transform.localRotation = navigation.CameraRotation;

            anchor.transform.position = viewpoint.transform.position;
            anchor.transform.rotation = viewpoint.transform.rotation;

            GetComponent<GenericObject3D>().PropertiesHandler.NotifyUpdate();
        }

        /// <summary>
        /// Update the dynamic appearance of the user's avatar by destroying, generating, or updating bones from an AvatarDto.
        /// </summary>
        private void RenewAppearance()
        {
            if (this.defaultDisplay)
            {
                Destroy(this.anchor.transform.GetChild(0));
                defaultDisplay = !defaultDisplay;
            }

            List<BoneDto> newBoneListFromBrowser = avatarDto.boneList;

            List<string> bonesToDelete = new List<string>();
            List<string> bonesToUpdate = new List<string>();
            List<string> bonesToCreate = new List<string>();

            UMI3DAvatarBone.instancesByUserId.TryGetValue(user.UserId, out Dictionary<string, UMI3DAvatarBone> oldUserSkeleton);

            List<UMI3DAvatarBone> oldSkeleton = new List<UMI3DAvatarBone>(oldUserSkeleton.Values);

            bonesToDelete = oldSkeleton
                .ConvertAll<string>(avatarBone => avatarBone.boneId)
                .FindAll(oldBoneId =>
                    !newBoneListFromBrowser.Exists(newBone => newBone.Id.Equals(oldBoneId)));

            bonesToUpdate = oldSkeleton
                .ConvertAll<string>(avatarBone => avatarBone.boneId)
                .FindAll(oldBoneId =>
                    newBoneListFromBrowser.Exists(newBone => newBone.Id.Equals(oldBoneId)));

            bonesToCreate = newBoneListFromBrowser.FindAll(newBoneDto => 
                !oldSkeleton.Exists(oldBone => 
                    oldBone.boneId.Equals(newBoneDto.Id)))            
                .ConvertAll<string>(bone => bone.Id);


            foreach (string boneId in bonesToDelete)
            {
                Destroy(UMI3D.Scene.GetObject(boneId).gameObject);
                UMI3DAvatarBone umi3DAvatarBone = oldSkeleton.Find(avatarBone => avatarBone.boneId.Equals(boneId));
                umi3DAvatarBone.UnRegister();
            }

            foreach (string boneId in bonesToUpdate)
            {
                UMI3DAvatarBone umi3DAvatarBone = UMI3DAvatarBone.instancesByUserId[user.UserId][boneId];
                UpdateBone(UMI3D.Scene.GetObject(umi3DAvatarBone.boneAnchorId).gameObject, avatarDto.boneList.Find(boneDto => boneDto.Id.Equals(boneId)));
            }


            foreach (string boneId in bonesToCreate)
            {
                UMI3DAvatarBone umi3DAvatarBone = InstanciateBone(avatarDto.boneList.Find(boneDto => boneDto.Id.Equals(boneId)));
                umi3DAvatarBone.Register(); 
            }


            if (bonesToCreate.Count != 0 || bonesToDelete.Count != 0)
            {
                AvatarMappingDto avatarMappingDto = anchor.GetComponent<CVEAvatar>().ToDto(user);
                user.Send(avatarMappingDto);
            }

            this.anchor.transform.localScale = new Vector3(1 / avatarDto.ScaleScene.X, 1 / avatarDto.ScaleScene.Y, 1 / avatarDto.ScaleScene.Z);
        }

        /// <summary>
        /// Return a list containing Id and BoneType pairs for each visible avatar parts.
        /// </summary>
        public BonePairDictionary setUserMapping()
        {
            BonePairDictionary BoneDictionary = new BonePairDictionary();

            if (UMI3DAvatarBone.instancesByUserId.TryGetValue(user.UserId, out Dictionary<string, UMI3DAvatarBone> userSkeleton))
            {
                foreach (KeyValuePair<string, UMI3DAvatarBone> bone in userSkeleton)
                {
                    foreach (string objectId in bone.Value.meshes)
                    {
                        BoneObjectPair pair = new BoneObjectPair();
                        pair.boneId = bone.Value.boneId;
                        pair.objectId = objectId;
                        BoneDictionary.TryAdd(pair);
                    }
                }
            }
            return BoneDictionary;
        }

        /// <summary>
        /// Define if an avatar part is visible or not for self representation.
        /// </summary>
        private void FilterBones(GameObject bone, BoneDto boneDto)
        {
            if (bonesToFilter.Contains(boneDto.type) && (bone.GetComponent<AvatarFilter>() == null))
            {
                bone.AddComponent<AvatarFilter>();
            }
            else if (!bonesToFilter.Contains(boneDto.type) && (bone.GetComponent<AvatarFilter>() != null))
            {
                Destroy(bone.GetComponent<AvatarFilter>());
            }
        }

        /// <summary>
        /// Instanciate a new bone with the parameters of a BoneDto.
        /// </summary>
        private UMI3DAvatarBone InstanciateBone(BoneDto bone)
        {
            GameObject Go = null;
            if (bone.type != BoneType.None)
            {
                List<string> meshesIds = new List<string>();
                GameObject Prefab;
                if (listOfPrefabs.TryGetValue(bone.type, out Prefab))
                {
                    Go = Instantiate(Prefab, anchor.transform);
                    foreach (CVEAvatarPart item in Go.GetComponentsInChildren<CVEAvatarPart>())
                    {
                        item.UserId = user.UserId;
                        meshesIds.Add(item.Id);
                    }
                }
                else
                {
                    Go = new GameObject(bone.type.ToString());
                    Go.AddComponent<EmptyObject3D>();
                    Go.transform.parent = anchor.transform;
                }
                UMI3DAvatarBone umi3DAvatarBone = new UMI3DAvatarBone(user.UserId, bone.Id) 
                { 
                    meshes = meshesIds.ToArray(), 
                };
                GenericObject3D GoComp = Go.GetComponent<GenericObject3D>();
                if (GoComp)
                {
                    umi3DAvatarBone.boneAnchorId = GoComp.Id;
                }
                //umi3DAvatarBone.Register();
                UpdateBone(Go, bone);
                return umi3DAvatarBone;
            }
            return null;
        }

        /// <summary>
        /// Update a bone with the parameters of a BoneDto.
        /// </summary>
        private void UpdateBone(GameObject go, BoneDto boneDto)
        {
            go.transform.localPosition = boneDto.Position;
            go.transform.localRotation = boneDto.Rotation;
            go.transform.localScale = boneDto.Scale;
            FilterBones(go, boneDto);
        }
    }
}