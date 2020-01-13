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
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    public class ResourceDtoLoader : AbstractDTOLoader<ResourceDto, Resource>
    {

        public override void LoadDTO(ResourceDto dto, Action<Resource> callback)
        {
            // TO CHANGE
            Resource resource = new Resource();

            resource.Url = dto.Url;
            resource.ApiKey = dto.ApiKey;
            resource.Login = dto.Login;
            resource.Password = dto.Password;
            resource.RequestType = dto.RequestType;

            callback(resource);
        }

        public override void UpdateFromDTO(Resource resource, ResourceDto olddto, ResourceDto newdto)
        {
            if (newdto == null)
                return;
        }

    }
}