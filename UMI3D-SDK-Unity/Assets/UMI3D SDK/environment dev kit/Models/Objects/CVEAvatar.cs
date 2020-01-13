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
using umi3d.common;

namespace umi3d.edk
{
    class CVEAvatar : AbstractObject3D<AvatarMappingDto>
    {
        public string UserId { private get; set; }

        private BonePairDictionary bonePairDictionary = new BonePairDictionary();

        public UMI3DAsyncProperty<BonePairDictionary> AsyncBonePairDictionaryProperty;

        public override AvatarMappingDto CreateDto()
        {
            return new AvatarMappingDto();
        }

        public override AvatarMappingDto ToDto(UMI3DUser user)
        {
            var dto = base.ToDto(user);
            dto.bonePairDictionary = AsyncBonePairDictionaryProperty.GetValue(user);
            dto.userId = UserId;
            return dto;
        }

        protected override void SyncProperties()
        {
            base.SyncProperties();
            SyncDictionary();
        }

        /// <summary>
        /// Update the list of Id and BoneType pairs.
        /// </summary>
        private void SyncDictionary()
        {
            foreach (UMI3DUser user in UMI3D.UserManager.GetUsers())
            {
                if (user.UserId == this.UserId)
                {
                    setDictionary(user);
                    AsyncBonePairDictionaryProperty.SetValue(bonePairDictionary);
                    return;
                }
            }
        }

        /// <summary>
        /// Set the list of Id and BoneType pairs.
        /// </summary>
        private void setDictionary(UMI3DUser user)
        {
            this.bonePairDictionary = user.avatar.setUserMapping();
        }

        protected override void initDefinition()
        {
            base.initDefinition();

            AsyncBonePairDictionaryProperty = new UMI3DAsyncProperty<BonePairDictionary>(PropertiesHandler, bonePairDictionary);
            AsyncBonePairDictionaryProperty.OnValueChanged += (BonePairDictionary value) => bonePairDictionary = value;
        }
    }
}
