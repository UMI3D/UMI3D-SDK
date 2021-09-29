﻿/*
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

namespace umi3d.common
{

    public class Physics : MonoBehaviour
    {
        public static RaycastHit[] RaycastAll(Ray ray, float maxDistance = Mathf.Infinity, int layerMask = UnityEngine.Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            return Sort(UnityEngine.Physics.RaycastAll(ray, maxDistance, layerMask, queryTriggerInteraction));
        }

        public static RaycastHit[] RaycastAll(Vector3 origin, Vector3 direction, float maxDistance = Mathf.Infinity, int layerMask = UnityEngine.Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            return Sort(UnityEngine.Physics.RaycastAll(origin, direction, maxDistance, layerMask, queryTriggerInteraction));
        }


        static RaycastHit[] Sort(RaycastHit[] array)
        {
            List<RaycastHit> l = new List<RaycastHit>();
            foreach (RaycastHit r in array)
                l.Add(r);
            l.Sort(Comparison);
            return l.ToArray();
        }

        static int Comparison(RaycastHit x, RaycastHit y)
        {
            return x.distance == y.distance ? 0 : x.distance < y.distance ? -1 : 1;
        }


    }
}