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

using System.Collections.Generic;
using System.Linq;
using inetum.unityUtils;
using umi3d.common;
using umi3d.common.lbe;
using UnityEngine;
using umi3d.edk.collaboration;
using umi3d.common.lbe.description;
using umi3d.edk.userCapture.tracking;

namespace umi3d.edk
{
    public class LBEManager : Singleton<LBEManager>
    {
        public static Dictionary<ulong, LBEGroup> LBEGroups = new Dictionary<ulong, LBEGroup>();
        public static Dictionary<ulong, LBEGroup> LBELeadersGroups = new Dictionary<ulong, LBEGroup>();

        public bool enforceDistanceConstraint = false;

        public LBEManager() { }

        // Méthode d'initialisation qui peut être appelée après l'instance du singleton
        public void Init()
        {
            UMI3DServer.Instance.OnUserLeave.AddListener(OnUserLeave);
            //UMI3DServer.Instance.OnUserRegistered.AddListener(u => { if (LBEGroups.TryGetValue(1, out LBEGroup group)) UMI3DEnvironment.objectStartPosition.SetValue(u, group.SpawnPoint);});
        }

        #region Synchronisation
        //public void MDMAddUser(UMI3DUser user, UserGuardianDto userGuardianDto)
        //{
        //    UnassignedUser.Add(user.Id(), userGuardianDto);
        //}

        public void LBEAddUser(UMI3DUser user, ulong groupId)
        {
            if (groupId == 0)
            {
                List<LBEGroup> groups = LBEGroups.Values.ToList();

                foreach (LBEGroup group in groups)
                {
                    if (group.ContainsUser(user.Id()))
                    {
                        if (group.leaderId == user.Id())
                            LBELeadersGroups.Remove(user.Id());

                        group.TryRemoveUser(user.Id());
                        return;
                    }
                }
                return;
            }

            if (!LBEGroups.TryGetValue(groupId, out LBEGroup selectedGroup))
            {
                selectedGroup = CreateNewLBEGroup(groupId);
            }

            selectedGroup.TryAddUser(user.Id());
        }

        public void LBERemoveUser(ulong userId, ulong groupId)
        {
            if (!LBEGroups.TryGetValue(groupId, out LBEGroup group))
            {
                group.TryRemoveUser(userId);
            }
        }

        //public void LBECreateGroup(ulong groupeID)
        //{
        //    Debug.Log("Ajout d'un nouveau group");

        //    //ToDo -> Modifier les arguments de la méthode CreateLBEGroup groupe pour fournir d'autres informations du groupe créer par le MDM
        //    CreateLBEGroup(groupeID);
        //}


        public bool LBESetNewLeader(UMI3DUser user, ulong groupId)
        {
            if (!LBEGroups.TryGetValue(groupId, out LBEGroup group))
                return false;

            if (LBELeadersGroups.ContainsKey(user.Id()))
                LBELeadersGroups[user.Id()].SetNewLeader(0);

            group.SetNewLeader(user.Id());
            LBELeadersGroups[user.Id()] = group;

            Debug.Log("NEW LEADER FOR " + group.groupId + " IS " + group.leaderId);

            return true;
        }

        #endregion

        public void OnUserLeave(UMI3DUser user)
        {
            if (!(user as UMI3DCollaborationUser).isColocated)
                return;

            List<LBEGroup> groups = LBEGroups.Values.ToList();

            foreach (LBEGroup group in groups)
            {
                if (group.ContainsUser(user.Id()))
                {
                    group.TryRemoveUser(user.Id());
                    return;
                }
            }

            if (LBELeadersGroups.ContainsKey(user.Id()))
            {
                LBELeadersGroups[user.Id()].SetNewLeader(0);
                LBELeadersGroups.Remove(user.Id());
            }
        }

        //public void OnGuardianReception(UMI3DUser user, UserGuardianDto userGuardDto)
        //{
        //    Debug.Log("<color=cyan>User avec un Group ID = " + userGuardDto.lbeGroupId + "LBE Groups count -> " + LBEGroups.Count + "</color>");

        //    LBEGroup group;

        //    if (!LBEGroups.TryGetValue(userGuardDto.lbeGroupId, out group))
        //        group = CreateLBEGroup(userGuardDto.lbeGroupId);

        //    ulong userId = user.Id();

        //    if (userGuardDto.isAdmin == true && group.AdminId == 0)
        //        SetAdminUser(userId, group, userGuardDto.ARAnchors);

        //    AddUserToGroup(userId, group, userGuardDto);
        //}

        //public void SetLBEGroupDTO(UMI3DUser user, LBEGroupSyncRequestDto lBEGroupDto)
        //{
        //    SyncLBEGroupOperation sendLBEGroupRequest = new SyncLBEGroupOperation(lBEGroupDto);
        //    sendLBEGroupRequest.users = new HashSet<UMI3DUser> { user };

        //    Transaction transaction = new Transaction()
        //    {
        //        reliable = true,
        //    };

        //    transaction.AddIfNotNull(sendLBEGroupRequest);

        //    UMI3DServer.Dispatch(transaction);
        //}

        //public void AddNewUserLBE(LBEAddUserGroupOperationDto addUserGroupOperationsDto, ulong excludedUserId)
        //{
        //    Debug.Log("<color=orange>Add New User LBE</color>");

        //    LBEAddUserGroupOperation addUserLBEGroupRequest = new LBEAddUserGroupOperation()
        //    {
        //        userId = addUserGroupOperationsDto.userId,
        //        isImmersive = addUserGroupOperationsDto.isImmersive,
        //    };

        //    HashSet<UMI3DUser> userSet = UMI3DCollaborationServer.Instance.UserSet();

        //    HashSet<UMI3DUser> filteredUsers = new HashSet<UMI3DUser>();

        //    foreach (UMI3DUser user in userSet)
        //    {
        //        if (user.Id() != excludedUserId)
        //        {
        //            filteredUsers.Add(user);
        //        }
        //    }

        //    addUserLBEGroupRequest.users = filteredUsers;

        //    Transaction transaction = new Transaction()
        //    {
        //        reliable = true,
        //    };

        //    transaction.AddIfNotNull(addUserLBEGroupRequest);

        //    UMI3DServer.Dispatch(transaction);
        //}

        //public void DeleteUserLBE(LBERemoveUserGroupOperationDto delUserGroupOperationsDto, ulong excludedUserId)
        //{
        //    LBERemoveUserGroupOperation delUserLBEGroupRequest = new LBERemoveUserGroupOperation()
        //    {
        //        userId = delUserGroupOperationsDto.userId,
        //    };

        //    var userSet = UMI3DCollaborationServer.Instance.UserSet();

        //    HashSet<UMI3DUser> filteredUsers = new HashSet<UMI3DUser>();

        //    foreach (UMI3DUser user in userSet)
        //    {
        //        if (user.Id() != excludedUserId)
        //        {
        //            filteredUsers.Add(user);
        //        }
        //    }

        //    delUserLBEGroupRequest.users = filteredUsers;

        //    Transaction transaction = new Transaction()
        //    {
        //        reliable = true,
        //    };

        //    transaction.AddIfNotNull(delUserLBEGroupRequest);

        //    UMI3DServer.Dispatch(transaction);
        //}

        public LBEGroup CreateNewLBEGroup(ulong newGroupId)
        {
            LBEGroup newGroup = new LBEGroup(newGroupId, UMI3DEnvironment.objectStartPosition.GetValue());
            LBEGroups.Add(newGroupId, newGroup);

            return newGroup;
        }

        //public void AddUserToGroup(ulong userId, LBEGroup group, UserGuardianDto userGuardianDto)
        //{
        //    Debug.Log("<color=cyan>Add User To Group</color>");

        //    // Ajouter l'utilisateur au groupe
        //    if (userGuardianDto.isImmersive)
        //    {
        //        group.LBEDto.UserVR.Add(userId);
        //    }
        //    else
        //    {
        //        group.LBEDto.UserAR.Add(userId);
        //    }

        //    //Vérifier si la sécurité est nécessaire (Partage des guardians)
        //    if (group.AdminId != 0)
        //    {
        //        SendDataUsers(group);
        //    }
        //}

        //public void SetAdminUser(ulong userId, LBEGroup group, List<ARAnchorDto> aRAnchorDtos)
        //{
        //    Debug.Log("<color=orange>Set Admin User</color>");

        //    if (userId == group.LBEDto.AdminUserId)
        //        return;

        //    if (group.LBEDto.AdminUserId != 0 && LBELeadersGroups.ContainsKey(group.LBEDto.AdminUserId))
        //        LBELeadersGroups.Remove(group.LBEDto.AdminUserId);

        //    LBELeadersGroups.Add(userId, group);
        //    group.LBEDto.AdminUserId = userId;
        //    //group.LBEDto.ARAnchors = aRAnchorDtos;

        //    SendDataUsers(group);
        //}

        //Refacto ici
        //public void SendDataUsers(LBEGroup group)
        //{
        //    foreach (ulong userId in group.LBEDto.UserAR)
        //    {
        //        UMI3DUser user = UMI3DCollaborationServer.Collaboration.GetUser(userId);
        //        if (user != null)
        //        {
        //            SetLBEGroupDTO(user, group.LBEDto);
        //        }
        //    }

        //    foreach (ulong userId in group.LBEDto.UserVR)
        //    {
        //        UMI3DUser user = UMI3DCollaborationServer.Collaboration.GetUser(userId);
        //        if (user != null)
        //        {
        //            SetLBEGroupDTO(user, group.LBEDto);
        //        }
        //    }
        //}

        #region TPGroup
        private List<UMI3DUser> RetrieveUsersByIds(List<ulong> userIds)
        {
            List<UMI3DUser> foundUsers = new List<UMI3DUser>();
            foreach (ulong id in userIds)
                foundUsers.Add(UMI3DCollaborationServer.Collaboration.GetUser(id));
            return foundUsers;
        }


        public void TeleportGroup(UMI3DCollaborationAbstractUser leader, Vector3Dto teleportationVectorDto)
        {
            if (leader == null)
            {
                Debug.LogError("Leader is null");
                return;
            }

            LBEGroup group = LBELeadersGroups[leader.Id()];

            if (group != null)
            {
                Vector3 leaderDeltaPosition = new Vector3(teleportationVectorDto.X, teleportationVectorDto.Y, teleportationVectorDto.Z);
                group.SetSpawnPoint(new Vector3(group.spawnPoint.x + leaderDeltaPosition.x, leaderDeltaPosition.y, group.spawnPoint.z + leaderDeltaPosition.z));

                // Combine les utilisateurs AR et VR du groupe
                List<ulong> userIds = new List<ulong>();
                //userIds.AddRange(group.LBEDto.UserAR);
                //userIds.AddRange(group.LBEDto.UserVR);

                // TO DO 

                // Ajouter les bons Ids autrement, avec la connexion du JoinLBEDto

                userIds.Remove(leader.Id());

                List<UMI3DUser> targetUsers = RetrieveUsersByIds(userIds);

                Transaction Tr = new Transaction(true);

                foreach (UMI3DUser user in targetUsers)
                {
                    if (user is UMI3DTrackedUser trackedUser)
                    {
                        Vector3 currentPosition = trackedUser.CurrentTrackingFrame.position.Struct();
                        Vector3 newPosition = new Vector3(currentPosition.x + leaderDeltaPosition.x, leaderDeltaPosition.y, currentPosition.z + leaderDeltaPosition.z);

                        if (enforceDistanceConstraint)
                            newPosition = EnforceMaxDistanceFromLeader(leaderDeltaPosition, newPosition, 5.0f);

                        TeleportRequest TPRequest = new TeleportRequest(newPosition, trackedUser.CurrentTrackingFrame.rotation.Quaternion());
                        TPRequest.users = new HashSet<UMI3DUser> { user };
                        Tr.AddIfNotNull(TPRequest);
                    }
                }
                Tr.Dispatch();
            }
        }

        private Vector3 EnforceMaxDistanceFromLeader(Vector3 leaderPosition, Vector3 newUserPosition, float maxDistance)
        {
            float distance = Vector3.Distance(leaderPosition, newUserPosition);
            Debug.Log($"Distance avec le leader : {distance} + Leader position : {leaderPosition} + userposition : {newUserPosition}");

            Vector3 newOffsetPosition = newUserPosition;

            if (distance > maxDistance)
            {
                Vector3 direction = (newUserPosition - leaderPosition).normalized;
                return leaderPosition + direction * maxDistance;
            }
            return new Vector3(newOffsetPosition.x, newUserPosition.y, newOffsetPosition.z);
        }
    }
    #endregion
}