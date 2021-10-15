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

namespace umi3d.edk.volume
{
    /// <summary>
    /// For testing only.
    /// </summary>
    public class CubicSlice : MonoBehaviour
    {
        public Bounds cube;
        public Point pointPrefab;
        public Face facePrefab;
        public UMI3DUserEvent onVolumeEnter;
        public UMI3DUserEvent onVolumeExit;

        // Start is called before the first frame update
        void Start()
        {
            GameObject storage = new GameObject("cubic slice storage");
            storage.transform.parent = this.transform;
            Point p0 = Instantiate(pointPrefab, storage.transform);
            Point p1 = Instantiate(pointPrefab, storage.transform);
            Point p2 = Instantiate(pointPrefab, storage.transform);
            Point p3 = Instantiate(pointPrefab, storage.transform);

            p0.transform.position = cube.min + Vector3.Scale(cube.size, new Vector3(0, 0, 0));
            p1.transform.position = cube.min + Vector3.Scale(cube.size, new Vector3(0, 1, 0));
            p2.transform.position = cube.min + Vector3.Scale(cube.size, new Vector3(1, 1, 0));
            p3.transform.position = cube.min + Vector3.Scale(cube.size, new Vector3(1, 0, 0));

            List<Point> points = new List<Point>();
            points.Add(p0);
            points.Add(p1);
            points.Add(p2);
            points.Add(p3);

            Face face = Instantiate(facePrefab, storage.transform);
            face.Setup();
            face.SetPoints(points);
            Face.ExtrusionInfo einfo = face.Extrude(pointPrefab);
            einfo.extrusionFace.MoveBegin();
            einfo.extrusionFace.Move(new Vector3(0, 0, cube.size.z));
            einfo.extrusionFace.MoveEnd();

            GameObject slice = new GameObject("volume slice");
            slice.transform.parent = this.transform;
            slice.AddComponent<VolumeSlice>().Setup(einfo.createdSlice.GetPoints(), einfo.createdSlice.GetEdges(), einfo.createdSlice.GetFaces());

            GameObject sliceGroupGO = new GameObject("volume slice group");
            sliceGroupGO.transform.parent = this.transform;
            VolumeSlicesGroup sliceGroup = sliceGroupGO.AddComponent<VolumeSlicesGroup>();
            sliceGroup.volumeSlices.Add(slice.GetComponent<VolumeSlice>());
            sliceGroup.GetUserEnter().AddListener(u => onVolumeEnter.Invoke(u));
            sliceGroup.GetUserExit().AddListener(u => onVolumeExit.Invoke(u));            
        }

        

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(cube.center, cube.size);
        }
    }
}