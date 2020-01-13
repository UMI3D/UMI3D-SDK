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
        /// Collection of instantiated bones for each bonetype (if any).
        /// </summary>
        /// <see cref="listOfPrefabs"/>
        public Dictionary<BoneType, GameObject> instanciatedBones = new Dictionary<BoneType, GameObject>();


        /// <summary>
        /// Contain the BoneTypes not to use for self-representation.
        /// </summary>
        public List<BoneType> BonesToFilter;

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

        public bool DefaultDisplay = false;

        /// <summary>
        /// The display mode of the UMI3D environment.
        /// </summary>
        public AvatarDisplayMode DisplayMode;

        /// <summary>
        /// The avatar's anchor.
        /// </summary>
        public GameObject Anchor;

        private void Start()
        {
            CVEAvatar cveavt = Anchor.AddComponent<CVEAvatar>();
            cveavt.UserId = user.UserId;
        }

        /// <summary>
        /// Update the user's avatar according to the display mode and the received informations.
        /// </summary>
        public void UpdateAvatar(NavigationRequestDto navigation)
        {
            updateGobalPosition(navigation);

            AvatarDto avatar = navigation.Avatar;

            if (this.DisplayMode == AvatarDisplayMode.DynamicDisplay)
            {
                this.renewAppearance(avatar);
            }
            else if (this.DisplayMode == AvatarDisplayMode.DefaultDisplay && !this.DefaultDisplay)
            {  
                foreach (Transform child in this.Anchor.transform)
                {
                    Destroy(child.gameObject);
                }

                if (this.defaultAvatar != null)
                {
                   Instantiate(this.defaultAvatar, this.Anchor.transform);
                }                  

                this.DefaultDisplay = !this.DefaultDisplay;

                this.Anchor.transform.localScale = new Vector3(
                    1 / avatar.ScaleScene.X, 
                    1 / avatar.ScaleScene.Y, 
                    1 / avatar.ScaleScene.Z);
            }
            else if (this.DisplayMode == AvatarDisplayMode.APIDisplay)
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

            AvatarDto avatar = navigation.Avatar;
            Vector3 position = new Vector3(
                navigation.CameraPosition.X / avatar.ScaleScene.X,
                navigation.CameraPosition.Y / avatar.ScaleScene.Y,
                navigation.CameraPosition.Z / avatar.ScaleScene.Z);

            viewpoint.transform.position = position;
            viewpoint.transform.localRotation = navigation.CameraRotation;

            Anchor.transform.position = viewpoint.transform.position;
            Anchor.transform.rotation = viewpoint.transform.rotation;

            GetComponent<GenericObject3D>().PropertiesHandler.NotifyUpdate();
        }

        /// <summary>
        /// Update the dynamic appearance of the user's avatar by destroying, generating, or updating bones from an AvatarDto.
        /// </summary>
        private void renewAppearance(AvatarDto avatar)
        {
            if (this.DefaultDisplay)
            {
                Destroy(this.Anchor.transform.GetChild(0));
                DefaultDisplay = !DefaultDisplay;
            }

            List<BoneDto> BonesList = avatar.BoneList;
            int n = BonesList.Count;
            int childCount = Anchor.transform.childCount;

            if (n == childCount)
            {
                updateBones(BonesList, n);
            }
            else
            if (n > childCount)
            {
                updateBones(BonesList, childCount);
                generateBones(BonesList, childCount);
            }
            else
            {
                updateBones(BonesList, n);
                destroyBones(n);
            }

            this.Anchor.transform.localScale = new Vector3(1 / avatar.ScaleScene.X, 1 / avatar.ScaleScene.Y, 1 / avatar.ScaleScene.Z);
        }

        /// <summary>
        /// Return a list containing Id and BoneType pairs for each visible avatar parts.
        /// </summary>
        public BonePairDictionary setUserMapping()
        {
            BonePairDictionary BoneDictionary = new BonePairDictionary();

            List<UMI3DBoneType> Children = new List<UMI3DBoneType>();
            Anchor.transform.GetComponentsInChildren<UMI3DBoneType>(Children);

            foreach (UMI3DBoneType child in Children)
            {
                if (child.transform.childCount == 1)
                {
                    if (child.GetComponent<UMI3DBoneType>() != null && child.GetComponent<UMI3DBoneType>().BoneType != BoneType.None && !BonesToFilter.Contains(child.GetComponent<UMI3DBoneType>().BoneType) && (child.GetComponentInChildren<CVEModel>() != null || child.GetComponentInChildren<CVEPrimitive>() != null))
                    {
                        BoneObjectPair pair = new BoneObjectPair();
                        pair.boneType = child.GetComponent<UMI3DBoneType>().BoneType;
                        if (child.GetComponentInChildren<CVEModel>() != null)
                            pair.objectId = child.GetComponentInChildren<CVEModel>().Id;
                        else
                            pair.objectId = child.GetComponentInChildren<CVEPrimitive>().Id;
                        BoneDictionary.TryAdd(pair);
                    }
                }
                else
                {
                    if (child.transform.childCount != 0)
                    {
                        for (int i = 0; i < child.transform.childCount; i++)
                        {
                            if (child.GetComponent<UMI3DBoneType>() != null && child.GetComponent<UMI3DBoneType>().BoneType != BoneType.None && !BonesToFilter.Contains(child.GetComponent<UMI3DBoneType>().BoneType) && (child.transform.GetChild(i).GetComponentInChildren<CVEModel>() != null || child.transform.GetChild(i).GetComponentInChildren<CVEPrimitive>() != null))
                            {
                                BoneObjectPair pair = new BoneObjectPair();
                                pair.boneType = child.GetComponent<UMI3DBoneType>().BoneType;
                                if (child.transform.GetChild(i).GetComponentInChildren<CVEModel>() != null)
                                    pair.objectId = child.transform.GetChild(i).GetComponentInChildren<CVEModel>().Id;
                                else
                                    pair.objectId = child.transform.GetChild(i).GetComponentInChildren<CVEPrimitive>().Id;
                                BoneDictionary.Add(pair);
                            }
                        }
                    }
                }
            }
            return BoneDictionary;
        }

        /// <summary>
        /// Update the first n bones in the children hierarchy with a list of BoneDto.
        /// </summary>
        private void updateBones(List<BoneDto> bonesList, int n)
        {
            for (int i = 0; i < n; i++)
            {
                if (!checkExistingBone(Anchor.transform.GetChild(i), bonesList[i]))
                {
                    if (Anchor.transform.GetChild(i).GetComponent<UMI3DBoneType>().BoneType == bonesList[i].type)
                    {
                        updateBone(Anchor.transform.GetChild(i).gameObject, bonesList[i]);
                    }
                    else
                    {
                        GameObject NewBone = instanciateBone(bonesList[i]);

                        BoneType boneTypeToRemove;
                        if (Enum.TryParse(Anchor.transform.GetChild(i).gameObject.tag, out boneTypeToRemove))
                        {
                            instanciatedBones.Remove(boneTypeToRemove);
                        }
                        else
                        {
                            throw new System.Exception("Internal Error : Unknown bone type !");
                        }
                        DestroyImmediate(Anchor.transform.GetChild(i).gameObject);
                        NewBone.transform.SetSiblingIndex(i);
                        filterBones(NewBone);
                    }                    
                }
            }
        }

        /// <summary>
        /// Define if an avatar part is visible or not for self representation.
        /// </summary>
        private void filterBones(GameObject bone)
        {
            if (BonesToFilter.Contains(bone.GetComponent<UMI3DBoneType>().BoneType) && (bone.GetComponent<AvatarFilter>() == null))
            {
                bone.AddComponent<AvatarFilter>();
            } 
            else if (!BonesToFilter.Contains(bone.GetComponent<UMI3DBoneType>().BoneType) && (bone.GetComponent<AvatarFilter>() != null))
            {
                Destroy(bone.GetComponent<AvatarFilter>());
            }
        }

        /// <summary>
        /// Instanciate new bones from each BoneDto in a list after a certain index.
        /// </summary>
        private void generateBones(List<BoneDto> BonesList, int index)
        {
            for (int i = index; i < BonesList.Count; i++)
            {
                GameObject NewBone = instanciateBone(BonesList[i]);
            }
        }

        /// <summary>
        /// Destroy bones in the children hierarchy after a certain index.
        /// </summary>
        private void destroyBones(int index)
        {
            List<Transform> Children = new List<Transform>();
            Anchor.transform.GetComponentsInChildren<Transform>(Children);
            for (int i = index; i < Children.Count; i++)
            {
                BoneType boneTypeToRemove;
                if (Enum.TryParse(Children[i].gameObject.tag, out boneTypeToRemove))
                {
                    instanciatedBones.Remove(boneTypeToRemove);
                }
                else
                {
                    throw new System.Exception("Internal Error : Unknown bone type !");
                }
                Destroy(Children[i].gameObject);
            }
        }

        /// <summary>
        /// Check if the parameters of a bone and those of a boneDto are the same.
        /// </summary>
        private bool checkExistingBone(Transform existingBone, BoneDto savedBone)
        {
            return existingBone.GetComponent<UMI3DBoneType>().BoneType == savedBone.type
                && existingBone.localPosition == savedBone.Position
                && existingBone.localRotation == savedBone.Rotation
                && existingBone.localScale == savedBone.Scale;
        }

        /// <summary>
        /// Instanciate a new bone with the parameters of a BoneDto.
        /// </summary>
        private GameObject instanciateBone(BoneDto bone)
        {
            GameObject Go = null;
            if (bone.type != BoneType.None)
            {
                GameObject Prefab;
                if (listOfPrefabs.TryGetValue(bone.type, out Prefab))
                {
                    Go = Instantiate(Prefab, Anchor.transform);
                    foreach (CVEAvatarPart item in Go.GetComponentsInChildren<CVEAvatarPart>())
                    {
                        item.UserId = user.UserId;
                    }
                }
                else
                {
                    Go = new GameObject(bone.type.ToString());
                    Go.transform.parent = Anchor.transform;
                }
                Go.AddComponent<UMI3DBoneType>().BoneType = bone.type; 
                instanciatedBones.Remove(bone.type);
                instanciatedBones.Add(bone.type, Go);
                updateBone(Go, bone);
            }
            return Go;
        }

        /// <summary>
        /// Update a bone with the parameters of a BoneDto.
        /// </summary>
        private void updateBone(GameObject go, BoneDto bone)
        {
            go.transform.localPosition = bone.Position;
            go.transform.localRotation = bone.Rotation;
            go.transform.localScale = bone.Scale;
            filterBones(go);
        }
    }
}