using System.Collections;
using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using UnityEngine;

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
