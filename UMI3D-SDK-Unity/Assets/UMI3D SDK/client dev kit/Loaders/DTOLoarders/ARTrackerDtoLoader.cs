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
using System.Collections;
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    public class ARTrackerDtoLoader : AbstractDTOLoader<ARTrackerDto, ARTrackerObject>
    {
        public override void LoadDTO(ARTrackerDto dto, Action<ARTrackerObject> callback)
        {
            var entity = GetComponent<ARTrackerObject>() ?? gameObject.AddComponent<ARTrackerObject>();
            UpdateFromDTO(entity, null, dto);
            callback(entity);
        }

        public override void UpdateFromDTO(ARTrackerObject entity, ARTrackerDto olddto, ARTrackerDto newdto)
        {
            entity.TrackerId = newdto.TrackerId;
        }
    }
}