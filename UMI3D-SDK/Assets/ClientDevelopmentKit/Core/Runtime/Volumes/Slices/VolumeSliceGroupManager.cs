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
using UnityEngine;
using UnityEngine.Events;
using umi3d.common.volume;
using umi3d.common;

namespace umi3d.cdk.volumes
{
    /// <summary>
    /// Centralise volume slice group management.
    /// </summary>
	public class VolumeSliceGroupManager : Singleton<VolumeSliceGroupManager>
	{
        /// <summary>
        /// Handle waiting for an item reception.
        /// </summary>
        public static class PendingManager
        {
            private static Dictionary<List<string>, List<UnityAction>> pendingCallbacks = new Dictionary<List<string>, List<UnityAction>>();

            public static void OnItemRecieved(string itemId)
            {
                Dictionary<List<string>, List<UnityAction>> pendingCallbacksCopy = new Dictionary<List<string>, List<UnityAction>>();

                foreach (var pending in pendingCallbacks)
                {
                    if (pending.Key.Contains(itemId))
                    {
                        pending.Key.Remove(itemId);
                        if (pending.Key.Count == 0)
                        {
                            foreach(var callback in pending.Value)
                            {
                                Debug.Log("PENDING CALL");
                                callback.Invoke();
                            }
                        }
                        else
                        {
                            pendingCallbacksCopy.Add(pending.Key, pending.Value);
                        }
                    }
                    else
                    {
                        pendingCallbacksCopy.Add(pending.Key, pending.Value);
                    }
                }

                pendingCallbacks = pendingCallbacksCopy;
            }

            public static void SubscribeToVolumeItemReception(List<string> itemsId, UnityAction callback)
            {

                Debug.Log("PENDING LOG");
                List<UnityAction> callbacks = new List<UnityAction>();
                if (pendingCallbacks.TryGetValue(itemsId, out callbacks))
                {
                    pendingCallbacks.Remove(itemsId);
                }

                callbacks.Add(callback);
                pendingCallbacks.Add(itemsId, callbacks);
            }
        }

        private Dictionary<string, Point> points = new Dictionary<string, Point>();
        private Dictionary<string, Face> faces = new Dictionary<string, Face>();
        private Dictionary<string, VolumeSlice> volumeSlices = new Dictionary<string, VolumeSlice>();
        private Dictionary<string, VolumeSliceGroup> volumeSliceGroups = new Dictionary<string, VolumeSliceGroup>();

        public List<Point> GetPoints() => new List<Point>(points.Values);
		public List<Face> GetFaces() => new List<Face>(faces.Values);
        public List<VolumeSlice> GetVolumeSlices() => new List<VolumeSlice>(volumeSlices.Values);
        public List<VolumeSliceGroup> GetVolumeSliceGroups() => new List<VolumeSliceGroup>(volumeSliceGroups.Values);


        #region Exists

        public bool PointExists(string id)
        {
            return points.ContainsKey(id);
        }

        public bool FaceExists(string id)
        {
            return faces.ContainsKey(id);
        }

        public bool VolumeSliceExists(string id)
        {
            return volumeSlices.ContainsKey(id);
        }

        public bool VolumeSliceGroupExist(string id)
        {
            return volumeSliceGroups.ContainsKey(id);
        }

        #endregion

        #region Create

        public Point GetPoint(string id)
        {
            return points[id];
        }

        public Face GetFace(string id)
        {
            return faces[id];
        }

        public VolumeSlice GetVolumeSlice(string id)
        {
            return volumeSlices[id];
        }

        public VolumeSliceGroup GetVolumeSliceGroup(string id)
        {
            return volumeSliceGroups[id];
        }

        #endregion

        #region Create

        public void CreatePoint(PointDto dto, UnityAction<Point> finished)
        {
            if (PointExists(dto.id))
                throw new System.Exception("Point already exists");

            Point p = new Point();
            p.Setup(dto);

            points.Add(dto.id, p);
            finished(p);
        }

        public void CreateFace(FaceDto dto, UnityAction<Face> finished)
        {
            if (FaceExists(dto.id))
                throw new System.Exception("Face already exists");

            UnityAction creation = () =>
            {
                Face face = new Face();
                face.Setup(dto);
                faces.Add(dto.id, face);
                finished(face);
            };

            List<string> pointsNeeded = new List<string>(); 
            foreach(string pointId in dto.pointsIds)
            {
                if (!PointExists(pointId))
                    pointsNeeded.Add(pointId);
            }
            if (pointsNeeded.Count == 0)
                creation();
            else
                PendingManager.SubscribeToVolumeItemReception(pointsNeeded, creation);
        }

        public void CreateVolumeSlice(VolumeSliceDto dto, UnityAction<VolumeSlice> finished)
        {
            if (VolumeSliceExists(dto.id))
                throw new System.Exception("Volume slice group already exists");

            UnityAction creation = () =>
            {
                List<Point> points = new List<Point>();
                List<int> edges = new List<int>();
                List<Face> faces = new List<Face>();

                VolumeSlice slice = new VolumeSlice(dto);
                volumeSlices.Add(dto.id, slice);
                finished(slice);
            };

            List<string> idsNeeded = new List<string>();
            foreach (string pid in dto.points)
            {
                if (!PointExists(pid))
                    idsNeeded.Add(pid);
            }
            foreach (string fid in dto.faces)
            {
                if (!FaceExists(fid))
                    idsNeeded.Add(fid);
            }

            if (idsNeeded.Count == 0)
                creation();
            else
                PendingManager.SubscribeToVolumeItemReception(idsNeeded, creation);

        }

        public void CreateVolumeSliceGroup(VolumeSlicesGroupDto dto, UnityAction<VolumeSliceGroup> finished)
        {
            if (VolumeSliceGroupExist(dto.id))
                throw new System.Exception("Volume slice group already exists");

            Debug.Log("Volume register");
            UnityAction creation = () =>
            {
                VolumeSliceGroup volume = new VolumeSliceGroup();
                volume.Setup(dto);
                volumeSliceGroups.Add(dto.id, volume);

                Debug.Log("Volume ceated");
                finished(volume);
            };

            List<string> idsNeeded = new List<string>();
            foreach(string slice in dto.slicesIds)
            {
                if (!VolumeSliceExists(slice))
                    idsNeeded.Add(slice);
            }
            if (idsNeeded.Count == 0)
                creation();
            else
                PendingManager.SubscribeToVolumeItemReception(idsNeeded, creation);
        }
        
        #endregion

        #region Update
        /*
         * TODO : update functions
         */
        #endregion

        #region Delete

        public void DeletePoint(string id)
        {
            //delete every volume featuring the given point
            foreach(var entry in volumeSliceGroups)
            {
                if (new List<VolumeSlice>(entry.Value.GetSlices()).Exists(s => s.GetPoints().Exists(p => p.id == id)))
                    UMI3DEnvironmentLoader.GetEntity(entry.Value.Id()).Delete();
            }

            points.Remove(id);
        }

        public void DeleteFace(string id)
        {
            //delete every volume featuring the given face
            foreach (var entry in volumeSliceGroups)
            {
                if (new List<VolumeSlice>(entry.Value.GetSlices()).Exists(s => s.GetFaces().Exists(f => f.id == id)))
                    UMI3DEnvironmentLoader.GetEntity(entry.Value.Id()).Delete();
            }

            faces.Remove(id);
        }

        public void DeleteVolumeSliceGroup(string id)
        {
            volumeSliceGroups.Remove(id);
        }

        public void DeleteVolumeSlice(string id)
        {
            volumeSlices.Remove(id);

        }

        #endregion

        public void OnDrawGizmos()
        {
            foreach(var volumeSliceGroup in volumeSliceGroups)
            {
            }
        }
    }
}